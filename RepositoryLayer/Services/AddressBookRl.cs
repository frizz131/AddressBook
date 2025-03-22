using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RepositoryLayer.AddressBookDbContext;
using RepositoryLayer.Models;

namespace RepositoryLayer.Services
{
    public class AddressBookRL : IAddressBookRL
    {
        private readonly AddressBookContext _context;

        public AddressBookRL(AddressBookContext context)
        {
            _context = context;
        }

        public IEnumerable<AddressBookEntry> GetAllEntries()
        {
            return _context.Contacts.ToList();
        }

        public AddressBookEntry GetEntryById(int id)
        {
            return _context.Contacts.Find(id);
        }

        public AddressBookEntry AddEntry(AddressBookEntry entry)
        {
            _context.Contacts.Add(entry);
            _context.SaveChanges();
            return entry;
        }

        public AddressBookEntry UpdateEntry(AddressBookEntry entry)
        {
            var existingEntry = _context.Contacts.Find(entry.Id);
            if (existingEntry == null)
            {
                return null;
            }

            existingEntry.Name = entry.Name;
            existingEntry.Email = entry.Email;
            existingEntry.Phone = entry.Phone;

            _context.SaveChanges();
            return existingEntry;
        }

        public void DeleteEntry(int id)
        {
            var entry = _context.Contacts.Find(id);
            if (entry != null)
            {
                _context.Contacts.Remove(entry);
                _context.SaveChanges();
            }
        }
    }
}
