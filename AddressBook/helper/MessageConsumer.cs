using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Net.Mail;
using System.Text;
using System.Text.Json;

namespace AddressBook.helper
{
    public class MessageConsumer : BackgroundService
    {
        private readonly IConnection _connection;
        private readonly IConfiguration _configuration;

        public MessageConsumer(IConnection connection, IConfiguration configuration)
        {
            _connection = connection;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var channel = _connection.CreateModel();
            var exchange = _configuration["RabbitMQ:Exchange"];
            var userQueue = _configuration["RabbitMQ:Queues:UserRegistered"];
            var contactQueue = _configuration["RabbitMQ:Queues:ContactAdded"];

            channel.ExchangeDeclare(exchange, "direct", durable: true);
            channel.QueueDeclare(userQueue, durable: true, exclusive: false, autoDelete: false);
            channel.QueueDeclare(contactQueue, durable: true, exclusive: false, autoDelete: false);
            channel.QueueBind(userQueue, exchange, "user.registered");
            channel.QueueBind(contactQueue, exchange, "contact.added");

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var routingKey = ea.RoutingKey;

                if (routingKey == "user.registered")
                {
                    var userEvent = JsonSerializer.Deserialize<UserRegisteredEvent>(message);
                    await SendWelcomeEmail(userEvent.Email);
                }
                else if (routingKey == "contact.added")
                {
                    var contactEvent = JsonSerializer.Deserialize<ContactAddedEvent>(message);
                    Console.WriteLine($"Contact added: {contactEvent.Name}, {contactEvent.Email}");
                }

                channel.BasicAck(ea.DeliveryTag, false);
            };

            channel.BasicConsume(userQueue, autoAck: false, consumer: consumer);
            channel.BasicConsume(contactQueue, autoAck: false, consumer: consumer);

            await Task.CompletedTask; // Keep service running
        }

        private async Task SendWelcomeEmail(string email)
        {
            var smtpHost = _configuration["Smtp:Host"];
            var smtpPort = int.Parse(_configuration["Smtp:Port"]);
            var smtpUsername = _configuration["Smtp:Username"];
            var smtpPassword = _configuration["Smtp:Password"];
            var fromEmail = _configuration["Smtp:FromEmail"];

            using var client = new SmtpClient(smtpHost, smtpPort);
            client.EnableSsl = true;
            client.Credentials = new System.Net.NetworkCredential(smtpUsername, smtpPassword);

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail),
                Subject = "Welcome to AddressBook!",
                Body = "Thank you for registering with AddressBook.",
                IsBodyHtml = true
            };
            mailMessage.To.Add(email);

            await client.SendMailAsync(mailMessage);
        }
    }

    public class UserRegisteredEvent
    {
        public string Email { get; set; }
    }

    public class ContactAddedEvent
    {
        public string Name { get; set; }
        public string Email { get; set; }
    }
}