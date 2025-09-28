using Obeli_K.Enums;
using Obeli_K.Models.Enums;

namespace Obeli_K.Models.ViewModels
{
    public class CommandeListViewModel
    {
        public Guid IdCommande { get; set; }
        public DateTime Date { get; set; }
        public DateTime? DateConsommation { get; set; }
        public StatutCommande StatusCommande { get; set; }
        public string? CodeCommande { get; set; }
        public Periode PeriodeService { get; set; }
        public decimal Montant { get; set; }
        public int Quantite { get; set; }
        public bool Instantanee { get; set; }
        public bool AnnuleeParPrestataire { get; set; }
        public string? MotifAnnulation { get; set; }
        public TypeClientCommande TypeClient { get; set; }
        public SiteType? Site { get; set; }
        public DateTime? DateLivraisonPrevueUtc { get; set; }
        public DateTime? DateReceptionUtc { get; set; }

        // Informations utilisateur
        public string? UtilisateurNom { get; set; }
        public string? UtilisateurPrenoms { get; set; }
        public string? UtilisateurMatricule { get; set; }
        public string? UtilisateurNomComplet { get; set; }

        // Informations groupe non-CIT
        public string? GroupeNonCitNom { get; set; }

        // Informations visiteur
        public string? VisiteurNom { get; set; }
        public string? VisiteurTelephone { get; set; }

        // Informations formule
        public string? FormuleNom { get; set; }
        public DateTime? FormuleDate { get; set; }
        public string? TypeFormuleNom { get; set; }
        public string? NomPlat { get; set; } // Nom du plat commandé

        // Audit
        public DateTime? CreatedOn { get; set; }
        public string? CreatedBy { get; set; }
    }
}
