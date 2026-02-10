using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Obeli_K.Data;
using Obeli_K.Models;
using Obeli_K.Models.ViewModels;
using System.Globalization;

namespace Obeli_K.Controllers
{
    [Authorize(Roles = "Administrateur,RessourcesHumaines,Prestataire")]
    public class FormuleJourController : Controller
    {
        private readonly ObeliDbContext _context;
        private readonly ILogger<FormuleJourController> _logger;

        public FormuleJourController(ObeliDbContext context, ILogger<FormuleJourController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Affiche la liste des formules
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(DateTime? dateDebut, DateTime? dateFin, bool semaineSuivante = false)
        {
            try
            {
                // Si semaine suivante demandée, calculer automatiquement les dates
                if (semaineSuivante)
                {
                    var aujourdhui = DateTime.Today;
                    // Calculer le lundi de la semaine suivante
                    var debutSemaineSuivante = aujourdhui.AddDays(7 - (int)aujourdhui.DayOfWeek + 1);
                    // Si on est déjà lundi, prendre le lundi suivant
                    if (aujourdhui.DayOfWeek == DayOfWeek.Monday)
                    {
                        debutSemaineSuivante = aujourdhui.AddDays(7);
                    }
                    var finSemaineSuivante = debutSemaineSuivante.AddDays(6); // Dimanche
                    
                    dateDebut = debutSemaineSuivante;
                    dateFin = finSemaineSuivante;
                }

                var query = _context.FormulesJour
                    .Include(f => f.NomFormuleNavigation)
                    .Where(f => f.Supprimer == 0);

                // Filtrer par dates si spécifiées
                if (dateDebut.HasValue)
                    query = query.Where(f => f.Date >= dateDebut.Value);
                
                if (dateFin.HasValue)
                    query = query.Where(f => f.Date <= dateFin.Value);

                var formules = await query
                    .OrderByDescending(f => f.Date)
                    .Select(f => new FormuleJourViewModel
                    {
                        IdFormule = f.IdFormule,
                        Date = f.Date,
                        NomFormule = f.NomFormule,
                        TypeFormuleId = f.TypeFormuleId,
                        TypeFormuleNom = f.NomFormuleNavigation!.Nom,
                        Entree = f.Entree,
                        Plat = f.Plat,
                        Garniture = f.Garniture,
                        Dessert = f.Dessert,
                        PlatStandard1 = f.PlatStandard1,
                        GarnitureStandard1 = f.GarnitureStandard1,
                        PlatStandard2 = f.PlatStandard2,
                        GarnitureStandard2 = f.GarnitureStandard2,
                        Feculent = f.Feculent,
                        Legumes = f.Legumes,
                        Marge = f.Marge,
                        Statut = f.Statut,
                        Verrouille = f.Verrouille,
                        CreatedOn = f.CreatedOn,
                        CreatedBy = f.CreatedBy,
                        ModifiedOn = f.ModifiedOn,
                        ModifiedBy = f.ModifiedBy
                    })
                    .ToListAsync();

                ViewBag.DateDebut = dateDebut?.ToString("yyyy-MM-dd");
                ViewBag.DateFin = dateFin?.ToString("yyyy-MM-dd");
                ViewBag.SemaineSuivante = semaineSuivante;

                return View(formules);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement des formules");
                TempData["ErrorMessage"] = "Une erreur est survenue lors du chargement des formules.";
                return View(new List<FormuleJourViewModel>());
            }
        }

        /// <summary>
        /// Affiche les détails d'une formule
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var formule = await _context.FormulesJour
                    .Include(f => f.NomFormuleNavigation)
                    .FirstOrDefaultAsync(f => f.IdFormule == id && f.Supprimer == 0);

                if (formule == null)
                {
                    TempData["ErrorMessage"] = "Formule non trouvée.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new FormuleJourViewModel
                {
                    IdFormule = formule.IdFormule,
                    Date = formule.Date,
                    NomFormule = formule.NomFormule,
                    TypeFormuleId = formule.TypeFormuleId,
                    TypeFormuleNom = formule.NomFormuleNavigation?.Nom,
                    Entree = formule.Entree,
                    Plat = formule.Plat,
                    Garniture = formule.Garniture,
                    Dessert = formule.Dessert,
                    PlatStandard1 = formule.PlatStandard1,
                    GarnitureStandard1 = formule.GarnitureStandard1,
                    PlatStandard2 = formule.PlatStandard2,
                    GarnitureStandard2 = formule.GarnitureStandard2,
                    Feculent = formule.Feculent,
                    Legumes = formule.Legumes,
                    Marge = formule.Marge,
                    Statut = formule.Statut,
                    Verrouille = formule.Verrouille,
                    Historique = formule.Historique,
                    CreatedOn = formule.CreatedOn,
                    CreatedBy = formule.CreatedBy,
                    ModifiedOn = formule.ModifiedOn,
                    ModifiedBy = formule.ModifiedBy
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement des détails de la formule {Id}", id);
                TempData["ErrorMessage"] = "Une erreur est survenue lors du chargement des détails.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Affiche le formulaire de création de formule
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                await PopulateViewBags();
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement du formulaire de création");
                TempData["ErrorMessage"] = "Une erreur est survenue lors du chargement du formulaire.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Traite la création d'une nouvelle formule
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FormuleJourViewModel model)
        {
            try
            {
                if (model == null)
                {
                    ModelState.AddModelError("", "Les données de la formule sont manquantes.");
                    await PopulateViewBags();
                    return View();
                }

                // Vérifier si une formule existe déjà pour cette date
                var formuleExistante = await _context.FormulesJour
                    .FirstOrDefaultAsync(f => f.Date.Date == model.Date.Date && f.Supprimer == 0);

                if (formuleExistante != null)
                {
                    ModelState.AddModelError(nameof(model.Date), "Une formule existe déjà pour cette date.");
                }

                if (ModelState.IsValid)
                {
                    var formule = new FormuleJour
                    {
                        IdFormule = Guid.NewGuid(),
                        Date = model.Date,
                        NomFormule = model.NomFormule,
                        TypeFormuleId = model.TypeFormuleId,
                        Entree = model.Entree,
                        Plat = model.Plat,
                        Garniture = model.Garniture,
                        Dessert = model.Dessert,
                        PlatStandard1 = model.PlatStandard1,
                        GarnitureStandard1 = model.GarnitureStandard1,
                        PlatStandard2 = model.PlatStandard2,
                        GarnitureStandard2 = model.GarnitureStandard2,
                        Feculent = model.Feculent,
                        Legumes = model.Legumes,
                        Marge = model.Marge,
                        Statut = model.Statut,
                        Verrouille = model.Verrouille,
                        Historique = model.Historique,
                        CreatedOn = DateTime.Now,
                        CreatedBy = User.Identity?.Name ?? "System"
                    };

                    _context.FormulesJour.Add(formule);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Formule créée avec succès: {Id} pour {Date}", formule.IdFormule, formule.Date);
                    TempData["SuccessMessage"] = "Formule créée avec succès.";
                    return RedirectToAction(nameof(Index));
                }

                await PopulateViewBags();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création de la formule");
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la création de la formule.";
                await PopulateViewBags();
                return View(model);
            }
        }

        /// <summary>
        /// Affiche le formulaire de modification de formule
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var formule = await _context.FormulesJour
                    .Include(f => f.NomFormuleNavigation)
                    .FirstOrDefaultAsync(f => f.IdFormule == id && f.Supprimer == 0);

                if (formule == null)
                {
                    TempData["ErrorMessage"] = "Formule non trouvée.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new FormuleJourViewModel
                {
                    IdFormule = formule.IdFormule,
                    Date = formule.Date,
                    NomFormule = formule.NomFormule,
                    TypeFormuleId = formule.TypeFormuleId,
                    Entree = formule.Entree,
                    Plat = formule.Plat,
                    Garniture = formule.Garniture,
                    Dessert = formule.Dessert,
                    PlatStandard1 = formule.PlatStandard1,
                    GarnitureStandard1 = formule.GarnitureStandard1,
                    PlatStandard2 = formule.PlatStandard2,
                    GarnitureStandard2 = formule.GarnitureStandard2,
                    Feculent = formule.Feculent,
                    Legumes = formule.Legumes,
                    Marge = formule.Marge,
                    Statut = formule.Statut,
                    Verrouille = formule.Verrouille,
                    Historique = formule.Historique
                };

                await PopulateViewBags();
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement de la formule {Id}", id);
                TempData["ErrorMessage"] = "Une erreur est survenue lors du chargement de la formule.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Traite la modification d'une formule
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, FormuleJourViewModel model)
        {
            try
            {
                if (id != model.IdFormule)
                {
                    TempData["ErrorMessage"] = "Identifiant de formule invalide.";
                    return RedirectToAction(nameof(Index));
                }

                var formule = await _context.FormulesJour
                    .FirstOrDefaultAsync(f => f.IdFormule == id && f.Supprimer == 0);

                if (formule == null)
                {
                    TempData["ErrorMessage"] = "Formule non trouvée.";
                    return RedirectToAction(nameof(Index));
                }

                // Vérifier si une autre formule existe pour cette date
                var formuleExistante = await _context.FormulesJour
                    .FirstOrDefaultAsync(f => f.Date.Date == model.Date.Date && f.IdFormule != id && f.Supprimer == 0);

                if (formuleExistante != null)
                {
                    ModelState.AddModelError(nameof(model.Date), "Une autre formule existe déjà pour cette date.");
                }

                if (ModelState.IsValid)
                {
                    formule.Date = model.Date;
                    formule.NomFormule = model.NomFormule;
                    formule.TypeFormuleId = model.TypeFormuleId;
                    formule.Entree = model.Entree;
                    formule.Plat = model.Plat;
                    formule.Garniture = model.Garniture;
                    formule.Dessert = model.Dessert;
                    formule.PlatStandard1 = model.PlatStandard1;
                    formule.GarnitureStandard1 = model.GarnitureStandard1;
                    formule.PlatStandard2 = model.PlatStandard2;
                    formule.GarnitureStandard2 = model.GarnitureStandard2;
                    formule.Feculent = model.Feculent;
                    formule.Legumes = model.Legumes;
                    formule.Marge = model.Marge;
                    formule.Statut = model.Statut;
                    formule.Verrouille = model.Verrouille;
                    formule.Historique = model.Historique;
                    formule.ModifiedOn = DateTime.Now;
                    formule.ModifiedBy = User.Identity?.Name ?? "System";

                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Formule modifiée avec succès: {Id}", formule.IdFormule);
                    TempData["SuccessMessage"] = "Formule modifiée avec succès.";
                    return RedirectToAction(nameof(Index));
                }

                await PopulateViewBags();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la modification de la formule {Id}", id);
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la modification de la formule.";
                await PopulateViewBags();
                return View(model);
            }
        }

        /// <summary>
        /// Supprime une formule (soft delete)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var formule = await _context.FormulesJour
                    .FirstOrDefaultAsync(f => f.IdFormule == id && f.Supprimer == 0);

                if (formule == null)
                {
                    TempData["ErrorMessage"] = "Formule non trouvée.";
                    return RedirectToAction(nameof(Index));
                }

                // Vérifier s'il y a des commandes liées à cette formule
                var commandesLiees = await _context.Commandes
                    .AnyAsync(c => c.IdFormule == id && c.Supprimer == 0);

                if (commandesLiees)
                {
                    TempData["ErrorMessage"] = "Impossible de supprimer cette formule car elle est liée à des commandes.";
                    return RedirectToAction(nameof(Index));
                }

                formule.Supprimer = 1;
                formule.ModifiedOn = DateTime.Now;
                formule.ModifiedBy = User.Identity?.Name ?? "System";

                await _context.SaveChangesAsync();

                _logger.LogInformation("Formule supprimée avec succès: {Id}", formule.IdFormule);
                TempData["SuccessMessage"] = "Formule supprimée avec succès.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression de la formule {Id}", id);
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la suppression de la formule.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Affiche l'historique des menus
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Historique(DateTime? dateDebut, DateTime? dateFin, string? nomFormule)
        {
            try
            {
                var query = _context.FormulesJour
                    .Include(f => f.NomFormuleNavigation)
                    .Where(f => f.Supprimer == 0);

                // Filtrer par dates si spécifiées
                if (dateDebut.HasValue)
                    query = query.Where(f => f.Date >= dateDebut.Value);
                
                if (dateFin.HasValue)
                    query = query.Where(f => f.Date <= dateFin.Value);

                // Filtrer par nom de formule si spécifié
                if (!string.IsNullOrEmpty(nomFormule))
                    query = query.Where(f => f.NomFormule != null && f.NomFormule.Contains(nomFormule));

                var formules = await query
                    .OrderByDescending(f => f.Date)
                    .Select(f => new FormuleJourViewModel
                    {
                        IdFormule = f.IdFormule,
                        Date = f.Date,
                        NomFormule = f.NomFormule,
                        TypeFormuleId = f.TypeFormuleId,
                        TypeFormuleNom = f.NomFormuleNavigation != null ? f.NomFormuleNavigation.Nom : null,
                        Entree = f.Entree,
                        Plat = f.Plat,
                        Garniture = f.Garniture,
                        Dessert = f.Dessert,
                        PlatStandard1 = f.PlatStandard1,
                        GarnitureStandard1 = f.GarnitureStandard1,
                        PlatStandard2 = f.PlatStandard2,
                        GarnitureStandard2 = f.GarnitureStandard2,
                        Feculent = f.Feculent,
                        Legumes = f.Legumes,
                        Marge = f.Marge,
                        Statut = f.Statut,
                        Verrouille = f.Verrouille,
                        Historique = f.Historique,
                        CreatedOn = f.CreatedOn,
                        CreatedBy = f.CreatedBy,
                        ModifiedOn = f.ModifiedOn,
                        ModifiedBy = f.ModifiedBy
                    })
                    .ToListAsync();

                ViewBag.DateDebut = dateDebut?.ToString("yyyy-MM-dd");
                ViewBag.DateFin = dateFin?.ToString("yyyy-MM-dd");
                ViewBag.NomFormule = nomFormule;

                return View(formules);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement de l'historique des formules");
                TempData["ErrorMessage"] = "Une erreur est survenue lors du chargement de l'historique.";
                return View(new List<FormuleJourViewModel>());
            }
        }

        /// <summary>
        /// Affiche le formulaire d'importation
        /// </summary>
        [HttpGet]
        public IActionResult Import()
        {
            return View();
        }

        /// <summary>
        /// Affiche le formulaire de création en lot
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CreateBulk()
        {
            try
            {
                await PopulateViewBags();
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement du formulaire de création en lot");
                TempData["ErrorMessage"] = "Une erreur est survenue lors du chargement du formulaire.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Traite la création en lot de formules
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBulk(CreateBulkFormuleViewModel model)
        {
            try
            {
                if (model == null)
                {
                    ModelState.AddModelError("", "Les données de la formule sont manquantes.");
                    await PopulateViewBags();
                    return View();
                }

                // Validation des dates
                if (model.DateFin < model.DateDebut)
                {
                    ModelState.AddModelError(nameof(model.DateFin), "La date de fin doit être postérieure à la date de début.");
                }

                if (ModelState.IsValid)
                {
                    var formulesCreees = 0;
                    var formulesModifiees = 0;
                    var formulesIgnorees = 0;

                    // Créer des formules pour chaque jour de la période
                    for (var date = model.DateDebut; date <= model.DateFin; date = date.AddDays(1))
                    {
                        // Éviter les weekends si demandé
                        if (model.ExclureWeekends && (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday))
                        {
                            continue;
                        }

                        // Vérifier si une formule existe déjà pour cette date
                        var formuleExistante = await _context.FormulesJour
                            .FirstOrDefaultAsync(f => f.Date.Date == date.Date && f.Supprimer == 0);

                        if (formuleExistante != null && !model.RemplacerExistantes)
                        {
                            formulesIgnorees++;
                            continue;
                        }

                        var formule = new FormuleJour
                        {
                            IdFormule = Guid.NewGuid(),
                            Date = date,
                            NomFormule = model.NomFormule,
                            TypeFormuleId = model.TypeFormuleId,
                            Entree = model.Entree,
                            Plat = model.Plat,
                            Garniture = model.Garniture,
                            Dessert = model.Dessert,
                            PlatStandard1 = model.PlatStandard1,
                            GarnitureStandard1 = model.GarnitureStandard1,
                            PlatStandard2 = model.PlatStandard2,
                            GarnitureStandard2 = model.GarnitureStandard2,
                            Feculent = model.Feculent,
                            Legumes = model.Legumes,
                            Marge = model.Marge,
                            Statut = model.Statut,
                            Verrouille = false,
                            CreatedOn = DateTime.Now,
                            CreatedBy = User.Identity?.Name ?? "System"
                        };

                        if (formuleExistante != null && model.RemplacerExistantes)
                        {
                            // Remplacer la formule existante
                            formuleExistante.NomFormule = formule.NomFormule;
                            formuleExistante.TypeFormuleId = formule.TypeFormuleId;
                            formuleExistante.Entree = formule.Entree;
                            formuleExistante.Plat = formule.Plat;
                            formuleExistante.Garniture = formule.Garniture;
                            formuleExistante.Dessert = formule.Dessert;
                            formuleExistante.PlatStandard1 = formule.PlatStandard1;
                            formuleExistante.GarnitureStandard1 = formule.GarnitureStandard1;
                            formuleExistante.PlatStandard2 = formule.PlatStandard2;
                            formuleExistante.GarnitureStandard2 = formule.GarnitureStandard2;
                            formuleExistante.Feculent = formule.Feculent;
                            formuleExistante.Legumes = formule.Legumes;
                            formuleExistante.Marge = formule.Marge;
                            formuleExistante.Statut = formule.Statut;
                            formuleExistante.ModifiedOn = DateTime.Now;
                            formuleExistante.ModifiedBy = User.Identity?.Name ?? "System";
                            formulesModifiees++;
                        }
                        else
                        {
                            _context.FormulesJour.Add(formule);
                            formulesCreees++;
                        }
                    }

                    await _context.SaveChangesAsync();

                    var message = $"{formulesCreees} formules créées avec succès";
                    if (formulesModifiees > 0)
                    {
                        message += $", {formulesModifiees} formules modifiées";
                    }
                    if (formulesIgnorees > 0)
                    {
                        message += $", {formulesIgnorees} formules ignorées (déjà existantes)";
                    }
                    message += ".";

                    _logger.LogInformation("Création en lot terminée: {Creees} créées, {Modifiees} modifiées, {Ignorees} ignorées", 
                        formulesCreees, formulesModifiees, formulesIgnorees);
                    
                    TempData["SuccessMessage"] = message;
                    return RedirectToAction(nameof(Index));
                }

                await PopulateViewBags();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création en lot des formules");
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la création en lot des formules.";
                await PopulateViewBags();
                return View(model);
            }
        }

        /// <summary>
        /// Traite l'importation de formules depuis un fichier Excel
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(ImportFormuleViewModel model)
        {
            try
            {
                if (model.FichierExcel == null || model.FichierExcel.Length == 0)
                {
                    ModelState.AddModelError(nameof(model.FichierExcel), "Veuillez sélectionner un fichier.");
                    return View(model);
                }

                // Vérifier l'extension du fichier
                var extension = Path.GetExtension(model.FichierExcel.FileName).ToLowerInvariant();
                if (extension != ".xlsx" && extension != ".xls")
                {
                    ModelState.AddModelError(nameof(model.FichierExcel), "Le fichier doit être au format Excel (.xlsx ou .xls).");
                    return View(model);
                }

                var result = await ProcessImportFile(model);
                
                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                }
                else
                {
                    TempData["ErrorMessage"] = result.Message;
                    if (result.Erreurs.Any())
                    {
                        TempData["ImportErrors"] = result.Erreurs;
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'importation des formules");
                TempData["ErrorMessage"] = "Une erreur est survenue lors de l'importation.";
                return View(model);
            }
        }

        /// <summary>
        /// Télécharge un modèle Excel pour l'importation (Format simplifié : 1 ligne = 1 jour avec toutes les formules)
        /// </summary>
        [HttpGet]
        public IActionResult DownloadTemplate()
        {
            try
            {
                using var workbook = new ClosedXML.Excel.XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Menus Semaine");

                // En-têtes (Format simplifié : 1 ligne = 1 jour complet)
                var headers = new[]
                {
                    "Date", "Entree", "Dessert", "Plat", "Garniture", "Feculent", "Legumes",
                    "Plat standard 1", "Garniture standard 1", "Plat standard 2", "Garniture standard 2"
                };

                // Style pour les en-têtes
                var headerRow = worksheet.Row(1);
                headerRow.Style.Font.Bold = true;
                headerRow.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromArgb(79, 129, 189); // Bleu
                headerRow.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
                headerRow.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                headerRow.Style.Alignment.Vertical = ClosedXML.Excel.XLAlignmentVerticalValues.Center;
                headerRow.Height = 30;

                // Ajouter les en-têtes
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cell(1, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                }

                // Données d'exemple (7 jours de la semaine)
                var sampleData = new[]
                {
                    new[] { "02/02/2026", "Salade de Crudités", "Yaourt", "Filet de Sosso au Four", "Pois Chiches Sautés", "", "", "Lasagne Bolognaise", "Salade Verte", "Soupe de Poulet", "Riz Blanc" },
                    new[] { "03/02/2026", "Salade Verdurette", "Brownie", "Gratin de Cabillaud", "Pommes de Terre Vapeur", "", "", "APF", "Attiéké", "Bœuf Sauce Bawin", "Riz Blanc" },
                    new[] { "04/02/2026", "Friand au Fromage", "Beignet Nature", "Émincé de Bœuf à La Moutarde", "Riz Safrané", "", "", "Poulet au Four", "Pommes de Terre Sautées", "Poisson Fumé Sauce Gouagouassou", "Riz Blanc" },
                    new[] { "05/02/2026", "Salade Composée", "Gâteau Semoule Raisins", "Lapin aux Pruneaux", "Purée de Patates Douces", "", "", "Choukouya de Bœuf", "Attiéké", "Akpéssi de Banane au Poulet", "Banane Plantain" },
                    new[] { "06/02/2026", "Mini Quiche Légumes", "Salade de Fruits Maison", "Chili Con Carne Doux", "Riz Blanc", "", "", "Poisson Frit Abolo", "Abolo", "Bœuf Sauce Pistache", "Riz" },
                    new[] { "07/02/2026", "Cocktail de Crudités", "Pain Perdu", "Colombo de Poulet", "Couscous", "", "", "Poulet à L'Ivoirienne", "Attiéké", "Poisson Frit Sauce Feuilles", "Riz Blanc" },
                    new[] { "08/02/2026", "Œufs Brouillés aux Légumes", "Moka Café", "Saumon Grillé", "Patates Douces Rôties", "", "", "Chicken Burger", "Pommes de Terre Sautées", "Poulet Fumé Sauce Doumglé", "Riz Blanc" }
                };

                // Ajouter les exemples avec style
                for (int row = 0; row < sampleData.Length; row++)
                {
                    for (int col = 0; col < sampleData[row].Length; col++)
                    {
                        var cell = worksheet.Cell(row + 2, col + 1);
                        cell.Value = sampleData[row][col];
                        cell.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                        
                        // Alterner les couleurs des lignes
                        if (row % 2 == 0)
                        {
                            cell.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromArgb(220, 230, 241);
                        }
                    }
                }

                // Ajuster la largeur des colonnes
                worksheet.Columns().AdjustToContents();
                
                // Largeur minimale pour la lisibilité
                foreach (var column in worksheet.ColumnsUsed())
                {
                    if (column.Width < 15)
                        column.Width = 15;
                }

                // Ajouter des instructions
                var instructionRow = 10;
                worksheet.Cell(instructionRow, 1).Value = "INSTRUCTIONS :";
                worksheet.Cell(instructionRow, 1).Style.Font.Bold = true;
                worksheet.Cell(instructionRow, 1).Style.Font.FontSize = 12;
                
                var instructions = new[]
                {
                    "",
                    "FORMAT SIMPLIFIÉ : 1 ligne = 1 jour complet avec toutes les formules",
                    "",
                    "• Date : Format JJ/MM/AAAA (ex: 02/02/2026) - OBLIGATOIRE",
                    "• Entree : Entrée de la formule améliorée",
                    "• Dessert : Dessert de la formule améliorée",
                    "• Plat : Plat principal de la formule améliorée",
                    "• Garniture : Garniture de la formule améliorée",
                    "• Feculent : Féculent commun (optionnel)",
                    "• Legumes : Légumes communs (optionnel)",
                    "• Plat standard 1 : Premier plat standard",
                    "• Garniture standard 1 : Garniture du premier plat standard",
                    "• Plat standard 2 : Deuxième plat standard",
                    "• Garniture standard 2 : Garniture du deuxième plat standard",
                    "",
                    "AVANTAGES :",
                    "• 1 ligne par jour au lieu de 3 lignes",
                    "• 7 lignes pour une semaine complète",
                    "• Plus simple et plus rapide à remplir",
                    "",
                    "NOTES :",
                    "• Les champs vides sont autorisés",
                    "• Le système créera automatiquement 3 formules par jour :",
                    "  - Formule Améliorée (si Entree, Plat, Garniture ou Dessert remplis)",
                    "  - Formule Standard 1 (si Plat standard 1 rempli)",
                    "  - Formule Standard 2 (si Plat standard 2 rempli)"
                };

                for (int i = 0; i < instructions.Length; i++)
                {
                    var cell = worksheet.Cell(instructionRow + 1 + i, 1);
                    cell.Value = instructions[i];
                    cell.Style.Font.Italic = true;
                    cell.Style.Font.FontColor = ClosedXML.Excel.XLColor.DarkGray;
                    worksheet.Range(instructionRow + 1 + i, 1, instructionRow + 1 + i, headers.Length).Merge();
                }

                // Générer le fichier en mémoire
                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;

                return File(stream.ToArray(), 
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                    "modele_menus_semaine.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la génération du modèle Excel");
                TempData["ErrorMessage"] = "Erreur lors de la génération du modèle Excel.";
                return RedirectToAction(nameof(Import));
            }
        }

        /// <summary>
        /// Popule les ViewBags nécessaires
        /// </summary>
        private async Task PopulateViewBags()
        {
            // Types de formules
            var typesFormules = await _context.TypesFormule
                .Where(t => t.Supprimer == 0)
                .OrderBy(t => t.Nom)
                .Select(t => new { Value = t.Id.ToString(), Text = t.Nom })
                .ToListAsync();
            ViewBag.TypeFormules = new SelectList(typesFormules, "Value", "Text");
        }

        /// <summary>
        /// Traite le fichier d'importation Excel (Format simplifié : 1 ligne = 1 jour avec toutes les formules)
        /// </summary>
        private async Task<ImportResultViewModel> ProcessImportFile(ImportFormuleViewModel model)
        {
            var result = new ImportResultViewModel();
            var formulesCreees = 0;
            var lignesErreur = 0;
            var erreurs = new List<string>();

            try
            {
                using var stream = model.FichierExcel?.OpenReadStream();
                using var workbook = new ClosedXML.Excel.XLWorkbook(stream);
                var worksheet = workbook.Worksheets.First();

                var usedRows = worksheet.RowsUsed().Skip(1); // Ignorer l'en-tête
                var totalRows = usedRows.Count();
                result.TotalLignes = totalRows;

                // Traiter chaque ligne (1 ligne = 1 jour avec 3 formules potentielles)
                foreach (var row in usedRows)
                {
                    try
                    {
                        var rowNumber = row.RowNumber();
                        
                        // Lire les valeurs des cellules (nouveau format)
                        var dateValue = row.Cell(1).GetString().Trim();
                        var entree = row.Cell(2).GetString().Trim();
                        var dessert = row.Cell(3).GetString().Trim();
                        var plat = row.Cell(4).GetString().Trim();
                        var garniture = row.Cell(5).GetString().Trim();
                        var feculent = row.Cell(6).GetString().Trim();
                        var legumes = row.Cell(7).GetString().Trim();
                        var platStandard1 = row.Cell(8).GetString().Trim();
                        var garnitureStandard1 = row.Cell(9).GetString().Trim();
                        var platStandard2 = row.Cell(10).GetString().Trim();
                        var garnitureStandard2 = row.Cell(11).GetString().Trim();

                        // Vérifier le champ obligatoire
                        if (string.IsNullOrEmpty(dateValue))
                        {
                            erreurs.Add($"Ligne {rowNumber}: La date est obligatoire");
                            lignesErreur++;
                            continue;
                        }

                        // Parser la date (supporter plusieurs formats)
                        DateTime date;
                        if (!DateTime.TryParseExact(dateValue, new[] { "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd" }, 
                            System.Globalization.CultureInfo.InvariantCulture, 
                            System.Globalization.DateTimeStyles.None, out date))
                        {
                            erreurs.Add($"Ligne {rowNumber}: Format de date invalide '{dateValue}'. Utilisez JJ/MM/AAAA (ex: 02/02/2026)");
                            lignesErreur++;
                            continue;
                        }

                        // Vérifier si au moins une formule est remplie
                        var hasAmeliore = !string.IsNullOrEmpty(entree) || !string.IsNullOrEmpty(plat) || 
                                         !string.IsNullOrEmpty(garniture) || !string.IsNullOrEmpty(dessert);
                        var hasStandard1 = !string.IsNullOrEmpty(platStandard1);
                        var hasStandard2 = !string.IsNullOrEmpty(platStandard2);

                        if (!hasAmeliore && !hasStandard1 && !hasStandard2)
                        {
                            erreurs.Add($"Ligne {rowNumber}: Au moins une formule doit être remplie");
                            lignesErreur++;
                            continue;
                        }

                        // Créer les formules pour ce jour
                        var formulesCreeesPourCeJour = 0;

                        // 1. Formule Améliorée (si au moins un champ est rempli)
                        if (hasAmeliore)
                        {
                            var formuleExistante = await _context.FormulesJour
                                .FirstOrDefaultAsync(f => f.Date.Date == date.Date && f.NomFormule == "Amélioré" && f.Supprimer == 0);

                            if (formuleExistante != null && !model.RemplacerExistantes)
                            {
                                erreurs.Add($"Ligne {rowNumber}: Une formule Améliorée existe déjà pour le {date:dd/MM/yyyy}");
                            }
                            else
                            {
                                var formule = new FormuleJour
                                {
                                    IdFormule = Guid.NewGuid(),
                                    Date = date,
                                    NomFormule = "Amélioré",
                                    Entree = string.IsNullOrEmpty(entree) ? null : entree,
                                    Plat = string.IsNullOrEmpty(plat) ? null : plat,
                                    Garniture = string.IsNullOrEmpty(garniture) ? null : garniture,
                                    Dessert = string.IsNullOrEmpty(dessert) ? null : dessert,
                                    Feculent = string.IsNullOrEmpty(feculent) ? null : feculent,
                                    Legumes = string.IsNullOrEmpty(legumes) ? null : legumes,
                                    Marge = 15, // Marge par défaut pour Amélioré
                                    Statut = 1,
                                    CreatedOn = DateTime.Now,
                                    CreatedBy = User.Identity?.Name ?? "System"
                                };

                                if (formuleExistante != null && model.RemplacerExistantes)
                                {
                                    // Remplacer
                                    formuleExistante.Entree = formule.Entree;
                                    formuleExistante.Plat = formule.Plat;
                                    formuleExistante.Garniture = formule.Garniture;
                                    formuleExistante.Dessert = formule.Dessert;
                                    formuleExistante.Feculent = formule.Feculent;
                                    formuleExistante.Legumes = formule.Legumes;
                                    formuleExistante.ModifiedOn = DateTime.Now;
                                    formuleExistante.ModifiedBy = User.Identity?.Name ?? "System";
                                }
                                else
                                {
                                    _context.FormulesJour.Add(formule);
                                }
                                formulesCreeesPourCeJour++;
                            }
                        }

                        // 2. Formule Standard 1 (si le plat est rempli)
                        if (hasStandard1)
                        {
                            var formuleExistante = await _context.FormulesJour
                                .FirstOrDefaultAsync(f => f.Date.Date == date.Date && f.NomFormule == "Standard 1" && f.Supprimer == 0);

                            if (formuleExistante != null && !model.RemplacerExistantes)
                            {
                                erreurs.Add($"Ligne {rowNumber}: Une formule Standard 1 existe déjà pour le {date:dd/MM/yyyy}");
                            }
                            else
                            {
                                var formule = new FormuleJour
                                {
                                    IdFormule = Guid.NewGuid(),
                                    Date = date,
                                    NomFormule = "Standard 1",
                                    PlatStandard1 = platStandard1,
                                    GarnitureStandard1 = string.IsNullOrEmpty(garnitureStandard1) ? null : garnitureStandard1,
                                    Feculent = string.IsNullOrEmpty(feculent) ? null : feculent,
                                    Legumes = string.IsNullOrEmpty(legumes) ? null : legumes,
                                    Marge = 0, // Marge par défaut pour Standard
                                    Statut = 1,
                                    CreatedOn = DateTime.Now,
                                    CreatedBy = User.Identity?.Name ?? "System"
                                };

                                if (formuleExistante != null && model.RemplacerExistantes)
                                {
                                    // Remplacer
                                    formuleExistante.PlatStandard1 = formule.PlatStandard1;
                                    formuleExistante.GarnitureStandard1 = formule.GarnitureStandard1;
                                    formuleExistante.Feculent = formule.Feculent;
                                    formuleExistante.Legumes = formule.Legumes;
                                    formuleExistante.ModifiedOn = DateTime.Now;
                                    formuleExistante.ModifiedBy = User.Identity?.Name ?? "System";
                                }
                                else
                                {
                                    _context.FormulesJour.Add(formule);
                                }
                                formulesCreeesPourCeJour++;
                            }
                        }

                        // 3. Formule Standard 2 (si le plat est rempli)
                        if (hasStandard2)
                        {
                            var formuleExistante = await _context.FormulesJour
                                .FirstOrDefaultAsync(f => f.Date.Date == date.Date && f.NomFormule == "Standard 2" && f.Supprimer == 0);

                            if (formuleExistante != null && !model.RemplacerExistantes)
                            {
                                erreurs.Add($"Ligne {rowNumber}: Une formule Standard 2 existe déjà pour le {date:dd/MM/yyyy}");
                            }
                            else
                            {
                                var formule = new FormuleJour
                                {
                                    IdFormule = Guid.NewGuid(),
                                    Date = date,
                                    NomFormule = "Standard 2",
                                    PlatStandard2 = platStandard2,
                                    GarnitureStandard2 = string.IsNullOrEmpty(garnitureStandard2) ? null : garnitureStandard2,
                                    Feculent = string.IsNullOrEmpty(feculent) ? null : feculent,
                                    Legumes = string.IsNullOrEmpty(legumes) ? null : legumes,
                                    Marge = 0, // Marge par défaut pour Standard
                                    Statut = 1,
                                    CreatedOn = DateTime.Now,
                                    CreatedBy = User.Identity?.Name ?? "System"
                                };

                                if (formuleExistante != null && model.RemplacerExistantes)
                                {
                                    // Remplacer
                                    formuleExistante.PlatStandard2 = formule.PlatStandard2;
                                    formuleExistante.GarnitureStandard2 = formule.GarnitureStandard2;
                                    formuleExistante.Feculent = formule.Feculent;
                                    formuleExistante.Legumes = formule.Legumes;
                                    formuleExistante.ModifiedOn = DateTime.Now;
                                    formuleExistante.ModifiedBy = User.Identity?.Name ?? "System";
                                }
                                else
                                {
                                    _context.FormulesJour.Add(formule);
                                }
                                formulesCreeesPourCeJour++;
                            }
                        }

                        formulesCreees += formulesCreeesPourCeJour;
                    }
                    catch (Exception ex)
                    {
                        erreurs.Add($"Ligne {row.RowNumber()}: {ex.Message}");
                        lignesErreur++;
                        
                        if (!model.IgnorerErreurs)
                        {
                            break;
                        }
                    }
                }

                if (lignesErreur == 0 || model.IgnorerErreurs)
                {
                    await _context.SaveChangesAsync();
                    result.Success = true;
                    result.Message = $"{formulesCreees} formules importées avec succès (format simplifié : 1 ligne = 1 jour).";
                }
                else
                {
                    result.Success = false;
                    result.Message = $"{lignesErreur} erreurs détectées. Import annulé.";
                }

                result.LignesImportees = formulesCreees;
                result.LignesErreur = lignesErreur;
                result.Erreurs = erreurs;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Erreur lors du traitement du fichier Excel: {ex.Message}";
                result.Erreurs.Add(ex.Message);
            }

            return result;
        }
    }
}
