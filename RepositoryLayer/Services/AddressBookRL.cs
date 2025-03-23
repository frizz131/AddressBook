using RepositoryLayer.AddressBookDbContext;
using RepositoryLayer.Models;

namespace RepositoryLayer.Services
{
    public class AddressBookRL
    {
        private readonly AddressBookContext _context;

        public AddressBookRL(AddressBookContext context)
        {
            _context = context;
        }

        public IEnumerable<object> GetAllEntries()
        {
            var addressBookEntries = _context.AddressBookEntries.ToList<object>();
            var users = _context.Users.ToList<object>();
            return addressBookEntries.Concat(users);
        }

        public object GetEntryById(int id)
        {
            var addressBookEntry = _context.AddressBookEntries.Find(id);
            if (addressBookEntry != null) return addressBookEntry;

            var user = _context.Users.Find(id);
            return user;
        }

        public User GetUserByUsername(string username)
        {
            return _context.Users.FirstOrDefault(u => u.Username == username);
        }

        public User GetUserByEmail(string email)
        {
            return _context.Users.FirstOrDefault(u => u.Email == email);
        }

        public object AddEntry(object entry)
        {
            if (entry is AddressBookEntry addressBookEntry)
            {
                _context.AddressBookEntries.Add(addressBookEntry);
            }
            else if (entry is User user)
            {
                _context.Users.Add(user);
            }
            else
            {
                throw new ArgumentException("Unsupported entity type");
            }

            _context.SaveChanges();
            return entry;
        }

        public object UpdateEntry(object entry)
        {
            if (entry is AddressBookEntry addressBookEntry)
            {
                var existingEntry = _context.AddressBookEntries.Find(addressBookEntry.Id);
                if (existingEntry == null) return null;
                existingEntry.Name = addressBookEntry.Name;
                existingEntry.Email = addressBookEntry.Email;
                existingEntry.Phone = addressBookEntry.Phone;
                _context.SaveChanges();
                return existingEntry;
            }
            else if (entry is User user)
            {
                var existingUser = _context.Users.Find(user.Id);
                if (existingUser == null) return null;
                existingUser.Username = user.Username;
                existingUser.Email = user.Email;
                existingUser.PasswordHash = user.PasswordHash;
                _context.SaveChanges();
                return existingUser;
            }
            return null;
        }

        public void DeleteEntry(int id)
        {
            var addressBookEntry = _context.AddressBookEntries.Find(id);
            if (addressBookEntry != null)
            {
                _context.AddressBookEntries.Remove(addressBookEntry);
                _context.SaveChanges();
                return;
            }

            var user = _context.Users.Find(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }
        }
    }
}