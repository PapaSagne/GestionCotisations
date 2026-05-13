namespace GestionCotisations.Web.Models
{
    public class Membre
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telephone { get; set; } = string.Empty;
        public string Adresse { get; set; } = string.Empty;
        public DateTime DateInscription { get; set; } = DateTime.Now;
        public bool EstActif { get; set; } = true;

        // Un membre peut avoir plusieurs cotisations
        public ICollection<Cotisation> Cotisations { get; set; } = new List<Cotisation>();
    }
}