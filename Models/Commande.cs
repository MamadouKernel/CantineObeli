using Obeli_K.Enums;
using Obeli_K.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Obeli_K.Models
{
    public class Commande
    {
        [Key] public Guid IdCommande { get; set; } = Guid.NewGuid();

        // Date de création de la commande (hérité V1)
        public DateTime Date { get; set; }

        // PRD: date de consommation
        public DateTime? DateConsommation { get; set; }

        // V1: status int; utiliser l'enum StatutCommande côté logique
        public int StatusCommande { get; set; }

        public string? CodeCommande { get; set; }
        
        // Période de service (Jour ou Nuit)
        public Periode PeriodeService { get; set; } = Periode.Jour;

        public decimal Montant { get; set; }

        // Client CIT
        public Guid? UtilisateurId { get; set; }
        public Utilisateur? Utilisateur { get; set; }

        // Lien Formule du jour
        public Guid IdFormule { get; set; }
        public virtual FormuleJour? FormuleJour { get; set; }

        // PRD — type de client et variantes
        public TypeClientCommande TypeClient { get; set; } = TypeClientCommande.CitUtilisateur;

        public Guid? GroupeNonCitId { get; set; }
        public GroupeNonCit? GroupeNonCit { get; set; }

        [StringLength(120)] public string? VisiteurNom { get; set; }
        [StringLength(32)] public string? VisiteurTelephone { get; set; }
        
        // Direction d'origine du visiteur
        public Guid? DirectionId { get; set; }
        public Direction? Direction { get; set; }
        
        // Code de vérification pour le prestataire
        [StringLength(20)] public string? CodeVerification { get; set; }

        // PRD — site & logistique
        public SiteType? Site { get; set; }

        public DateTime? DateLivraisonPrevueUtc { get; set; }
        public DateTime? DateReceptionUtc { get; set; }
        public Guid? ReceptionConfirmeeParUtilisateurId { get; set; }
        [StringLength(120)] public string? ReceptionConfirmeeParNom { get; set; }

        [Range(1, int.MaxValue)] public int Quantite { get; set; } = 1;
        public bool Instantanee { get; set; }
        public bool AnnuleeParPrestataire { get; set; }
        [StringLength(256)] public string? MotifAnnulation { get; set; }

        public DateTime? CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? CreatedBy { get; set; }
        public string? ModifiedBy { get; set; }

        // Soft delete
        public int Supprimer { get; set; } = 0; // 0 = not deleted, 1 = deleted
    }
}
