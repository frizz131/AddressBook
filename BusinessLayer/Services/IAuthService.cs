using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RepositoryLayer.DTOs;

namespace BusinessLayer.Services
{
    public interface IAuthService
    {
        Task<string> Register(UserRegisterDto userDto);
        Task<string> Login(UserLoginDto userDto);
    }
}
