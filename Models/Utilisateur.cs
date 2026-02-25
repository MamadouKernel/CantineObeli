using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Obeli_K.Models.Enums;

namespace Obeli_K.Models
{
    public class Utilisateur
    {
        [Key] public Guid Id { get; set; } = Guid.NewGuid();

        [Required, StringLength(100)] public string Nom { get; set; } = null!;
        [Required, StringLength(100)] public string Prenoms { get; set; } = null!;
        [EmailAddress, StringLength(256)] public string? Email { get; set; }
        [Required, StringLength(50)] public string UserName { get; set; } = null!; // Matricule obligatoire
        [Phone, StringLength(32)] public string? PhoneNumber { get; set; }
        [StringLength(120)] public string? Lieu { get; set; }
        [StringLength(64)] public string? CodeCommande { get; set; } // legacy (déconseillé pour la sécu)

        // 🔐 Mots de passe : bcrypt
        [StringLength(100)] 
        [BindNever]
        public string MotDePasseHash { get; set; } = null!;

        // PRD — code personnel (PIN) & reset
        [StringLength(100)] public string? CodePinHash { get; set; }            // hash pin (facultatif)
        [StringLength(88)] public string? ResetTokenHash { get; set; }         // SHA-256 base64 du jeton
        public DateTime? ResetExpireLeUtc { get; set; }
        public bool ResetUtilise { get; set; }

        public bool MustResetPassword { get; set; }

        // Audit minimal
        [StringLength(100)] public string? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        [StringLength(100)] public string? ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }

        // Concurrence optimiste
        [Timestamp] public byte[]? RowVersion { get; set; }

        // Rattachements
        public RoleType Role { get; set; }

        public Guid? DirectionId { get; set; }
        public virtual Direction? Direction { get; set; }

        public Guid? ServiceId { get; set; }
        public virtual Service? Service { get; set; }

        public Guid? FonctionId { get; set; }
        public virtual Fonction? Fonction { get; set; }

        public SiteType? Site { get; set; }

        // Relations
        public virtual ICollection<Commande> Commandes { get; set; } = new List<Commande>();

        // Soft delete
        public int Supprimer { get; set; } = 0; // 0 = not deleted, 1 = deleted
    }
}