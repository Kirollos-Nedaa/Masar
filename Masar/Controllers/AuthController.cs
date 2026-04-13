using Microsoft.AspNetCore.Mvc;
using Masar.Core.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Masar.Domain.Models;
using Masar.Domain.ViewModels.AuthDtos;

namespace Masar.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AuthController(
            IAuthService authService,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _authService = authService;
            _userManager = userManager;
            _signInManager = signInManager;
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