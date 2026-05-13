using GestionCotisations.Web.Data;
using GestionCotisations.Web.Models;
using GestionCotisations.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionCotisations.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Tableau de bord";

            // Dashboard simplifié pour les Membres
            if (User.IsInRole("Membre"))
            {
                var userEmail = User.Identity!.Name;
                var membre = await _context.Membres
                    .FirstOrDefaultAsync(m => m.Email == userEmail);

                var vm = new DashboardViewModel();

                if (membre != null)
                {
                    vm.TotalCotisations = await _context.Cotisations
                        .CountAsync(c => c.MembreId == membre.Id);
                    vm.CotisationsPayees = await _context.Cotisations
                        .CountAsync(c => c.MembreId == membre.Id && c.Statut == StatutCotisation.Payee);
                    vm.CotisationsEnRetard = await _context.Cotisations
                        .CountAsync(c => c.MembreId == membre.Id && c.Statut == StatutCotisation.EnRetard);
                    vm.CotisationsEnAttente = await _context.Cotisations
                        .CountAsync(c => c.MembreId == membre.Id && c.Statut == StatutCotisation.EnAttente);
                    vm.TotalMontantAttendu = await _context.Cotisations
                        .Where(c => c.MembreId == membre.Id && c.Statut != StatutCotisation.Annulee)
                        .SumAsync(c => (decimal?)c.Montant) ?? 0;
                    vm.TotalMontantEncaisse = await _context.Paiements
                        .Where(p => p.Cotisation.MembreId == membre.Id)
                        .SumAsync(p => (decimal?)p.Montant) ?? 0;
                }

                return View("IndexMembre", vm);
            }

            // Dashboard complet Admin/Gestionnaire
            var dashboard = new DashboardViewModel();

            dashboard.TotalMembres = await _context.Membres.CountAsync();
            dashboard.MembresActifs = await _context.Membres.CountAsync(m => m.EstActif);
            dashboard.TotalCotisations = await _context.Cotisations.CountAsync();
            dashboard.CotisationsPayees = await _context.Cotisations.CountAsync(c => c.Statut == StatutCotisation.Payee);
            dashboard.CotisationsEnRetard = await _context.Cotisations.CountAsync(c => c.Statut == StatutCotisation.EnRetard);
            dashboard.CotisationsEnAttente = await _context.Cotisations.CountAsync(c => c.Statut == StatutCotisation.EnAttente);

            dashboard.TotalMontantAttendu = await _context.Cotisations
                .Where(c => c.Statut != StatutCotisation.Annulee)
                .SumAsync(c => (decimal?)c.Montant) ?? 0;

            dashboard.TotalMontantEncaisse = await _context.Paiements
                .SumAsync(p => (decimal?)p.Montant) ?? 0;

            dashboard.DerniersPaiements = await _context.Paiements
                .Include(p => p.Cotisation).ThenInclude(c => c.Membre)
                .OrderByDescending(p => p.DatePaiement)
                .Take(5)
                .Select(p => new PaiementRecent {
                    MembreNom = p.Cotisation.Membre.Prenom + " " + p.Cotisation.Membre.Nom,
                    Libelle = p.Cotisation.Libelle,
                    Montant = p.Montant,
                    Date = p.DatePaiement,
                    Mode = p.Mode.ToString()
                })
                .ToListAsync();

            dashboard.CotisationsRetard = await _context.Cotisations
                .Include(c => c.Membre)
                .Where(c => c.Statut == StatutCotisation.EnRetard)
                .OrderBy(c => c.DateEcheance)
                .Take(5)
                .Select(c => new CotisationEnRetard {
                    Id = c.Id,
                    MembreNom = c.Membre.Prenom + " " + c.Membre.Nom,
                    Libelle = c.Libelle,
                    Montant = c.Montant,
                    DateEcheance = c.DateEcheance
                })
                .ToListAsync();

            var sixMoisAvant = DateTime.Now.AddMonths(-5);
            var paiementsMensuels = await _context.Paiements
                .Where(p => p.DatePaiement >= new DateTime(sixMoisAvant.Year, sixMoisAvant.Month, 1))
                .GroupBy(p => new { p.DatePaiement.Year, p.DatePaiement.Month })
                .Select(g => new { Annee = g.Key.Year, Mois = g.Key.Month, Total = g.Sum(p => p.Montant) })
                .OrderBy(g => g.Annee).ThenBy(g => g.Mois)
                .ToListAsync();

            for (int i = 5; i >= 0; i--)
            {
                var date = DateTime.Now.AddMonths(-i);
                dashboard.MoisLabels.Add(date.ToString("MMM yyyy"));
                var data = paiementsMensuels.FirstOrDefault(p => p.Annee == date.Year && p.Mois == date.Month);
                dashboard.MoisMontants.Add(data?.Total ?? 0);
            }

            return View(dashboard);
        }
    }
}