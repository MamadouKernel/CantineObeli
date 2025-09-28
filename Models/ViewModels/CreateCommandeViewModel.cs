using System.ComponentModel.DataAnnotations;
using Obeli_K.Enums;
using Obeli_K.Models.Enums;

namespace Obeli_K.Models.ViewModels
{
    public class CreateCommandeViewModel
    {
        [Required(ErrorMessage = "La date de consommation est obligatoire.")]
        [Display(Name = "Date de consommation")]
        public DateTime DateConsommation { get; set; } = GetDefaultDateConsommation();

        private static DateTime GetDefaultDateConsommation()
        {
            var aujourdhui = DateTime.Today;
            var jourDeLaSemaine = (int)aujourdhui.DayOfWeek;
            // Calculer le lundi de la semaine suivante
            return aujourdhui.AddDays(7 - jourDeLaSemaine + 1);
        }

        [Required(ErrorMessage = "La formule est obligatoire.")]
        [Display(Name = "Formule")]
        public Guid IdFormule { get; set; }

        [Required(ErrorMessage = "Le type de client est obligatoire.")]
        [Display(Name = "Type de client")]
        public TypeClientCommande TypeClient { get; set; } = TypeClientCommande.CitUtilisateur;

        [Display(Name = "Utilisateur")]
        public Guid? UtilisateurId { get; set; }

        [Display(Name = "Groupe non-CIT")]
        public Guid? GroupeNonCitId { get; set; }

        [Display(Name = "Nom du visiteur")]
        [StringLength(120, ErrorMessage = "Le nom du visiteur ne peut pas dépasser 120 caractères.")]
        public string? VisiteurNom { get; set; }

        [Display(Name = "Téléphone du visiteur")]
        [StringLength(32, ErrorMessage = "Le téléphone ne peut pas dépasser 32 caractères.")]
        public string? VisiteurTelephone { get; set; }

        [Display(Name = "Site")]
        public SiteType? Site { get; set; }

        [Display(Name = "Période de service")]
        public Periode PeriodeService { get; set; } = Periode.Jour;

        [Display(Name = "Quantité")]
        [Range(1, int.MaxValue, ErrorMessage = "La quantité doit être au moins 1.")]
        public int Quantite { get; set; } = 1;

        [Display(Name = "Commande instantanée")]
        public bool Instantanee { get; set; } = false;

        [Display(Name = "Date de livraison prévue")]
        public DateTime? DateLivraisonPrevueUtc { get; set; }
    }
}
