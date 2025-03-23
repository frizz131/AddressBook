using BusinessLayer.Services;
using RepositoryLayer.DTOs;
using RepositoryLayer.Models;
using RepositoryLayer.Services;

namespace BusinessLayer.Service
{
    public class AddressBookBL : IAddressBookBL
    {
        private readonly AddressBookRL _repository;

        public AddressBookBL(AddressBookRL repository)
        {
            _repository = repository;
        }

        public IEnumerable<AddressBookEntryDto> GetAllEntries()
        {
            var entries = _repository.GetAllEntries()
                .OfType<AddressBookEntry>(); // Filter to AddressBookEntry only
            return entries.Select(e => new AddressBookEntryDto
            {
                Id = e.Id,
                Name = e.Name,
                Email = e.Email,
                Phone = e.Phone
            }).ToList();
        }

        public AddressBookEntryDto GetEntryById(int id)
        {
            var entry = _repository.GetEntryById(id);
            if (entry is AddressBookEntry addressBookEntry)
            {
                return new AddressBookEntryDto
                {
                    Id = addressBookEntry.Id,
                    Name = addressBookEntry.Name,
                    Email = addressBookEntry.Email,
                    Phone = addressBookEntry.Phone
                };
            }
            return null; // Return null if not an AddressBookEntry
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
                return new AddressBookEntryDto
                {
                    Id = addressBookEntry.Id,
                    Name = addressBookEntry.Name,
                    Email = addressBookEntry.Email,
                    Phone = addressBookEntry.Phone
                };
            }
            return null; // Return null if not found or not an AddressBookEntry
        }

        public void DeleteEntry(int id)
        {
            _repository.DeleteEntry(id);
        }
    }
}