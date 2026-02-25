using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Obeli_K.Models
{
    public class FormuleJour
    {
        [Key] public Guid IdFormule { get; set; } = Guid.NewGuid();

        // Champs pour formule Améliorée (1 plat/jour)
        public string? Entree { get; set; }
        public string? Dessert { get; set; }
        public string? Plat { get; set; }
        public string? Garniture { get; set; }

        // Champs pour formule Standard (2 plats/jour)
        public string? PlatStandard1 { get; set; }
        public string? GarnitureStandard1 { get; set; }
        public string? PlatStandard2 { get; set; }
        public string? GarnitureStandard2 { get; set; }

        // Champs communs (optionnels)
        public string? Feculent { get; set; }
        public string? Legumes { get; set; }

        public int? Marge { get; set; }
        
        // Quotas pour la gestion des commandes instantanées
        [Display(Name = "Quota Jour Restant")]
        public int? QuotaJourRestant { get; set; } = 0; // Quota initial pour le midi
        
        [Display(Name = "Quota Nuit Restant")]
        public int? QuotaNuitRestant { get; set; } = 0; // Quota initial pour le soir
        
        [Display(Name = "Marge Jour Restante")]
        public int? MargeJourRestante { get; set; } = 0; // Marge disponible après épuisement du quota jour
        
        [Display(Name = "Marge Nuit Restante")]
        public int? MargeNuitRestante { get; set; } = 0; // Marge disponible après épuisement du quota nuit
        
        public int? Statut { get; set; } = 1; // Par défaut activé

        public DateTime Date { get; set; }

        public DateTime? CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? CreatedBy { get; set; }
        public string? ModifiedBy { get; set; }

        public string? NomFormule { get; set; }
        public Guid? TypeFormuleId { get; set; }
        public TypeFormule? NomFormuleNavigation { get; set; }

        // PRD
        public bool Verrouille { get; set; }
        [MaxLength(2048)] public string? Historique { get; set; }

        public virtual ICollection<Commande> Commandes { get; set; } = new List<Commande>();

        // Soft delete
        public int Supprimer { get; set; } = 0; // 0 = not deleted, 1 = deleted
    }
}
