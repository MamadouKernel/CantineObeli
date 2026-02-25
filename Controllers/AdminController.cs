using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Obeli_K.Data;
using Obeli_K.Models;

namespace Obeli_K.Controllers
{
    /// <summary>
    /// Contrôleur d'administration pour la gestion de la base de données.
    /// Permet le nettoyage de la base de données et l'affichage des statistiques.
    /// Accessible uniquement aux administrateurs.
    /// </summary>
    [Authorize(Roles = "Administrateur")]
    public class AdminController : Controller
    {
        private readonly ObeliDbContext _context;
        private readonly ILogger<AdminController> _logger;

        /// <summary>
        /// Initialise une nouvelle instance du contrôleur d'administration.
        /// </summary>
        /// <param name="context">Contexte de base de données Obeli</param>
        /// <param name="logger">Service de journalisation</param>
        public AdminController(ObeliDbContext context, ILogger<AdminController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Affiche la page d'administration
        /// </summary>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Vide complètement la base de données en gardant seulement les utilisateurs admin
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NettoyerBaseDonnees()
        {
            try
            {
                _logger.LogWarning("🗑️ Début du nettoyage complet de la base de données");

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // 1. Supprimer toutes les données des tables principales (dans l'ordre pour respecter les contraintes FK)
                    await _context.PointsConsommation.ExecuteDeleteAsync();
                    _logger.LogInformation("✅ Points de consommation supprimés");

                    await _context.ExportCommandesPrestataire.ExecuteDeleteAsync();
                    _logger.LogInformation("✅ Exports commandes prestataire supprimés");

                    await _context.Commandes.ExecuteDeleteAsync();
                    _logger.LogInformation("✅ Commandes supprimées");

                    await _context.QuotasJournaliers.ExecuteDeleteAsync();
                    _logger.LogInformation("✅ Quotas journaliers supprimés");

                    await _context.ConfigurationsCommande.ExecuteDeleteAsync();
                    _logger.LogInformation("✅ Configurations commande supprimées");

                    await _context.FormulesJour.ExecuteDeleteAsync();
                    _logger.LogInformation("✅ Formules jour supprimées");

                    await _context.PrestataireCantines.ExecuteDeleteAsync();
                    _logger.LogInformation("✅ Prestataires cantine supprimés");

                    await _context.GroupesNonCit.ExecuteDeleteAsync();
                    _logger.LogInformation("✅ Groupes non CIT supprimés");

                    // 2. Garder seulement les utilisateurs admin
                    var utilisateursNonAdmin = await _context.Utilisateurs
                        .Where(u => u.Role != Models.Enums.RoleType.Admin)
                        .ToListAsync();

                    foreach (var utilisateur in utilisateursNonAdmin)
                    {
                        utilisateur.Supprimer = 1; // Soft delete
                    }

                    await _context.SaveChangesAsync();
                    _logger.LogInformation("✅ Utilisateurs non-admin supprimés (soft delete)");

                    // 3. Garder les données de référence (Directions, directions, Fonctions, Types de formule)
                    // Ces données sont nécessaires au fonctionnement de l'application

                    await transaction.CommitAsync();
                    _logger.LogInformation("✅ Transaction commitée avec succès");

                    TempData["SuccessMessage"] = "Base de données nettoyée avec succès ! Toutes les données ont été supprimées sauf les comptes administrateurs et les données de référence.";
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "❌ Erreur lors du nettoyage - Transaction annulée");
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors du nettoyage de la base de données");
                TempData["ErrorMessage"] = "Erreur lors du nettoyage : " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Supprime définitivement tous les utilisateurs (même les admin) - DANGEREUX !
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SupprimerTousUtilisateurs()
        {
            try
            {
                _logger.LogWarning("🚨 SUPPRESSION DE TOUS LES UTILISATEURS - OPÉRATION DANGEREUSE");

                // Soft delete de tous les utilisateurs
                var tousUtilisateurs = await _context.Utilisateurs.ToListAsync();
                foreach (var utilisateur in tousUtilisateurs)
                {
                    utilisateur.Supprimer = 1;
                }

                await _context.SaveChangesAsync();
                _logger.LogWarning("⚠️ Tous les utilisateurs ont été supprimés (soft delete)");

                TempData["WarningMessage"] = "ATTENTION : Tous les utilisateurs ont été supprimés ! Vous devrez recréer un compte admin pour accéder au système.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors de la suppression des utilisateurs");
                TempData["ErrorMessage"] = "Erreur lors de la suppression : " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Affiche les statistiques de la base de données
        /// </summary>
        public async Task<IActionResult> Statistiques()
        {
            try
            {
                var stats = new
                {
                    Utilisateurs = await _context.Utilisateurs.Where(u => u.Supprimer == 0).CountAsync(),
                    Commandes = await _context.Commandes.Where(c => c.Supprimer == 0).CountAsync(),
                    PointsConsommation = await _context.PointsConsommation.Where(pc => pc.Supprimer == 0).CountAsync(),
                    FormulesJour = await _context.FormulesJour.CountAsync(),
                    Prestataires = await _context.PrestataireCantines.CountAsync(),
                    directions = await _context.Directions.CountAsync(),
                    Fonctions = await _context.Fonctions.CountAsync(),
                    TypesFormule = await _context.TypesFormule.CountAsync(),
                    GroupesNonCit = await _context.GroupesNonCit.CountAsync(),
                    Configurations = await _context.ConfigurationsCommande.CountAsync()
                };

                return Json(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors de la récupération des statistiques");
                return Json(new { error = ex.Message });
            }
        }
    }
}
