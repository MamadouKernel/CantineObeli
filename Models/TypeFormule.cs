using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Obeli_K.Models
{
    public class TypeFormule
    {
        [Key] public Guid Id { get; set; } = Guid.NewGuid();

        [Required, StringLength(100)] public string Nom { get; set; } 
         public string? Description { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? CreatedBy { get; set; }
        public string? ModifiedBy { get; set; }

        // Relations
        public virtual ICollection<FormuleJour> FormulesJour { get; set; } = new List<FormuleJour>();

        // Soft delete
        public int Supprimer { get; set; } = 0; // 0 = not deleted, 1 = deleted
    }
}
