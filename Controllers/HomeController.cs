using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionCotisations.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Dashboard");
        }

        [AllowAnonymous]
        public IActionResult NotFound()
        {
            Response.StatusCode = 404;
            return View("NotFound");
        }

        [AllowAnonymous]
        public IActionResult Error()
        {
            return View("Error");
        }
    }
}