using System.ComponentModel.DataAnnotations;
using Obeli_K.Models.Enums;

namespace Obeli_K.Models.ViewModels
{
    public class EditUtilisateurViewModel
    {
        [Required]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Le nom est obligatoire.")]
        [StringLength(100, ErrorMessage = "Le nom ne peut pas dépasser 100 caractères.")]
        public string Nom { get; set; } = null!;

        [Required(ErrorMessage = "Les prénoms sont obligatoires.")]
        [StringLength(100, ErrorMessage = "Les prénoms ne peuvent pas dépasser 100 caractères.")]
        public string Prenoms { get; set; } = null!;

        [Required(ErrorMessage = "Le matricule est obligatoire.")]
        [StringLength(50, ErrorMessage = "Le matricule ne peut pas dépasser 50 caractères.")]
        public string UserName { get; set; } = null!;

        [EmailAddress(ErrorMessage = "L'adresse email n'est pas valide.")]
        [StringLength(256, ErrorMessage = "L'email ne peut pas dépasser 256 caractères.")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Le numéro de téléphone n'est pas valide.")]
        [StringLength(32, ErrorMessage = "Le numéro de téléphone ne peut pas dépasser 32 caractères.")]
        public string? PhoneNumber { get; set; }

        [StringLength(120, ErrorMessage = "Le lieu ne peut pas dépasser 120 caractères.")]
        public string? Lieu { get; set; }

        [StringLength(64, ErrorMessage = "Le code commande ne peut pas dépasser 64 caractères.")]
        public string? CodeCommande { get; set; }

        [Required(ErrorMessage = "Le rôle est obligatoire.")]
        public RoleType Role { get; set; }

        [Required(ErrorMessage = "La direction est obligatoire.")]
        public Guid? DirectionId { get; set; }

        public Guid? ServiceId { get; set; }

        public Guid? FonctionId { get; set; }

        public SiteType? Site { get; set; }

        // Champs pour le changement de mot de passe (optionnels)
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Le mot de passe doit contenir entre 6 et 100 caractères.")]
        public string? NouveauMotDePasse { get; set; }

        [Compare(nameof(NouveauMotDePasse), ErrorMessage = "Les mots de passe ne correspondent pas.")]
        public string? ConfirmerNouveauMotDePasse { get; set; }
    }
}
