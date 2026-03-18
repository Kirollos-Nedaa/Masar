using Masar.Core.IService;
using Masar.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Masar.Controllers
{
    [Authorize(Roles = "Candidate")]
    public class CandidateController : Controller
    {
        private readonly ICandidateDashboardService _dashboardService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CandidateController(
            ICandidateDashboardService dashboardService,
            UserManager<ApplicationUser> userManager)
        {
            _dashboardService = dashboardService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var userId = _userManager.GetUserId(User);
            var dto = await _dashboardService.GetDashboardAsync(userId);
            return View(dto);
        }
    }
}
