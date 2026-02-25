using System.ComponentModel.DataAnnotations;

namespace Obeli_K.Models
{
    public class PrestataireCantine
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required(ErrorMessage = "Le nom du prestataire est obligatoire.")]
        [StringLength(200, ErrorMessage = "Le nom ne peut pas dépasser 200 caractères.")]
        [Display(Name = "Nom du prestataire")]
        public string Nom { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Le contact ne peut pas dépasser 100 caractères.")]
        [Display(Name = "Personne de contact")]
        public string? Contact { get; set; }

        [EmailAddress(ErrorMessage = "L'adresse email n'est pas valide.")]
        [StringLength(100, ErrorMessage = "L'email ne peut pas dépasser 100 caractères.")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [StringLength(20, ErrorMessage = "Le téléphone ne peut pas dépasser 20 caractères.")]
        [Display(Name = "Téléphone")]
        public string? Telephone { get; set; }

        [StringLength(500, ErrorMessage = "L'adresse ne peut pas dépasser 500 caractères.")]
        [Display(Name = "Adresse")]
        public string? Adresse { get; set; }

        // Audit fields
        [Display(Name = "Date de création")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Créé par")]
        public string CreatedBy { get; set; } = string.Empty;

        [Display(Name = "Date de modification")]
        public DateTime? ModifiedAt { get; set; }

        [Display(Name = "Modifié par")]
        public string? ModifiedBy { get; set; }

        // Soft delete
        public int Supprimer { get; set; } = 0;
    }
}
