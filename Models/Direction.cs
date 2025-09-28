using System.ComponentModel.DataAnnotations;

namespace Obeli_K.Models
{
    public class Direction
    {
        [Key] public Guid Id { get; set; } = Guid.NewGuid();

        [Required, StringLength(100)] public string Nom { get; set; } = null!;
        [StringLength(500)] public string? Description { get; set; }
        [StringLength(10)] public string? Code { get; set; } // Code court pour identification

        public DateTime? CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? CreatedBy { get; set; }
        public string? ModifiedBy { get; set; }

        public int Supprimer { get; set; } = 0; // 0 = not deleted, 1 = deleted
    }
}