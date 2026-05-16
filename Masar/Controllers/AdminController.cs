using Masar.Core.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Masar.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public AdminController(IDashboardService adminService)
        {
            _dashboardService = adminService;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var dto = await _dashboardService.GetAdminDashboardAsync();
            return View(dto);
        }
    }
}
