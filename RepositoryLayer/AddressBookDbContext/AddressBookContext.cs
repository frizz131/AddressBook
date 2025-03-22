using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Models;

namespace RepositoryLayer.AddressBookDbContext
{
    public class AddressBookContext : DbContext
    {
        public AddressBookContext(DbContextOptions<AddressBookContext> options)
            : base(options) { }

        public DbSet<AddressBookEntry> Contacts { get; set; }
    }
}
