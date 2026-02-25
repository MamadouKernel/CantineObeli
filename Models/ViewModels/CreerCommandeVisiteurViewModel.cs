using System.ComponentModel.DataAnnotations;
using Obeli_K.Enums;
using Obeli_K.Models.Enums;

namespace Obeli_K.Models.ViewModels
{
    public class CreerCommandeVisiteurViewModel
    {
        [Required(ErrorMessage = "L'ID de la formule est obligatoire")]
        public Guid IdFormule { get; set; }

        [Required(ErrorMessage = "La date de consommation est obligatoire")]
        [Display(Name = "Date de consommation")]
        public DateTime DateConsommation { get; set; }


        [Display(Name = "Site")]
        public SiteType? Site { get; set; }

        [Required(ErrorMessage = "Le nom du visiteur est obligatoire")]
        [StringLength(100, ErrorMessage = "Le nom ne peut pas dépasser 100 caractères")]
        [Display(Name = "Nom du visiteur")]
        public string NomVisiteur { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "Le téléphone ne peut pas dépasser 20 caractères")]
        [Display(Name = "Téléphone du visiteur")]
        public string? TelephoneVisiteur { get; set; }

        [Required(ErrorMessage = "La quantité est obligatoire")]
        [Range(1, 10, ErrorMessage = "La quantité doit être entre 1 et 10")]
        [Display(Name = "Quantité")]
        public int Quantite { get; set; } = 1;
    }
}
