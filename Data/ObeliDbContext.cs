using Microsoft.EntityFrameworkCore;
using Obeli_K.Models;
using Obeli_K.Models.Enums;

namespace Obeli_K.Data
{
    public class ObeliDbContext : DbContext
    {
        public ObeliDbContext(DbContextOptions<ObeliDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            
            // Supprimer l'avertissement des changements en attente
            optionsBuilder.ConfigureWarnings(warnings => 
                warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        }

        // DbSets
        public DbSet<Utilisateur> Utilisateurs { get; set; }
        public DbSet<Direction> Directions { get; set; }
        public DbSet<Departement> Departements { get; set; }
        // public DbSet<Service> Services { get; set; }
        public DbSet<Fonction> Fonctions { get; set; }
        public DbSet<Commande> Commandes { get; set; }
        public DbSet<FormuleJour> FormulesJour { get; set; }
        public DbSet<TypeFormule> TypesFormule { get; set; }
        public DbSet<GroupeNonCit> GroupesNonCit { get; set; }
        public DbSet<PointConsommation> PointsConsommation { get; set; }
        public DbSet<ConfigurationCommande> ConfigurationsCommande { get; set; }
        public DbSet<QuotaJournalier> QuotasJournaliers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuration Direction
            modelBuilder.Entity<Direction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nom).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Code).HasMaxLength(10);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.ModifiedBy).HasMaxLength(100);
                entity.Property(e => e.Supprimer).HasDefaultValue(0);
            });

            // Configuration Utilisateur
            modelBuilder.Entity<Utilisateur>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nom).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Prenoms).IsRequired().HasMaxLength(100);
                entity.Property(e => e.UserName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).HasMaxLength(256);
                entity.Property(e => e.PhoneNumber).HasMaxLength(32);
                entity.Property(e => e.Lieu).HasMaxLength(120);
                entity.Property(e => e.CodeCommande).HasMaxLength(64);
                entity.Property(e => e.MotDePasseHash).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CodePinHash).HasMaxLength(100);
                entity.Property(e => e.ResetTokenHash).HasMaxLength(88);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.ModifiedBy).HasMaxLength(100);
                entity.Property(e => e.Supprimer).HasDefaultValue(0);

                // Relations
                entity.HasOne(e => e.Departement)
                    .WithMany(d => d.Utilisateurs)
                    .HasForeignKey(e => e.DepartementId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Temporairement commenté pour éviter les erreurs de colonnes manquantes
                /*
                entity.HasOne(e => e.Service)
                    .WithMany(s => s.Utilisateurs)
                    .HasForeignKey(e => e.ServiceId)
                    .OnDelete(DeleteBehavior.Restrict);
                */

                entity.HasOne(e => e.Fonction)
                    .WithMany(f => f.Utilisateurs)
                    .HasForeignKey(e => e.FonctionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.Commandes)
                    .WithOne(c => c.Utilisateur)
                    .HasForeignKey(c => c.UtilisateurId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuration Commande
            modelBuilder.Entity<Commande>(entity =>
            {
                entity.HasKey(e => e.IdCommande);
                entity.Property(e => e.CodeCommande).HasMaxLength(64);
                entity.Property(e => e.VisiteurNom).HasMaxLength(120);
                entity.Property(e => e.VisiteurTelephone).HasMaxLength(32);
                entity.Property(e => e.CodeVerification).HasMaxLength(20);
                entity.Property(e => e.ReceptionConfirmeeParNom).HasMaxLength(120);
                entity.Property(e => e.MotifAnnulation).HasMaxLength(256);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.ModifiedBy).HasMaxLength(100);
                entity.Property(e => e.Supprimer).HasDefaultValue(0);
                entity.Property(e => e.Montant).HasPrecision(18, 2);

                // Relations
                entity.HasOne(e => e.FormuleJour)
                    .WithMany(f => f.Commandes)
                    .HasForeignKey(e => e.IdFormule)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.GroupeNonCit)
                    .WithMany(g => g.Commandes)
                    .HasForeignKey(e => e.GroupeNonCitId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Direction)
                    .WithMany()
                    .HasForeignKey(e => e.DirectionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuration FormuleJour
            modelBuilder.Entity<FormuleJour>(entity =>
            {
                entity.HasKey(e => e.IdFormule);
                entity.Property(e => e.NomFormule).HasMaxLength(100);
                entity.Property(e => e.Entree).HasMaxLength(200);
                entity.Property(e => e.Dessert).HasMaxLength(200);
                entity.Property(e => e.Plat).HasMaxLength(200);
                entity.Property(e => e.Garniture).HasMaxLength(200);
                entity.Property(e => e.PlatStandard1).HasMaxLength(200);
                entity.Property(e => e.GarnitureStandard1).HasMaxLength(200);
                entity.Property(e => e.PlatStandard2).HasMaxLength(200);
                entity.Property(e => e.GarnitureStandard2).HasMaxLength(200);
                entity.Property(e => e.Feculent).HasMaxLength(200);
                entity.Property(e => e.Legumes).HasMaxLength(200);
                entity.Property(e => e.Historique).HasMaxLength(2048);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.ModifiedBy).HasMaxLength(100);
                entity.Property(e => e.Supprimer).HasDefaultValue(0);

                // Relations
                entity.HasOne(e => e.NomFormuleNavigation)
                    .WithMany(t => t.FormulesJour)
                    .HasForeignKey(e => e.TypeFormuleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuration Département
            modelBuilder.Entity<Departement>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nom).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.ModifiedBy).HasMaxLength(100);
                entity.Property(e => e.Supprimer).HasDefaultValue(0);
            });

            // Configuration Fonction
            modelBuilder.Entity<Fonction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nom).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.ModifiedBy).HasMaxLength(100);
                entity.Property(e => e.Supprimer).HasDefaultValue(0);
            });

            // Configuration TypeFormule
            modelBuilder.Entity<TypeFormule>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nom).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.ModifiedBy).HasMaxLength(100);
                entity.Property(e => e.Supprimer).HasDefaultValue(0);
            });

            // Configuration GroupeNonCit
            modelBuilder.Entity<GroupeNonCit>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nom).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                // entity.Property(e => e.CodeGroupe).HasMaxLength(50); // Temporairement commenté
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.ModifiedBy).HasMaxLength(100);
                entity.Property(e => e.Supprimer).HasDefaultValue(0);
                // entity.Property(e => e.RestrictionFormuleStandard).HasDefaultValue(false); // Temporairement commenté
            });

            // Configuration PointConsommation
            modelBuilder.Entity<PointConsommation>(entity =>
            {
                entity.HasKey(e => e.IdPointConsommation);
                entity.Property(e => e.TypeFormule).IsRequired().HasMaxLength(50);
                entity.Property(e => e.NomPlat).HasMaxLength(200);
                entity.Property(e => e.LieuConsommation).HasMaxLength(100);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.ModifiedBy).HasMaxLength(100);
                entity.Property(e => e.Supprimer).HasDefaultValue(0);

                // Relations
                entity.HasOne(e => e.Utilisateur)
                    .WithMany()
                    .HasForeignKey(e => e.UtilisateurId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Commande)
                    .WithMany()
                    .HasForeignKey(e => e.CommandeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuration des enums
            modelBuilder.Entity<Utilisateur>()
                .Property(e => e.Role)
                .HasConversion<int>();

            modelBuilder.Entity<Utilisateur>()
                .Property(e => e.Site)
                .HasConversion<int>();

            modelBuilder.Entity<Commande>()
                .Property(e => e.PeriodeService)
                .HasConversion<int>();

            modelBuilder.Entity<Commande>()
                .Property(e => e.TypeClient)
                .HasConversion<int>();

            modelBuilder.Entity<Commande>()
                .Property(e => e.Site)
                .HasConversion<int>();

            // Configuration Direction (temporairement commenté)
            /*
            modelBuilder.Entity<Direction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nom).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Code).HasMaxLength(10);
                entity.Property(e => e.Responsable).HasMaxLength(100);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.ModifiedBy).HasMaxLength(100);
                entity.Property(e => e.Supprimer).HasDefaultValue(0);
            });
            */

            // Configuration Departement
            modelBuilder.Entity<Departement>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nom).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                // Temporairement commenté pour éviter les erreurs de colonnes manquantes
                // entity.Property(e => e.Code).HasMaxLength(10);
                // entity.Property(e => e.Responsable).HasMaxLength(100);
                // entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.ModifiedBy).HasMaxLength(100);
                entity.Property(e => e.Supprimer).HasDefaultValue(0);

                // Relations (temporairement commenté)
                /*
                entity.HasOne(e => e.Direction)
                    .WithMany(d => d.Departements)
                    .HasForeignKey(e => e.DirectionId)
                    .OnDelete(DeleteBehavior.Restrict);
                */
            });

            // Configuration Service (temporairement commenté)
            /*
            modelBuilder.Entity<Service>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nom).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Code).HasMaxLength(10);
                entity.Property(e => e.Responsable).HasMaxLength(100);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.ModifiedBy).HasMaxLength(100);
                entity.Property(e => e.Supprimer).HasDefaultValue(0);

                // Relations
                entity.HasOne(e => e.Departement)
                    .WithMany(d => d.Services)
                    .HasForeignKey(e => e.DepartementId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            */

            // Configuration ConfigurationCommande
            modelBuilder.Entity<ConfigurationCommande>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Cle).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Valeur).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.ModifiedBy).HasMaxLength(100);
                entity.Property(e => e.Supprimer).HasDefaultValue(0);
                
                // Index unique sur la clé
                entity.HasIndex(e => e.Cle).IsUnique();
            });

            // Configuration QuotaJournalier
            modelBuilder.Entity<QuotaJournalier>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.ModifiedBy).HasMaxLength(100);
                entity.Property(e => e.Supprimer).HasDefaultValue(0);
                entity.Property(e => e.Commentaires).HasMaxLength(500);
                
                // Relations
                entity.HasOne(e => e.GroupeNonCit)
                    .WithMany()
                    .HasForeignKey(e => e.GroupeNonCitId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                // Index unique sur GroupeNonCitId + Date
                entity.HasIndex(e => new { e.GroupeNonCitId, e.Date }).IsUnique();
            });
        }
    }
}
