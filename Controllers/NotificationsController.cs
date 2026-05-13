using GestionCotisations.Web.Models;
using GestionCotisations.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GestionCotisations.Web.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly NotificationService _notificationService;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationsController(
            NotificationService notificationService,
            UserManager<ApplicationUser> userManager)
        {
            _notificationService = notificationService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Mes Notifications";

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Vérifier les cotisations en retard au chargement
            await _notificationService.VerifierCotisationsEnRetardAsync();

            var notifications = await _notificationService
                .GetToutesNotificationsAsync(user.Id);

            return View(notifications);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarquerLue(int id)
        {
            await _notificationService.MarquerCommeLueAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarquerToutesLues()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
                await _notificationService.MarquerToutesCommeLuesAsync(user.Id);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> GetCount()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new { count = 0 });

            var notifications = await _notificationService
                .GetNotificationsNonLuesAsync(user.Id);

            return Json(new { count = notifications.Count });
        }
    }
}