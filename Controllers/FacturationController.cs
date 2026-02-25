using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Obeli_K.Services;
using Obeli_K.Models.ViewModels;

namespace Obeli_K.Controllers
{
    [Authorize(Roles = "Administrateur,RH")]
    public class FacturationController : Controller
    {
        private readonly IFacturationService _facturationService;
        private readonly ILogger<FacturationController> _logger;

        public FacturationController(
            IFacturationService facturationService,
            ILogger<FacturationController> logger)
        {
            _facturationService = facturationService;
            _logger = logger;
        }

        // GET: Facturation
        public async Task<IActionResult> Index(DateTime? dateDebut, DateTime? dateFin)
        {
            try
            {
                // Par défaut, regarder le mois en cours
                if (!dateDebut.HasValue)
                    dateDebut = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                
                if (!dateFin.HasValue)
                    dateFin = DateTime.Now.Date;

                _logger.LogInformation("📊 Consultation facturation: {Debut} à {Fin}", dateDebut, dateFin);

                // Récupérer les commandes non consommées
                var commandesNonConsommees = await _facturationService.GetCommandesNonConsommeesAsync(dateDebut, dateFin);

                // Calculer la facturation
                var resultatFacturation = await _facturationService.CalculerFacturationAsync(commandesNonConsommees);

                ViewBag.DateDebut = dateDebut.Value.ToString("yyyy-MM-dd");
                ViewBag.DateFin = dateFin.Value.ToString("yyyy-MM-dd");
                ViewBag.ResultatFacturation = resultatFacturation;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement de la page de facturation");
                TempData["ErrorMessage"] = "Erreur lors du chargement des données de facturation.";
                return View();
            }
        }

        // POST: Facturation/Appliquer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Appliquer(DateTime? dateDebut, DateTime? dateFin)
        {
            try
            {
                if (!dateDebut.HasValue || !dateFin.HasValue)
                {
                    TempData["ErrorMessage"] = "Veuillez spécifier une période.";
                    return RedirectToAction(nameof(Index));
                }

                _logger.LogInformation("💰 Application de la facturation pour la période: {Debut} à {Fin}", dateDebut, dateFin);

                // Récupérer les commandes non consommées
                var commandesNonConsommees = await _facturationService.GetCommandesNonConsommeesAsync(dateDebut, dateFin);

                if (!commandesNonConsommees.Any())
                {
                    TempData["InfoMessage"] = "Aucune commande non consommée trouvée pour cette période.";
                    return RedirectToAction(nameof(Index));
                }

                // Calculer la facturation
                var resultatFacturation = await _facturationService.CalculerFacturationAsync(commandesNonConsommees);

                if (!resultatFacturation.FacturationActive)
                {
                    TempData["WarningMessage"] = "La facturation est désactivée. Activez-la dans les paramètres de facturation.";
                    return RedirectToAction(nameof(Index));
                }

                // Appliquer la facturation
                var success = await _facturationService.AppliquerFacturationAsync(commandesNonConsommees, resultatFacturation);

                if (success)
                {
                    TempData["SuccessMessage"] = $"Facturation appliquée avec succès ! {resultatFacturation.NombreCommandesFacturables} commandes facturées pour un montant total de {resultatFacturation.MontantTotalAFacturer:C}.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Erreur lors de l'application de la facturation.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'application de la facturation");
                TempData["ErrorMessage"] = "Erreur lors de l'application de la facturation.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Facturation/Details/{id}
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                // Récupérer une commande spécifique
                var commandes = await _facturationService.GetCommandesNonConsommeesAsync();
                var commande = commandes.FirstOrDefault(c => c.IdCommande == id);

                if (commande == null)
                {
                    TempData["ErrorMessage"] = "Commande non trouvée.";
                    return RedirectToAction(nameof(Index));
                }

                // Calculer la facturation pour cette commande
                var resultat = await _facturationService.CalculerFacturationAsync(new List<CommandeNonConsommeeViewModel> { commande });

                ViewBag.Commande = commande;
                ViewBag.ResultatFacturation = resultat;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement des détails de la commande {Id}", id);
                TempData["ErrorMessage"] = "Erreur lors du chargement des détails.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
