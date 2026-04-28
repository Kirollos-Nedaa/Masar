using Microsoft.AspNetCore.Mvc;
using Masar.Core.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Masar.Domain.Models;
using Masar.Domain.ViewModels.AuthDtos;
using Masar.Infrastructure.Context;

namespace Masar.Controllers
{
    public class AuthController : Controller
    {
        private const string ResetUserIdSessionKey = "PasswordResetUserId";
        private const string ResetTokenSessionKey = "PasswordResetToken";

        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly AppDbContext _context;

        public AuthController(
            IAuthService authService,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            AppDbContext context)
        {
            _authService = authService;
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }


        // REGISTER
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _authService.RegisterAsync(model);

            if (!result.Success)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error);
                return View(model);
            }

            await _authService.SignInAsync(result.UserId);
            SetSessionAfterLogin(result.UserId);

            return RedirectToAction(nameof(SelectRole));
        }


        // LOGIN
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToDashboard();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _authService.LoginAsync(model);

            if (!result.Success)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error);
                return View(model);
            }

            SetSessionAfterLogin(result.UserId!);

            return RedirectToDashboard();
        }


        // Google Login
        [HttpGet]
        public IActionResult ExternalLogin(string provider, string? returnUrl = null)
        {
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Auth", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        {
            if (remoteError != null)
                return RedirectToAction(nameof(Login));

            var info = await _signInManager.GetExternalLoginInfoAsync();

            if (info == null)
                return RedirectToAction(nameof(Login));

            var (result, isNewUser) = await _authService.ExternalGoogleLoginAsync(info);

            if (!result.Success)
                return RedirectToAction(nameof(Login));

            SetSessionAfterLogin(result.UserId!);

            return isNewUser
                ? RedirectToAction(nameof(SelectRole))
                : RedirectToDashboard();
        }


        // ROLE SELECTION
        [HttpGet]
        public IActionResult SelectRole()
        {
            if (User.IsInRole("Candidate"))
                return RedirectToAction("Dashboard", "Candidate");
            if (User.IsInRole("Company"))
                return RedirectToAction("Dashboard", "Company");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SelectRole(string role)
        {
            var allowedRoles = new[] { "Candidate", "Company" };
            if (!allowedRoles.Contains(role))
            {
                ModelState.AddModelError(string.Empty, "Invalid role selected.");
                return View();
            }

            var userId = _userManager.GetUserId(User);
            await _authService.AssignRoleAsync(userId!, role);

            // Refresh claims so IsInRole works immediately on next request
            var user = await _userManager.FindByIdAsync(userId!);
            await _signInManager.RefreshSignInAsync(user!);

            // Write role to session so layout/views can read it cheaply
            HttpContext.Session.SetString("UserRole", role);

            return role switch
            {
                "Candidate" => RedirectToAction("Dashboard", "Candidate"),
                "Company" => RedirectToAction("Dashboard", "Company"),
                _ => RedirectToAction("Index", "Home")
            };
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // ──────────────── Password Reset ───────────────────────────────────────────────────
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            HttpContext.Session.Remove(ResetUserIdSessionKey);
            HttpContext.Session.Remove(ResetTokenSessionKey);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto model)
        {
            HttpContext.Session.Remove(ResetUserIdSessionKey);
            HttpContext.Session.Remove(ResetTokenSessionKey);

            if (!ModelState.IsValid)
                return View(model);

            var (found, userId, token) = await _authService.GeneratePasswordResetTokenAsync(model.Email);

            if (!found)
            {
                ModelState.AddModelError(string.Empty, "No account found with that email address.");
                return View(model);
            }

            // Keep the reset token pair on the server between steps.
            HttpContext.Session.SetString(ResetUserIdSessionKey, userId);
            HttpContext.Session.SetString(ResetTokenSessionKey, token);

            return RedirectToAction(nameof(ResetPassword));
        }

        // ── Forgot Password — Step 2: enter new password ──────────

        [HttpGet]
        public IActionResult ResetPassword()
        {
            var userId = HttpContext.Session.GetString(ResetUserIdSessionKey);
            var token = HttpContext.Session.GetString(ResetTokenSessionKey);

            // Guard: only reachable after a valid forgot-password lookup.
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction(nameof(ForgotPassword));
            }

            var model = new ResetPasswordDto
            {
                UserId = userId,
                Token = token
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var (success, errors) = await _authService.ResetPasswordAsync(model);

            if (!success)
            {
                foreach (var error in errors)
                    ModelState.AddModelError(string.Empty, error);
                return View(model);
            }

            HttpContext.Session.Remove(ResetUserIdSessionKey);
            HttpContext.Session.Remove(ResetTokenSessionKey);

            // User is now signed in (AuthService calls SignInAsync on success).
            // Store session and bounce to the dashboard.
            SetSessionAfterLogin(model.UserId);
            return RedirectToDashboard();
        }

        // PRIVATE HELPERS
        private void SetSessionAfterLogin(string userId)
        {
            HttpContext.Session.SetString("UserId", userId);

            var role = User.IsInRole("Candidate") ? "Candidate"
                     : User.IsInRole("Company") ? "Company"
                     : string.Empty;

            if (!string.IsNullOrEmpty(role))
                HttpContext.Session.SetString("UserRole", role);
        }

        private IActionResult RedirectToDashboard()
        {
            if (User.IsInRole("Candidate"))
                return RedirectToAction("Dashboard", "Candidate");
            if (User.IsInRole("Company"))
                return RedirectToAction("Dashboard", "Company");

            return RedirectToAction(nameof(SelectRole));
        }

        private string GetDashboardUrl()
        {
            if (User.IsInRole("Candidate"))
                return Url.Action("Dashboard", "Candidate")!;
            if (User.IsInRole("Company"))
                return Url.Action("Dashboard", "Company")!;

            return Url.Action(nameof(SelectRole), "Auth")!;
        }
    }
}
