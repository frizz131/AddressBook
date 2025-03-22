using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RepositoryLayer.DTOs;
using RepositoryLayer.Services;
using RepositoryLayer.Models;

namespace BusinessLayer.Services
{
    public class AddressBookBL : IAddressBookBL
    {
        private readonly IAddressBookRL _repository;

        public AddressBookBL(IAddressBookRL repository)
        {
            _repository = repository;
        }

        public IEnumerable<AddressBookEntryDto> GetAllEntries()
        {
            var entries = _repository.GetAllEntries();
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
            if (entry == null)
            {
                return null;
            }

            return new AddressBookEntryDto
            {
                Id = entry.Id,
                Name = entry.Name,
                Email = entry.Email,
                Phone = entry.Phone
            };
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
            entryDto.Id = createdEntry.Id;
            return entryDto;
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
            if (updatedEntry == null)
            {
                return null;
            }

            return new AddressBookEntryDto
            {
                Id = updatedEntry.Id,
                Name = updatedEntry.Name,
                Email = updatedEntry.Email,
                Phone = updatedEntry.Phone
            };
        }

        public void DeleteEntry(int id)
        {
            _repository.DeleteEntry(id);
        }
    }
}
