using Masar.Core.IService;
using Masar.Domain.Models;
using Masar.Domain.ViewModels.CompanyDtos;
using Masar.Domain.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Masar.Domain.ViewModels.Job;
using Masar.Core.Services;
using Masar.Domain.Enums;
using Microsoft.AspNetCore.Components.RenderTree;
using System.ComponentModel.DataAnnotations;
using Masar.Domain.ViewModels.AuthDtos;
using System.Security.Claims;
using Masar.Domain.Helpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Humanizer;

namespace Masar.Controllers
{
    [Authorize(Roles = "Company")]
    public class CompanyController : Controller
    {
        private readonly IDashboardService _dashboardService;
        private readonly IProfileService _profileService;
        private readonly IJobService _jobService;
        private readonly IApplicationService _applicationService;
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CompanyController(
            IDashboardService dashboardService,
            IProfileService profileService,
            IJobService jobService,
            UserManager<ApplicationUser> userManager,
            IApplicationService applicationService,
            IAuthService authService)
        {
            _dashboardService = dashboardService;
            _profileService = profileService;
            _jobService = jobService;
            _userManager = userManager;
            _applicationService = applicationService;
            _authService = authService;
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

        // ── Profile ───────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadLogo(IFormFile Logo)
        {
            var userId = _userManager.GetUserId(User);
            var result = await _profileService.UpdateLogoAsync(userId, Logo);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                if (!result.Success) return Json(new { success = false, message = result.Error });
                return Json(new { success = true, message = "Logo uploaded successfully." });
            }

            return RedirectToAction(nameof(Profile));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLogo()
        {
            var userId = _userManager.GetUserId(User);
            var result = await _profileService.DeleteLogoAsync(userId);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                if (!result.Success) return Json(new { success = false, message = result.Error });
                return Json(new { success = true, message = "Logo deleted successfully." });
            }

            return RedirectToAction(nameof(Profile));
        }

