using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Obeli_K.Services.Configuration;

namespace Obeli_K.Controllers
{
    [Authorize(Roles = "Administrateur")]
    public class InitConfigController : Controller
    {
        private readonly IConfigurationService _configService;
        private readonly ILogger<InitConfigController> _logger;

        public InitConfigController(
            IConfigurationService configService,
            ILogger<InitConfigController> logger)
        {
            _configService = configService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("🚀 Initialisation des configurations de facturation...");
                await _configService.InitializeBillingConfigurationsAsync();
                
                TempData["SuccessMessage"] = "Configurations de facturation initialisées avec succès !";
                return RedirectToAction("Index", "ParametresFacturation");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'initialisation des configurations");
                TempData["ErrorMessage"] = "Erreur lors de l'initialisation des configurations.";
                return RedirectToAction("Index", "ParametresFacturation");
            }
        }
    }
}
