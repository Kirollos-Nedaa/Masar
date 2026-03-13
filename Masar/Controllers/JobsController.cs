using Microsoft.AspNetCore.Mvc;

namespace Masar.Controllers
{
    public class JobsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
