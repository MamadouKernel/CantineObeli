using System.ComponentModel.DataAnnotations;
using Obeli_K.Models;
using Obeli_K.Models.Enums;

namespace Obeli_K.Models.ViewModels
{
    public class ReportingDashboardViewModel
    {
        [Required(ErrorMessage = "La date de début est obligatoire.")]
        [Display(Name = "Date de début")]
        [DataType(DataType.Date)]
        public DateTime DateDebut { get; set; }

        [Required(ErrorMessage = "La date de fin est obligatoire.")]
        [Display(Name = "Date de fin")]
        [DataType(DataType.Date)]
        public DateTime DateFin { get; set; }

        [Display(Name = "Site")]
        public SiteType? Site { get; set; }

        [Display(Name = "Direction")]
        public Guid? DirectionId { get; set; }

        [Display(Name = "Fonction")]
        public Guid? FonctionId { get; set; }

        // Données pour les filtres
        public List<SiteType> Sites { get; set; } = new();
        public List<Direction> Directions { get; set; } = new();
        public List<Fonction> Fonctions { get; set; } = new();

        // Indicateurs calculés
        public ReportingIndicateursViewModel Indicateurs { get; set; } = new();
    }
}
