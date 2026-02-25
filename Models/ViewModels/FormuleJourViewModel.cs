using System.ComponentModel.DataAnnotations;

namespace Obeli_K.Models.ViewModels
{
    public class FormuleJourViewModel
    {
        public Guid IdFormule { get; set; }

        [Required(ErrorMessage = "La date est obligatoire.")]
        [Display(Name = "Date")]
        public DateTime Date { get; set; }

        [StringLength(100, ErrorMessage = "Le nom de la formule ne peut pas dépasser 100 caractères.")]
        [Display(Name = "Nom de la formule")]
        public string? NomFormule { get; set; }

        [Display(Name = "Type de formule")]
        public Guid? TypeFormuleId { get; set; }

        // Formule Améliorée
        [StringLength(200, ErrorMessage = "L'entrée ne peut pas dépasser 200 caractères.")]
        [Display(Name = "Entrée")]
        public string? Entree { get; set; }

        [StringLength(200, ErrorMessage = "Le plat ne peut pas dépasser 200 caractères.")]
        [Display(Name = "Plat")]
        public string? Plat { get; set; }

        [StringLength(200, ErrorMessage = "La garniture ne peut pas dépasser 200 caractères.")]
        [Display(Name = "Garniture")]
        public string? Garniture { get; set; }

        [StringLength(200, ErrorMessage = "Le dessert ne peut pas dépasser 200 caractères.")]
        [Display(Name = "Dessert")]
        public string? Dessert { get; set; }

        // Formule Standard
        [StringLength(200, ErrorMessage = "Le plat standard 1 ne peut pas dépasser 200 caractères.")]
        [Display(Name = "Plat Standard 1")]
        public string? PlatStandard1 { get; set; }

        [StringLength(200, ErrorMessage = "La garniture standard 1 ne peut pas dépasser 200 caractères.")]
        [Display(Name = "Garniture Standard 1")]
        public string? GarnitureStandard1 { get; set; }

        [StringLength(200, ErrorMessage = "Le plat standard 2 ne peut pas dépasser 200 caractères.")]
        [Display(Name = "Plat Standard 2")]
        public string? PlatStandard2 { get; set; }

        [StringLength(200, ErrorMessage = "La garniture standard 2 ne peut pas dépasser 200 caractères.")]
        [Display(Name = "Garniture Standard 2")]
        public string? GarnitureStandard2 { get; set; }

        // Champs communs
        [StringLength(200, ErrorMessage = "Le féculent ne peut pas dépasser 200 caractères.")]
        [Display(Name = "Féculent")]
        public string? Feculent { get; set; }

        [StringLength(200, ErrorMessage = "Les légumes ne peuvent pas dépasser 200 caractères.")]
        [Display(Name = "Légumes")]
        public string? Legumes { get; set; }

        [Range(0, 100, ErrorMessage = "La marge doit être entre 0 et 100.")]
        [Display(Name = "Marge")]
        public int? Marge { get; set; }

        [Display(Name = "Statut")]
        public int? Statut { get; set; }

        [Display(Name = "Verrouillé")]
        public bool Verrouille { get; set; }

        [StringLength(2048, ErrorMessage = "L'historique ne peut pas dépasser 2048 caractères.")]
        [Display(Name = "Historique")]
        public string? Historique { get; set; }

        // Propriétés de navigation
        public string? TypeFormuleNom { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? ModifiedBy { get; set; }
    }
}
