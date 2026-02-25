using System.ComponentModel.DataAnnotations;

namespace Obeli_K.Models.ViewModels
{
    /// <summary>
    /// ViewModel pour afficher le point de consommation CIT avec les montants par utilisateur
    /// </summary>
    public class PointConsommationCITViewModel
    {
        /// <summary>
        /// ID de l'utilisateur
        /// </summary>
        public Guid UtilisateurId { get; set; }

        /// <summary>
        /// Nom de l'utilisateur
        /// </summary>
        [Display(Name = "Nom")]
        public string UtilisateurNom { get; set; } = string.Empty;

        /// <summary>
        /// Prénoms de l'utilisateur
        /// </summary>
        [Display(Name = "Prénoms")]
        public string UtilisateurPrenoms { get; set; } = string.Empty;

        /// <summary>
        /// Nom complet de l'utilisateur
        /// </summary>
        [Display(Name = "Nom & Prénoms")]
        public string UtilisateurNomComplet { get; set; } = string.Empty;

        /// <summary>
        /// Email de l'utilisateur
        /// </summary>
        [Display(Name = "Email")]
        public string? Email { get; set; }

        /// <summary>
        /// Matricule de l'utilisateur
        /// </summary>
        [Display(Name = "Matricule")]
        public string Matricule { get; set; } = string.Empty;

        /// <summary>
        /// Nombre de consommations
        /// </summary>
        [Display(Name = "Nombre de Consommations")]
        public int NombreConsommations { get; set; }

        /// <summary>
        /// Nombre de formules standard consommées
        /// </summary>
        [Display(Name = "Standard Consommée")]
        public int StandardConsommee { get; set; }

        /// <summary>
        /// Nombre de formules standard non récupérées
        /// </summary>
        [Display(Name = "Standard Non Récupérée")]
        public int StandardNonRecuperee { get; set; }

        /// <summary>
        /// Nombre de formules standard indisponibles
        /// </summary>
        [Display(Name = "Standard Indisponible")]
        public int StandardIndisponible { get; set; }

        /// <summary>
        /// Nombre de formules améliorées consommées
        /// </summary>
        [Display(Name = "Améliorée Consommée")]
        public int AmelioreeConsommee { get; set; }

        /// <summary>
        /// Nombre de formules améliorées non récupérées
        /// </summary>
        [Display(Name = "Améliorée Non Récupérée")]
        public int AmelioreeNonRecuperee { get; set; }

        /// <summary>
        /// Nombre de formules améliorées indisponibles
        /// </summary>
        [Display(Name = "Améliorée Indisponible")]
        public int AmelioreeIndisponible { get; set; }

        /// <summary>
        /// Nombre total (tous les nombres additionnés)
        /// </summary>
        [Display(Name = "Total")]
        public int Total { get; set; }

        /// <summary>
        /// Montant total calculé : ((StandardNonRecuperee + StandardConsommee) * 550) + ((AmelioreeNonRecuperee + AmelioreeConsommee) * 2800)
        /// </summary>
        [Display(Name = "Montant Total")]
        [DisplayFormat(DataFormatString = "{0:N0} FCFA")]
        public decimal MontantTotal { get; set; }
    }
}
