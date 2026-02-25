using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Obeli_K.Data;
using Obeli_K.Models;
using Obeli_K.Services;
using Obeli_K.Services.Configuration;

namespace Obeli_K.Controllers
{
    [Authorize(Roles = "Administrateur,RH")]
    public class ConfigurationCommandeController : Controller
    {
        private readonly IConfigurationService _configurationService;
        private readonly ICommandeAutomatiqueService _commandeAutomatiqueService;
        private readonly ILogger<ConfigurationCommandeController> _logger;

        public ConfigurationCommandeController(
            IConfigurationService configurationService,
            ICommandeAutomatiqueService commandeAutomatiqueService,
            ILogger<ConfigurationCommandeController> logger)
        {
            _configurationService = configurationService;
            _commandeAutomatiqueService = commandeAutomatiqueService;
            _logger = logger;
        }

        /// <summary>
        /// Affiche la page de configuration des commandes
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var jourCloture = await _configurationService.GetConfigurationAsync("COMMANDE_JOUR_CLOTURE") ?? "Friday";
                var heureCloture = await _configurationService.GetConfigurationAsync("COMMANDE_HEURE_CLOTURE") ?? "12:00";
                var autoConfirm = await _configurationService.GetConfigurationAsync("COMMANDE_AUTO_CONFIRMATION") ?? "true";
                
                var prochaineCloture = await _configurationService.GetNextBlockingDateAsync();
                var isBlocked = await _configurationService.IsCommandeBlockedAsync();

                ViewBag.JourCloture = jourCloture;
                ViewBag.HeureCloture = heureCloture;
                ViewBag.AutoConfirm = autoConfirm;
                ViewBag.ProchaineCloture = prochaineCloture;
                ViewBag.IsBlocked = isBlocked;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement de la configuration");
                TempData["ErrorMessage"] = "Une erreur est survenue lors du chargement de la configuration.";
                return View();
            }
        }

        /// <summary>
        /// Met à jour la configuration des commandes
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateConfiguration(
            string jourCloture, 
            string heureCloture, 
            bool autoConfirm)
        {
            try
            {
                // Validation des données
                var joursValides = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
                if (!joursValides.Contains(jourCloture))
                {
                    TempData["ErrorMessage"] = "Jour de clôture invalide.";
                    return RedirectToAction(nameof(Index));
                }

                if (!TimeSpan.TryParse(heureCloture, out _))
                {
                    TempData["ErrorMessage"] = "Format d'heure invalide. Utilisez le format HH:mm.";
                    return RedirectToAction(nameof(Index));
                }

                // Mise à jour des configurations
                await _configurationService.SetConfigurationAsync(
                    "COMMANDE_JOUR_CLOTURE", 
                    jourCloture, 
                    "Jour de la semaine pour la clôture des commandes");

                await _configurationService.SetConfigurationAsync(
                    "COMMANDE_HEURE_CLOTURE", 
                    heureCloture, 
                    "Heure de clôture des commandes");

                await _configurationService.SetConfigurationAsync(
                    "COMMANDE_AUTO_CONFIRMATION", 
                    autoConfirm.ToString().ToLower(), 
                    "Activer la confirmation automatique des commandes");

                _logger.LogInformation("Configuration mise à jour par {User}: Jour={Jour}, Heure={Heure}, AutoConfirm={AutoConfirm}", 
                    User.Identity?.Name, jourCloture, heureCloture, autoConfirm);

                TempData["SuccessMessage"] = "Configuration mise à jour avec succès.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour de la configuration");
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la mise à jour de la configuration.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Teste le blocage des commandes
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TestBlocage()
        {
            try
            {
                var isBlocked = await _configurationService.IsCommandeBlockedAsync();
                var prochaineCloture = await _configurationService.GetNextBlockingDateAsync();
                
                if (isBlocked)
                {
                    TempData["InfoMessage"] = $"Les commandes sont actuellement BLOQUÉES. Prochaine clôture: {prochaineCloture:dd/MM/yyyy HH:mm}";
                }
                else
                {
                    TempData["InfoMessage"] = $"Les commandes sont actuellement AUTORISÉES. Prochaine clôture: {prochaineCloture:dd/MM/yyyy HH:mm}";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du test de blocage");
                TempData["ErrorMessage"] = "Une erreur est survenue lors du test de blocage.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Force la confirmation automatique des commandes
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForcerConfirmation()
        {
            try
            {
                var success = await _commandeAutomatiqueService.ConfirmerCommandesAutomatiquementAsync();
                
                if (success)
                {
                    TempData["SuccessMessage"] = "Confirmation automatique des commandes exécutée avec succès.";
                }
                else
                {
                    TempData["InfoMessage"] = "Aucune commande à confirmer ou conditions non remplies.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la confirmation forcée");
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la confirmation automatique.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Réinitialise les configurations par défaut
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetConfiguration()
        {
            try
            {
                await _commandeAutomatiqueService.InitialiserConfigurationsParDefautAsync();
                
                _logger.LogInformation("Configuration réinitialisée par {User}", User.Identity?.Name);
                TempData["SuccessMessage"] = "Configuration réinitialisée aux valeurs par défaut.";
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la réinitialisation de la configuration");
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la réinitialisation.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
