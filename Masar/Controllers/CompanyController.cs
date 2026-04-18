using Masar.Core.IService;
using Masar.Domain.Models;
using Masar.Domain.ViewModels.CompanyDtos;
using Masar.Domain.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Masar.Domain.ViewModels.Job;

namespace Masar.Controllers
{
    [Authorize(Roles = "Company")]
    public class CompanyController : Controller
    {
        private readonly IDashboardService _dashboardService;
        private readonly IProfileService _profileService;
        private readonly IJobService _jobService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CompanyController(
            IDashboardService dashboardService,
            IProfileService profileService,
            IJobService jobService,
            UserManager<ApplicationUser> userManager)
        {
            _dashboardService = dashboardService;
            _profileService = profileService;
            _jobService = jobService;
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

        // ── Update Links ──────────────────────────────────────

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
                return View(dto);

            var userId = _userManager.GetUserId(User);

            try
            {
                await _jobService.PostJobAsync(userId, dto);
                TempData["Success"] = "Job posted successfully!";
                return RedirectToAction(nameof(Jobs));
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
            var success = await _jobService.UpdateJobAsync(userId, jobId, dto);

            if (!success)
                return NotFound();

            TempData["Success"] = "Job updated successfully!";
            return RedirectToAction(nameof(Jobs));
        }

        // ── Toggle Active/Closed ──────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleJobStatus(int jobId)
        {
            var userId = _userManager.GetUserId(User);
            await _jobService.ToggleJobStatusAsync(userId, jobId);
            return RedirectToAction(nameof(Jobs));
        }

        // ── Jobs List ─────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> Jobs()
        {
            var userId = _userManager.GetUserId(User);
            var jobs = await _jobService.GetCompanyJobsAsync(userId);

            if (TempData["Success"] is string msg)
                ViewBag.SuccessMessage = msg;

            return View(jobs);
        }

        // ── Applicants (placeholder — next phase) ─────────────

        [HttpGet]
        public IActionResult Applicants(int jobId)
        {
            ViewBag.JobId = jobId;
            return View();
        }
    }
}