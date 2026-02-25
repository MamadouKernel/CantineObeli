using System.ComponentModel.DataAnnotations;

namespace Obeli_K.Models.ViewModels
{
    public class GenererCommandePrestataireViewModel
    {
        [Display(Name = "Prestataire")]
        [Required(ErrorMessage = "Veuillez sélectionner un prestataire.")]
        public Guid PrestataireId { get; set; }

        [Display(Name = "Date de début")]
        [Required(ErrorMessage = "La date de début est obligatoire.")]
        [DataType(DataType.Date)]
        public DateTime DateDebut { get; set; } = DateTime.Today;

        [Display(Name = "Date de fin")]
        [Required(ErrorMessage = "La date de fin est obligatoire.")]
        [DataType(DataType.Date)]
        public DateTime DateFin { get; set; } = DateTime.Today.AddDays(7);

        [Display(Name = "Type de commande")]
        [Required(ErrorMessage = "Veuillez sélectionner un type de commande.")]
        public string TypeCommande { get; set; } = string.Empty;

        [Display(Name = "Commentaires")]
        [DataType(DataType.MultilineText)]
        public string? Commentaires { get; set; }
    }
}
