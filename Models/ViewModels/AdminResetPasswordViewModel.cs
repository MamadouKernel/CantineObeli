using System.ComponentModel.DataAnnotations;

namespace Obeli_K.Models.ViewModels
{
    public class AdminResetPasswordViewModel
    {
        [Display(Name = "Matricules")]
        public string? Matricules { get; set; }

        [Required(ErrorMessage = "Le nouveau mot de passe est obligatoire")]
        [StringLength(100, ErrorMessage = "Le mot de passe doit contenir au moins {2} caractères", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Nouveau mot de passe")]
        public string NouveauMotDePasse { get; set; } = string.Empty;

        [Required(ErrorMessage = "La confirmation est obligatoire")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmer le mot de passe")]
        [Compare("NouveauMotDePasse", ErrorMessage = "Les mots de passe ne correspondent pas")]
        public string ConfirmerMotDePasse { get; set; } = string.Empty;

        public List<Guid> UtilisateurIds { get; set; } = new List<Guid>();
    }
}
