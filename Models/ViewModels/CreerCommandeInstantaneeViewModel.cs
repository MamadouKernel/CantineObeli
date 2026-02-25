using Obeli_K.Enums;
using Obeli_K.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Obeli_K.Models.ViewModels
{
    public class CreerCommandeInstantaneeViewModel
    {
        [Required(ErrorMessage = "La formule est obligatoire")]
        [Display(Name = "Formule")]
        public Guid IdFormule { get; set; }

        [Required(ErrorMessage = "Le type de client est obligatoire")]
        [Display(Name = "Type de client")]
        public TypeClientCommande TypeClient { get; set; }

        [Display(Name = "Utilisateur CIT")]
        public Guid? UtilisateurId { get; set; }

        [Display(Name = "Groupe non-CIT")]
        public Guid? GroupeNonCitId { get; set; }

        [Display(Name = "Nom du visiteur")]
        [StringLength(120, ErrorMessage = "Le nom ne peut pas dépasser 120 caractères")]
        public string? VisiteurNom { get; set; }

        [Display(Name = "Téléphone du visiteur")]
        [StringLength(32, ErrorMessage = "Le téléphone ne peut pas dépasser 32 caractères")]
        [Phone(ErrorMessage = "Format de téléphone invalide")]
        public string? VisiteurTelephone { get; set; }

        [Display(Name = "Direction d'origine")]
        public Guid? DirectionId { get; set; }

        [Display(Name = "Code de vérification")]
        [StringLength(20, ErrorMessage = "Le code ne peut pas dépasser 20 caractères")]
        public string? CodeVerification { get; set; }

        [Required(ErrorMessage = "La période de service est obligatoire")]
        [Display(Name = "Période de service")]
        public Periode PeriodeService { get; set; } = Periode.Jour;

        [Required(ErrorMessage = "La quantité est obligatoire")]
        [Range(1, 100, ErrorMessage = "La quantité doit être entre 1 et 100")]
        [Display(Name = "Quantité")]
        public int Quantite { get; set; } = 1;

        [Display(Name = "Site")]
        public SiteType? Site { get; set; }
    }
}
