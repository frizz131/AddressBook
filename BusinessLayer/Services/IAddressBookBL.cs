using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RepositoryLayer.DTOs;

namespace BusinessLayer.Services
{
    public interface IAddressBookBL
    {
        IEnumerable<AddressBookEntryDto> GetAllEntries();
        AddressBookEntryDto GetEntryById(int id);
        AddressBookEntryDto AddEntry(AddressBookEntryDto entryDto);
        AddressBookEntryDto UpdateEntry(int id, AddressBookEntryDto entryDto);
        void DeleteEntry(int id);
    }
}
