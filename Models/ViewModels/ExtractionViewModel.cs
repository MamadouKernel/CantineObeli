using System.ComponentModel.DataAnnotations;

namespace Obeli_K.Models.ViewModels
{
    public class ExtractionViewModel
    {
        [Required(ErrorMessage = "La date de début est obligatoire.")]
        [Display(Name = "Date de début")]
        public DateTime DateDebut { get; set; }

        [Required(ErrorMessage = "La date de fin est obligatoire.")]
        [Display(Name = "Date de fin")]
        public DateTime DateFin { get; set; }

        [Required(ErrorMessage = "La marge par formule est obligatoire.")]
        [Range(0, 100, ErrorMessage = "La marge doit être entre 0 et 100.")]
        [Display(Name = "Marge par formule (%)")]
        public int MargeParFormule { get; set; } = 10;
    }
}
