using Masar.Core.IService;
using Masar.Domain.Models;
using Masar.Domain.ViewModels;
using Masar.Domain.ViewModels.CandidateDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Masar.Controllers
{
    [Authorize(Roles = "Candidate")]
    public class CandidateController : Controller
    {
        private readonly IDashboardService _dashboardService;
        private readonly IProfileService _profileService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CandidateController(
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
            var dto = await _dashboardService.GetCandidateDashboardAsync(userId);

            return View(dto);
        }

        // ── Profile ───────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = _userManager.GetUserId(User);
            var dto = await _profileService.GetMyCandidateProfileAsync(userId);
            return View(dto);
        }

        // ── Personal Info ─────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> EditPersonalInfo()
        {
            var userId = _userManager.GetUserId(User);
            var profile = await _profileService.GetMyCandidateProfileAsync(userId);

            // Map display DTO → edit DTO
            var dto = new PersonalInfoDto
            {
                FirstName = profile.FirstName,
                LastName = profile.LastName,
                Email = profile.Email,
                PhoneNumber = profile.PhoneNumber,
                Gender = profile.Gender,
                Location = profile.Location,
                DateOfBirth = profile.DateOfBirth,
                Bio = profile.Bio
            };

            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> EditPersonalInfo(PersonalInfoDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var userId = _userManager.GetUserId(User);
            await _profileService.UpdatePersonalInfoAsync(userId, dto);
            return RedirectToAction(nameof(Profile));
        }

        // ── Education ─────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> EditEducation()
        {
            var userId = _userManager.GetUserId(User);
            var profile = await _profileService.GetMyCandidateProfileAsync(userId);

            // Use existing education or empty DTO if none yet
            var dto = profile.Education ?? new EducationDto();
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> EditEducation(EducationDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var userId = _userManager.GetUserId(User);
            await _profileService.UpdateEducationAsync(userId, dto);
            return RedirectToAction(nameof(Profile));
        }

        // ─── Skill section updates ──────────────────

        [HttpGet]
        public async Task<IActionResult> EditSkills()
        {
            var userId = _userManager.GetUserId(User);
            var dto = await _profileService.GetEditSkillsAsync(userId);
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> EditSkills(EditSkillsDto dto)
        {
            var userId = _userManager.GetUserId(User);
            var result = await _profileService.UpdateSkillsAsync(userId, dto);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage);
                var refreshed = await _profileService.GetEditSkillsAsync(userId);
                return View(refreshed);
            }

            return RedirectToAction(nameof(Profile));
        }

        // ── Edit links ──────────────────
        [HttpGet]
        public async Task<IActionResult> EditLinks()
        {
            var userId = _userManager.GetUserId(User);
            var dto = await _profileService.GetMyCandidateProfileAsync(userId);
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateLinks(List<ProfessionalLinkDto> links)
        {
            var userId = _userManager.GetUserId(User);
            await _profileService.UpdateCandidateLinksAsync(userId, links);
            return RedirectToAction(nameof(Profile));
        }

        // ── View Company Profile (read-only) ──────────────────

        [HttpGet]
        public async Task<IActionResult> ViewCompany(int id)
        {
            var dto = await _profileService.GetCompanyProfileAsync(id);
            return View(dto);
        }
    }
}