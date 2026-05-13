namespace GestionCotisations.Web.Models
{
    public enum ModePaiement
    {
        Especes,
        Virement,
        Cheque,
        MobileMoney
    }

    public class Paiement
    {
        public int Id { get; set; }
        public decimal Montant { get; set; }
        public DateTime DatePaiement { get; set; } = DateTime.Now;
        public ModePaiement Mode { get; set; }
        public string? Reference { get; set; }               
        public string? Remarque { get; set; }

        public int CotisationId { get; set; }
        public Cotisation? Cotisation { get; set; }
    }
}