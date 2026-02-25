using System.ComponentModel.DataAnnotations;

namespace Obeli_K.Models.ViewModels
{
    /// <summary>
    /// ViewModel pour l'export Excel des quantités de formules commandées
    /// </summary>
    public class ExportQuantiteFormuleViewModel
    {
        [Display(Name = "Date")]
        public string Date { get; set; } = "";

        [Display(Name = "Jour de la semaine")]
        public string JourSemaine { get; set; } = "";

        [Display(Name = "Type de formule")]
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

        [Display(Name = "Quota Douane")]
        public int QuotaDouane { get; set; }

        [Display(Name = "Marge")]
        public int Marge { get; set; }

        [Display(Name = "Total avec Marge")]
        public int TotalAvecMarge { get; set; }
    }
}


