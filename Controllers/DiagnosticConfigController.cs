using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Obeli_K.Data;

namespace Obeli_K.Controllers
{
    [Authorize(Roles = "Administrateur")]
    public class DiagnosticConfigController : Controller
    {
        private readonly ObeliDbContext _context;
        private readonly ILogger<DiagnosticConfigController> _logger;

        public DiagnosticConfigController(ObeliDbContext context, ILogger<DiagnosticConfigController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("🔍 Diagnostic des configurations de facturation...");

                // Récupérer toutes les configurations de facturation
                var facturationConfigs = await _context.ConfigurationsCommande
                    .Where(c => c.Cle.Contains("FACTURATION") && c.Supprimer == 0)
                    .ToListAsync();

                var result = new List<object>();

                if (!facturationConfigs.Any())
                {
                    result.Add(new { Cle = "AUCUNE CONFIGURATION TROUVÉE", Valeur = "❌", Description = "Les configurations de facturation n'existent pas dans la base de données" });
                }
                else
                {
                    foreach (var config in facturationConfigs)
                    {
                        result.Add(new
                        {
                            Cle = config.Cle,
                            Valeur = config.Valeur,
                            Description = config.Description,
                            CreatedOn = config.CreatedOn,
                            ModifiedOn = config.ModifiedOn,
                            CreatedBy = config.CreatedBy,
                            ModifiedBy = config.ModifiedBy
                        });
                    }
                }

                ViewBag.Configurations = result;
                ViewBag.TotalCount = facturationConfigs.Count;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du diagnostic des configurations");
                ViewBag.Error = ex.Message;
                return View();
            }
        }

        public async Task<IActionResult> TestSave()
        {
            try
            {
                _logger.LogInformation("🧪 Test de sauvegarde d'une configuration...");

                // Tester la sauvegarde d'une configuration de test
                var testConfig = new Models.ConfigurationCommande
                {
                    Id = Guid.NewGuid(),
                    Cle = "TEST_SAVE_CONFIG",
                    Valeur = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Description = "Test de sauvegarde",
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = "Test",
                    Supprimer = 0
                };

                _context.ConfigurationsCommande.Add(testConfig);
                await _context.SaveChangesAsync();

                _logger.LogInformation("✅ Test de sauvegarde réussi !");
                TempData["SuccessMessage"] = "Test de sauvegarde réussi !";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Test de sauvegarde échoué");
                TempData["ErrorMessage"] = $"Test de sauvegarde échoué : {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
