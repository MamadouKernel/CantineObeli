using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Obeli_K.Data;
using Obeli_K.Models;
using Obeli_K.Models.ViewModels;
using Obeli_K.Services;
using System.Globalization;

namespace Obeli_K.Controllers
{
    /// <summary>
    /// Contrôleur pour la gestion des formules journalières de restauration.
    /// Permet la création, modification et import des menus quotidiens.
    /// Accessible aux administrateurs, RH et prestataires de cantine.
    /// </summary>
    [Authorize(Roles = "Administrateur,RH,PrestataireCantine")]
    public class FormuleJourController : Controller
    {
        private readonly ObeliDbContext _context;
        private readonly ILogger<FormuleJourController> _logger;
        private readonly ExcelExportService _excelExportService;

        /// <summary>
        /// Initialise une nouvelle instance du contrôleur de formules journalières.
        /// </summary>
        /// <param name="context">Contexte de base de données Obeli</param>
        /// <param name="logger">Service de journalisation</param>
        /// <param name="excelExportService">Service d'export Excel</param>
        public FormuleJourController(ObeliDbContext context, ILogger<FormuleJourController> logger, ExcelExportService excelExportService)
        {
            _context = context;
            _logger = logger;
            _excelExportService = excelExportService;
        }

        /// <summary>
        /// Affiche la liste des formules
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(DateTime? dateDebut, DateTime? dateFin, bool semaineSuivante = false, int page = 1, int pageSize = 5)
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

                var totalCount = await query.CountAsync();

                var formules = await query
                    .OrderByDescending(f => f.Date)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
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

                // Créer le modèle de pagination
                var pagination = new PaginationViewModel(HttpContext, "Index", "FormuleJour")
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalItems = totalCount
                };

                var pagedModel = new PagedViewModel<FormuleJourViewModel>
                {
                    Items = formules,
                    Pagination = pagination
                };

                ViewBag.DateDebut = dateDebut?.ToString("yyyy-MM-dd");
                ViewBag.DateFin = dateFin?.ToString("yyyy-MM-dd");
                ViewBag.SemaineSuivante = semaineSuivante;

