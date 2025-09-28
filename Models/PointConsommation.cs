using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Obeli_K.Models
{
    public class PointConsommation
    {
        [Key]
        public Guid IdPointConsommation { get; set; }

        [Required]
        public Guid UtilisateurId { get; set; }

        [Required]
        public Guid CommandeId { get; set; }

        [Required]
        public DateTime DateConsommation { get; set; }

        [Required]
        [StringLength(50)]
        public string TypeFormule { get; set; } = string.Empty;

        [StringLength(200)]
        public string? NomPlat { get; set; }

        public int QuantiteConsommee { get; set; } = 1;

        [StringLength(100)]
        public string? LieuConsommation { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        public string? CreatedBy { get; set; }

        public DateTime? ModifiedOn { get; set; }

        [StringLength(100)]
        public string? ModifiedBy { get; set; }

        public int Supprimer { get; set; } = 0;

        // Navigation properties
        [ForeignKey("UtilisateurId")]
        public virtual Utilisateur? Utilisateur { get; set; }

        [ForeignKey("CommandeId")]
        public virtual Commande? Commande { get; set; }
    }
}
