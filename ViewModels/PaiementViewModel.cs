using GestionCotisations.Web.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace GestionCotisations.Web.ViewModels
{
    public class PaiementViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Le montant est obligatoire")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Le montant doit être supérieur à 0")]
        [Display(Name = "Montant")]
        public decimal Montant { get; set; }

        [Display(Name = "Date de paiement")]
        public DateTime DatePaiement { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Le mode de paiement est obligatoire")]
        [Display(Name = "Mode de paiement")]
        public ModePaiement Mode { get; set; }

        [Display(Name = "Référence")]
        [StringLength(100)]
        public string? Reference { get; set; }

        [Display(Name = "Remarque")]
        [StringLength(500)]
        public string? Remarque { get; set; }

        [Required(ErrorMessage = "La cotisation est obligatoire")]
        [Display(Name = "Cotisation")]
        public int CotisationId { get; set; }

        
        public IEnumerable<SelectListItem> Cotisations { get; set; } = new List<SelectListItem>();
    }
}