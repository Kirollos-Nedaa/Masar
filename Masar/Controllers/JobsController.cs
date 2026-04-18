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
        private readonly UserManager<ApplicationUser> _userManager;

        public JobsController(IJobService jobService, UserManager<ApplicationUser> userManager)
        {
            _jobService = jobService;
            _userManager = userManager;
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
    }
}
