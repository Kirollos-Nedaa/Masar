using Masar.Domain.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masar.Core.IService
{
    public interface IAuthService
    {
        Task<(bool Success, string UserId, IEnumerable<string> Errors)> RegisterAsync(RegisterDto dto);

        Task AssignRoleAsync(string userId, string role);
        Task SignInAsync(string userId);
        Task<AuthResult> LoginAsync(LoginDto model);
    }
}
