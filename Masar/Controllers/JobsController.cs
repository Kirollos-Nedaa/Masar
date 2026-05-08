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
        private readonly IFileService _fileService;
        private readonly UserManager<ApplicationUser> _userManager;

        public JobsController
        (
            IJobService jobService, 
            UserManager<ApplicationUser> userManager, 
            IApplicationService applicationService,
            IFileService fileService
        )
        {
            _jobService = jobService;
            _userManager = userManager;
            _applicationService = applicationService;
            _fileService = fileService;
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

            var vm = await _applicationService.GetApplyViewAsync(jobId, userId);
            if (vm == null) return NotFound();

            if (vm.Job.RequireCoverLetter && string.IsNullOrWhiteSpace(Form.CoverLetter))
                ModelState.AddModelError("Form.CoverLetter", "Cover letter is required for this position.");

            if (vm.Job.RequireCoverLetter && Form.CoverLetter?.Length < 100)
                ModelState.AddModelError("Form.CoverLetter", "Cover letter must be at least 100 characters.");

            if (!ModelState.IsValid)
            {
                vm.Form = Form;
                return View(vm);
            }

            string? uploadedResumeUrl = null;

            if (Request.Form.Files.Count > 0)
            {
                var resumeFile = Request.Form.Files["resumeFile"];
                if (resumeFile != null && resumeFile.Length > 0)
                {
                    uploadedResumeUrl = await _fileService.SaveResumeAsync(resumeFile, userId);
                }
            }

            var (success, error) = await _applicationService.SubmitApplicationAsync(
                jobId, userId, Form, uploadedResumeUrl);

            if (!success)
            {
                vm.Form = Form;
                ModelState.AddModelError(string.Empty, error ?? "An error occurred.");
                return View(vm);
            }

            TempData["ApplySuccess"] = "Your application has been submitted successfully!";

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, redirectUrl = Url.Action("Details", "Jobs", new { id = jobId }) });
            }

            return RedirectToAction("Details", "Jobs", new { id = jobId });
        }
    }
}
