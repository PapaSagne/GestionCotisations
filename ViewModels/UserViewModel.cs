using System.ComponentModel.DataAnnotations;

namespace GestionCotisations.Web.ViewModels
{
    public class UserViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Display(Name = "Nom")]
        public string Nom { get; set; } = string.Empty;

        [Display(Name = "Prénom")]
        public string Prenom { get; set; } = string.Empty;

        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Rôle")]
        public string Role { get; set; } = string.Empty;

        [Display(Name = "Actif")]
        public bool EstActif { get; set; }

        [Display(Name = "Date de création")]
        public DateTime DateCreation { get; set; }
    }

    public class EditUserViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nom est obligatoire")]
        [Display(Name = "Nom")]
        public string Nom { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le prénom est obligatoire")]
        [Display(Name = "Prénom")]
        public string Prenom { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'email est obligatoire")]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le rôle est obligatoire")]
        [Display(Name = "Rôle")]
        public string Role { get; set; } = string.Empty;

        [Display(Name = "Actif")]
        public bool EstActif { get; set; }

        public List<string> RolesDisponibles { get; set; } = new();
    }

    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "Le nom est obligatoire")]
        [Display(Name = "Nom")]
        public string Nom { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le prénom est obligatoire")]
        [Display(Name = "Prénom")]
        public string Prenom { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'email est obligatoire")]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le mot de passe est obligatoire")]
        [MinLength(6, ErrorMessage = "Minimum 6 caractères")]
        [DataType(DataType.Password)]
        [Display(Name = "Mot de passe")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le rôle est obligatoire")]
        [Display(Name = "Rôle")]
        public string Role { get; set; } = string.Empty;

        public List<string> RolesDisponibles { get; set; } = new();
    }
}