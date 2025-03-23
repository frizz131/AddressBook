﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using RepositoryLayer.DTOs;
using RepositoryLayer.Models;
using RepositoryLayer.Services;
using BCrypt.Net;
using BusinessLayer.Services;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore.Storage;
using StackExchange.Redis;
using RabbitMQ.Client;
using System.Text.Json;



namespace BusinessLayer.Service
{
    public class AuthService : IAuthService
    {
        private readonly AddressBookRL _repository;
        private readonly IConfiguration _configuration;
        private readonly StackExchange.Redis.IDatabase _redis; // Explicitly specify StackExchange.Redis
        private readonly IConnection _rabbitMQConnection;

        public AuthService(AddressBookRL repository, IConfiguration configuration, IConnectionMultiplexer redis, IConnection rabbitMQConnection)
        {
            _repository = repository;
            _configuration = configuration;
            _redis = redis.GetDatabase();
            _rabbitMQConnection = rabbitMQConnection;
        }

        public async Task<string> Register(UserRegisterDto userDto)
        {
            var existingUserByUsername = _repository.GetUserByUsername(userDto.Username);
            var existingUserByEmail = _repository.GetUserByEmail(userDto.Email);
            if (existingUserByUsername != null || existingUserByEmail != null)
            {
                throw new Exception("Username or email already exists");
            }

            var passwordHash = HashPassword(userDto.Password);

            var user = new User
            {
                Username = userDto.Username,
                Email = userDto.Email,
                PasswordHash = passwordHash
            };

            _repository.AddEntry(user);

            var token = await GenerateJwtToken(user);
            await _redis.StringSetAsync($"session:{user.Id}", token, TimeSpan.FromHours(1));

            // Publish user registered event
            using var channel = _rabbitMQConnection.CreateModel();
            var exchange = _configuration["RabbitMQ:Exchange"];
            channel.ExchangeDeclare(exchange, "direct", durable: true);
            var message = JsonSerializer.Serialize(new { Email = user.Email });
            var body = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(exchange, "user.registered", null, body);

            return token;
        }

        public async Task<string> Login(UserLoginDto userDto)
        {
            var user = _repository.GetUserByUsername(userDto.Username);
            if (user == null || !VerifyPassword(userDto.Password, user.PasswordHash))
            {
                throw new Exception("Invalid username or password");
            }

            var cachedToken = await _redis.StringGetAsync($"session:{user.Id}");
            if (!cachedToken.IsNullOrEmpty)
            {
                return cachedToken;
            }

            var token = await GenerateJwtToken(user);
            await _redis.StringSetAsync($"session:{user.Id}", token, TimeSpan.FromHours(1));
            return token;
        }

        public async Task ForgotPassword(ForgotPasswordDto forgotPasswordDto)
        {
            var user = _repository.GetUserByEmail(forgotPasswordDto.Email);
            if (user == null)
            {
                return;
            }

            var resetToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            user.ResetToken = resetToken;
            user.ResetTokenExpiry = DateTime.UtcNow.AddHours(1);

            _repository.UpdateEntry(user);
            await SendResetEmail(user.Email, resetToken);
        }

        public async Task ResetPassword(ResetPasswordDto resetPasswordDto)
        {
            var user = _repository.GetUserByEmail(resetPasswordDto.Email);
            if (user == null || user.ResetToken != resetPasswordDto.Token || user.ResetTokenExpiry < DateTime.UtcNow)
            {
                throw new Exception("Invalid or expired reset token");
            }

            user.PasswordHash = HashPassword(resetPasswordDto.NewPassword);
            user.ResetToken = null;
            user.ResetTokenExpiry = null;

            _repository.UpdateEntry(user);
            await _redis.KeyDeleteAsync($"session:{user.Id}");
            await Task.CompletedTask;
        }

        // Single definition of GenerateJwtToken
        private Task<string> GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
        }

        private string HashPassword(string password)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(password)); // Fallback
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            var hash = HashPassword(password);
            return hash == storedHash;
        }

        private async Task SendResetEmail(string email, string token)
        {
            var smtpHost = _configuration["Smtp:Host"];
            var smtpPort = int.Parse(_configuration["Smtp:Port"]);
            var smtpUsername = _configuration["Smtp:Username"];
            var smtpPassword = _configuration["Smtp:Password"];
            var fromEmail = _configuration["Smtp:FromEmail"];

            var resetLink = $"http://localhost:5000/api/auth/reset-password?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";

            using (var client = new SmtpClient(smtpHost, smtpPort))
            {
                client.EnableSsl = true;
                client.Credentials = new System.Net.NetworkCredential(smtpUsername, smtpPassword);

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail),
                    Subject = "Password Reset Request",
                    Body = $"Click the link to reset your password: <a href='{resetLink}'>{resetLink}</a>",
                    IsBodyHtml = true
                };
                mailMessage.To.Add(email);

                await client.SendMailAsync(mailMessage);
            }
        }
    }
}