using System.ComponentModel.DataAnnotations;

namespace GestionCotisations.Web.ViewModels
{
    public class MembreViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Le nom est obligatoire")]
        [StringLength(100, ErrorMessage = "Maximum 100 caractères")]
        [Display(Name = "Nom")]
        public string Nom { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le prénom est obligatoire")]
        [StringLength(100, ErrorMessage = "Maximum 100 caractères")]
        [Display(Name = "Prénom")]
        public string Prenom { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'email est obligatoire")]
        [EmailAddress(ErrorMessage = "Email invalide")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Téléphone")]
        [StringLength(20)]
        public string Telephone { get; set; } = string.Empty;

        [Display(Name = "Adresse")]
        [StringLength(300)]
        public string Adresse { get; set; } = string.Empty;

        [Display(Name = "Date d'inscription")]
        public DateTime DateInscription { get; set; } = DateTime.Now;

        [Display(Name = "Actif")]
        public bool EstActif { get; set; } = true;
    }
}