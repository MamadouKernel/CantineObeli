using System.ComponentModel.DataAnnotations;

namespace Obeli_K.Models.ViewModels
{
    public class DefinirMargesViewModel
    {
        [Required]
        public DateTime DateDebut { get; set; }
        
        [Required]
        public DateTime DateFin { get; set; }
        
        public List<MenuAvecMargeViewModel> MenusAvecMarges { get; set; } = new();
    }

    public class MenuAvecMargeViewModel
    {
        public Guid IdFormule { get; set; }
        public DateTime Date { get; set; }
        public string? NomFormule { get; set; }
        public string? TypeFormule { get; set; }
        public int MargeActuelle { get; set; }
        
        [Range(0, 100, ErrorMessage = "La marge doit être entre 0 et 100.")]
        public int NouvelleMarge { get; set; }
    }
}
