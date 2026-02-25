using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Obeli_K.Data;
using Obeli_K.Models;
using Obeli_K.Models.ViewModels;

namespace Obeli_K.Controllers
{
    /// <summary>
    /// Contrôleur pour la gestion des marges jour et nuit par les RH et Administrateurs
    /// </summary>
    [Authorize(Roles = "Administrateur,RH")]
    public class GestionMargesController : Controller
    {
        private readonly ObeliDbContext _context;
        private readonly ILogger<GestionMargesController> _logger;

        public GestionMargesController(ObeliDbContext context, ILogger<GestionMargesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Affiche la page de gestion des marges avec sélection de période
        /// </summary>
        [HttpGet]
        public IActionResult Index(DateTime? dateDebut, DateTime? dateFin)
        {
            // Par défaut, afficher la semaine suivante
            var aujourdhui = DateTime.Today;
            var joursJusquaLundi = ((int)aujourdhui.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            var lundiProchain = aujourdhui.AddDays(joursJusquaLundi == 0 ? 7 : 7 - joursJusquaLundi);
            var dimancheProchain = lundiProchain.AddDays(6);

            var model = new GestionMargesViewModel
            {
                DateDebut = dateDebut ?? lundiProchain,
                DateFin = dateFin ?? dimancheProchain
            };

            return View(model);
        }

        /// <summary>
        /// Charge les formules pour la période sélectionnée
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChargerFormules(GestionMargesViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            try
            {
                // Récupérer toutes les formules de la période
                var formules = await _context.FormulesJour
                    .Where(f => f.Date.Date >= model.DateDebut.Date &&
                               f.Date.Date <= model.DateFin.Date &&
                               f.Supprimer == 0)
                    .OrderBy(f => f.Date)
                    .ThenBy(f => f.NomFormule)
                    .ToListAsync();

                if (!formules.Any())
                {
                    TempData["InfoMessage"] = "Aucune formule trouvée pour la période sélectionnée.";
                    return View("Index", model);
                }

                // Convertir en ViewModel
                model.Formules = formules.Select(f => new FormuleMargeViewModel
                {
                    IdFormule = f.IdFormule,
                    Date = f.Date,
                    NomFormule = f.NomFormule ?? "N/A",
                    Plat = GetNomPlatFromFormule(f),
                    MargeJourRestante = f.MargeJourRestante ?? 0,
                    MargeNuitRestante = f.MargeNuitRestante ?? 0,
                    QuotaJourRestant = f.QuotaJourRestant,
                    QuotaNuitRestant = f.QuotaNuitRestant
                }).ToList();

                return View("Index", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement des formules");
                TempData["ErrorMessage"] = "Une erreur est survenue lors du chargement des formules.";
                return View("Index", model);
            }
        }

        /// <summary>
        /// Sauvegarde les marges jour et nuit modifiées
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SauvegarderMarges(GestionMargesViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            try
            {
                int margesModifiees = 0;

                foreach (var formuleVM in model.Formules)
                {
                    var formule = await _context.FormulesJour
                        .FirstOrDefaultAsync(f => f.IdFormule == formuleVM.IdFormule && f.Supprimer == 0);

                    if (formule != null)
                    {
                        // Vérifier si les valeurs ont changé
                        bool aChange = false;

                        if (formule.MargeJourRestante != formuleVM.MargeJourRestante)
                        {
                            formule.MargeJourRestante = formuleVM.MargeJourRestante;
                            aChange = true;
                        }

                        if (formule.MargeNuitRestante != formuleVM.MargeNuitRestante)
                        {
                            formule.MargeNuitRestante = formuleVM.MargeNuitRestante;
                            aChange = true;
                        }

                        if (aChange)
                        {
                            formule.ModifiedOn = DateTime.UtcNow;
                            formule.ModifiedBy = User.Identity?.Name ?? "System";
                            margesModifiees++;

                            _logger.LogInformation(
                                "Marge mise à jour pour formule {IdFormule} ({NomFormule}) du {Date}: " +
                                "MargeJourRestante = {MargeJour}, MargeNuitRestante = {MargeNuit}",
                                formule.IdFormule, formule.NomFormule, formule.Date.ToString("dd/MM/yyyy"),
                                formule.MargeJourRestante, formule.MargeNuitRestante);
                        }
                    }
                }

                if (margesModifiees > 0)
                {
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Marges sauvegardées avec succès ! {margesModifiees} formule(s) modifiée(s).";
                    _logger.LogInformation("Marges sauvegardées avec succès pour la période du {DateDebut} au {DateFin}. {Count} formules modifiées.",
                        model.DateDebut.ToString("dd/MM/yyyy"), model.DateFin.ToString("dd/MM/yyyy"), margesModifiees);
                }
                else
                {
                    TempData["InfoMessage"] = "Aucune modification détectée.";
                }

                // Recharger les formules pour afficher les valeurs sauvegardées
                return await ChargerFormules(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la sauvegarde des marges");
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la sauvegarde des marges.";
                return View("Index", model);
            }
        }

        /// <summary>
        /// Obtient le nom du plat à partir de la formule
        /// </summary>
        private string GetNomPlatFromFormule(FormuleJour formule)
        {
            if (formule == null) return "";

            var nomFormule = formule.NomFormule?.ToLower() ?? "";

            switch (nomFormule)
            {
                case "amélioré":
                case "ameliore":
                case "formule améliorée":
                    return !string.IsNullOrEmpty(formule.Plat) ? formule.Plat : "Plat amélioré";

                case "standard 1":
                case "standard1":
                    return !string.IsNullOrEmpty(formule.PlatStandard1) ? formule.PlatStandard1 : "Plat Standard 1";

                case "standard 2":
                case "standard2":
                    return !string.IsNullOrEmpty(formule.PlatStandard2) ? formule.PlatStandard2 : "Plat Standard 2";

                default:
                    if (!string.IsNullOrEmpty(formule.Plat)) return formule.Plat;
                    if (!string.IsNullOrEmpty(formule.PlatStandard1)) return formule.PlatStandard1;
                    if (!string.IsNullOrEmpty(formule.PlatStandard2)) return formule.PlatStandard2;
                    return "Plat du jour";
            }
        }
    }
}

