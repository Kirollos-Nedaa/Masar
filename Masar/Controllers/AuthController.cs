using Masar.Domain.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Masar.Core.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Masar.Domain.Models;
using Sprache;

namespace Masar.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;


        public AuthController
            (
                IAuthService authService, 
                UserManager<ApplicationUser> userManager,
                SignInManager<ApplicationUser> signInManager
            )
        {
            _authService = authService;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // Registration action that creates the user, signs them in, and redirects to role selection
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

            // Sign in after registration, then go to role selection
            await _authService.SignInAsync(result.UserId);
            return RedirectToAction("SelectRole");
        }

        [HttpGet]
        public IActionResult SelectRole()
        {
            // If user already has a role, skip selection
            if (User.IsInRole("Candidate"))
                return RedirectToAction("Dashboard", "Candidate");
            if (User.IsInRole("Company"))
                return RedirectToAction("Dashboard", "Company");

            return View();
        }

        // Handle role selection and assignment, then redirect to the appropriate dashboard
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
            await _authService.AssignRoleAsync(userId, role);

            // Re-sign in to refresh the claims with the new role
            var user = await _userManager.FindByIdAsync(userId);
            await _signInManager.RefreshSignInAsync(user);

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
            return RedirectToAction("Index", "Home");
        }

        // If user is already authenticated, redirect to their dashboard instead of showing login page
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToDashboard();

            return View();
        }

        // Login action that handles authentication and redirects to the appropriate dashboard based on role
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

            return RedirectToDashboard();
        }

        // Helper method to redirect authenticated users to their respective dashboards
        private IActionResult RedirectToDashboard()
        {
            if (User.IsInRole("Candidate"))
                return RedirectToAction("Dashboard", "Candidate");
            if (User.IsInRole("Company"))
                return RedirectToAction("Dashboard", "Company");

            return RedirectToAction("SelectRole", "Auth"); // no role yet
        }
    }
}
