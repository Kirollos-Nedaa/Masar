using Masar.Core.IService;
using Masar.Domain.Models;
using Masar.Domain.ViewModels.AuthDtos;
using Masar.Infrastructure.Context;
using Microsoft.AspNetCore.Identity;

namespace Masar.Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly AppDbContext _context;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            AppDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
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

                // Create appropriate profile based on role
                if (role == "Company")
                {
                    var companyProfile = new CompanyProfile { UserId = userId };
                    _context.CompanyProfiles.Add(companyProfile);
                }
                else if (role == "Candidate")
                {
                    var candidateProfile = new CandidateProfile { UserId = userId };
                    _context.CandidateProfiles.Add(candidateProfile);
                }

                await _context.SaveChangesAsync();
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

        // ────────────────── Password reset methods ────────────────────────────────────
        public async Task<(bool Found, string UserId, string Token)> GeneratePasswordResetTokenAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email.Trim().ToLower());
            if (user == null)
                return (false, string.Empty, string.Empty);

            // ASP.NET Identity's built-in token generation — no email sending required.
            // The token is passed directly through the URL (server-side reset flow).
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            return (true, user.Id, token);
        }

        public async Task<(bool Success, IEnumerable<string> Errors)> ResetPasswordAsync(ResetPasswordDto dto)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null)
                return (false, ["User not found."]);

            var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
            if (!result.Succeeded)
                return (false, result.Errors.Select(e => e.Description));

            // Sign the user in immediately after a successful password reset
            await _signInManager.SignInAsync(user, isPersistent: false);
            return (true, []);
        }

        public async Task<(bool Success, IEnumerable<string> Errors)> ChangePasswordAsync(string userId, ChangePasswordDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return (false, ["User not found."]);

            var result = await _userManager.ChangePasswordAsync(
                user, dto.CurrentPassword, dto.NewPassword);

            if (!result.Succeeded)
                return (false, result.Errors.Select(e => e.Description));

            // Refresh sign-in cookie so the security stamp stays valid
            await _signInManager.RefreshSignInAsync(user);
            return (true, []);
        }
    }
}
