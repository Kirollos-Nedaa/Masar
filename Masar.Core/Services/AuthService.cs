using Masar.Core.IService;
using Masar.Domain.Models;
using Masar.Domain.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace Masar.Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager; 
        private readonly IGoogleTokenValidator _googleValidator;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IGoogleTokenValidator googleValidator)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _googleValidator = googleValidator;
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
        public async Task<(AuthResult Result, bool IsNewUser)> GoogleLoginAsync(string idToken)
        {
            // Step 1 — validate the token Google signed, get the claims back
            var payload = await _googleValidator.ValidateAsync(idToken);

            if (payload == null)
                return (new AuthResult { Success = false, Errors = ["Invalid Google token."] }, false);

            var email = payload.Email;

            // Step 2 — check if this Google account is already linked to a local account
            var existingLogin = await _userManager.FindByLoginAsync("Google", payload.Subject);

            if (existingLogin != null)
            {
                // Returning Google user — sign them straight in
                await _signInManager.SignInAsync(existingLogin, isPersistent: false);
                return (new AuthResult { Success = true, UserId = existingLogin.Id }, false);
            }

            // Step 3 — check if a local account exists with this email
            // (user may have registered with a password before trying Google)
            var userByEmail = await _userManager.FindByEmailAsync(email);

            if (userByEmail != null)
            {
                // Link Google to the existing account so next time Step 2 is used
                await _userManager.AddLoginAsync(
                    userByEmail,
                    new UserLoginInfo("Google", payload.Subject, "Google"));

                // Auto-confirm email since Google already verified it
                if (!userByEmail.EmailConfirmed)
                {
                    userByEmail.EmailConfirmed = true;
                    await _userManager.UpdateAsync(userByEmail);
                }

                await _signInManager.SignInAsync(userByEmail, isPersistent: false);
                return (new AuthResult { Success = true, UserId = userByEmail.Id }, false);
            }

            // Step 4 — brand-new user, create the account from Google claims
            var newUser = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,   // Google already verified it
                FirstName = payload.GivenName ?? string.Empty,
                LastName = payload.FamilyName ?? string.Empty,
            };

            var createResult = await _userManager.CreateAsync(newUser);

            if (!createResult.Succeeded)
            {
                var errors = createResult.Errors.Select(e => e.Description).ToList();
                return (new AuthResult { Success = false, Errors = errors }, false);
            }

            // Link Google login and sign in
            await _userManager.AddLoginAsync(
                newUser,
                new UserLoginInfo("Google", payload.Subject, "Google"));

            await _signInManager.SignInAsync(newUser, isPersistent: false);

            // IsNewUser = true so the controller sends them to SelectRole
            return (new AuthResult { Success = true, UserId = newUser.Id }, true);
        }
    }
}
