using Masar.Core.IService;
using Masar.Domain.Models;
using Masar.Domain.ViewModels.AuthDtos;
using Microsoft.AspNetCore.Identity;

namespace Masar.Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<(bool Success, string UserId, IEnumerable<string> Errors)> RegisterAsync(RegisterDto dto)
        {
            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
                return (false, null, result.Errors.Select(e => e.Description));

            return (true, user.Id, null);
        }

        public async Task AssignRoleAsync(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user != null)
            {
                await _userManager.AddToRoleAsync(user, role);
            }
        }

        // Sign in method to be called after registration to set the auth cookie
        public async Task SignInAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            await _signInManager.SignInAsync(user!, isPersistent: false);
        }

        public async Task<AuthResult> LoginAsync(LoginDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return new AuthResult { Success = false, Errors = ["Invalid email or password."] };

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);
            if (!result.Succeeded)
                return new AuthResult { Success = false, Errors = ["Invalid email or password."] };

            return new AuthResult { Success = true, UserId = user.Id };
        }

        // Google OAuth
        public async Task<(AuthResult Result, bool IsNewUser)> ExternalGoogleLoginAsync(ExternalLoginInfo info)
        {
            // Existing linked Google account
            var existingLogin = await _userManager.FindByLoginAsync(
                info.LoginProvider,
                info.ProviderKey);

            if (existingLogin != null)
            {
                await _signInManager.SignInAsync(existingLogin, isPersistent: false);

                return (
                    new AuthResult
                    {
                        Success = true,
                        UserId = existingLogin.Id
                    },
                    false
                );
            }

            var email = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(email))
            {
                return (
                    new AuthResult
                    {
                        Success = false,
                        Errors = ["Google account did not provide an email address."]
                    },
                    false
                );
            }

            // Existing account by email → link Google account
            var userByEmail = await _userManager.FindByEmailAsync(email);

            if (userByEmail != null)
            {
                var addLoginResult = await _userManager.AddLoginAsync(userByEmail, info);

                if (!addLoginResult.Succeeded)
                {
                    return (
                        new AuthResult
                        {
                            Success = false
                        },
                        false
                    );
                }

                if (!userByEmail.EmailConfirmed)
                {
                    userByEmail.EmailConfirmed = true;
                    await _userManager.UpdateAsync(userByEmail);
                }

                await _signInManager.SignInAsync(userByEmail, isPersistent: false);

                return (
                    new AuthResult
                    {
                        Success = true,
                        UserId = userByEmail.Id
                    },
                    false
                );
            }

            // Brand new user
            var newUser = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value ?? string.Empty,
                LastName = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Surname)?.Value ?? string.Empty
            };

            var createResult = await _userManager.CreateAsync(newUser);

            if (!createResult.Succeeded)
            {
                return (
                    new AuthResult
                    {
                        Success = false
                    },
                    false
                );
            }

            var linkResult = await _userManager.AddLoginAsync(newUser, info);

            if (!linkResult.Succeeded)
            {
                return (
                    new AuthResult
                    {
                        Success = false
                    },
                    false
                );
            }

            await _signInManager.SignInAsync(newUser, isPersistent: false);

            return (
                new AuthResult
                {
                    Success = true,
                    UserId = newUser.Id
                },
                true
            );
        }
    }
}
