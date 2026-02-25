using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Obeli_K.Models
{
    public class Service
    {
        [Key] public Guid Id { get; set; } = Guid.NewGuid();

        [Required, StringLength(100)] public string Nom { get; set; } = null!;
        [StringLength(500)] public string? Description { get; set; }
        
        [StringLength(10)] public string? Code { get; set; } // Code court pour identification
        [StringLength(100)] public string? Responsable { get; set; } // Nom du responsable
        [StringLength(100)] public string? Email { get; set; } // Email du responsable

        // Relation avec la direction parente
        public Guid? DirectionId { get; set; }
        public virtual Direction? Direction { get; set; }

        public DateTime? CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? CreatedBy { get; set; }
        public string? ModifiedBy { get; set; }

        // Relations
        public virtual ICollection<Utilisateur> Utilisateurs { get; set; } = new List<Utilisateur>();

        // Soft delete
        public int Supprimer { get; set; } = 0; // 0 = not deleted, 1 = deleted
    }
}
