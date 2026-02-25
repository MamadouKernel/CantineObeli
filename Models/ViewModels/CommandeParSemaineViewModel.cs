using Obeli_K.Enums;
using Obeli_K.Models.Enums;

namespace Obeli_K.Models.ViewModels
{
    public class CommandeParSemaineViewModel
    {
        public DateTime DateDebutSemaine { get; set; }
        public DateTime DateFinSemaine { get; set; }
        public List<JourSemaineViewModel> JoursSemaine { get; set; } = new List<JourSemaineViewModel>();
        public List<CommandeExistanteViewModel> CommandesExistantes { get; set; } = new List<CommandeExistanteViewModel>();
    }

    public class JourSemaineViewModel
    {
        public DateTime Date { get; set; }
        public string NomJour { get; set; } = string.Empty;
        public List<FormuleJourSemaineViewModel> Formules { get; set; } = new List<FormuleJourSemaineViewModel>();
    }

    public class FormuleJourSemaineViewModel
    {
        public Guid IdFormule { get; set; }
        public string TypeFormule { get; set; } = string.Empty;
        public string NomFormule { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        
        // Formule Améliorée
        public string? Entree { get; set; }
        public string? Dessert { get; set; }
        public string? Plat { get; set; }
        public string? Garniture { get; set; }
        
        // Formule Standard
        public string? PlatStandard1 { get; set; }
        public string? GarnitureStandard1 { get; set; }
        public string? PlatStandard2 { get; set; }
        public string? GarnitureStandard2 { get; set; }
        
        // Champs communs
        public string? Feculent { get; set; }
        public string? Legumes { get; set; }
        
        public bool EstCommande { get; set; }
        public Guid? CommandeId { get; set; }
        public Periode PeriodeCommande { get; set; }
        public string? CodeCommande { get; set; }
    }

    public class CommandeExistanteViewModel
    {
        public Guid IdCommande { get; set; }
        public DateTime DateConsommation { get; set; }
        public string NomJour { get; set; } = string.Empty;
        public string TypeFormule { get; set; } = string.Empty;
        public Periode PeriodeService { get; set; }
        public string CodeCommande { get; set; } = string.Empty;
        public string NomPlat { get; set; } = string.Empty; // Nom du plat commandé
    }
}
