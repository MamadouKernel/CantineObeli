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
        /// Télécharge un modèle Excel pour l'importation
        /// </summary>
        [HttpGet]
        public IActionResult DownloadTemplate()
        {
            try
            {
                using var workbook = new ClosedXML.Excel.XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Formules");

                // En-têtes
                var headers = new[]
                {
                    "Date", "NomFormule", "Entree", "Plat", "Garniture", "Dessert",
                    "PlatStandard1", "GarnitureStandard1", "PlatStandard2", "GarnitureStandard2",
                    "Feculent", "Legumes", "Marge", "Statut"
                };

                // Style pour les en-têtes
                var headerRow = worksheet.Row(1);
                headerRow.Style.Font.Bold = true;
                headerRow.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightBlue;
                headerRow.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

                // Ajouter les en-têtes
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cell(1, i + 1).Value = headers[i];
                }

                // Données d'exemple - Menu 1 (Formule Améliorée)
                var sampleData1 = new[]
                {
                    "2024-01-15", "Formule Améliorée", "Salade verte aux tomates", "Poulet rôti aux herbes", "Riz pilaf aux légumes", "Fruit de saison",
                    "Sauce graine", "Viande de bœuf", "Attieke", "Poisson grillé", "Riz blanc", "Légumes de saison", "0", "1"
                };

                // Données d'exemple - Menu 2 (Formule Standard)
                var sampleData2 = new[]
                {
                    "2024-01-16", "Formule Standard", "", "", "", "",
                    "Sauce arachide", "Poulet", "Foutou", "Poisson", "Riz", "Épinards", "0", "1"
                };

                // Données d'exemple - Menu 3 (Formule Mixte)
                var sampleData3 = new[]
                {
                    "2024-01-17", "Formule Mixte", "Carottes râpées", "Agouti sauce", "Riz parfumé", "Banane",
                    "Sauce tomate", "Agouti", "Igname pilée", "Poisson fumé", "Riz", "Gombo", "5", "1"
                };

                // Ajouter les exemples
                for (int i = 0; i < sampleData1.Length; i++)
                {
                    worksheet.Cell(2, i + 1).Value = sampleData1[i];
                }

                for (int i = 0; i < sampleData2.Length; i++)
                {
                    worksheet.Cell(3, i + 1).Value = sampleData2[i];
                }

                for (int i = 0; i < sampleData3.Length; i++)
                {
                    worksheet.Cell(4, i + 1).Value = sampleData3[i];
                }

                // Ajuster la largeur des colonnes
                worksheet.Columns().AdjustToContents();

                // Ajouter des instructions
                worksheet.Cell(6, 1).Value = "Instructions :";
                worksheet.Cell(6, 1).Style.Font.Bold = true;
                
                var instructions = new[]
                {
                    "• Date : Format YYYY-MM-DD (ex: 2024-01-15)",
                    "• NomFormule : Nom de la formule (ex: Formule Améliorée, Formule Standard, Formule Mixte)",
                    "",
                    "TYPES DE FORMULES :",
                    "• FORMULE AMÉLIORÉE : Remplir Entree, Plat, Garniture, Dessert (ligne 2)",
                    "• FORMULE STANDARD : Remplir PlatStandard1, GarnitureStandard1, PlatStandard2, GarnitureStandard2 (ligne 3)",
                    "• FORMULE MIXTE : Combiner éléments améliorés ET standard (ligne 4)",
                    "",
                    "CHAMPS OBLIGATOIRES :",
                    "• Date et NomFormule sont toujours obligatoires",
                    "• Pour formule améliorée : au moins Entree OU Plat",
                    "• Pour formule standard : au moins PlatStandard1 OU PlatStandard2",
                    "",
                    "AUTRES CHAMPS :",
                    "• Feculent, Legumes : Compléments communs (optionnels)",
                    "• Marge : Pourcentage de marge (0-100, peut être vide ou 0)",
                    "• Statut : 1 pour actif, 0 pour inactif"
                };

                for (int i = 0; i < instructions.Length; i++)
                {
                    worksheet.Cell(7 + i, 1).Value = instructions[i];
                }

                // Style pour les instructions
                var instructionRange = worksheet.Range(7, 1, 7 + instructions.Length - 1, 1);
                instructionRange.Style.Font.Italic = true;
                instructionRange.Style.Font.FontColor = ClosedXML.Excel.XLColor.DarkGray;

                // Générer le fichier en mémoire
                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;

                return File(stream.ToArray(), 
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                    "modele_formules.xlsx");
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
        /// Traite le fichier d'importation Excel
        /// </summary>
        private async Task<ImportResultViewModel> ProcessImportFile(ImportFormuleViewModel model)
        {
            var result = new ImportResultViewModel();
            var lignesImportees = 0;
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

                // Traiter chaque ligne
                foreach (var row in usedRows)
                {
                    try
                    {
                        var rowNumber = row.RowNumber();
                        
                        // Lire les valeurs des cellules
                        var dateValue = row.Cell(1).GetString();
                        var nomFormule = row.Cell(2).GetString();
                        var entree = row.Cell(3).GetString();
                        var plat = row.Cell(4).GetString();
                        var garniture = row.Cell(5).GetString();
                        var dessert = row.Cell(6).GetString();
                        var platStandard1 = row.Cell(7).GetString();
                        var garnitureStandard1 = row.Cell(8).GetString();
                        var platStandard2 = row.Cell(9).GetString();
                        var garnitureStandard2 = row.Cell(10).GetString();
                        var feculent = row.Cell(11).GetString();
                        var legumes = row.Cell(12).GetString();
                        var margeValue = row.Cell(13).GetString();
                        var statutValue = row.Cell(14).GetString();

                        // Vérifier les champs obligatoires
                        if (string.IsNullOrEmpty(dateValue) || string.IsNullOrEmpty(nomFormule))
                        {
                            erreurs.Add($"Ligne {rowNumber}: Date et NomFormule sont obligatoires");
                            lignesErreur++;
                            continue;
                        }

                        var formule = new FormuleJour
                        {
                            IdFormule = Guid.NewGuid(),
                            Date = DateTime.Parse(dateValue),
                            NomFormule = nomFormule,
                            Entree = string.IsNullOrEmpty(entree) ? null : entree,
                            Plat = string.IsNullOrEmpty(plat) ? null : plat,
                            Garniture = string.IsNullOrEmpty(garniture) ? null : garniture,
                            Dessert = string.IsNullOrEmpty(dessert) ? null : dessert,
                            PlatStandard1 = string.IsNullOrEmpty(platStandard1) ? null : platStandard1,
                            GarnitureStandard1 = string.IsNullOrEmpty(garnitureStandard1) ? null : garnitureStandard1,
                            PlatStandard2 = string.IsNullOrEmpty(platStandard2) ? null : platStandard2,
                            GarnitureStandard2 = string.IsNullOrEmpty(garnitureStandard2) ? null : garnitureStandard2,
                            Feculent = string.IsNullOrEmpty(feculent) ? null : feculent,
                            Legumes = string.IsNullOrEmpty(legumes) ? null : legumes,
                            Marge = string.IsNullOrEmpty(margeValue) || margeValue == "0" ? 0 : int.Parse(margeValue),
                            Statut = string.IsNullOrEmpty(statutValue) ? null : int.Parse(statutValue),
                            CreatedOn = DateTime.Now,
                            CreatedBy = User.Identity?.Name ?? "System"
                        };

                        // Vérifier si une formule existe déjà pour cette date
                        var formuleExistante = await _context.FormulesJour
                            .FirstOrDefaultAsync(f => f.Date.Date == formule.Date.Date && f.Supprimer == 0);

                        if (formuleExistante != null && !model.RemplacerExistantes)
                        {
                            erreurs.Add($"Ligne {rowNumber}: Une formule existe déjà pour la date {formule.Date:dd/MM/yyyy}");
                            lignesErreur++;
                            continue;
                        }

                        if (formuleExistante != null && model.RemplacerExistantes)
                        {
                            // Remplacer la formule existante
                            formuleExistante.NomFormule = formule.NomFormule;
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
                        }
                        else
                        {
                            _context.FormulesJour.Add(formule);
                        }

                        lignesImportees++;
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
                    result.Message = $"{lignesImportees} formules importées avec succès.";
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
    }
}