        // ── Edit Company Info ─────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> EditInfo()
        {
            var userId = _userManager.GetUserId(User);
            var dto = await _profileService.GetCompanyInfoForEditAsync(userId);
            if (dto == null) return NotFound();
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditInfo(EditCompanyInfoDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            var userId = _userManager.GetUserId(User);
            await _profileService.UpdateCompanyInfoAsync(userId, dto);

            TempData["SuccessMessage"] = "Company information updated successfully.";
            return RedirectToAction(nameof(Profile));
        }

        // ── Edit Contact Info ─────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> EditContact()
        {
            var userId = _userManager.GetUserId(User);
            var dto = await _profileService.GetCompanyContactForEditAsync(userId);
            if (dto == null) return NotFound();
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditContact(EditCompanyContactDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            var userId = _userManager.GetUserId(User);
            await _profileService.UpdateCompanyContactAsync(userId, dto);

            TempData["SuccessMessage"] = "Contact information updated successfully.";
            return RedirectToAction(nameof(Profile));
        }

        // ── Edit Links ──────────────────────────────────────
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
            TempData["SuccessMessage"] = "Links updated successfully.";
            return RedirectToAction(nameof(Profile));
        }

        // ── View Candidate Profile (read-only) ────────────────
        [HttpGet]
        public async Task<IActionResult> ViewCandidate(int id)
        {
            var dto = await _profileService.GetCandidateProfileAsync(id);
            return View(dto);
        }

        // ── Job Posting ──────────────────────────────────────────
        [HttpGet]
        public IActionResult PostJob()
        {
            return View(new PostJobDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostJob(PostJobDto dto)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please complete all of the rquired fields.";
                return View(dto);
            }

            var userId = _userManager.GetUserId(User);

            try
            {
                await _jobService.PostJobAsync(userId, dto);

                TempData["SuccessMessage"] = "Job posted successfully!";
                return RedirectToAction(nameof(Jobs));
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError(nameof(dto.ApplicationDeadline), ex.Message);
                return View(dto);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
        }

        // ── Edit Job ──────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> EditJob(int id)
        {
            var userId = _userManager.GetUserId(User);
            var dto = await _jobService.GetJobForEditAsync(userId, id);

            if (dto == null)
                return NotFound();

            ViewData["IsEdit"] = true;
            ViewData["JobId"] = id;
            return View("PostJob", dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditJob(int jobId, PostJobDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewData["IsEdit"] = true;
                ViewData["JobId"] = jobId;
                return View("PostJob", dto);
            }

            var userId = _userManager.GetUserId(User);
            bool success;

            try
            {
                success = await _jobService.UpdateJobAsync(userId, jobId, dto);
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError(nameof(dto.ApplicationDeadline), ex.Message);
                ViewData["IsEdit"] = true;
                ViewData["JobId"] = jobId;
                return View("PostJob", dto);
            }

            if (!success)
                return NotFound();

            TempData["SuccessMessage"] = "Job updated successfully!";
            return RedirectToAction(nameof(Jobs));
        }

        // ── Change Password ─────────────────────────────────────────
        [HttpGet]
        public IActionResult ChangePassword()
        {
            ViewData["ReturnController"] = "Company";
            return View(new ChangePasswordDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewData["ReturnController"] = "Company";
                return View(dto);
            }

            var userId = _userManager.GetUserId(User);
            var (success, errors) = await _authService.ChangePasswordAsync(userId!, dto);

            if (!success)
            {
                foreach (var error in errors)
                    ModelState.AddModelError(string.Empty, error);

                ViewData["ReturnController"] = "Company";
                return View(dto);
            }

            TempData["SuccessMessage"] = "Password changed successfully.";
            return RedirectToAction(nameof(Profile));
        }

        // ── Toggle Active/Closed ──────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleJobStatus(int jobId, string returnUrl = null)
        {
            var userId = _userManager.GetUserId(User);
            var result = await _jobService.ToggleJobStatusAsync(userId, jobId);

            if (result.Success && result.WasExtended)
            {
                TempData["SuccessMessage"] = "The application deadline was automatically extended by 15 days.";
            }
            else if (result.Success)
            {
                TempData["SuccessMessage"] = "Job status updated successfully.";
            }

            return LocalRedirect(returnUrl ?? Url.Action("Jobs"));
        }

        // ── Jobs List ─────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Jobs(int page = 1)
        {
            var userId = _userManager.GetUserId(User);
            var jobs = await _jobService.GetCompanyJobsAsync(userId, page);

            return View(jobs);
        }

        // ── Applicants ────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Applicants(int jobId, string? search = null, string? status = null, string? sort = null, int page = 1)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId is null) return RedirectToAction("Login", "Auth");

            var vm = await _applicationService.GetApplicantsAsync(jobId, userId, search, status, sort, page);
            if (vm is null) return NotFound();

            ViewData["HasSidebar"] = true;
            return View(vm);
        }

        // ── Review application (GET — transitions Applied → UnderReview) ──────────────
        [HttpGet]
        public async Task<IActionResult> ReviewApplication(int applicationId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId is null) return RedirectToAction("Login", "Auth");

            var vm = await _applicationService.StartReviewAsync(applicationId, userId);
            if (vm is null) return NotFound();

            ViewData["HasSidebar"] = true;
            return View(vm);
        }

        // ── Accept (POST) ─────────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptApplication(int applicationId, int jobId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId is null) return RedirectToAction("Login", "Auth");

            var ok = await _applicationService.AcceptApplicationAsync(applicationId, userId);
            if (!ok) return NotFound();

            TempData["SuccessMessage"] = "Applicant accepted successfully.";
            return RedirectToAction(nameof(Applicants), new { jobId });
        }

        // ── Reject (POST) ─────────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectApplication(int applicationId, int jobId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId is null) return RedirectToAction("Login", "Auth");

            var ok = await _applicationService.RejectApplicationAsync(applicationId, userId);
            if (!ok) return NotFound();

            TempData["SuccessMessage"] = "Applicant rejected.";
            return RedirectToAction(nameof(Applicants), new { jobId });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateApplicationStatus(int applicationId, int jobId, string newStatus)
        {
            if (!Enum.TryParse<ApplicationStatus>(newStatus, out var status))
                return BadRequest();

            var userId = _userManager.GetUserId(User);
            var (_, error) = await _applicationService
                .UpdateApplicationStatusAsync(userId, applicationId, status);

            if (error != null) TempData["Error"] = error;
            return RedirectToAction(nameof(Applicants), new { jobId });
        }
    }
}
