using Masar.Core.IService;
using Masar.Core.Services;
using Masar.Domain.Models;
using Masar.Domain.ViewModels;
using Masar.Domain.ViewModels.AuthDtos;
using Masar.Domain.ViewModels.CandidateDtos;
using Masar.Domain.ViewModels.JobDtos;
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
        private readonly IApplicationService _applicationService;
        private readonly IAuthService _authService;
        private readonly IFileService _fileService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CandidateController(
            IDashboardService dashboardService,
            IProfileService profileService,
            IApplicationService applicationService,
            UserManager<ApplicationUser> userManager,
            IAuthService authService,
            IFileService fileService)
        {
            _dashboardService = dashboardService;
            _profileService = profileService;
            _applicationService = applicationService;
            _userManager = userManager;
            _authService = authService;
            _fileService = fileService;
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

        // ── UploadResume ───────────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> UploadResume(IFormFile Resume)
        {
            var userId = _userManager.GetUserId(User);
            var result = await _profileService.UpdateResumeAsync(userId, Resume);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                if (!result.Success) return Json(new { success = false, message = result.Error });
                return Json(new { success = true, message = "Resume uploaded successfully." });
            }

            if (!result.Success) TempData["ProfileError"] = result.Error;
            else TempData["SuccessMessage"] = "Resume uploaded successfully.";

            return RedirectToAction(nameof(Profile));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteResume()
        {
            var userId = _userManager.GetUserId(User);
            var result = await _profileService.DeleteResumeAsync(userId);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                if (!result.Success) return Json(new { success = false, message = result.Error });
                return Json(new { success = true, message = "Resume deleted successfully." });
            }

            return RedirectToAction(nameof(Profile));
        }

        // ── Personal Info ─────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> EditPersonalInfo()
        {
            var userId = _userManager.GetUserId(User);
            var profile = await _profileService.GetMyCandidateProfileAsync(userId);

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
            TempData["SuccessMessage"] = "Personal information updated successfully.";
            return RedirectToAction(nameof(Profile));
        }

        // ── Change Password ─────────────────────────────────────────
        [HttpGet]
        public IActionResult ChangePassword()
        {
            ViewData["ReturnController"] = "Candidate";
            return View(new ChangePasswordDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewData["ReturnController"] = "Candidate";
                return View(dto);
            }

            var userId = _userManager.GetUserId(User);
            var (success, errors) = await _authService.ChangePasswordAsync(userId!, dto);

            if (!success)
            {
                foreach (var error in errors)
                    ModelState.AddModelError(string.Empty, error);

                ViewData["ReturnController"] = "Candidate";
                return View(dto);
            }

            TempData["SuccessMessage"] = "Password changed successfully.";
            return RedirectToAction(nameof(Profile));
        }

        // ── Education ─────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> EditEducation()
        {
            var userId = _userManager.GetUserId(User);
            var profile = await _profileService.GetMyCandidateProfileAsync(userId);
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
            TempData["SuccessMessage"] = "Education updated successfully.";
            return RedirectToAction(nameof(Profile));
        }

        // ── Skills ────────────────────────────────────────────

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

            TempData["SuccessMessage"] = "Skills updated successfully.";
            return RedirectToAction(nameof(Profile));
        }

        // ── Links ─────────────────────────────────────────────

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
            TempData["SuccessMessage"] = "Links updated successfully.";
            return RedirectToAction(nameof(Profile));
        }

        // ── View Company (read-only) ──────────────────────────

        [HttpGet]
        public async Task<IActionResult> ViewCompany(int id)
        {
            var dto = await _profileService.GetCompanyProfileAsync(id);
            return View(dto);
        }

        // ── Applications ──────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> Applications()
        {
            var userId = _userManager.GetUserId(User);
            var apps = await _applicationService.GetCandidateApplicationsAsync(userId);
            return View(apps);
        }

        // ── Saved Jobs ────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> SavedJobs(string? search = null, string? sortBy = null)
        {
            var userId = _userManager.GetUserId(User);
            var saved = await _profileService.GetSavedJobsAsync(userId, search, sortBy);

            ViewBag.Search = search;
            ViewBag.SortBy = sortBy ?? "recent";

            return View(saved);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleSaveJob(int jobId, string? returnUrl = null)
        {
            var userId = _userManager.GetUserId(User);
            await _profileService.ToggleSaveJobAsync(jobId, userId);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Details", "Jobs", new { id = jobId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearSavedJobs()
        {
            var userId = _userManager.GetUserId(User);
            await _profileService.ClearSavedJobsAsync(userId);
            TempData["SuccessMessage"] = "All saved jobs have been cleared.";
            return RedirectToAction(nameof(SavedJobs));
        }
    }
}