namespace GestionCotisations.Web.Models
{
    public enum StatutCotisation
    {
        EnAttente,
        Payee,
        EnRetard,
        Annulee
    }

    public class Cotisation
    {
        public int Id { get; set; }
        public string Libelle { get; set; } = string.Empty;  
        public decimal Montant { get; set; }
        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }
        public DateTime DateEcheance { get; set; }           
        public StatutCotisation Statut { get; set; } = StatutCotisation.EnAttente;

        
        public int MembreId { get; set; }
        public Membre? Membre { get; set; }

        // Une cotisation peut avoir plusieurs paiements
        public ICollection<Paiement> Paiements { get; set; } = new List<Paiement>();
    }
}