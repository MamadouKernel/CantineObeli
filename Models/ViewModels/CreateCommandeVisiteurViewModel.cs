using System.ComponentModel.DataAnnotations;
using Obeli_K.Enums;

namespace Obeli_K.Models.ViewModels
{
    /// <summary>
    /// ViewModel pour créer des commandes pour les visiteurs
    /// </summary>
    public class CreateCommandeVisiteurViewModel
    {
        [Required(ErrorMessage = "La direction est obligatoire")]
        [Display(Name = "Direction")]
        public Guid DirectionId { get; set; }

        [Required(ErrorMessage = "La date de début est obligatoire")]
        [Display(Name = "Date de début")]
        [DataType(DataType.Date)]
        public DateTime DateDebut { get; set; }

        [Required(ErrorMessage = "La date de fin est obligatoire")]
        [Display(Name = "Date de fin")]
        [DataType(DataType.Date)]
        public DateTime DateFin { get; set; }

        [Required(ErrorMessage = "Le nombre de visiteurs est obligatoire")]
        [Display(Name = "Nombre de visiteurs")]
        [Range(1, 1000, ErrorMessage = "Le nombre de visiteurs doit être entre 1 et 1000")]
        public int NombreVisiteurs { get; set; }

        [Required(ErrorMessage = "Le type de formule est obligatoire")]
        [Display(Name = "Type de formule")]
        public string TypeFormule { get; set; } = "Standard 1";

        [Required(ErrorMessage = "La période de service est obligatoire")]
        [Display(Name = "Période de service")]
        public Periode PeriodeService { get; set; } = Periode.Jour;

        // Propriétés pour affichage
        public string? DirectionNom { get; set; }
        public List<FormuleJour>? FormulesDisponibles { get; set; }
    }
}

