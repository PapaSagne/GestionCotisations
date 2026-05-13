using GestionCotisations.Web.Models;
using GestionCotisations.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionCotisations.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Gestion des Utilisateurs";

            var users = await _userManager.Users.ToListAsync();
            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    Nom = user.Nom,
                    Prenom = user.Prenom,
                    Email = user.Email ?? "",               
                    Role = roles.FirstOrDefault() ?? "Aucun",
                    EstActif = user.EstActif,
                    DateCreation = user.DateCreation
                });
            }

            return View(userViewModels);
        }

        public async Task<IActionResult> Create()
        {
            ViewData["Title"] = "Nouvel Utilisateur";

            var model = new CreateUserViewModel
            {
                RolesDisponibles = await _roleManager.Roles
                    .Select(r => r.Name!)
                    .ToListAsync()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.RolesDisponibles = await _roleManager.Roles
                    .Select(r => r.Name!).ToListAsync();
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                Nom = model.Nom,
                Prenom = model.Prenom,
                EmailConfirmed = true,
                EstActif = true,
                DateCreation = DateTime.Now
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, model.Role);
                TempData["Success"] = $"Utilisateur {user.Prenom} {user.Nom} créé avec succès !";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            model.RolesDisponibles = await _roleManager.Roles
                .Select(r => r.Name!).ToListAsync();

            return View(model);
        }

        public async Task<IActionResult> Edit(string id)
        {
            ViewData["Title"] = "Modifier l'Utilisateur";

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            var model = new EditUserViewModel
            {
                Id = user.Id,
                Nom = user.Nom,
                Prenom = user.Prenom,
                Email = user.Email ?? "",
                Role = roles.FirstOrDefault() ?? "",
                EstActif = user.EstActif,
                RolesDisponibles = await _roleManager.Roles
                    .Select(r => r.Name!).ToListAsync()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, EditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.RolesDisponibles = await _roleManager.Roles
                    .Select(r => r.Name!).ToListAsync();
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.Nom = model.Nom;
            user.Prenom = model.Prenom;
            user.Email = model.Email;
            user.UserName = model.Email;
            user.EstActif = model.EstActif;

            await _userManager.UpdateAsync(user);

            // Mettre à jour le rôle
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, model.Role);

            TempData["Success"] = "Utilisateur modifié avec succès !";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.EstActif = !user.EstActif;
            await _userManager.UpdateAsync(user);

            TempData["Success"] = user.EstActif
                ? $"{user.Prenom} {user.Nom} activé !"
                : $"{user.Prenom} {user.Nom} désactivé !";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Empêcher la suppression de son propre compte
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Id == id)
            {
                TempData["Error"] = "Vous ne pouvez pas supprimer votre propre compte !";
                return RedirectToAction(nameof(Index));
            }

            await _userManager.DeleteAsync(user);
            TempData["Success"] = "Utilisateur supprimé avec succès !";
            return RedirectToAction(nameof(Index));
        }
    }
}