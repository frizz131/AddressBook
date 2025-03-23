using System.IdentityModel.Tokens.Jwt;
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

namespace BusinessLayer.Service
{
    public class AuthService : IAuthService
    {
        private readonly AddressBookRL _repository;
        private readonly IConfiguration _configuration;

        public AuthService(AddressBookRL repository, IConfiguration configuration)
        {
            _repository = repository;
            _configuration = configuration;
        }

        public async Task<string> Register(UserRegisterDto userDto)
        {
            // Check for existing username or email
            var existingUserByUsername = _repository.GetUserByUsername(userDto.Username);
            var existingUserByEmail = _repository.GetUserByEmail(userDto.Email);
            if (existingUserByUsername != null || existingUserByEmail != null)
            {
                throw new Exception("Username or email already exists");
            }

            // Hash password with SHA256 (fallback)
            var passwordHash = HashPassword(userDto.Password);

            var user = new User
            {
                Username = userDto.Username,
                Email = userDto.Email,
                PasswordHash = passwordHash
            };

            _repository.AddEntry(user);

            return await GenerateJwtToken(user);
        }

        public async Task<string> Login(UserLoginDto userDto)
        {
            var user = _repository.GetUserByUsername(userDto.Username);
            if (user == null || !VerifyPassword(userDto.Password, user.PasswordHash))
            {
                throw new Exception("Invalid username or password");
            }

            return await GenerateJwtToken(user);
        }

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
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            var hash = HashPassword(password);
            return hash == storedHash;
        }
    }
}