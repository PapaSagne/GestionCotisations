using GestionCotisations.Web.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace GestionCotisations.Web.ViewModels
{
    public class CotisationViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Le libellé est obligatoire")]
        [StringLength(200)]
        [Display(Name = "Libellé")]
        public string Libelle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le montant est obligatoire")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Le montant doit être supérieur à 0")]
        [Display(Name = "Montant")]
        public decimal Montant { get; set; }

        [Required(ErrorMessage = "La date de début est obligatoire")]
        [Display(Name = "Date de début")]
        public DateTime DateDebut { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "La date de fin est obligatoire")]
        [Display(Name = "Date de fin")]
        public DateTime DateFin { get; set; } = DateTime.Now.AddMonths(1);

        [Required(ErrorMessage = "La date d'échéance est obligatoire")]
        [Display(Name = "Date d'échéance")]
        public DateTime DateEcheance { get; set; } = DateTime.Now.AddMonths(1);

        [Display(Name = "Statut")]
        public StatutCotisation Statut { get; set; } = StatutCotisation.EnAttente;

        [Required(ErrorMessage = "Le membre est obligatoire")]
        [Display(Name = "Membre")]
        public int MembreId { get; set; }

        // Liste des membres pour le dropdown
        public IEnumerable<SelectListItem> Membres { get; set; } = new List<SelectListItem>();
    }
}