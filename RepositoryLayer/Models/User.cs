using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryLayer.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; } // Store hashed password
        public string? ResetToken { get; set; } // New: Reset token
        public DateTime? ResetTokenExpiry { get; set; } // New: Token expiry
    }
}
