using Masar.Core.IService;
using Masar.Domain.Models;
using Masar.Domain.ViewModels.CompanyDtos;
using Masar.Domain.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Masar.Controllers
{
    [Authorize(Roles = "Company")]
    public class CompanyController : Controller
    {
        private readonly IDashboardService _dashboardService;
        private readonly IProfileService _profileService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CompanyController(
            IDashboardService dashboardService,
            IProfileService profileService,
            UserManager<ApplicationUser> userManager)
        {
            _dashboardService = dashboardService;
            _profileService = profileService;
            _userManager = userManager;
        }

        // ── Dashboard ─────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var userId = _userManager.GetUserId(User);
            var dto = await _dashboardService.GetCompanyDashboardAsync(userId);
            return View(dto);
        }

        // ── Profile ───────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = _userManager.GetUserId(User);
            var dto = await _profileService.GetMyCompanyProfileAsync(userId);
            return View(dto);
        }

        // ── Edit Company Info ─────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> EditInfo()
        {
            var userId = _userManager.GetUserId(User);
            var profile = await _profileService.GetMyCompanyProfileAsync(userId);

            var dto = new CompanyInfoDto
            {
                CompanyName = profile.CompanyName,
                Industry = profile.Industry,
                Size = profile.Size,
                Description = profile.Description,
                ContactEmail = profile.ContactEmail,
                ContactPhone = profile.ContactPhone,
                Address = profile.Address,
                LogoUrl = profile.LogoUrl
            };

            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> EditInfo(CompanyInfoDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var userId = _userManager.GetUserId(User);
            await _profileService.UpdateCompanyInfoAsync(userId, dto);
            return RedirectToAction(nameof(Profile));
        }

        // ── Update Links (inline on profile page) ─────────────
        [HttpGet]
        public async Task<IActionResult> EditLinks()
        {
            var userId = _userManager.GetUserId(User);
            var dto = await _profileService.GetMyCompanyProfileAsync(userId);
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateLinks(List<ProfessionalLinkDto> links)
        {
            var userId = _userManager.GetUserId(User);
            await _profileService.UpdateCompanyLinksAsync(userId, links);
            return RedirectToAction(nameof(Profile));
        }

        // ── View Candidate Profile (read-only) ────────────────

        [HttpGet]
        public async Task<IActionResult> ViewCandidate(int id)
        {
            var dto = await _profileService.GetCandidateProfileAsync(id);
            return View(dto);
        }
    }
}