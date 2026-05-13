using GestionCotisations.Web.Data;
using GestionCotisations.Web.Models;
using GestionCotisations.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionCotisations.Web.Controllers
{
    [Authorize]
    public class MembresController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MembresController(ApplicationDbContext context)
        {
            _context = context;
        }

       
        public async Task<IActionResult> Index(string? search)
        {
            ViewData["Title"] = "Gestion des Membres";
            ViewData["Search"] = search;

            var query = _context.Membres.AsQueryable();

            // Recherche par nom, prénom ou email
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(m =>
                    m.Nom.Contains(search) ||
                    m.Prenom.Contains(search) ||
                    m.Email.Contains(search));
            }

            var membres = await query
                .OrderBy(m => m.Nom)
                .ToListAsync();

            return View(membres);
        }

        public async Task<IActionResult> Details(int id)
        {
            ViewData["Title"] = "Détails du Membre";

            var membre = await _context.Membres
                .Include(m => m.Cotisations)
                    .ThenInclude(c => c.Paiements)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (membre == null)
                return NotFound();

            return View(membre);
        }

        public IActionResult Create()
        {
            ViewData["Title"] = "Nouveau Membre";
            return View(new MembreViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MembreViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Vérifier si l'email existe déjà
            var emailExiste = await _context.Membres
                .AnyAsync(m => m.Email == model.Email);

            if (emailExiste)
            {
                ModelState.AddModelError("Email", "Cet email est déjà utilisé.");
                return View(model);
            }

            var membre = new Membre
            {
                Nom = model.Nom,
                Prenom = model.Prenom,
                Email = model.Email,
                Telephone = model.Telephone,
                Adresse = model.Adresse,
                DateInscription = model.DateInscription,
                EstActif = model.EstActif
            };

            _context.Membres.Add(membre);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Membre {membre.Prenom} {membre.Nom} ajouté avec succès !";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            ViewData["Title"] = "Modifier le Membre";

            var membre = await _context.Membres.FindAsync(id);
            if (membre == null)
                return NotFound();

            var model = new MembreViewModel
            {
                Id = membre.Id,
                Nom = membre.Nom,
                Prenom = membre.Prenom,
                Email = membre.Email,
                Telephone = membre.Telephone,
                Adresse = membre.Adresse,
                DateInscription = membre.DateInscription,
                EstActif = membre.EstActif
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MembreViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var membre = await _context.Membres.FindAsync(id);
            if (membre == null)
                return NotFound();

            var emailExiste = await _context.Membres
                .AnyAsync(m => m.Email == model.Email && m.Id != id);

            if (emailExiste)
            {
                ModelState.AddModelError("Email", "Cet email est déjà utilisé.");
                return View(model);
            }

            membre.Nom = model.Nom;
            membre.Prenom = model.Prenom;
            membre.Email = model.Email;
            membre.Telephone = model.Telephone;
            membre.Adresse = model.Adresse;
            membre.DateInscription = model.DateInscription;
            membre.EstActif = model.EstActif;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Membre {membre.Prenom} {membre.Nom} modifié avec succès !";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var membre = await _context.Membres.FindAsync(id);
            if (membre == null)
                return NotFound();

            _context.Membres.Remove(membre);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Membre supprimé avec succès !";
            return RedirectToAction(nameof(Index));
        }
    }
}