using System.ComponentModel.DataAnnotations;
using Obeli_K.Enums;

namespace Obeli_K.Models.ViewModels
{
    public class VisiteurSelectionViewModel
    {
        [Required(ErrorMessage = "La date de début est obligatoire")]
        [Display(Name = "Date de début")]
        public DateTime DateDebut { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "La date de fin est obligatoire")]
        [Display(Name = "Date de fin")]
        public DateTime DateFin { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "La direction est obligatoire")]
        [Display(Name = "Direction")]
        public Guid DirectionId { get; set; }

        [Required(ErrorMessage = "Le nom du visiteur est obligatoire")]
        [StringLength(120, ErrorMessage = "Le nom ne peut pas dépasser 120 caractères")]
        [Display(Name = "Nom du visiteur")]
        public string VisiteurNom { get; set; } = string.Empty;

        [StringLength(32, ErrorMessage = "Le téléphone ne peut pas dépasser 32 caractères")]
        [Display(Name = "Téléphone (optionnel)")]
        public string? VisiteurTelephone { get; set; }
    }
}
