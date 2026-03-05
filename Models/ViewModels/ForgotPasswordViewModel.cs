using System.ComponentModel.DataAnnotations;

namespace Obeli_K.Models.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Le matricule est obligatoire")]
        [Display(Name = "Matricule")]
        public string Matricule { get; set; } = string.Empty;
    }
}
