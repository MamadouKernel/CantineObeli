using Obeli_K.Enums;
using Obeli_K.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Obeli_K.Models.ViewModels
{
    public class CommandeGroupeeViewModel
    {
        [Required(ErrorMessage = "Le groupe non-CIT est obligatoire")]
        [Display(Name = "Groupe non-CIT")]
        public Guid GroupeNonCitId { get; set; }

        [Required(ErrorMessage = "La date est obligatoire")]
        [Display(Name = "Date")]
        public DateTime Date { get; set; } = DateTime.Today;

        [Display(Name = "Type de Formule")]
        public string? TypeFormule { get; set; }

        [Required(ErrorMessage = "Le menu est obligatoire")]
        [Display(Name = "Menu")]
        public Guid IdFormule { get; set; }

        [Required(ErrorMessage = "La période de service est obligatoire")]
        [Display(Name = "Période de service")]
        public Periode PeriodeService { get; set; } = Periode.Jour;

        [Required(ErrorMessage = "La quantité est obligatoire")]
        [Range(1, 1000, ErrorMessage = "La quantité doit être entre 1 et 1000")]
        [Display(Name = "Quantité")]
        public int Quantite { get; set; } = 1;

        [Display(Name = "Site")]
        public SiteType? Site { get; set; }

        [Display(Name = "Commentaires")]
        [MaxLength(500)]
        public string? Commentaires { get; set; }

        // Propriétés pour l'affichage
        public string? GroupeNom { get; set; }
        public string? FormuleNom { get; set; }
        public int QuotaDisponible { get; set; }
        public bool QuotaDepasse { get; set; }
    }

    public class GestionQuotasViewModel
    {
        [Required(ErrorMessage = "Le groupe non-CIT est obligatoire")]
        [Display(Name = "Groupe non-CIT")]
        public Guid GroupeNonCitId { get; set; }

        [Required(ErrorMessage = "La date est obligatoire")]
        [Display(Name = "Date")]
        public DateTime Date { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Le quota jour est obligatoire")]
        [Range(0, 1000, ErrorMessage = "Le quota doit être entre 0 et 1000")]
        [Display(Name = "Quota Jour")]
        public int QuotaJour { get; set; } = 0;

        [Required(ErrorMessage = "Le quota nuit est obligatoire")]
        [Range(0, 1000, ErrorMessage = "Le quota doit être entre 0 et 1000")]
        [Display(Name = "Quota Nuit")]
        public int QuotaNuit { get; set; } = 0;

        [Display(Name = "Restriction à la Formule Standard")]
        public bool RestrictionFormuleStandard { get; set; } = true;

        [Display(Name = "Commentaires")]
        [MaxLength(500)]
        public string? Commentaires { get; set; }

        // Propriétés pour l'affichage
        public string? GroupeNom { get; set; }
        public int PlatsConsommesJour { get; set; }
        public int PlatsConsommesNuit { get; set; }
        public int PlatsRestantsJour { get; set; }
        public int PlatsRestantsNuit { get; set; }
    }
}
