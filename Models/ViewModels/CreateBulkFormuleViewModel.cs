using System.ComponentModel.DataAnnotations;

namespace Obeli_K.Models.ViewModels
{
    public class CreateBulkFormuleViewModel
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

        // Champs pour formule Améliorée
        [Display(Name = "Entrée")]
        public string? Entree { get; set; }

        [Display(Name = "Plat")]
        public string? Plat { get; set; }

        [Display(Name = "Garniture")]
        public string? Garniture { get; set; }

        [Display(Name = "Dessert")]
        public string? Dessert { get; set; }

        // Champs pour formule Standard
        [Display(Name = "Plat Standard 1")]
        public string? PlatStandard1 { get; set; }

        [Display(Name = "Garniture Standard 1")]
        public string? GarnitureStandard1 { get; set; }

        [Display(Name = "Plat Standard 2")]
        public string? PlatStandard2 { get; set; }

        [Display(Name = "Garniture Standard 2")]
        public string? GarnitureStandard2 { get; set; }

        // Champs communs
        [Display(Name = "Féculent")]
        public string? Feculent { get; set; }

        [Display(Name = "Légumes")]
        public string? Legumes { get; set; }

        [Display(Name = "Marge (%)")]
        [Range(0, 100, ErrorMessage = "La marge doit être entre 0 et 100")]
        public int? Marge { get; set; }

        [Display(Name = "Statut")]
        public int? Statut { get; set; } = 1;

        [Display(Name = "Exclure les weekends")]
        public bool ExclureWeekends { get; set; } = true;

        [Display(Name = "Remplacer les formules existantes")]
        public bool RemplacerExistantes { get; set; } = false;
    }
}
