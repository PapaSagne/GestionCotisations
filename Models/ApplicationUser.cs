using Microsoft.AspNetCore.Identity;

namespace GestionCotisations.Web.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public DateTime DateCreation { get; set; } = DateTime.Now;
        public bool EstActif { get; set; } = true;
    }
}