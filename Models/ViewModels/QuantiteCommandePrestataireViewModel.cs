using System.ComponentModel.DataAnnotations;

namespace Obeli_K.Models.ViewModels
{
    public class QuantiteCommandePrestataireViewModel
    {
        public DateTime Date { get; set; }
        public List<FormuleQuantiteViewModel> Formules { get; set; } = new List<FormuleQuantiteViewModel>();
    }

    public class FormuleQuantiteViewModel
    {
        public Guid IdFormule { get; set; }

        [Display(Name = "Type de Formule")]
        public string TypeFormule { get; set; } = "";

        [Display(Name = "Plat")]
        public string Plat { get; set; } = "";

        [Display(Name = "Garniture")]
        public string Garniture { get; set; } = "";

        [Display(Name = "Entrée")]
        public string Entree { get; set; } = "";

        [Display(Name = "Dessert")]
        public string Dessert { get; set; } = "";

        [Display(Name = "Légumes")]
        public string Legumes { get; set; } = "";

        [Display(Name = "Féculent")]
        public string Feculent { get; set; } = "";

        [Display(Name = "Quantité Jour")]
        public int QuantiteJour { get; set; }

        [Display(Name = "Quantité Nuit")]
        public int QuantiteNuit { get; set; }

        [Display(Name = "Marge Jour")]
        public int MargeJour { get; set; } = 0;

        [Display(Name = "Marge Nuit")]
        public int MargeNuit { get; set; } = 0;

        [Display(Name = "Marge")]
        public int Marge { get; set; } = 0;
    }
}
