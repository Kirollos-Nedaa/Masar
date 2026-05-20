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
        public async Task<IActionResult> Index(string? search, string? location, [FromQuery(Name = "jobTypes")] List<string>? jobTypes, [FromQuery(Name = "industries")] List<string>? industries, [FromQuery(Name = "Departments")] List<string>? departments, string? salaryRange, string sortBy = "recent",int page = 1)
        {
            var filter = new JobFilterDto
            {
                Search = search,
                Location = location,
                JobTypes = jobTypes ?? new List<string>(),
                Industries = industries ?? new List<string>(),
                Departments = departments ?? new List<string>(),
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
                ModelState.AddModelError("Form.CoverLetter", "You must fill out the Cover letter.");

            if (vm.Job.RequireCoverLetter && Form.CoverLetter?.Length < 100)
                ModelState.AddModelError("Form.CoverLetter", "Cover letter must be at least 100 characters.");

            if (vm.Job.RequireCv)
            {
                var uploadedFile = Request.Form.Files["resumeFile"];
                bool hasUpload = uploadedFile != null && uploadedFile.Length > 0;
                bool hasExisting = Form.UseExistingResume && !string.IsNullOrEmpty(Form.ExistingResumeUrl);

                if (!hasUpload && !hasExisting)
                {
                    // Bind error specifically to "resumeFile" HTML name
                    ModelState.AddModelError("resumeFile", "Please upload a resume or select your profile resume.");
                }
                else if (hasUpload)
                {
                    var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
                    var ext = Path.GetExtension(uploadedFile.FileName).ToLowerInvariant();

                    if (!allowedExtensions.Contains(ext))
                        ModelState.AddModelError("resumeFile", "Invalid file type. Allowed formats: PDF, DOC, DOCX.");
                    else if (uploadedFile.Length > 5 * 1024 * 1024)
                        ModelState.AddModelError("resumeFile", "Resume file size exceeds the 5MB limit.");
                }
            }

            // 3. Validate Additional Questions (Server-Side)
            if (vm.Questions != null && vm.Questions.Any())
            {
                for (int i = 0; i < vm.Questions.Count; i++)
                {
                    var q = vm.Questions[i];
                    if (q.IsRequired)
                    {
                        var answer = Form.Answers?.FirstOrDefault(a => a.QuestionId == q.Id);
                        if (answer == null || string.IsNullOrWhiteSpace(answer.AnswerText))
                        {
                            // Bind error exactly to the input name in the HTML view
                            ModelState.AddModelError($"Form.Answers[{i}].AnswerText", $"Question '{q.QuestionText}' is required.");
                        }
                    }
                }
            }

            // 4. Return combined errors to the AJAX Toast AND mapping for the red borders
            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    // Flat list of strings for the toast popup
                    var flatErrors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    // Map of field names so the JavaScript knows exactly what to outline in red
                    var fieldErrors = ModelState
                        .Where(x => x.Value.Errors.Any())
                        .Select(x => new {
                            field = x.Key,
                            messages = x.Value.Errors.Select(e => e.ErrorMessage).ToList()
                        })
                        .ToList();

                    return Json(new { success = false, messages = flatErrors, fieldErrors = fieldErrors });
                }

                vm.Form = Form;
                return View(vm);
            }

            string? uploadedResumeUrl = null;

            if (Request.Form.Files.Count > 0)
            {
                var resumeFile = Request.Form.Files["resumeFile"];
                if (resumeFile != null && resumeFile.Length > 0)
                    uploadedResumeUrl = await _fileService.SaveResumeAsync(resumeFile, userId);
            }

            var (success, error) = await _applicationService.SubmitApplicationAsync(
                jobId, userId, Form, uploadedResumeUrl);

            if (!success)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, messages = new[] { error ?? "An error occurred." } });

                vm.Form = Form;
                ModelState.AddModelError(string.Empty, error ?? "An error occurred.");
                return View(vm);
            }

            TempData["SuccessMessage"] = "Your application has been submitted successfully!";

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = true, redirectUrl = Url.Action("Details", "Jobs", new { id = jobId }) });

            return RedirectToAction("Details", "Jobs", new { id = jobId });
        }
    }
}
