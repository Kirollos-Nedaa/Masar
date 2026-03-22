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
        Task<AuthResult> LoginAsync(LoginDto model);
        Task SignInAsync(string userId);
        Task AssignRoleAsync(string userId, string role);
        Task<(AuthResult Result, bool IsNewUser)> GoogleLoginAsync(string idToken);
    }
}
