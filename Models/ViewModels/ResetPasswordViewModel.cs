using System.ComponentModel.DataAnnotations;

namespace Obeli_K.Models.ViewModels
{
    public class ResetPasswordViewModel
    {
        [Display(Name = "Matricules")]
        [Required(ErrorMessage = "Veuillez saisir au moins un matricule.")]
        public string Matricules { get; set; } = string.Empty;

        [Display(Name = "Nouveau mot de passe")]
        [Required(ErrorMessage = "Le nouveau mot de passe est obligatoire.")]
        [MinLength(6, ErrorMessage = "Le mot de passe doit contenir au moins 6 caractères.")]
        [DataType(DataType.Password)]
        public string NouveauMotDePasse { get; set; } = string.Empty;

        [Display(Name = "Confirmer le nouveau mot de passe")]
        [Required(ErrorMessage = "La confirmation du mot de passe est obligatoire.")]
        [DataType(DataType.Password)]
        [Compare(nameof(NouveauMotDePasse), ErrorMessage = "Les mots de passe ne correspondent pas.")]
        public string ConfirmerMotDePasse { get; set; } = string.Empty;
    }
}
