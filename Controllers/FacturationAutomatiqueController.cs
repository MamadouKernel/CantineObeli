using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Obeli_K.Services;

namespace Obeli_K.Controllers
{
    [Authorize(Roles = "Administrateur,RH")]
    public class FacturationAutomatiqueController : Controller
    {
        private readonly IFacturationService _facturationService;
        private readonly ILogger<FacturationAutomatiqueController> _logger;

        public FacturationAutomatiqueController(
            IFacturationService facturationService,
            ILogger<FacturationAutomatiqueController> logger)
        {
            _facturationService = facturationService;
            _logger = logger;
        }

        // GET: FacturationAutomatique/Index
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("🔍 Chargement de la page de facturation automatique");

                // Récupérer les commandes non consommées des 7 derniers jours
                var dateDebut = DateTime.Today.AddDays(-7);
                var commandesNonConsommees = await _facturationService.GetCommandesNonConsommeesAsync(dateDebut, null);

                // Calculer la facturation
                var resultatFacturation = await _facturationService.CalculerFacturationAsync(commandesNonConsommees);

                ViewBag.CommandesNonConsommees = commandesNonConsommees;
                ViewBag.ResultatFacturation = resultatFacturation;
                ViewBag.DateDebut = dateDebut;
                ViewBag.DateFin = DateTime.Today;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors du chargement de la page de facturation automatique");
                TempData["ErrorMessage"] = "Erreur lors du chargement de la page de facturation.";
                return View();
            }
        }

        // POST: FacturationAutomatique/Executer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Executer(DateTime? dateDebut, DateTime? dateFin)
        {
            try
            {
                _logger.LogInformation("💰 Exécution manuelle de la facturation automatique");

                // Utiliser les dates par défaut si non spécifiées
                var dateDebutEffective = dateDebut ?? DateTime.Today.AddDays(-7);
                var dateFinEffective = dateFin ?? DateTime.Today;

                _logger.LogInformation("📅 Période de facturation: {Debut} à {Fin}", 
                    dateDebutEffective.ToString("dd/MM/yyyy"), 
                    dateFinEffective.ToString("dd/MM/yyyy"));

                // Récupérer les commandes non consommées
                var commandesNonConsommees = await _facturationService.GetCommandesNonConsommeesAsync(dateDebutEffective, dateFinEffective);

                if (!commandesNonConsommees.Any())
                {
                    TempData["InfoMessage"] = "Aucune commande non consommée trouvée pour la période sélectionnée.";
                    return RedirectToAction(nameof(Index));
                }

                // Calculer la facturation
                var resultatFacturation = await _facturationService.CalculerFacturationAsync(commandesNonConsommees);

                if (!resultatFacturation.FacturationActive)
                {
                    TempData["WarningMessage"] = "La facturation est désactivée dans les paramètres. Activez-la dans Paramètres → Paramètres de Facturation.";
                    return RedirectToAction(nameof(Index));
                }

                // Appliquer la facturation
                var facturationAppliquee = await _facturationService.AppliquerFacturationAsync(commandesNonConsommees, resultatFacturation);

                if (facturationAppliquee)
                {
                    _logger.LogInformation("✅ Facturation manuelle appliquée avec succès");
                    TempData["SuccessMessage"] = $"Facturation appliquée avec succès: {resultatFacturation.NombreCommandesFacturables} commandes facturées, {resultatFacturation.NombreCommandesNonFacturables} exemptées, montant total: {resultatFacturation.MontantTotalAFacturer:C}";
                }
                else
                {
                    _logger.LogError("❌ Échec de l'application de la facturation manuelle");
                    TempData["ErrorMessage"] = "Échec de l'application de la facturation.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors de l'exécution manuelle de la facturation");
                TempData["ErrorMessage"] = "Erreur lors de l'exécution de la facturation.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: FacturationAutomatique/Historique
        public async Task<IActionResult> Historique()
        {
            try
            {
                _logger.LogInformation("📊 Chargement de l'historique de facturation");

                // Récupérer les commandes non consommées des 30 derniers jours
                var dateDebut = DateTime.Today.AddDays(-30);
                var commandesNonConsommees = await _facturationService.GetCommandesNonConsommeesAsync(dateDebut, null);

                // Calculer la facturation
                var resultatFacturation = await _facturationService.CalculerFacturationAsync(commandesNonConsommees);

                ViewBag.CommandesNonConsommees = commandesNonConsommees;
                ViewBag.ResultatFacturation = resultatFacturation;
                ViewBag.DateDebut = dateDebut;
                ViewBag.DateFin = DateTime.Today;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors du chargement de l'historique de facturation");
                TempData["ErrorMessage"] = "Erreur lors du chargement de l'historique de facturation.";
                return View();
            }
        }
    }
}
