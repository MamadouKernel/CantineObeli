using Obeli_K.Enums;

namespace Obeli_K.Models.ViewModels
{
    public class CommandeNonConsommeeViewModel
    {
        public Guid IdCommande { get; set; }
        public string CodeCommande { get; set; } = "";
        public DateTime DateCommande { get; set; }
        public DateTime DateConsommation { get; set; }
        public string NomUtilisateur { get; set; } = "";
        public string EmailUtilisateur { get; set; } = "";
        public string NomFormule { get; set; } = "";
        public string Plat { get; set; } = "";
        public decimal Montant { get; set; }
        public StatutCommande StatusCommande { get; set; }
        public TypeClientCommande TypeClient { get; set; }
        public bool EstWeekend { get; set; }
        public bool EstJourFerie { get; set; }
        public int NombreJoursRetard { get; set; }
    }

    public class FacturationResult
    {
        public bool FacturationActive { get; set; }
        public int PourcentageFacturation { get; set; }
        public int NombreAbsencesGratuites { get; set; }
        public bool FacturationWeekend { get; set; }
        public bool FacturationJoursFeries { get; set; }
        public List<CommandeFacturable> CommandesFacturables { get; set; } = new();
        public List<CommandeNonFacturable> CommandesNonFacturables { get; set; } = new();
        public decimal MontantTotalAFacturer { get; set; }
        public int NombreCommandesFacturables { get; set; }
        public int NombreCommandesNonFacturables { get; set; }
    }

    public class CommandeFacturable
    {
        public CommandeNonConsommeeViewModel Commande { get; set; } = new();
        public decimal MontantAFacturer { get; set; }
        public decimal MontantOriginal { get; set; }
        public int PourcentageApplique { get; set; }
    }

    public class CommandeNonFacturable
    {
        public CommandeNonConsommeeViewModel Commande { get; set; } = new();
        public string Motif { get; set; } = "";
    }
}
