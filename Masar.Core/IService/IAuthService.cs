using Masar.Domain.ViewModels.AuthDtos;
using Microsoft.AspNetCore.Identity;
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
        Task<AuthResult> LoginAsync(LoginDto model);
        Task SignInAsync(string userId);
        Task<(AuthResult Result, bool IsNewUser)> ExternalGoogleLoginAsync(ExternalLoginInfo info);

        // Password reset methods
        Task<(bool Found, string UserId, string Token)> GeneratePasswordResetTokenAsync(string email);
        Task<(bool Success, IEnumerable<string> Errors)> ResetPasswordAsync(ResetPasswordDto dto);
        Task<(bool Success, IEnumerable<string> Errors)> ChangePasswordAsync(string userId, ChangePasswordDto dto);
    }
}
