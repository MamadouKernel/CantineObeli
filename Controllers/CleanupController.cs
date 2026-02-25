using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Obeli_K.Data;
using Obeli_K.Models;

namespace Obeli_K.Controllers
{
    [Authorize]
    public class CleanupController : Controller
    {
        private readonly ObeliDbContext _context;
        private readonly ILogger<CleanupController> _logger;

        public CleanupController(ObeliDbContext context, ILogger<CleanupController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> CleanupDuplicateInstantOrders()
        {
            try
            {
                _logger.LogInformation("Début du nettoyage des commandes instantanées en doublon");

                // Récupérer toutes les commandes instantanées non supprimées pour aujourd'hui et les jours suivants
                var today = DateTime.Today;
                var duplicateCommands = await _context.Commandes
                    .Include(c => c.Utilisateur)
                    .Where(c => c.Instantanee == true 
                        && c.Supprimer == 0 
                        && c.DateConsommation.HasValue && c.DateConsommation.Value.Date >= today)
                    .ToListAsync();

                // Grouper par utilisateur et date de consommation
                var groupedCommands = duplicateCommands
                    .GroupBy(c => new { c.UtilisateurId, DateConsommation = c.DateConsommation.Value.Date })
                    .Where(g => g.Count() > 1)
                    .ToList();

                var commandsToDelete = new List<Commande>();
                var cleanupReport = new List<object>();

                foreach (var group in groupedCommands)
                {
                    // Trier par date de création (plus récente en premier)
                    var sortedCommands = group.OrderByDescending(c => c.Date).ToList();
                    
                    // Garder la première (plus récente) et marquer les autres comme supprimées
                    var commandsToMarkAsDeleted = sortedCommands.Skip(1).ToList();
                    
                    foreach (var cmd in commandsToMarkAsDeleted)
                    {
                        cmd.Supprimer = 1;
                        cmd.ModifiedOn = DateTime.Now;
                        cmd.ModifiedBy = "SYSTEM_CLEANUP";
                        commandsToDelete.Add(cmd);
                        
                        cleanupReport.Add(new
                        {
                            UtilisateurId = cmd.UtilisateurId,
                            UserName = cmd.Utilisateur?.UserName ?? "N/A",
                            NomComplet = $"{cmd.Utilisateur?.Nom} {cmd.Utilisateur?.Prenoms}",
                            DateConsommation = cmd.DateConsommation?.ToString("yyyy-MM-dd") ?? "N/A",
                            IdCommande = cmd.IdCommande,
                            DateCreation = cmd.Date.ToString("yyyy-MM-dd HH:mm:ss"),
                            Action = "SUPPRIMÉ"
                        });
                    }
                }

                if (commandsToDelete.Any())
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Nettoyage terminé: {Count} commandes en doublon supprimées", commandsToDelete.Count);
                }

                // Récupérer les statistiques après nettoyage
                var remainingCommands = await _context.Commandes
                    .Include(c => c.Utilisateur)
                    .Where(c => c.Instantanee == true 
                        && c.Supprimer == 0 
                        && c.DateConsommation.HasValue && c.DateConsommation.Value.Date >= today)
                    .ToListAsync();

                var userStats = remainingCommands
                    .GroupBy(c => c.UtilisateurId)
                    .Select(g => new
                    {
                        UserId = g.Key,
                        UserName = g.First().Utilisateur?.UserName ?? "N/A",
                        NomComplet = $"{g.First().Utilisateur?.Nom} {g.First().Utilisateur?.Prenoms}",
                        Count = g.Count()
                    })
                    .OrderByDescending(s => s.Count)
                    .ToList();

                var result = new
                {
                    Success = true,
                    Message = $"Nettoyage terminé avec succès",
                    CommandsDeleted = commandsToDelete.Count,
                    CommandsRemaining = remainingCommands.Count,
                    CleanupReport = cleanupReport,
                    UserStatistics = userStats
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du nettoyage des commandes en doublon");
                return Json(new { Success = false, Message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ViewDuplicateInstantOrders()
        {
            try
            {
                var today = DateTime.Today;
                var commands = await _context.Commandes
                    .Include(c => c.Utilisateur)
                    .Where(c => c.Instantanee == true 
                        && c.Supprimer == 0 
                        && c.DateConsommation.HasValue && c.DateConsommation.Value.Date >= today)
                    .OrderBy(c => c.UtilisateurId)
                    .ThenBy(c => c.DateConsommation)
                    .ThenByDescending(c => c.Date)
                    .ToListAsync();

                var groupedCommands = commands
                    .GroupBy(c => new { c.UtilisateurId, DateConsommation = c.DateConsommation.Value.Date })
                    .Where(g => g.Count() > 1)
                    .ToList();

                var duplicateReport = new List<object>();

                foreach (var group in groupedCommands)
                {
                    var user = group.First().Utilisateur;
                    var groupInfo = new
                    {
                        UtilisateurId = group.Key.UtilisateurId,
                        UserName = user?.UserName ?? "N/A",
                        NomComplet = $"{user?.Nom} {user?.Prenoms}",
                        DateConsommation = group.Key.DateConsommation.ToString("yyyy-MM-dd"),
                        NombreCommandes = group.Count(),
                        Commandes = group.Select(c => new
                        {
                            IdCommande = c.IdCommande,
                            DateCreation = c.Date.ToString("yyyy-MM-dd HH:mm:ss"),
                            StatusCommande = c.StatusCommande
                        }).ToList()
                    };
                    duplicateReport.Add(groupInfo);
                }

                return Json(new
                {
                    Success = true,
                    TotalCommands = commands.Count,
                    DuplicateGroups = groupedCommands.Count,
                    DuplicateReport = duplicateReport
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des commandes en doublon");
                return Json(new { Success = false, Message = ex.Message });
            }
        }
    }
}
