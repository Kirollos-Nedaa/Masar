using Microsoft.AspNetCore.Mvc;

namespace Masar.Controllers
{
    public class CandidateController : Controller
    {
        [HttpGet]
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
