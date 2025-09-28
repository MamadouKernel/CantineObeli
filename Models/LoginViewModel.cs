using System.ComponentModel.DataAnnotations;

namespace Obeli_K.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Le matricule est requis")]
        [Display(Name = "Matricule")]
        [StringLength(50, ErrorMessage = "Le matricule ne peut pas dépasser {1} caractères")]
        public string Matricule { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le mot de passe est requis")]
        [Display(Name = "Mot de passe")]
        [StringLength(100, ErrorMessage = "Le mot de passe ne peut pas dépasser {1} caractères")]
        [DataType(DataType.Password)]
        public string MotDePasse { get; set; } = string.Empty;

        [Display(Name = "Se souvenir de moi")]
        public bool SeSouvenirDeMoi { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
