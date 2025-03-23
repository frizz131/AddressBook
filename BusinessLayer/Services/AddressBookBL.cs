using BusinessLayer.Services;
using RepositoryLayer.DTOs;
using RepositoryLayer.Models;
using RepositoryLayer.Services;
using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using RabbitMQ.Client;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace BusinessLayer.Service
{
    public class AddressBookBL : IAddressBookBL
    {
        private readonly AddressBookRL _repository;
        private readonly StackExchange.Redis.IDatabase _redis; // Explicitly specify StackExchange.Redis
        private readonly IConfiguration _configuration;
        private readonly IConnection _rabbitMQConnection;

        public AddressBookBL(AddressBookRL repository, IConnectionMultiplexer redis, IConfiguration configuration, IConnection rabbitMQConnection)
        {
            _repository = repository;
            _redis = redis.GetDatabase();
            _configuration = configuration;
            _rabbitMQConnection = rabbitMQConnection;
        }

        public IEnumerable<AddressBookEntryDto> GetAllEntries()
        {
            var cacheKey = "addressbook:all";
            var cached = _redis.StringGet(cacheKey);
            if (!cached.IsNullOrEmpty)
            {
                return JsonSerializer.Deserialize<IEnumerable<AddressBookEntryDto>>(cached);
            }

            var entries = _repository.GetAllEntries()
                .OfType<AddressBookEntry>()
                .Select(e => new AddressBookEntryDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    Email = e.Email,
                    Phone = e.Phone
                }).ToList();

            _redis.StringSet(cacheKey, JsonSerializer.Serialize(entries), TimeSpan.FromMinutes(10)); // Fixed typo
            return entries;
        }

        public AddressBookEntryDto GetEntryById(int id)
        {
            var cacheKey = $"addressbook:{id}";
            var cached = _redis.StringGet(cacheKey);
            if (!cached.IsNullOrEmpty)
            {
                return JsonSerializer.Deserialize<AddressBookEntryDto>(cached);
            }

            var entry = _repository.GetEntryById(id);
            if (entry is AddressBookEntry addressBookEntry)
            {
                var dto = new AddressBookEntryDto
                {
                    Id = addressBookEntry.Id,
                    Name = addressBookEntry.Name,
                    Email = addressBookEntry.Email,
                    Phone = addressBookEntry.Phone
                };
                _redis.StringSet(cacheKey, JsonSerializer.Serialize(dto), TimeSpan.FromMinutes(10));
                return dto;
            }
            return null;
        }

        public AddressBookEntryDto AddEntry(AddressBookEntryDto entryDto)
        {
            var entry = new AddressBookEntry
            {
                Name = entryDto.Name,
                Email = entryDto.Email,
                Phone = entryDto.Phone
            };

            var createdEntry = _repository.AddEntry(entry);
            if (createdEntry is AddressBookEntry addressBookEntry)
            {
                entryDto.Id = addressBookEntry.Id;
                _redis.KeyDelete("addressbook:all");
                _redis.StringSet($"addressbook:{entryDto.Id}", JsonSerializer.Serialize(entryDto), TimeSpan.FromMinutes(10));

                // Publish contact added event
                using var channel = _rabbitMQConnection.CreateModel();
                var exchange = _configuration["RabbitMQ:Exchange"];
                channel.ExchangeDeclare(exchange, "direct", durable: true);
                var message = JsonSerializer.Serialize(new { Name = entryDto.Name, Email = entryDto.Email });
                var body = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish(exchange, "contact.added", null, body);

                return entryDto;
            }
            throw new InvalidOperationException("Failed to create AddressBookEntry");
        }

        public AddressBookEntryDto UpdateEntry(int id, AddressBookEntryDto entryDto)
        {
            if (id != entryDto.Id)
            {
                throw new ArgumentException("ID in URL must match ID in body");
            }

            var entry = new AddressBookEntry
            {
                Id = id,
                Name = entryDto.Name,
                Email = entryDto.Email,
                Phone = entryDto.Phone
            };

            var updatedEntry = _repository.UpdateEntry(entry);
            if (updatedEntry is AddressBookEntry addressBookEntry)
            {
                var dto = new AddressBookEntryDto
                {
                    Id = addressBookEntry.Id,
                    Name = addressBookEntry.Name,
                    Email = addressBookEntry.Email,
                    Phone = addressBookEntry.Phone
                };
                _redis.KeyDelete("addressbook:all");
                _redis.StringSet($"addressbook:{id}", JsonSerializer.Serialize(dto), TimeSpan.FromMinutes(10));
                return dto;
            }
            return null;
        }

        public void DeleteEntry(int id)
        {
            _repository.DeleteEntry(id);
            _redis.KeyDelete("addressbook:all");
            _redis.KeyDelete($"addressbook:{id}");
        }
    }
}