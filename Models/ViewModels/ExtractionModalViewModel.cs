using System.ComponentModel.DataAnnotations;

namespace Obeli_K.Models.ViewModels
{
    public class ExtractionModalViewModel
    {
        [Required(ErrorMessage = "La date de début est obligatoire.")]
        [Display(Name = "Date de début")]
        [DataType(DataType.Date)]
        public DateTime DateDebut { get; set; }

        [Required(ErrorMessage = "La date de fin est obligatoire.")]
        [Display(Name = "Date de fin")]
        [DataType(DataType.Date)]
        public DateTime DateFin { get; set; }

        public List<CommandeAvecMargeViewModel> CommandesAvecMarges { get; set; } = new();
    }

    public class CommandeAvecMargeViewModel
    {
        public Guid IdFormule { get; set; }
        public DateTime Date { get; set; }
        public string? TypeFormule { get; set; }
        public string? NomFormule { get; set; }
        public int NombreCommandes { get; set; }
        public int Marge { get; set; }
        
        public int TotalCommande => Marge + NombreCommandes;
    }
}
