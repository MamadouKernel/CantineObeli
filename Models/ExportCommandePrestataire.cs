using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Obeli_K.Models
{
    /// <summary>
    /// Modèle pour tracker les exports de commandes prestataires
    /// </summary>
    public class ExportCommandePrestataire
    {
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Date de début de la période exportée
        /// </summary>
        [Required]
        public DateTime DateDebut { get; set; }

        /// <summary>
        /// Date de fin de la période exportée
        /// </summary>
        [Required]
        public DateTime DateFin { get; set; }

        /// <summary>
        /// Nom du fichier exporté
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string NomFichier { get; set; } = string.Empty;

        /// <summary>
        /// Taille du fichier en octets
        /// </summary>
        public long TailleFichier { get; set; }

        /// <summary>
        /// Utilisateur qui a effectué l'export
        /// </summary>
        [Required]
        public Guid UtilisateurId { get; set; }

        /// <summary>
        /// Navigation vers l'utilisateur
        /// </summary>
        [ForeignKey("UtilisateurId")]
        public virtual Utilisateur? Utilisateur { get; set; }

        /// <summary>
        /// Date et heure de l'export
        /// </summary>
        [Required]
        public DateTime DateExport { get; set; }

        /// <summary>
        /// Indique si l'export a été effectué (pour éviter les doublons)
        /// </summary>
        public bool ExportEffectue { get; set; } = true;

        /// <summary>
        /// Commentaires ou notes sur l'export
        /// </summary>
        [MaxLength(500)]
        public string? Commentaires { get; set; }

        /// <summary>
        /// Date de création
        /// </summary>
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Utilisateur qui a créé l'enregistrement
        /// </summary>
        [MaxLength(100)]
        public string CreatedBy { get; set; } = string.Empty;

        /// <summary>
        /// Date de dernière modification
        /// </summary>
        public DateTime? ModifiedOn { get; set; }

        /// <summary>
        /// Utilisateur qui a modifié l'enregistrement
        /// </summary>
        [MaxLength(100)]
        public string? ModifiedBy { get; set; }

        /// <summary>
        /// Indicateur de suppression (soft delete)
        /// </summary>
        public int Supprimer { get; set; } = 0;
    }
}

