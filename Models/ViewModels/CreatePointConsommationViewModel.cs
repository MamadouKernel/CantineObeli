using System.ComponentModel.DataAnnotations;
using Obeli_K.Models.Enums;

namespace Obeli_K.Models.ViewModels
{
    public class CreatePointConsommationViewModel
    {
        [Required(ErrorMessage = "La date de consommation est obligatoire.")]
        [Display(Name = "Date de consommation")]
        public DateTime DateConsommation { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Le type de consommation est obligatoire.")]
        [Display(Name = "Type de consommation")]
        public TypeConsommation TypeConsommation { get; set; }

        [Display(Name = "Description")]
        [StringLength(200, ErrorMessage = "La description ne peut pas dépasser 200 caractères.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Le montant est obligatoire.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Le montant doit être supérieur à 0.")]
        [Display(Name = "Montant")]
        public decimal Montant { get; set; }

        [Required(ErrorMessage = "La quantité est obligatoire.")]
        [Range(1, int.MaxValue, ErrorMessage = "La quantité doit être au moins 1.")]
        [Display(Name = "Quantité")]
        public int Quantite { get; set; } = 1;

        [Display(Name = "Lieu de consommation")]
        [StringLength(100, ErrorMessage = "Le lieu ne peut pas dépasser 100 caractères.")]
        public string? LieuConsommation { get; set; }

        [Display(Name = "Utilisateur")]
        public Guid? UtilisateurId { get; set; }
    }

    public class EditPointConsommationViewModel : CreatePointConsommationViewModel
    {
        [Required]
        public Guid IdPointConsommation { get; set; }
    }
}
