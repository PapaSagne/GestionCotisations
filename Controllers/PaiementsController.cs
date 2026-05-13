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
    public class PaiementsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PaiementsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // /Paiements
        public async Task<IActionResult> Index(string? search)
        {
            ViewData["Title"] = "Gestion des Paiements";
            ViewData["Search"] = search;

            var query = _context.Paiements
                .Include(p => p.Cotisation)
                    .ThenInclude(c => c.Membre)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p =>
                    p.Cotisation.Membre.Nom.Contains(search) ||
                    p.Cotisation.Membre.Prenom.Contains(search) ||
                    p.Reference != null && p.Reference.Contains(search));
            }

            var paiements = await query
                .OrderByDescending(p => p.DatePaiement)
                .ToListAsync();

            return View(paiements);
        }

        public async Task<IActionResult> Create(int? cotisationId)
        {
            ViewData["Title"] = "Nouveau Paiement";

            var model = new PaiementViewModel
            {
                CotisationId = cotisationId ?? 0,
                Cotisations = await GetCotisationsSelectList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PaiementViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Cotisations = await GetCotisationsSelectList();
                return View(model);
            }

            // Récupérer la cotisation 
            var cotisation = await _context.Cotisations
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == model.CotisationId);

            if (cotisation == null)
            {
                ModelState.AddModelError("", "Cotisation introuvable.");
                model.Cotisations = await GetCotisationsSelectList();
                return View(model);
            }

            //  Enregistrer le nouveau paiement
            var paiement = new Paiement
            {
                Montant = model.Montant,
                DatePaiement = model.DatePaiement,
                Mode = model.Mode,
                Reference = model.Reference,
                Remarque = model.Remarque,
                CotisationId = model.CotisationId
            };

            _context.Paiements.Add(paiement);
            await _context.SaveChangesAsync();

            // Recalculer le total 
            var totalPaye = await _context.Paiements
                .Where(p => p.CotisationId == model.CotisationId)
                .SumAsync(p => p.Montant);

            // Mettre à jour le statut
            var cotisationAMettreAJour = await _context.Cotisations
                .FirstOrDefaultAsync(c => c.Id == model.CotisationId);

            if (cotisationAMettreAJour != null)
            {
                if (totalPaye >= cotisationAMettreAJour.Montant)
                    cotisationAMettreAJour.Statut = StatutCotisation.Payee;
                else if (cotisationAMettreAJour.DateEcheance < DateTime.Now)
                    cotisationAMettreAJour.Statut = StatutCotisation.EnRetard;
                else
                    cotisationAMettreAJour.Statut = StatutCotisation.EnAttente;

                await _context.SaveChangesAsync();
            }

            TempData["Success"] = $"Paiement de {model.Montant:N0} FCFA enregistré ! Total payé : {totalPaye:N0} FCFA";
            return RedirectToAction("Details", "Cotisations", new { id = model.CotisationId });
        }

        public async Task<IActionResult> Edit(int id)
        {
            ViewData["Title"] = "Modifier le Paiement";

            var paiement = await _context.Paiements.FindAsync(id);
            if (paiement == null)
                return NotFound();

            var model = new PaiementViewModel
            {
                Id = paiement.Id,
                Montant = paiement.Montant,
                DatePaiement = paiement.DatePaiement,
                Mode = paiement.Mode,
                Reference = paiement.Reference,
                Remarque = paiement.Remarque,
                CotisationId = paiement.CotisationId,
                Cotisations = await GetCotisationsSelectList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PaiementViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Cotisations = await GetCotisationsSelectList();
                return View(model);
            }

            var paiement = await _context.Paiements.FindAsync(id);
            if (paiement == null)
                return NotFound();

            paiement.Montant = model.Montant;
            paiement.DatePaiement = model.DatePaiement;
            paiement.Mode = model.Mode;
            paiement.Reference = model.Reference;
            paiement.Remarque = model.Remarque;
            paiement.CotisationId = model.CotisationId;

            await _context.SaveChangesAsync();

            // Recalculer le statut après modification
            var totalPaye = await _context.Paiements
                .Where(p => p.CotisationId == model.CotisationId)
                .SumAsync(p => p.Montant);

            var cotisation = await _context.Cotisations
                .FirstOrDefaultAsync(c => c.Id == model.CotisationId);

            if (cotisation != null)
            {
                if (totalPaye >= cotisation.Montant)
                    cotisation.Statut = StatutCotisation.Payee;
                else if (cotisation.DateEcheance < DateTime.Now)
                    cotisation.Statut = StatutCotisation.EnRetard;
                else
                    cotisation.Statut = StatutCotisation.EnAttente;

                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Paiement modifié avec succès !";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var paiement = await _context.Paiements.FindAsync(id);
            if (paiement == null)
                return NotFound();

            int cotisationId = paiement.CotisationId;

            _context.Paiements.Remove(paiement);
            await _context.SaveChangesAsync();

            // Recalculer le statut après suppression
            var totalPaye = await _context.Paiements
                .Where(p => p.CotisationId == cotisationId)
                .SumAsync(p => p.Montant);

            var cotisation = await _context.Cotisations
                .FirstOrDefaultAsync(c => c.Id == cotisationId);

            if (cotisation != null)
            {
                if (totalPaye >= cotisation.Montant)
                    cotisation.Statut = StatutCotisation.Payee;
                else if (cotisation.DateEcheance < DateTime.Now)
                    cotisation.Statut = StatutCotisation.EnRetard;
                else
                    cotisation.Statut = StatutCotisation.EnAttente;

                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Paiement supprimé avec succès !";
            return RedirectToAction(nameof(Index));
        }

        private async Task<IEnumerable<SelectListItem>> GetCotisationsSelectList()
        {
            return await _context.Cotisations
                .Include(c => c.Membre)
                .Where(c => c.Statut != StatutCotisation.Annulee)
                .OrderByDescending(c => c.DateEcheance)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.Membre.Prenom} {c.Membre.Nom} — {c.Libelle} ({c.Montant:N0} FCFA)"
                })
                .ToListAsync();
        }

    // /Paiements/MesPaiements
[Authorize(Roles = "Membre")]
public async Task<IActionResult> MesPaiements()
{
    ViewData["Title"] = "Mes Paiements";

    var userEmail = User.Identity!.Name;

    var membre = await _context.Membres
        .FirstOrDefaultAsync(m => m.Email == userEmail);

    if (membre == null)
    {
        ViewBag.Message = "Aucun profil membre trouvé pour votre compte.";
        return View(new List<Paiement>());
    }

    var paiements = await _context.Paiements
        .Include(p => p.Cotisation)
        .Where(p => p.Cotisation.MembreId == membre.Id)
        .OrderByDescending(p => p.DatePaiement)
        .ToListAsync();

    return View(paiements);
}


    }

}