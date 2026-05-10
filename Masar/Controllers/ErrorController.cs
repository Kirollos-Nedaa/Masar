using Microsoft.AspNetCore.Mvc;

namespace Masar.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            switch (statusCode)
            {
                case 404:
                    ViewBag.ErrorMessage = "Sorry, the page you are looking for does not exist.";
                    return View("NotFound");

                case 403:
                    ViewBag.ErrorMessage = "Sorry, you are not authorized to view this route.";
                    return View("AccessDenied");

                default:
                    ViewBag.ErrorMessage = "An unexpected error occurred.";
                    return View("GenericError");
            }
        }

        [HttpGet("Error/AccessDenied")]
        public IActionResult AccessDenied()
        {
            return View("AccessDenied");
        }
    }
}
