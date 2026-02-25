using System.ComponentModel.DataAnnotations;

namespace Obeli_K.Models.ViewModels
{
    public class ImportFormuleViewModel
    {
        [Required(ErrorMessage = "Le fichier est obligatoire.")]
        [Display(Name = "Fichier Excel")]
        public IFormFile? FichierExcel { get; set; }

        [Display(Name = "Remplacer les formules existantes")]
        public bool RemplacerExistantes { get; set; } = false;

        [Display(Name = "Ignorer les erreurs")]
        public bool IgnorerErreurs { get; set; } = false;
    }

    public class ImportResultViewModel
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int TotalLignes { get; set; }
        public int LignesImportees { get; set; }
        public int LignesErreur { get; set; }
        public List<string> Erreurs { get; set; } = new List<string>();
        public List<string> Avertissements { get; set; } = new List<string>();
    }
}
