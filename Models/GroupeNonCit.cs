using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Obeli_K.Models
{
    public class GroupeNonCit
    {
        [Key] public Guid Id { get; set; } = Guid.NewGuid();

        [Required, StringLength(100)] public string Nom { get; set; } = null!;
        [StringLength(500)] public string? Description { get; set; }

        // Gestion des quotas pour les groupes spéciaux (ex: Douaniers)
        // Temporairement commenté pour éviter les erreurs de colonnes manquantes
        // public int? QuotaJournalier { get; set; } // Quota fixe de plats par jour
        // public bool RestrictionFormuleStandard { get; set; } = false; // Limite aux formules standard uniquement
        // public string? CodeGroupe { get; set; } // Code commun pour les commandes groupées

        public DateTime? CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? CreatedBy { get; set; }
        public string? ModifiedBy { get; set; }

        // Relations
        public virtual ICollection<Commande> Commandes { get; set; } = new List<Commande>();

        // Soft delete
        public int Supprimer { get; set; } = 0; // 0 = not deleted, 1 = deleted
    }
}
