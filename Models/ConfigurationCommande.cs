using System.ComponentModel.DataAnnotations;

namespace Obeli_K.Models
{
    public class ConfigurationCommande
    {
        [Key] 
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(100)]
        public string Cle { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Valeur { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public DateTime? CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? CreatedBy { get; set; }
        public string? ModifiedBy { get; set; }

        // Soft delete
        public int Supprimer { get; set; } = 0;
    }
}
