using System.ComponentModel.DataAnnotations;

namespace Obeli_K.Models.ViewModels
{
    public class CreateMultiDayFormuleViewModel
    {
        [Required(ErrorMessage = "La date de début est obligatoire")]
        [Display(Name = "Date de début")]
        public DateTime DateDebut { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "La date de fin est obligatoire")]
        [Display(Name = "Date de fin")]
        public DateTime DateFin { get; set; } = DateTime.Today.AddDays(6);

        [Required(ErrorMessage = "Le nom de la formule est obligatoire")]
        [StringLength(100, ErrorMessage = "Le nom ne peut pas dépasser 100 caractères")]
        [Display(Name = "Nom de la formule")]
        public string NomFormule { get; set; } = string.Empty;

        [Display(Name = "Type de formule")]
        public Guid? TypeFormuleId { get; set; }

        [Display(Name = "Statut")]
        public int? Statut { get; set; } = 1;

        [Display(Name = "Exclure les weekends")]
        public bool ExclureWeekends { get; set; } = false;

        [Display(Name = "Remplacer les formules existantes")]
        public bool RemplacerExistantes { get; set; } = false;

        public List<FormuleJourViewModel> Formules { get; set; } = new List<FormuleJourViewModel>();
    }
}