                return View(pagedModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement des formules");
                TempData["ErrorMessage"] = "Une erreur est survenue lors du chargement des formules.";
                return View(new PagedViewModel<FormuleJourViewModel>());
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
        /// Affiche le formulaire de sélection de période pour créer des formules
        /// </summary>
        [HttpGet]
        public IActionResult Create()
        {
            return View("SelectPeriod");
        }

        /// <summary>
        /// Traite la sélection de période et vérifie les conflits
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SelectPeriod(DateTime dateDebut, DateTime dateFin)
        {
            try
            {
                // Vérifier s'il existe déjà des formules pour cette période
                var formulesExistantes = await _context.FormulesJour
                    .Where(f => f.Date.Date >= dateDebut.Date && f.Date.Date <= dateFin.Date && f.Supprimer == 0)
                    .OrderBy(f => f.Date)
                    .ToListAsync();

                if (formulesExistantes.Any())
                {
                    // Afficher les formules existantes et demander confirmation
                    ViewBag.DateDebut = dateDebut;
                    ViewBag.DateFin = dateFin;
                    ViewBag.FormulesExistantes = formulesExistantes;
                    return View("ConfirmPeriod", formulesExistantes);
                }

                // Aucun conflit, rediriger vers le formulaire de création avec la période
                return RedirectToAction(nameof(CreateFormules), new { dateDebut = dateDebut.ToString("yyyy-MM-dd"), dateFin = dateFin.ToString("yyyy-MM-dd") });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la vérification de la période");
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la vérification de la période.";
                return View("SelectPeriod");
            }
        }

        /// <summary>
        /// Affiche le formulaire de création de formules pour une période donnée
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CreateFormules(DateTime dateDebut, DateTime dateFin)
        {
            try
            {
                ViewBag.DateDebut = dateDebut;
                ViewBag.DateFin = dateFin;
                await PopulateViewBags();
                
                // Créer un modèle pour la création multi-jours
                var model = new CreateMultiDayFormuleViewModel
                {
                    DateDebut = dateDebut,
                    DateFin = dateFin,
                    NomFormule = "Formule du jour",
                    Statut = 1,
                    ExclureWeekends = false,
                    RemplacerExistantes = false
                };

                // Générer les formules pour chaque jour
                var currentDate = dateDebut;
                while (currentDate <= dateFin)
                {
                    if (!model.ExclureWeekends || (currentDate.DayOfWeek != DayOfWeek.Saturday && currentDate.DayOfWeek != DayOfWeek.Sunday))
                    {
                        model.Formules.Add(new FormuleJourViewModel
                        {
                            Date = currentDate,
                            NomFormule = model.NomFormule,
                            Statut = model.Statut,
                            Verrouille = false
                        });
                    }
                    currentDate = currentDate.AddDays(1);
                }
                
                return View("Create", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement du formulaire de création");
                TempData["ErrorMessage"] = "Une erreur est survenue lors du chargement du formulaire.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Traite la création de formules multi-jours - Crée 3 formules distinctes par date
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("CreateMultiDay")]
        public async Task<IActionResult> CreateBulk(CreateMultiDayFormuleViewModel model)
        {
            try
            {
                if (model == null || model.Formules == null || !model.Formules.Any())
                {
                    ModelState.AddModelError("", "Aucune formule à créer.");
                    await PopulateViewBags();
                    return View("Create", model);
                }

                var formulesCreees = 0;
                var formulesModifiees = 0;
                var formulesIgnorees = 0;

                // Créer 3 formules distinctes pour chaque date
                foreach (var formuleViewModel in model.Formules)
                {
                    // Supprimer les formules existantes pour cette date si remplacement demandé
                    if (model.RemplacerExistantes)
                    {
                        var formulesExistantes = await _context.FormulesJour
                            .Where(f => f.Date.Date == formuleViewModel.Date.Date && f.Supprimer == 0)
                            .ToListAsync();
                        
                        foreach (var formuleExistante in formulesExistantes)
                        {
                            formuleExistante.Supprimer = 1;
                            formuleExistante.ModifiedOn = DateTime.Now;
                            formuleExistante.ModifiedBy = User.Identity?.Name ?? "System";
                            formulesModifiees++;
                        }
                    }
                    else
                    {
                        // Vérifier s'il existe déjà des formules pour cette date
                        var formuleExistante = await _context.FormulesJour
                            .FirstOrDefaultAsync(f => f.Date.Date == formuleViewModel.Date.Date && f.Supprimer == 0);

                        if (formuleExistante != null)
                        {
                            formulesIgnorees++;
                            continue;
                        }
                    }

                    // Créer seulement les formules qui ont du contenu
                    var formulesPourDate = new List<FormuleJour>();

                    // 1. Formule Améliorée - Créée seulement si au moins un champ est rempli
                    if (!string.IsNullOrEmpty(formuleViewModel.Entree) || 
                        !string.IsNullOrEmpty(formuleViewModel.Plat) ||
                        !string.IsNullOrEmpty(formuleViewModel.Garniture) ||
                        !string.IsNullOrEmpty(formuleViewModel.Dessert) ||
                        !string.IsNullOrEmpty(formuleViewModel.Feculent) ||
                        !string.IsNullOrEmpty(formuleViewModel.Legumes))
                    {
                        formulesPourDate.Add(new FormuleJour
                        {
                            IdFormule = Guid.NewGuid(),
                            Date = formuleViewModel.Date,
                            NomFormule = "Formule Améliorée",
                            TypeFormuleId = formuleViewModel.TypeFormuleId,
                            Entree = formuleViewModel.Entree,
                            Plat = formuleViewModel.Plat,
                            Garniture = formuleViewModel.Garniture,
                            Dessert = formuleViewModel.Dessert,
                            Feculent = formuleViewModel.Feculent,
                            Legumes = formuleViewModel.Legumes,
                            Marge = null,
                            Statut = formuleViewModel.Statut ?? model.Statut ?? 1,
                            Verrouille = formuleViewModel.Verrouille,
                            Historique = formuleViewModel.Historique,
                            CreatedOn = DateTime.Now,
                            CreatedBy = User.Identity?.Name ?? "System"
                        });
                    }

                    // 2. Formule Standard 1 - Créée seulement si au moins un champ est rempli
                    if (!string.IsNullOrEmpty(formuleViewModel.PlatStandard1) || 
                        !string.IsNullOrEmpty(formuleViewModel.GarnitureStandard1))
                    {
                        formulesPourDate.Add(new FormuleJour
                        {
                            IdFormule = Guid.NewGuid(),
                            Date = formuleViewModel.Date,
                            NomFormule = "Formule Standard 1",
                            TypeFormuleId = formuleViewModel.TypeFormuleId,
                            PlatStandard1 = formuleViewModel.PlatStandard1,
                            GarnitureStandard1 = formuleViewModel.GarnitureStandard1,
                            Marge = null,
                            Statut = formuleViewModel.Statut ?? model.Statut ?? 1,
                            Verrouille = formuleViewModel.Verrouille,
                            Historique = formuleViewModel.Historique,
                            CreatedOn = DateTime.Now,
                            CreatedBy = User.Identity?.Name ?? "System"
                        });
                    }

                    // 3. Formule Standard 2 - Créée seulement si au moins un champ est rempli
                    if (!string.IsNullOrEmpty(formuleViewModel.PlatStandard2) || 
                        !string.IsNullOrEmpty(formuleViewModel.GarnitureStandard2))
                    {
                        formulesPourDate.Add(new FormuleJour
                        {
                            IdFormule = Guid.NewGuid(),
                            Date = formuleViewModel.Date,
                            NomFormule = "Formule Standard 2",
                            TypeFormuleId = formuleViewModel.TypeFormuleId,
                            PlatStandard2 = formuleViewModel.PlatStandard2,
                            GarnitureStandard2 = formuleViewModel.GarnitureStandard2,
                            Marge = null,
                            Statut = formuleViewModel.Statut ?? model.Statut ?? 1,
                            Verrouille = formuleViewModel.Verrouille,
                            Historique = formuleViewModel.Historique,
                            CreatedOn = DateTime.Now,
                            CreatedBy = User.Identity?.Name ?? "System"
                        });
                    }

                    // Ajouter toutes les formules créées
                    foreach (var formule in formulesPourDate)
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

                _logger.LogInformation("Création multi-jours terminée: {Creees} créées, {Modifiees} modifiées, {Ignorees} ignorées", 
                    formulesCreees, formulesModifiees, formulesIgnorees);
                
                TempData["SuccessMessage"] = message;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création multi-jours des formules");
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la création des formules.";
                await PopulateViewBags();
                return View("Create", model);
            }
        }

        /// <summary>
        /// Traite la création d'une nouvelle formule - Crée 3 formules distinctes
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

                // Vérifier si des formules existent déjà pour cette date
                var formulesExistantes = await _context.FormulesJour
                    .Where(f => f.Date.Date == model.Date.Date && f.Supprimer == 0)
                    .ToListAsync();

                if (formulesExistantes.Any())
                {
                    ModelState.AddModelError(nameof(model.Date), "Des formules existent déjà pour cette date.");
                }

                if (ModelState.IsValid)
                {
                    var formulesCreees = 0;
                    var formulesPourDate = new List<FormuleJour>();

                    // 1. Formule Améliorée - Créée seulement si au moins un champ est rempli
                    if (!string.IsNullOrEmpty(model.Entree) || 
                        !string.IsNullOrEmpty(model.Plat) ||
                        !string.IsNullOrEmpty(model.Garniture) ||
                        !string.IsNullOrEmpty(model.Dessert) ||
                        !string.IsNullOrEmpty(model.Feculent) ||
                        !string.IsNullOrEmpty(model.Legumes))
                    {
                        formulesPourDate.Add(new FormuleJour
                        {
                            IdFormule = Guid.NewGuid(),
                            Date = model.Date,
                            NomFormule = "Formule Améliorée",
                            TypeFormuleId = model.TypeFormuleId,
                            Entree = model.Entree,
                            Plat = model.Plat,
                            Garniture = model.Garniture,
                            Dessert = model.Dessert,
                            Feculent = model.Feculent,
                            Legumes = model.Legumes,
                            Marge = null,
                            Statut = model.Statut ?? 1,
                            Verrouille = model.Verrouille,
                            Historique = model.Historique,
                            CreatedOn = DateTime.Now,
                            CreatedBy = User.Identity?.Name ?? "System"
                        });
                    }

                    // 2. Formule Standard 1 - Créée seulement si au moins un champ est rempli
                    if (!string.IsNullOrEmpty(model.PlatStandard1) || 
                        !string.IsNullOrEmpty(model.GarnitureStandard1))
                    {
                        formulesPourDate.Add(new FormuleJour
                        {
                            IdFormule = Guid.NewGuid(),
                            Date = model.Date,
                            NomFormule = "Formule Standard 1",
                            TypeFormuleId = model.TypeFormuleId,
                            PlatStandard1 = model.PlatStandard1,
                            GarnitureStandard1 = model.GarnitureStandard1,
                            Marge = null,
                            Statut = model.Statut ?? 1,
                            Verrouille = model.Verrouille,
                            Historique = model.Historique,
                            CreatedOn = DateTime.Now,
                            CreatedBy = User.Identity?.Name ?? "System"
                        });
                    }

                    // 3. Formule Standard 2 - Créée seulement si au moins un champ est rempli
                    if (!string.IsNullOrEmpty(model.PlatStandard2) || 
                        !string.IsNullOrEmpty(model.GarnitureStandard2))
                    {
                        formulesPourDate.Add(new FormuleJour
                        {
                            IdFormule = Guid.NewGuid(),
                            Date = model.Date,
                            NomFormule = "Formule Standard 2",
                            TypeFormuleId = model.TypeFormuleId,
                            PlatStandard2 = model.PlatStandard2,
                            GarnitureStandard2 = model.GarnitureStandard2,
                            Marge = null,
                            Statut = model.Statut ?? 1,
                            Verrouille = model.Verrouille,
                            Historique = model.Historique,
                            CreatedOn = DateTime.Now,
                            CreatedBy = User.Identity?.Name ?? "System"
                        });
                    }

                    // Ajouter toutes les formules créées
                    foreach (var formule in formulesPourDate)
                    {
                        _context.FormulesJour.Add(formule);
                        formulesCreees++;
                    }

                    await _context.SaveChangesAsync();

                    _logger.LogInformation("{Count} formules créées avec succès pour {Date}", formulesCreees, model.Date);
                    TempData["SuccessMessage"] = $"{formulesCreees} formules créées avec succès.";
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
                _logger.LogInformation("=== DÉBUT MODIFICATION FORMULE ===");
                _logger.LogInformation("ID reçu: {Id}, ID du modèle: {ModelId}", id, model.IdFormule);
                
                if (id != model.IdFormule)
                {
                    _logger.LogWarning("Identifiant de formule invalide. ID reçu: {Id}, ID du modèle: {ModelId}", id, model.IdFormule);
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

                // Vérifier si une autre formule existe pour cette date (en excluant la formule actuelle)
                _logger.LogInformation("Vérification des formules existantes pour la date: {Date}", model.Date.Date);
                var formuleExistante = await _context.FormulesJour
                    .FirstOrDefaultAsync(f => f.Date.Date == model.Date.Date && f.IdFormule != id && f.Supprimer == 0);

                _logger.LogInformation("Formule existante trouvée: {Found}, ID: {ExistingId}, Nom: {ExistingNom}", 
                    formuleExistante != null, 
                    formuleExistante?.IdFormule, 
                    formuleExistante?.NomFormule);

                if (formuleExistante != null)
                {
                    _logger.LogWarning("Conflit détecté: Une autre formule existe pour cette date. Formule existante: {ExistingNom} (ID: {ExistingId})", 
                        formuleExistante.NomFormule, formuleExistante.IdFormule);
                    ModelState.AddModelError(nameof(model.Date), $"Une autre formule existe déjà pour cette date. Formule existante: {formuleExistante.NomFormule} (ID: {formuleExistante.IdFormule})");
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
        /// Affiche le formulaire d'importation
        /// </summary>
        [HttpGet]
        public IActionResult Import()
        {
            return View();
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
        /// Télécharge un modèle Excel pour l'importation - Format condensé: 1 ligne = 1 jour complet
        /// </summary>
        [HttpGet]
        public IActionResult DownloadTemplate()
        {
            try
            {
                using var workbook = new ClosedXML.Excel.XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Formules");

                // En-têtes selon le nouveau format
                var headers = new[]
                {
                    "Date", "Entree", "Dessert", "Plat", "Garniture", "Feculent", "Legumes",
                    "Plat standard 1", "Garniture standard 1", "Plat standard 2", "Garniture standard 2"
                };

                // Style pour les en-têtes
                var headerRow = worksheet.Row(1);
                headerRow.Style.Font.Bold = true;
                headerRow.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightBlue;
                headerRow.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                headerRow.Height = 30;

                // Ajouter les en-têtes
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cell(1, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Alignment.WrapText = true;
                }

                // Données d'exemple - Semaine du 02/02/2026 au 08/02/2026
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

                // Ajouter les données d'exemple
                for (int row = 0; row < sampleData.Length; row++)
                {
                    for (int col = 0; col < sampleData[row].Length; col++)
                    {
                        worksheet.Cell(row + 2, col + 1).Value = sampleData[row][col];
                    }
                }

                // Ajuster la largeur des colonnes
                worksheet.Column(1).Width = 12; // Date
                worksheet.Column(2).Width = 25; // Entree
                worksheet.Column(3).Width = 25; // Dessert
                worksheet.Column(4).Width = 30; // Plat
                worksheet.Column(5).Width = 25; // Garniture
                worksheet.Column(6).Width = 20; // Feculent
                worksheet.Column(7).Width = 20; // Legumes
                worksheet.Column(8).Width = 25; // Plat standard 1
                worksheet.Column(9).Width = 25; // Garniture standard 1
                worksheet.Column(10).Width = 30; // Plat standard 2
                worksheet.Column(11).Width = 25; // Garniture standard 2

                // Ajouter des instructions sur une nouvelle feuille
                var instructionsSheet = workbook.Worksheets.Add("Instructions");
                
                instructionsSheet.Cell(1, 1).Value = "GUIDE D'IMPORTATION DES MENUS";
                instructionsSheet.Cell(1, 1).Style.Font.Bold = true;
                instructionsSheet.Cell(1, 1).Style.Font.FontSize = 16;
                
                var instructions = new[]
                {
                    "",
                    "FORMAT DU FICHIER :",
                    "• 1 ligne = 1 jour complet avec toutes les formules",
                    "• 7 lignes pour une semaine complète (au lieu de 21 lignes)",
                    "",
                    "COLONNES :",
                    "• Colonne A : Date (format DD/MM/YYYY, ex: 02/02/2026)",
                    "• Colonnes B-G : Formule Améliorée (Entrée, Dessert, Plat, Garniture, Féculent, Légumes)",
                    "• Colonnes H-I : Formule Standard 1 (Plat standard 1, Garniture standard 1)",
                    "• Colonnes J-K : Formule Standard 2 (Plat standard 2, Garniture standard 2)",
                    "",
                    "RÈGLES D'IMPORTATION :",
                    "• La date est OBLIGATOIRE pour chaque ligne",
                    "• Au moins un champ de formule doit être rempli par ligne",
                    "• Le système crée automatiquement 3 formules distinctes par jour :",
                    "  - Formule Améliorée (si colonnes B-G remplies)",
                    "  - Formule Standard 1 (si colonnes H-I remplies)",
                    "  - Formule Standard 2 (si colonnes J-K remplies)",
                    "",
                    "EXEMPLE :",
                    "• Ligne 1 : 02/02/2026 avec entrée, plat, dessert, etc.",
                    "  → Crée 3 formules pour le 02/02/2026",
                    "• Ligne 2 : 03/02/2026 avec d'autres plats",
                    "  → Crée 3 formules pour le 03/02/2026",
                    "",
                    "AVANTAGES DU NOUVEAU FORMAT :",
                    "• Plus simple : 7 lignes au lieu de 21 pour une semaine",
                    "• Plus rapide : toutes les infos d'un jour sur une seule ligne",
                    "• Moins d'erreurs : format plus compact et lisible",
                    "",
                    "NOTES IMPORTANTES :",
                    "• Les formules vides ne sont pas créées",
                    "• Les marges sont définies séparément (pas à l'import)",
                    "• Toutes les formules sont créées avec le statut 'Actif'",
                    "• Cochez 'Remplacer les formules existantes' pour écraser les menus existants",
                    "",
                    "FORMATS DE DATE ACCEPTÉS :",
                    "• DD/MM/YYYY (ex: 02/02/2026)",
                    "• YYYY-MM-DD (ex: 2026-02-02)",
                    "• DD-MM-YYYY (ex: 02-02-2026)"
                };

                for (int i = 0; i < instructions.Length; i++)
                {
                    var cell = instructionsSheet.Cell(i + 2, 1);
                    cell.Value = instructions[i];
                    
                    if (instructions[i].StartsWith("•"))
                    {
                        cell.Style.Font.FontSize = 11;
                    }
                    else if (instructions[i].EndsWith(":"))
                    {
                        cell.Style.Font.Bold = true;
                        cell.Style.Font.FontSize = 12;
                    }
                }

                instructionsSheet.Column(1).Width = 100;
                instructionsSheet.Rows().Style.Alignment.WrapText = true;

                // Générer le fichier en mémoire
                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;

                return File(stream.ToArray(), 
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                    "modele_import_menus.xlsx");
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
        /// Traite le fichier d'importation Excel - Format condensé: 1 ligne = 1 jour avec toutes les formules
        /// </summary>
        private async Task<ImportResultViewModel> ProcessImportFile(ImportFormuleViewModel model)
        {
            var result = new ImportResultViewModel();
            var lignesImportees = 0;
            var lignesErreur = 0;
            var formulesCreees = 0;
            var erreurs = new List<string>();

            try
            {
                using var stream = model.FichierExcel?.OpenReadStream();
                using var workbook = new ClosedXML.Excel.XLWorkbook(stream);
                var worksheet = workbook.Worksheets.First();

                var usedRows = worksheet.RowsUsed().Skip(1); // Ignorer l'en-tête
                var totalRows = usedRows.Count();
                result.TotalLignes = totalRows;

                // Traiter chaque ligne - Format: 1 ligne = 1 jour complet
                foreach (var row in usedRows)
                {
                    try
                    {
                        var rowNumber = row.RowNumber();
                        
                        // Lire les valeurs des cellules selon le nouveau format
                        var dateValue = row.Cell(1).GetString();
                        var entree = row.Cell(2).GetString();
                        var dessert = row.Cell(3).GetString();
                        var plat = row.Cell(4).GetString();
                        var garniture = row.Cell(5).GetString();
                        var feculent = row.Cell(6).GetString();
                        var legumes = row.Cell(7).GetString();
                        var platStandard1 = row.Cell(8).GetString();
                        var garnitureStandard1 = row.Cell(9).GetString();
                        var platStandard2 = row.Cell(10).GetString();
                        var garnitureStandard2 = row.Cell(11).GetString();

                        // Vérifier les champs obligatoires
                        var erreursLigne = new List<string>();
                        
                        if (string.IsNullOrEmpty(dateValue))
                        {
                            erreursLigne.Add("Colonne A (Date) est vide");
                        }
                        
                        // Vérifier qu'au moins un champ de formule est rempli
                        var hasAnyFormulaContent = !string.IsNullOrEmpty(entree) || 
                                                 !string.IsNullOrEmpty(plat) ||
                                                 !string.IsNullOrEmpty(garniture) ||
                                                 !string.IsNullOrEmpty(dessert) ||
                                                 !string.IsNullOrEmpty(platStandard1) ||
                                                 !string.IsNullOrEmpty(garnitureStandard1) ||
                                                 !string.IsNullOrEmpty(platStandard2) ||
                                                 !string.IsNullOrEmpty(garnitureStandard2) ||
                                                 !string.IsNullOrEmpty(feculent) ||
                                                 !string.IsNullOrEmpty(legumes);
                        
                        if (!hasAnyFormulaContent)
                        {
                            erreursLigne.Add("Aucun champ de formule rempli. Remplissez au moins un champ pour créer une formule");
                        }
                        
                        if (erreursLigne.Any())
                        {
                            erreurs.Add($"Ligne {rowNumber}: {string.Join(", ", erreursLigne)}");
                            lignesErreur++;
                            continue;
                        }

                        // Validation de la date avec détails - Support DD/MM/YYYY, YYYY-MM-DD et formats avec heure
                        DateTime dateFormule;
                        try
                        {
                            // Nettoyer la valeur de date (enlever les espaces en début/fin)
                            dateValue = dateValue.Trim();
                            
                            // Essayer d'abord le format DD/MM/YYYY avec heure (format Excel)
                            if (dateValue.Contains("/") && dateValue.Contains(":"))
                            {
                                dateFormule = DateTime.ParseExact(dateValue, "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                            }
                            // Essayer le format DD/MM/YYYY sans heure
                            else if (dateValue.Contains("/"))
                            {
                                dateFormule = DateTime.ParseExact(dateValue, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                            }
                            // Essayer le format DD-MM-YYYY avec tirets
                            else if (dateValue.Contains("-") && dateValue.Split('-')[0].Length == 2)
                            {
                                dateFormule = DateTime.ParseExact(dateValue, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture);
                            }
                            // Essayer le format YYYY-MM-DD avec heure
                            else if (dateValue.Contains("-") && dateValue.Contains(":"))
                            {
                                dateFormule = DateTime.ParseExact(dateValue, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                // Format YYYY-MM-DD (ISO) ou autres formats supportés par DateTime.Parse
                                dateFormule = DateTime.Parse(dateValue);
                            }
                        }
                        catch (Exception)
                        {
                            erreurs.Add($"Ligne {rowNumber}, Colonne A (Date): Format de date invalide '{dateValue}'. Utilisez le format DD/MM/YYYY (ex: 02/02/2026) ou YYYY-MM-DD (ex: 2026-02-02)");
                            lignesErreur++;
                            continue;
                        }

                        // Supprimer les formules existantes pour cette date si remplacement demandé
                        if (model.RemplacerExistantes)
                        {
                            var formulesExistantes = await _context.FormulesJour
                                .Where(f => f.Date.Date == dateFormule.Date && f.Supprimer == 0)
                                .ToListAsync();
                            
                            foreach (var formuleExistante in formulesExistantes)
                            {
                                formuleExistante.Supprimer = 1;
                                formuleExistante.ModifiedOn = DateTime.Now;
                                formuleExistante.ModifiedBy = User.Identity?.Name ?? "System";
                            }
                        }
                        else
                        {
                            // Vérifier si des formules existent déjà pour cette date
                            var formuleExistante = await _context.FormulesJour
                                .FirstOrDefaultAsync(f => f.Date.Date == dateFormule.Date && f.Supprimer == 0);

                            if (formuleExistante != null)
                            {
                                erreurs.Add($"Ligne {rowNumber}: Des formules existent déjà pour la date {dateFormule:dd/MM/yyyy}. Cochez 'Remplacer les formules existantes' pour les remplacer.");
                                lignesErreur++;
                                continue;
                            }
                        }

                        // Créer les 3 formules pour cette date (seulement si les champs correspondants sont remplis)
                        var formulesPourDate = new List<FormuleJour>();

                        // 1. Formule Améliorée - Créée si au moins un champ est rempli
                        if (!string.IsNullOrEmpty(entree) || 
                            !string.IsNullOrEmpty(plat) ||
                            !string.IsNullOrEmpty(garniture) ||
                            !string.IsNullOrEmpty(dessert) ||
                            !string.IsNullOrEmpty(feculent) ||
                            !string.IsNullOrEmpty(legumes))
                        {
                            formulesPourDate.Add(new FormuleJour
                            {
                                IdFormule = Guid.NewGuid(),
                                Date = dateFormule,
                                NomFormule = "Formule Améliorée",
                                Entree = string.IsNullOrEmpty(entree) ? null : entree,
                                Plat = string.IsNullOrEmpty(plat) ? null : plat,
                                Garniture = string.IsNullOrEmpty(garniture) ? null : garniture,
                                Dessert = string.IsNullOrEmpty(dessert) ? null : dessert,
                                Feculent = string.IsNullOrEmpty(feculent) ? null : feculent,
                                Legumes = string.IsNullOrEmpty(legumes) ? null : legumes,
                                Marge = null,
                                Statut = 1,
                                Verrouille = false,
                                CreatedOn = DateTime.Now,
                                CreatedBy = User.Identity?.Name ?? "System"
                            });
                        }

                        // 2. Formule Standard 1 - Créée si au moins un champ est rempli
                        if (!string.IsNullOrEmpty(platStandard1) || 
                            !string.IsNullOrEmpty(garnitureStandard1))
                        {
                            formulesPourDate.Add(new FormuleJour
                            {
                                IdFormule = Guid.NewGuid(),
                                Date = dateFormule,
                                NomFormule = "Formule Standard 1",
                                PlatStandard1 = string.IsNullOrEmpty(platStandard1) ? null : platStandard1,
                                GarnitureStandard1 = string.IsNullOrEmpty(garnitureStandard1) ? null : garnitureStandard1,
                                Marge = null,
                                Statut = 1,
                                Verrouille = false,
                                CreatedOn = DateTime.Now,
                                CreatedBy = User.Identity?.Name ?? "System"
                            });
                        }

                        // 3. Formule Standard 2 - Créée si au moins un champ est rempli
                        if (!string.IsNullOrEmpty(platStandard2) || 
                            !string.IsNullOrEmpty(garnitureStandard2))
                        {
                            formulesPourDate.Add(new FormuleJour
                            {
                                IdFormule = Guid.NewGuid(),
                                Date = dateFormule,
                                NomFormule = "Formule Standard 2",
                                PlatStandard2 = string.IsNullOrEmpty(platStandard2) ? null : platStandard2,
                                GarnitureStandard2 = string.IsNullOrEmpty(garnitureStandard2) ? null : garnitureStandard2,
                                Marge = null,
                                Statut = 1,
                                Verrouille = false,
                                CreatedOn = DateTime.Now,
                                CreatedBy = User.Identity?.Name ?? "System"
                            });
                        }

                        // Ajouter toutes les formules créées
                        foreach (var formule in formulesPourDate)
                        {
                            _context.FormulesJour.Add(formule);
                            formulesCreees++;
                        }

                        lignesImportees++;
                    }
                    catch (Exception ex)
                    {
                        var rowNumber = row.RowNumber();
                        var cellValues = new List<string>();
                        
                        // Récupérer les valeurs des cellules pour diagnostic
                        for (int col = 1; col <= 11; col++)
                        {
                            try
                            {
                                var cellValue = row.Cell(col).GetString();
                                cellValues.Add($"Col{col}: '{cellValue}'");
                            }
                            catch
                            {
                                cellValues.Add($"Col{col}: [Erreur lecture]");
                            }
                        }
                        
                        erreurs.Add($"Ligne {rowNumber}: Erreur générale - {ex.Message}. Valeurs: {string.Join(", ", cellValues)}");
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
                    result.Message = $"{lignesImportees} lignes traitées, {formulesCreees} formules créées avec succès.";
                }
                else
                {
                    result.Success = false;
                    result.Message = $"{lignesErreur} erreurs détectées. Import annulé.";
                }

                result.LignesImportees = lignesImportees;
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

        /// <summary>
        /// Exporte la liste des formules vers Excel
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ExportExcel(DateTime? dateDebut, DateTime? dateFin, bool semaineSuivante = false)
        {
            try
            {
                // Utiliser la même logique que l'action Index pour récupérer les données
                var query = _context.FormulesJour
                    .Include(f => f.NomFormuleNavigation)
                    .Where(f => f.Supprimer == 0);

                // Appliquer les filtres de date
                if (dateDebut.HasValue)
                {
                    query = query.Where(f => f.Date >= dateDebut.Value.Date);
                }

                if (dateFin.HasValue)
                {
                    query = query.Where(f => f.Date <= dateFin.Value.Date);
                }

                // Filtre semaine suivante
                if (semaineSuivante)
                {
                    var (lundi, dimanche) = GetSemaineSuivanteComplete();
                    query = query.Where(f => f.Date >= lundi && f.Date <= dimanche);
                }

                // Récupérer toutes les données (sans pagination pour l'export)
                var formules = await query
                    .OrderByDescending(f => f.Date)
                    .Select(f => new FormuleJourViewModel
                    {
                        IdFormule = f.IdFormule,
                        Date = f.Date,
                        NomFormule = f.NomFormule,
                        Entree = f.Entree,
                        Plat = f.Plat,
                        Garniture = f.Garniture,
                        Dessert = f.Dessert,
                        PlatStandard1 = f.PlatStandard1,
                        GarnitureStandard1 = f.GarnitureStandard1,
                        PlatStandard2 = f.PlatStandard2,
                        GarnitureStandard2 = f.GarnitureStandard2,
                        CreatedOn = f.CreatedOn,
                        CreatedBy = f.CreatedBy
                    })
                    .ToListAsync();

                // Générer le nom du fichier
                var fileName = $"Formules_{DateTime.Now:yyyyMMdd_HHmmss}";
                var title = "Liste des Formules du Jour";

                // Exporter vers Excel
                var excelBytes = _excelExportService.ExportToExcel(formules, fileName, "Formules", title);

                _logger.LogInformation("Export Excel des formules effectué - {Count} formules exportées", formules.Count);

                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{fileName}.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'export Excel des formules");
                TempData["ErrorMessage"] = "Une erreur est survenue lors de l'export Excel.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Retourne le lundi et le dimanche de la semaine N+1 (semaine complète)
        /// </summary>
        private (DateTime Lundi, DateTime Dimanche) GetSemaineSuivanteComplete()
        {
            var today = DateTime.Today;
            int diffToMonday = ((int)today.DayOfWeek + 6) % 7; // Lundi=0
            var thisWeekMonday = today.AddDays(-diffToMonday).Date;

            var nextWeekMonday = thisWeekMonday.AddDays(7);
            var nextWeekSunday = nextWeekMonday.AddDays(6);
            return (nextWeekMonday, nextWeekSunday);
        }
    }
}
