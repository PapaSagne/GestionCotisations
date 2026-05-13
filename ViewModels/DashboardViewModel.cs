namespace GestionCotisations.Web.ViewModels
{
    public class DashboardViewModel
    {
        // Statistiques principales
        public int TotalMembres { get; set; }
        public int MembresActifs { get; set; }
        public int TotalCotisations { get; set; }
        public int CotisationsPayees { get; set; }
        public int CotisationsEnRetard { get; set; }
        public int CotisationsEnAttente { get; set; }
        public decimal TotalMontantAttendu { get; set; }
        public decimal TotalMontantEncaisse { get; set; }
        public decimal TauxRecouvrement => TotalMontantAttendu > 0
            ? Math.Round((TotalMontantEncaisse / TotalMontantAttendu) * 100, 1)
            : 0;

        // Derniers paiements
        public List<PaiementRecent> DerniersPaiements { get; set; } = new();

        // Cotisations en retard
        public List<CotisationEnRetard> CotisationsRetard { get; set; } = new();

        // Données graphique mensuel
        public List<string> MoisLabels { get; set; } = new();
        public List<decimal> MoisMontants { get; set; } = new();
    }

    public class PaiementRecent
    {
        public string MembreNom { get; set; } = string.Empty;
        public string Libelle { get; set; } = string.Empty;
        public decimal Montant { get; set; }
        public DateTime Date { get; set; }
        public string Mode { get; set; } = string.Empty;
    }

    public class CotisationEnRetard
    {
        public int Id { get; set; }
        public string MembreNom { get; set; } = string.Empty;
        public string Libelle { get; set; } = string.Empty;
        public decimal Montant { get; set; }
        public DateTime DateEcheance { get; set; }
        public int JoursRetard => (DateTime.Now - DateEcheance).Days;
    }
}