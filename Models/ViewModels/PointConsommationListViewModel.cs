using Obeli_K.Models.Enums;

namespace Obeli_K.Models.ViewModels
{
    public class PointConsommationListViewModel
    {
        public Guid IdPointConsommation { get; set; }
        public DateTime DateConsommation { get; set; }
        public string TypeFormule { get; set; } = string.Empty;
        public string? NomPlat { get; set; }
        public int QuantiteConsommee { get; set; }
        public decimal Cout { get; set; }
        public string? LieuConsommation { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? CreatedBy { get; set; }

        // Informations commande
        public string? CodeCommande { get; set; }
        public Guid CommandeId { get; set; }
        public int StatusCommande { get; set; }

        // Informations utilisateur
        public string? UtilisateurNom { get; set; }
        public string? UtilisateurPrenoms { get; set; }
        public string? UtilisateurMatricule { get; set; }
        public string? UtilisateurNomComplet { get; set; }
        public SiteType? UtilisateurSite { get; set; }
    }
}
