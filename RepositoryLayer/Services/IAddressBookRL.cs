using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RepositoryLayer.Models;

namespace RepositoryLayer.Services
{
    public interface IAddressBookRL
    {
        IEnumerable<AddressBookEntry> GetAllEntries();
        AddressBookEntry GetEntryById(int id);
        AddressBookEntry AddEntry(AddressBookEntry entry);
        AddressBookEntry UpdateEntry(AddressBookEntry entry);
        void DeleteEntry(int id);
    }
}
