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
        IEnumerable<object> GetAllEntries();
        object GetEntryById(int id);
        object AddEntry(object entry);
        object UpdateEntry(object entry);
        void DeleteEntry(int id);
    }
}