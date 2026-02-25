using Obeli_K.Enums;
using Obeli_K.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Obeli_K.Models.ViewModels
{
    public class CommandeGroupeeViewModel
    {
        [Display(Name = "Nom du groupe de visiteurs")]
        [StringLength(120)]
        public string? VisiteurNom { get; set; }

        [Required(ErrorMessage = "Le département est obligatoire")]
        [Display(Name = "Département")]
        public Guid DirectionId { get; set; }

        [Required(ErrorMessage = "La date de début est obligatoire")]
        [Display(Name = "Date de début")]
        public DateTime DateDebut { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "La date de fin est obligatoire")]
        [Display(Name = "Date de fin")]
        public DateTime DateFin { get; set; } = DateTime.Today;

        [Display(Name = "Type de Formule")]
        public string? TypeFormule { get; set; }

        [Required(ErrorMessage = "La période de service est obligatoire")]
        [Display(Name = "Période de service")]
        public Periode PeriodeService { get; set; } = Periode.Jour;

        [Required(ErrorMessage = "Le nombre de visiteurs est obligatoire")]
        [Range(1, 1000, ErrorMessage = "Le nombre de visiteurs doit être entre 1 et 1000")]
        [Display(Name = "Nombre de visiteurs")]
        public int NombreVisiteurs { get; set; } = 1;

        [Display(Name = "Site")]
        public SiteType? Site { get; set; }

        [Display(Name = "Commentaires")]
        [MaxLength(500)]
        public string? Commentaires { get; set; }

        // Propriétés pour l'affichage
        public string? FormuleNom { get; set; }
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
