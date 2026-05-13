using GestionCotisations.Web.Data;
using GestionCotisations.Web.Models;
using GestionCotisations.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GestionCotisations.Web.Controllers
{
    [Authorize]
    public class CotisationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CotisationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? search, string? statut)
        {
            ViewData["Title"] = "Gestion des Cotisations";
            ViewData["Search"] = search;
            ViewData["Statut"] = statut;

            var query = _context.Cotisations
                .Include(c => c.Membre)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c =>
                    c.Libelle.Contains(search) ||
                    c.Membre.Nom.Contains(search) ||
                    c.Membre.Prenom.Contains(search));
            }

            if (!string.IsNullOrEmpty(statut) && Enum.TryParse<StatutCotisation>(statut, out var statutEnum))
            {
                query = query.Where(c => c.Statut == statutEnum);
            }

            // Mettre à jour  les cotisations en retard
            var cotisationsEnAttente = await _context.Cotisations
                .Where(c => c.Statut == StatutCotisation.EnAttente && c.DateEcheance < DateTime.Now)
                .ToListAsync();

            foreach (var c in cotisationsEnAttente)
            {
                c.Statut = StatutCotisation.EnRetard;
            }

            if (cotisationsEnAttente.Any())
                await _context.SaveChangesAsync();

            var cotisations = await query
                .OrderByDescending(c => c.DateEcheance)
                .ToListAsync();

            return View(cotisations);
        }

        public async Task<IActionResult> Details(int id)
        {
            ViewData["Title"] = "Détails de la Cotisation";

            var cotisation = await _context.Cotisations
                .Include(c => c.Membre)
                .Include(c => c.Paiements)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cotisation == null)
                return NotFound();

            return View(cotisation);
        }

        public async Task<IActionResult> Create()
        {
            ViewData["Title"] = "Nouvelle Cotisation";

            var model = new CotisationViewModel
            {
                Membres = await GetMembresSelectList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CotisationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Membres = await GetMembresSelectList();
                return View(model);
            }

            var cotisation = new Cotisation
            {
                Libelle = model.Libelle,
                Montant = model.Montant,
                DateDebut = model.DateDebut,
                DateFin = model.DateFin,
                DateEcheance = model.DateEcheance,
                Statut = model.Statut,
                MembreId = model.MembreId
            };

            _context.Cotisations.Add(cotisation);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Cotisation créée avec succès !";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            ViewData["Title"] = "Modifier la Cotisation";

            var cotisation = await _context.Cotisations.FindAsync(id);
            if (cotisation == null)
                return NotFound();

            var model = new CotisationViewModel
            {
                Id = cotisation.Id,
                Libelle = cotisation.Libelle,
                Montant = cotisation.Montant,
                DateDebut = cotisation.DateDebut,
                DateFin = cotisation.DateFin,
                DateEcheance = cotisation.DateEcheance,
                Statut = cotisation.Statut,
                MembreId = cotisation.MembreId,
                Membres = await GetMembresSelectList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CotisationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Membres = await GetMembresSelectList();
                return View(model);
            }

            var cotisation = await _context.Cotisations.FindAsync(id);
            if (cotisation == null)
                return NotFound();

            cotisation.Libelle = model.Libelle;
            cotisation.Montant = model.Montant;
            cotisation.DateDebut = model.DateDebut;
            cotisation.DateFin = model.DateFin;
            cotisation.DateEcheance = model.DateEcheance;
            cotisation.Statut = model.Statut;
            cotisation.MembreId = model.MembreId;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Cotisation modifiée avec succès !";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var cotisation = await _context.Cotisations.FindAsync(id);
            if (cotisation == null)
                return NotFound();

            _context.Cotisations.Remove(cotisation);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Cotisation supprimée avec succès !";
            return RedirectToAction(nameof(Index));
        }

        private async Task<IEnumerable<SelectListItem>> GetMembresSelectList()
        {
            return await _context.Membres
                .Where(m => m.EstActif)
                .OrderBy(m => m.Nom)
                .Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = $"{m.Prenom} {m.Nom}"
                })
                .ToListAsync();
        }


[Authorize(Roles = "Membre")]
public async Task<IActionResult> MesCotisations()
{
    ViewData["Title"] = "Mes Cotisations";

    // Récupérer l'email de l'utilisateur connecté
    var userEmail = User.Identity!.Name;

    // Trouver le membre correspondant
    var membre = await _context.Membres
        .FirstOrDefaultAsync(m => m.Email == userEmail);

    if (membre == null)
    {
        ViewBag.Message = "Aucun profil membre trouvé pour votre compte.";
        return View(new List<Cotisation>());
    }

    var cotisations = await _context.Cotisations
        .Include(c => c.Paiements)
        .Where(c => c.MembreId == membre.Id)
        .OrderByDescending(c => c.DateEcheance)
        .ToListAsync();

    return View(cotisations);
}

    }

}