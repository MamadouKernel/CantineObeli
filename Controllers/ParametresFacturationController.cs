using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Obeli_K.Services.Configuration;

namespace Obeli_K.Controllers
{
    [Authorize(Roles = "Administrateur,RH")]
    public class ParametresFacturationController : Controller
    {
        private readonly IConfigurationService _configService;
        private readonly ILogger<ParametresFacturationController> _logger;

        public ParametresFacturationController(
            IConfigurationService configService,
            ILogger<ParametresFacturationController> logger)
        {
            _configService = configService;
            _logger = logger;
        }

        // GET: ParametresFacturation
        public async Task<IActionResult> Index()
        {
            try
            {
                // Récupérer les paramètres actuels
                var facturationActive = await _configService.GetConfigurationAsync("FACTURATION_NON_CONSOMMEES_ACTIVE");
                var pourcentageFacturation = await _configService.GetConfigurationAsync("FACTURATION_POURCENTAGE");
                var nombreAbsencesGratuites = await _configService.GetConfigurationAsync("FACTURATION_ABSENCES_GRATUITES");
                var delaiAnnulationGratuite = await _configService.GetConfigurationAsync("FACTURATION_DELAI_ANNULATION_GRATUITE");
                var facturationWeekend = await _configService.GetConfigurationAsync("FACTURATION_WEEKEND");
                var facturationJoursFeries = await _configService.GetConfigurationAsync("FACTURATION_JOURS_FERIES");

                ViewBag.FacturationActive = !string.IsNullOrEmpty(facturationActive) && facturationActive.ToLower() == "true";
                ViewBag.PourcentageFacturation = !string.IsNullOrEmpty(pourcentageFacturation) ? int.Parse(pourcentageFacturation) : 100;
                ViewBag.NombreAbsencesGratuites = !string.IsNullOrEmpty(nombreAbsencesGratuites) ? int.Parse(nombreAbsencesGratuites) : 0;
                ViewBag.DelaiAnnulationGratuite = !string.IsNullOrEmpty(delaiAnnulationGratuite) ? int.Parse(delaiAnnulationGratuite) : 24;
                ViewBag.FacturationWeekend = !string.IsNullOrEmpty(facturationWeekend) && facturationWeekend.ToLower() == "true";
                ViewBag.FacturationJoursFeries = !string.IsNullOrEmpty(facturationJoursFeries) && facturationJoursFeries.ToLower() == "true";

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement des paramètres de facturation");
                TempData["ErrorMessage"] = "Erreur lors du chargement des paramètres.";
                return View();
            }
        }

        // POST: ParametresFacturation/Update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(
            bool facturationActive,
            int pourcentageFacturation,
            int nombreAbsencesGratuites,
            int delaiAnnulationGratuite,
            bool facturationWeekend,
            bool facturationJoursFeries)
        {
            try
            {
                _logger.LogInformation("🔍 Réception des paramètres de facturation:");
                _logger.LogInformation("   - FacturationActive: {Active}", facturationActive);
                _logger.LogInformation("   - PourcentageFacturation: {Pourcentage}", pourcentageFacturation);
                _logger.LogInformation("   - NombreAbsencesGratuites: {Nombre}", nombreAbsencesGratuites);
                _logger.LogInformation("   - DelaiAnnulationGratuite: {Delai}", delaiAnnulationGratuite);
                _logger.LogInformation("   - FacturationWeekend: {Weekend}", facturationWeekend);
                _logger.LogInformation("   - FacturationJoursFeries: {Feries}", facturationJoursFeries);

                // Valider les entrées
                if (pourcentageFacturation < 0 || pourcentageFacturation > 100)
                {
                    TempData["ErrorMessage"] = "Le pourcentage de facturation doit être entre 0 et 100.";
                    return RedirectToAction(nameof(Index));
                }

                if (nombreAbsencesGratuites < 0)
                {
                    TempData["ErrorMessage"] = "Le nombre d'absences gratuites ne peut pas être négatif.";
                    return RedirectToAction(nameof(Index));
                }

                if (delaiAnnulationGratuite < 0)
                {
                    TempData["ErrorMessage"] = "Le délai d'annulation gratuite ne peut pas être négatif.";
                    return RedirectToAction(nameof(Index));
                }

                // Mettre à jour les configurations
                _logger.LogInformation("💾 Sauvegarde des paramètres...");
                
                await _configService.SetConfigurationAsync(
                    "FACTURATION_NON_CONSOMMEES_ACTIVE",
                    facturationActive.ToString().ToLower(),
                    "Active ou désactive la facturation des commandes non consommées");
                _logger.LogInformation("✅ FACTURATION_NON_CONSOMMEES_ACTIVE = {Value}", facturationActive.ToString().ToLower());

                await _configService.SetConfigurationAsync(
                    "FACTURATION_POURCENTAGE",
                    pourcentageFacturation.ToString(),
                    "Pourcentage du prix de la commande à facturer (0-100%)");
                _logger.LogInformation("✅ FACTURATION_POURCENTAGE = {Value}", pourcentageFacturation);

                await _configService.SetConfigurationAsync(
                    "FACTURATION_ABSENCES_GRATUITES",
                    nombreAbsencesGratuites.ToString(),
                    "Nombre d'absences non consommées gratuites par mois");
                _logger.LogInformation("✅ FACTURATION_ABSENCES_GRATUITES = {Value}", nombreAbsencesGratuites);

                await _configService.SetConfigurationAsync(
                    "FACTURATION_DELAI_ANNULATION_GRATUITE",
                    delaiAnnulationGratuite.ToString(),
                    "Délai en heures avant la consommation pour annuler gratuitement");
                _logger.LogInformation("✅ FACTURATION_DELAI_ANNULATION_GRATUITE = {Value}", delaiAnnulationGratuite);

                await _configService.SetConfigurationAsync(
                    "FACTURATION_WEEKEND",
                    facturationWeekend.ToString().ToLower(),
                    "Facturer les commandes non consommées le weekend");
                _logger.LogInformation("✅ FACTURATION_WEEKEND = {Value}", facturationWeekend.ToString().ToLower());

                await _configService.SetConfigurationAsync(
                    "FACTURATION_JOURS_FERIES",
                    facturationJoursFeries.ToString().ToLower(),
                    "Facturer les commandes non consommées les jours fériés");
                _logger.LogInformation("✅ FACTURATION_JOURS_FERIES = {Value}", facturationJoursFeries.ToString().ToLower());

                _logger.LogInformation("✅ Paramètres de facturation mis à jour avec succès par {User}", User.Identity?.Name ?? "Inconnu");
                TempData["SuccessMessage"] = "Paramètres de facturation mis à jour avec succès.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour des paramètres de facturation");
                TempData["ErrorMessage"] = "Erreur lors de la mise à jour des paramètres.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}

