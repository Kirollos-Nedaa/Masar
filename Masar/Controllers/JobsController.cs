using Masar.Core.IService;
using Masar.Domain.Models;
using Masar.Domain.ViewModels.JobDtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Masar.Controllers
{
    public class JobsController : Controller
    {
        private readonly IJobService _jobService;
        private readonly IApplicationService _applicationService;
        private readonly UserManager<ApplicationUser> _userManager;

        public JobsController
        (
            IJobService jobService, 
            UserManager<ApplicationUser> userManager, 
            IApplicationService applicationService
        )
        {
            _jobService = jobService;
            _userManager = userManager;
            _applicationService = applicationService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? search,
            string? location,
            [FromQuery(Name = "jobTypes")] List<string>? jobTypes,
            [FromQuery(Name = "industries")] List<string>? industries,
            string? salaryRange,
            string sortBy = "recent",
            int page = 1)
        {
            var filter = new JobFilterDto
            {
                Search = search,
                Location = location,
                JobTypes = jobTypes ?? new List<string>(),
                Industries = industries ?? new List<string>(),
                SalaryRange = salaryRange,
                SortBy = sortBy,
                Page = page,
                PageSize = 6
            };

            var candidateUserId = User.Identity?.IsAuthenticated == true
                ? _userManager.GetUserId(User)
                : null;

            var result = await _jobService.BrowseJobsAsync(filter, candidateUserId);
            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var candidateUserId = User.Identity?.IsAuthenticated == true
                ? _userManager.GetUserId(User)
                : null;

            var dto = await _jobService.GetJobDetailAsync(id, candidateUserId);

            if (dto == null)
                return NotFound();

            return View(dto);
        }

        // ─────────────────────────────────────────────────────
        //  JOB APPLICATION
        // ─────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> Apply(int jobId)
        {
            var userId = _userManager.GetUserId(User);
            var vm = await _applicationService.GetApplyViewAsync(jobId, userId);

            if (vm == null)
                return NotFound();

            // Already applied?
            if (vm.Job.HasApplied)
                return RedirectToAction("Details", "Jobs", new { id = jobId });

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(int jobId, ApplyJobDto Form)
        {
            var userId = _userManager.GetUserId(User);

            if (!ModelState.IsValid)
            {
                // Rebuild the view model to re-render the form
                var vm = await _applicationService.GetApplyViewAsync(jobId, userId);
                if (vm == null) return NotFound();
                vm.Form = Form;
                return View(vm);
            }

            // Handle resume upload (placeholder — wire to S3 when ready)
            string? uploadedResumeUrl = null;
            // TODO: if (!dto.UseExistingResume && Request.Form.Files["resumeFile"] != null) { upload }

            var (success, error) = await _applicationService.SubmitApplicationAsync(
                jobId, userId, Form, uploadedResumeUrl);

            if (!success)
            {
                var vm = await _applicationService.GetApplyViewAsync(jobId, userId);
                if (vm != null) vm.Form = Form;
                ModelState.AddModelError(string.Empty, error ?? "An error occurred.");
                return View(vm);
            }

            TempData["ApplySuccess"] = "Your application has been submitted successfully!";
            return RedirectToAction("Details", "Jobs", new { id = jobId });
        }
    }
}
