using System.ComponentModel.DataAnnotations;

namespace Obeli_K.Models
{
    /// <summary>
    /// Modèle pour gérer les quotas journaliers des groupes non-CIT
    /// </summary>
    public class QuotaJournalier
    {
        [Key] public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid GroupeNonCitId { get; set; }
        public virtual GroupeNonCit? GroupeNonCit { get; set; }

        [Required]
        [Display(Name = "Date")]
        public DateTime Date { get; set; }

        [Required]
        [Display(Name = "Quota Jour")]
        [Range(0, int.MaxValue, ErrorMessage = "Le quota doit être positif")]
        public int QuotaJour { get; set; } = 0;

        [Required]
        [Display(Name = "Quota Nuit")]
        [Range(0, int.MaxValue, ErrorMessage = "Le quota doit être positif")]
        public int QuotaNuit { get; set; } = 0;

        [Display(Name = "Plats consommés Jour")]
        public int PlatsConsommesJour { get; set; } = 0;

        [Display(Name = "Plats consommés Nuit")]
        public int PlatsConsommesNuit { get; set; } = 0;

        [Display(Name = "Restriction Formule")]
        public bool RestrictionFormuleStandard { get; set; } = true;

        [MaxLength(500)]
        [Display(Name = "Commentaires")]
        public string? Commentaires { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedOn { get; set; }
        public string? CreatedBy { get; set; }
        public string? ModifiedBy { get; set; }

        // Soft delete
        public int Supprimer { get; set; } = 0; // 0 = not deleted, 1 = deleted

        // Propriétés calculées
        public int PlatsRestantsJour => Math.Max(0, QuotaJour - PlatsConsommesJour);
        public int PlatsRestantsNuit => Math.Max(0, QuotaNuit - PlatsConsommesNuit);
        public int TotalQuota => QuotaJour + QuotaNuit;
        public int TotalConsomme => PlatsConsommesJour + PlatsConsommesNuit;
        public int TotalRestant => PlatsRestantsJour + PlatsRestantsNuit;
    }
}
