using System.ComponentModel.DataAnnotations;

namespace Obeli_K.Models.ViewModels
{
    public class GestionMargesViewModel
    {
        public DateTime DateDebut { get; set; } = DateTime.Today;
        public DateTime DateFin { get; set; } = DateTime.Today;
        
        public List<FormuleMargeViewModel> Formules { get; set; } = new List<FormuleMargeViewModel>();
    }

    public class FormuleMargeViewModel
    {
        public Guid IdFormule { get; set; }
        
        [Display(Name = "Date")]
        public DateTime Date { get; set; }
        
        [Display(Name = "Type de Formule")]
        public string NomFormule { get; set; } = "";
        
        [Display(Name = "Plat")]
        public string Plat { get; set; } = "";
        
        [Display(Name = "Marge Jour Restante")]
        [Range(0, 1000, ErrorMessage = "La marge jour doit être entre 0 et 1000")]
        public int MargeJourRestante { get; set; } = 0;
        
        [Display(Name = "Marge Nuit Restante")]
        [Range(0, 1000, ErrorMessage = "La marge nuit doit être entre 0 et 1000")]
        public int MargeNuitRestante { get; set; } = 0;
        
        [Display(Name = "Quota Jour Restant")]
        public int? QuotaJourRestant { get; set; }
        
        [Display(Name = "Quota Nuit Restant")]
        public int? QuotaNuitRestant { get; set; }
    }
}

