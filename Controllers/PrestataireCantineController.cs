using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Obeli_K.Data;
using Obeli_K.Models;
using Obeli_K.Models.ViewModels;
using Obeli_K.Models.Enums;
using Obeli_K.Services;
using Obeli_K.Enums;
using System.Security.Claims;

namespace Obeli_K.Controllers
{
    [Authorize(Roles = "Administrateur,RH,PrestataireCantine")]
    public class PrestataireCantineController : Controller
    {
        private readonly ObeliDbContext _context;
        private readonly ILogger<PrestataireCantineController> _logger;
        private readonly ExcelExportService _excelExportService;

        public PrestataireCantineController(ObeliDbContext context, ILogger<PrestataireCantineController> logger, ExcelExportService excelExportService)
        {
            _context = context;
            _logger = logger;
            _excelExportService = excelExportService;
        }

        /// <summary>
        /// Affiche la liste des prestataires
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> List()
        {
            try
            {
                var prestataires = await _context.PrestataireCantines
                    .Where(p => p.Supprimer == 0)
                    .OrderBy(p => p.Nom)
                    .Select(p => new
                    {
                        p.Id,
                        p.Nom,
                        p.Contact,
                        p.Email,
                        p.Telephone,
                        p.Adresse,
                        p.CreatedAt,
                        p.CreatedBy,
                        p.ModifiedAt,
                        p.ModifiedBy
                    })
                    .ToListAsync();

                ViewBag.Prestataires = prestataires;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement de la liste des prestataires");
                TempData["ErrorMessage"] = "Une erreur est survenue lors du chargement des prestataires.";
                return View(new List<object>());
            }
        }

        /// <summary>
        /// Affiche le formulaire de création d'un prestataire
        /// </summary>
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Traite la création d'un nouveau prestataire
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PrestataireCantine prestataire)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    prestataire.CreatedAt = DateTime.UtcNow;
                    prestataire.CreatedBy = User.Identity?.Name ?? "System";
                    prestataire.ModifiedAt = DateTime.UtcNow;
                    prestataire.ModifiedBy = User.Identity?.Name ?? "System";
                    prestataire.Supprimer = 0;

                    _context.PrestataireCantines.Add(prestataire);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Prestataire créé avec succès : {Nom}", prestataire.Nom);
                    TempData["SuccessMessage"] = $"Le prestataire {prestataire.Nom} a été créé avec succès.";
                    return RedirectToAction(nameof(List));
                }

                return View(prestataire);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création du prestataire");
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la création du prestataire.";
                return View(prestataire);
            }
        }

        /// <summary>
        /// Affiche les détails d'un prestataire
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var prestataire = await _context.PrestataireCantines
                    .FirstOrDefaultAsync(p => p.Id == id && p.Supprimer == 0);

                if (prestataire == null)
                {
                    TempData["ErrorMessage"] = "Prestataire introuvable.";
                    return RedirectToAction(nameof(List));
                }

                return View(prestataire);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement des détails du prestataire {Id}", id);
                TempData["ErrorMessage"] = "Une erreur est survenue lors du chargement des détails.";
                return RedirectToAction(nameof(List));
            }
        }

        /// <summary>
        /// Affiche le formulaire d'édition d'un prestataire
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var prestataire = await _context.PrestataireCantines
                    .FirstOrDefaultAsync(p => p.Id == id && p.Supprimer == 0);

                if (prestataire == null)
                {
                    TempData["ErrorMessage"] = "Prestataire introuvable.";
                    return RedirectToAction(nameof(List));
                }

                return View(prestataire);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement du prestataire pour édition {Id}", id);
                TempData["ErrorMessage"] = "Une erreur est survenue lors du chargement du prestataire.";
                return RedirectToAction(nameof(List));
            }
        }

        /// <summary>
        /// Traite la modification d'un prestataire
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, PrestataireCantine model)
        {
            try
            {
                if (id != model.Id)
                {
                    TempData["ErrorMessage"] = "Identifiant invalide.";
                    return RedirectToAction(nameof(List));
                }

                var prestataire = await _context.PrestataireCantines
                    .FirstOrDefaultAsync(p => p.Id == id && p.Supprimer == 0);

                if (prestataire == null)
                {
                    TempData["ErrorMessage"] = "Prestataire introuvable.";
                    return RedirectToAction(nameof(List));
                }

                if (ModelState.IsValid)
                {
                    prestataire.Nom = model.Nom;
                    prestataire.Contact = model.Contact;
                    prestataire.Email = model.Email;
                    prestataire.Telephone = model.Telephone;
                    prestataire.Adresse = model.Adresse;
                    prestataire.ModifiedAt = DateTime.UtcNow;
                    prestataire.ModifiedBy = User.Identity?.Name ?? "System";

                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Prestataire modifié avec succès : {Nom}", prestataire.Nom);
                    TempData["SuccessMessage"] = $"Le prestataire {prestataire.Nom} a été modifié avec succès.";
                    return RedirectToAction(nameof(List));
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la modification du prestataire {Id}", id);
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la modification du prestataire.";
                return View(model);
            }
        }

        /// <summary>
        /// Supprime un prestataire (soft delete)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var prestataire = await _context.PrestataireCantines
                    .FirstOrDefaultAsync(p => p.Id == id && p.Supprimer == 0);

                if (prestataire == null)
                {
                    TempData["ErrorMessage"] = "Prestataire introuvable.";
                    return RedirectToAction(nameof(List));
                }

                prestataire.Supprimer = 1;
                prestataire.ModifiedAt = DateTime.UtcNow;
                prestataire.ModifiedBy = User.Identity?.Name ?? "System";

                await _context.SaveChangesAsync();

                _logger.LogInformation("Prestataire supprimé : {Nom}", prestataire.Nom);
                TempData["SuccessMessage"] = $"Le prestataire {prestataire.Nom} a été supprimé avec succès.";

                return RedirectToAction(nameof(List));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression du prestataire {Id}", id);
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la suppression du prestataire.";
                return RedirectToAction(nameof(List));
            }
        }

        /// <summary>
        /// Affiche la page de génération de commande pour prestataires
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GenererCommande(DateTime? dateDebut, DateTime? dateFin, int page = 1, int pageSize = 5, bool showQuantities = false)
        {
            try
            {
                // Si aucune date n'est fournie, utiliser la semaine N+1 par défaut
                if (!dateDebut.HasValue || !dateFin.HasValue)
                {
                    var semaineSuivante = GetSemaineSuivanteComplete();
                    dateDebut = semaineSuivante.DateDebut;
                    dateFin = semaineSuivante.DateFin;
                }

                ViewBag.DateDebut = dateDebut;
                ViewBag.DateFin = dateFin;
                ViewBag.ShowQuantities = showQuantities;

                // Si on veut afficher les quantités à commander au prestataire
                if (showQuantities)
                {
                    return await ShowQuantitiesToOrder(dateDebut.Value, dateFin.Value);
                }

                // Récupérer les commandes de la semaine N+1
                var query = _context.Commandes
                    .Where(c => c.Supprimer == 0 && 
                               c.Date >= dateDebut.Value && 
                               c.Date <= dateFin.Value)
                    .Include(c => c.Utilisateur)
                    .Include(c => c.FormuleJour)
                    .OrderByDescending(c => c.Date);

                var totalCount = await query.CountAsync();

                var commandes = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(c => new CommandeListViewModel
                    {
                        IdCommande = c.IdCommande,
                        CodeCommande = c.CodeCommande,
                        Date = c.Date,
                        DateConsommation = c.DateConsommation,
                        Quantite = c.Quantite,
                        StatusCommande = (StatutCommande)c.StatusCommande,
                        PeriodeService = c.PeriodeService,
                        Site = c.Site,
                        UtilisateurNom = c.Utilisateur != null ? (c.Utilisateur.Nom ?? "") + " " + (c.Utilisateur.Prenoms ?? "") : "N/A",
                        UtilisateurMatricule = c.Utilisateur != null ? c.Utilisateur.UserName : "N/A",
                        FormuleNom = c.FormuleJour != null ? 
                            (c.FormuleJour.Plat ?? "N/A") : "N/A",
                        TypeFormuleNom = "Standard", // Valeur par défaut
                        Montant = c.Montant
                    })
                    .ToListAsync();

                // Créer le modèle de pagination
                var pagination = new PaginationViewModel(HttpContext, "GenererCommande", "PrestataireCantine")
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalItems = totalCount
                };

                // Les paramètres de filtre sont automatiquement conservés par la méthode GetPageUrl

                var pagedModel = new PagedViewModel<CommandeListViewModel>
                {
                    Items = commandes,
                    Pagination = pagination
                };

                return View(pagedModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement de la page de génération de commande");
                TempData["ErrorMessage"] = "Une erreur est survenue lors du chargement de la page.";
                return View(new PagedViewModel<CommandeListViewModel>());
            }
        }

        /// <summary>
        /// Calcule la semaine suivante complète (lundi au dimanche)
        /// </summary>
        private (DateTime DateDebut, DateTime DateFin) GetSemaineSuivanteComplete()
        {
            try
            {
                _logger.LogInformation("GetSemaineSuivanteComplete: Début");
                
                var aujourdhui = DateTime.Today;
                _logger.LogInformation("GetSemaineSuivanteComplete: Aujourd'hui = {Aujourdhui}", aujourdhui);
                
                var joursJusquaLundi = ((int)DayOfWeek.Monday - (int)aujourdhui.DayOfWeek + 7) % 7;
                _logger.LogInformation("GetSemaineSuivanteComplete: Jours jusqu'au lundi = {JoursJusquaLundi}", joursJusquaLundi);
                
                var lundiProchain = aujourdhui.AddDays(joursJusquaLundi == 0 ? 7 : joursJusquaLundi);
                _logger.LogInformation("GetSemaineSuivanteComplete: Lundi prochain = {LundiProchain}", lundiProchain);
                
                var dimancheProchain = lundiProchain.AddDays(6);
                _logger.LogInformation("GetSemaineSuivanteComplete: Dimanche prochain = {DimancheProchain}", dimancheProchain);

                _logger.LogInformation("GetSemaineSuivanteComplete: Retour ({DateDebut}, {DateFin})", lundiProchain, dimancheProchain);
                return (lundiProchain, dimancheProchain);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur dans GetSemaineSuivanteComplete: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Affiche les quantités à commander au prestataire par date
        /// </summary>
        private async Task<IActionResult> ShowQuantitiesToOrder(DateTime dateDebut, DateTime dateFin)
        {
            try
            {
                // Récupérer les quotas douaniers
                var groupeDouaniers = await _context.GroupesNonCit
                    .FirstOrDefaultAsync(g => g.Nom == "Douaniers" && g.Supprimer == 0);

                // Récupérer TOUTES les formules de la période (même sans commandes)
                var formulesAvecQuantites = await _context.FormulesJour
                    .Where(f => f.Supprimer == 0 && f.Date >= dateDebut && f.Date <= dateFin)
                    .Include(f => f.Commandes.Where(c => c.Supprimer == 0))
                    .OrderBy(f => f.Date)
                    .ThenBy(f => f.NomFormule) // Trier par type de formule pour un affichage cohérent
                    .ToListAsync();

                // Créer les formules pour chaque date
                var quantitesParDate = new List<QuantiteCommandePrestataireViewModel>();

                var dates = formulesAvecQuantites.Select(f => f.Date.Date).Distinct().OrderBy(d => d);

                foreach (var date in dates)
                {
                    var formulesDuJour = formulesAvecQuantites.Where(f => f.Date.Date == date).ToList();
                    var formules = new List<FormuleQuantiteViewModel>();

                    foreach (var formule in formulesDuJour)
                    {
                        // Calculer les quantités pour cette formule spécifique
                        var quantiteJour = formule.Commandes.Where(c => c.Supprimer == 0 && c.PeriodeService == Periode.Jour).Sum(c => c.Quantite);
                        var quantiteNuit = formule.Commandes.Where(c => c.Supprimer == 0 && c.PeriodeService == Periode.Nuit).Sum(c => c.Quantite);

                        // Ajouter les quotas douaniers par défaut si le groupe existe
                        if (groupeDouaniers != null)
                        {
                            quantiteJour += groupeDouaniers.QuotaJournalier ?? 0;
                            quantiteNuit += groupeDouaniers.QuotaNuit ?? 0;
                        }

                        // Déterminer le type de formule basé sur NomFormule
                        string typeFormule = "Standard";
                        if (formule.NomFormule?.Contains("Améliorée") == true || formule.NomFormule?.Contains("Amélioré") == true)
                        {
                            typeFormule = "Amélioré";
                        }
                        else if (formule.NomFormule?.Contains("Standard 1") == true)
                        {
                            typeFormule = "Standard 1";
                        }
                        else if (formule.NomFormule?.Contains("Standard 2") == true)
                        {
                            typeFormule = "Standard 2";
                        }

                        // Créer la formule avec les bonnes données selon le type
                        var formuleQuantite = new FormuleQuantiteViewModel
                        {
                            IdFormule = formule.IdFormule,
                            TypeFormule = typeFormule,
                            QuantiteJour = quantiteJour,
                            QuantiteNuit = quantiteNuit,
                            Marge = formule.Marge ?? 0
                        };

                        // Remplir les champs selon le type de formule
                        if (typeFormule == "Amélioré")
                        {
                            formuleQuantite.Plat = formule.Plat ?? "";
                            formuleQuantite.Garniture = formule.Garniture ?? "";
                            formuleQuantite.Entree = formule.Entree ?? "";
                            formuleQuantite.Dessert = formule.Dessert ?? "";
                            formuleQuantite.Legumes = formule.Legumes ?? "";
                            formuleQuantite.Feculent = formule.Feculent ?? "";
                        }
                        else if (typeFormule == "Standard 1")
                        {
                            formuleQuantite.Plat = formule.PlatStandard1 ?? "";
                            formuleQuantite.Garniture = formule.GarnitureStandard1 ?? "";
                            formuleQuantite.Entree = "";
                            formuleQuantite.Dessert = "";
                            formuleQuantite.Legumes = formule.Legumes ?? "";
                            formuleQuantite.Feculent = formule.Feculent ?? "";
                        }
                        else if (typeFormule == "Standard 2")
                        {
                            formuleQuantite.Plat = formule.PlatStandard2 ?? "";
                            formuleQuantite.Garniture = formule.GarnitureStandard2 ?? "";
                            formuleQuantite.Entree = "";
                            formuleQuantite.Dessert = "";
                            formuleQuantite.Legumes = formule.Legumes ?? "";
                            formuleQuantite.Feculent = formule.Feculent ?? "";
                        }

                        formules.Add(formuleQuantite);
                    }

                    if (formules.Any())
                    {
                        quantitesParDate.Add(new QuantiteCommandePrestataireViewModel
                        {
                            Date = date,
                            Formules = formules.OrderBy(f => f.TypeFormule).ToList() // Trier par type
                        });
                    }
                }

                ViewBag.DateDebut = dateDebut;
                ViewBag.DateFin = dateFin;
                ViewBag.ShowQuantities = true;

                // Vérifier si un export a déjà été effectué pour cette période
                var exportAlreadyDone = await IsExportAlreadyDoneAsync(dateDebut, dateFin);
                var canModifyMarges = CanModifyMarges();
                var isBlocked = exportAlreadyDone && !canModifyMarges;

                ViewBag.IsExportAlreadyDone = exportAlreadyDone;
                ViewBag.CanModifyMarges = canModifyMarges;
                ViewBag.IsBlocked = isBlocked;

                // Si bloqué, récupérer les informations du dernier export
                if (isBlocked)
                {
                    var lastExport = await GetLastExportAsync(dateDebut, dateFin);
                    ViewBag.LastExport = lastExport;
                }
                
                // Ajouter les informations sur les quotas douaniers
                if (groupeDouaniers != null)
                {
                    ViewBag.QuotaDouaniers = new
                    {
                        QuotaJournalier = groupeDouaniers.QuotaJournalier ?? 0,
                        QuotaNuit = groupeDouaniers.QuotaNuit ?? 0,
                        Nom = groupeDouaniers.Nom
                    };
                }
                else
                {
                    ViewBag.QuotaDouaniers = null;
                }

                return View("QuantitesCommandePrestataire", quantitesParDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement des quantités à commander");
                TempData["ErrorMessage"] = "Une erreur est survenue lors du chargement des quantités.";
                return RedirectToAction(nameof(GenererCommande));
            }
        }

        /// <summary>
        /// Traite la génération de commande pour prestataires
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GenererCommande(GenererCommandePrestataireViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Logique de génération de commande pour prestataires
                    // À implémenter selon les besoins spécifiques
                    
                    _logger.LogInformation("Commande générée pour le prestataire {PrestataireId}", model.PrestataireId);
                    TempData["SuccessMessage"] = "Commande générée avec succès pour le prestataire.";
                    return RedirectToAction(nameof(List));
                }

                // Recharger les données en cas d'erreur
                return RedirectToAction(nameof(GenererCommande));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la génération de commande");
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la génération de commande.";
                return RedirectToAction(nameof(GenererCommande));
            }
        }

        /// <summary>
        /// Affiche la page d'exportations pour prestataires
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Exportations()
        {
            try
            {
                // Récupérer les données d'exportation
                var prestataires = await _context.PrestataireCantines
                    .Where(p => p.Supprimer == 0)
                    .OrderBy(p => p.Nom)
                    .Select(p => new
                    {
                        p.Id,
                        p.Nom,
                        p.Contact,
                        p.Email,
                        p.Telephone,
                        p.Adresse,
                        p.CreatedAt,
                        p.CreatedBy
                    })
                    .ToListAsync();

                ViewBag.Prestataires = prestataires;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement de la page d'exportations");
                TempData["ErrorMessage"] = "Une erreur est survenue lors du chargement de la page.";
                return View();
            }
        }

        /// <summary>
        /// Exporte les données des prestataires en Excel
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ExportExcel()
        {
            try
            {
                var prestataires = await _context.PrestataireCantines
                    .Where(p => p.Supprimer == 0)
                    .OrderBy(p => p.Nom)
                    .Select(p => new
                    {
                        Nom = p.Nom,
                        Contact = p.Contact,
                        Email = p.Email,
                        Telephone = p.Telephone,
                        Adresse = p.Adresse,
                        DateCreation = p.CreatedAt,
                        CreePar = p.CreatedBy
                    })
                    .ToListAsync();

                var fileBytes = _excelExportService.ExportToExcel(
                    prestataires,
                    "Prestataires_Cantine.xlsx",
                    "Prestataires",
                    "Liste des prestataires de cantine"
                );

                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Prestataires_Cantine.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'export Excel des prestataires");
                TempData["ErrorMessage"] = "Une erreur est survenue lors de l'export.";
                return RedirectToAction(nameof(Exportations));
            }
        }

        /// <summary>
        /// Action de test simple pour vérifier que le contrôleur fonctionne
        /// </summary>
        [HttpGet]
        public IActionResult TestAjouterMarges()
        {
            _logger.LogInformation("=== TEST AjouterMarges ===");
            return Content("Test réussi - Le contrôleur fonctionne !");
        }

        /// <summary>
        /// Affiche la page de gestion simple des marges
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GestionMarges()
        {
            try
            {
                // Utiliser la semaine N+1 par défaut
                var semaineSuivante = GetSemaineSuivanteComplete();
                var dateDebut = semaineSuivante.DateDebut;
                var dateFin = semaineSuivante.DateFin;

                ViewBag.DateDebut = dateDebut;
                ViewBag.DateFin = dateFin;

                // Récupérer toutes les formules de la période
                var formules = await _context.FormulesJour
                    .Where(f => f.Supprimer == 0 && f.Date >= dateDebut && f.Date <= dateFin)
                    .OrderBy(f => f.Date)
                    .ThenBy(f => f.NomFormule)
                    .ToListAsync();

                return View(formules);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement de la gestion des marges");
                TempData["ErrorMessage"] = $"Une erreur est survenue: {ex.Message}";
                return RedirectToAction(nameof(GenererCommande), new { showQuantities = true });
            }
        }

        /// <summary>
        /// Sauvegarde les marges modifiées
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SauvegarderMarges(List<FormuleJour> model, DateTime dateDebut, DateTime dateFin)
        {
            try
            {
                int margesModifiees = 0;

                foreach (var formule in model)
                {
                    // Récupérer la formule en base
                    var formuleEnBase = await _context.FormulesJour
                        .FirstOrDefaultAsync(f => f.IdFormule == formule.IdFormule && f.Supprimer == 0);

                    if (formuleEnBase != null && formuleEnBase.Marge != formule.Marge)
                    {
                        formuleEnBase.Marge = formule.Marge;
                        formuleEnBase.ModifiedOn = DateTime.UtcNow;
                        formuleEnBase.ModifiedBy = User.Identity?.Name ?? "System";
                        margesModifiees++;
                    }
                }

                if (margesModifiees > 0)
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Marges sauvegardées avec succès. {Count} formules modifiées.", margesModifiees);
                    TempData["SuccessMessage"] = $"Marges sauvegardées avec succès ! {margesModifiees} formules ont été modifiées.";
                }
                else
                {
                    TempData["InfoMessage"] = "Aucune modification détectée.";
                }

                return RedirectToAction(nameof(GestionMarges));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la sauvegarde des marges");
                TempData["ErrorMessage"] = $"Une erreur est survenue lors de la sauvegarde: {ex.Message}";
                return RedirectToAction(nameof(GestionMarges));
            }
        }

        /// <summary>
        /// Affiche la page d'ajout de marges aux quantités
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> AjouterMarges()
        {
            _logger.LogInformation("=== DÉBUT AjouterMarges GET ===");
            
            try
            {
                _logger.LogInformation("Point 1: Début de l'action AjouterMarges");
                
                // Utiliser la semaine N+1 par défaut
                _logger.LogInformation("Point 2: Appel de GetSemaineSuivanteComplete()");
                var semaineSuivante = GetSemaineSuivanteComplete();
                var dateDebut = semaineSuivante.DateDebut;
                var dateFin = semaineSuivante.DateFin;

                _logger.LogInformation("Point 3: Dates calculées - DateDebut: {DateDebut}, DateFin: {DateFin}", dateDebut, dateFin);

                ViewBag.DateDebut = dateDebut;
                ViewBag.DateFin = dateFin;

                // Vérifier si un export a déjà été effectué pour cette période
                var exportAlreadyDone = await IsExportAlreadyDoneAsync(dateDebut, dateFin);
                var canModifyMarges = CanModifyMarges();
                var isBlocked = exportAlreadyDone && !canModifyMarges;

                _logger.LogInformation("Point 4: Vérifications - ExportDone: {ExportDone}, CanModify: {CanModify}, IsBlocked: {IsBlocked}", 
                    exportAlreadyDone, canModifyMarges, isBlocked);

                ViewBag.IsExportAlreadyDone = exportAlreadyDone;
                ViewBag.CanModifyMarges = canModifyMarges;
                ViewBag.IsBlocked = isBlocked;

                // Si bloqué, récupérer les informations du dernier export
                if (isBlocked)
                {
                    var lastExport = await GetLastExportAsync(dateDebut, dateFin);
                    ViewBag.LastExport = lastExport;
                    _logger.LogInformation("Point 5: Export bloqué - Dernier export par {User} le {Date}", 
                        lastExport?.Utilisateur?.UserName, lastExport?.DateExport);
                }

                _logger.LogInformation("Point 5: ViewBag configuré");

                // Récupérer les quantités actuelles
                _logger.LogInformation("Point 6: Appel de GetQuantitesParDate avec dates {DateDebut} à {DateFin}", dateDebut, dateFin);
                var quantitesParDate = await GetQuantitesParDate(dateDebut, dateFin);
                
                _logger.LogInformation("Point 6: Données récupérées, nombre de jours: {Count}", quantitesParDate.Count);
                
                _logger.LogInformation("Point 7: Retour de la vue avec modèle");
                return View(quantitesParDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "=== ERREUR dans AjouterMarges GET ===");
                _logger.LogError("Message d'erreur: {Message}", ex.Message);
                _logger.LogError("Stack trace: {StackTrace}", ex.StackTrace);
                
                TempData["ErrorMessage"] = $"Erreur détaillée: {ex.Message}";
                return RedirectToAction(nameof(GenererCommande), new { showQuantities = true });
            }
        }

        /// <summary>
        /// Traite l'ajout de marges aux quantités
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AjouterMarges(List<QuantiteCommandePrestataireViewModel> model, DateTime dateDebut, DateTime dateFin)
        {
            try
            {
                // Vérifier si un export a déjà été effectué et si l'utilisateur peut modifier
                var exportAlreadyDone = await IsExportAlreadyDoneAsync(dateDebut, dateFin);
                var canModifyMarges = CanModifyMarges();
                var isBlocked = exportAlreadyDone && !canModifyMarges;

                if (isBlocked)
                {
                    _logger.LogWarning("Tentative de modification des marges bloquée - Export déjà effectué par un autre utilisateur");
                    TempData["ErrorMessage"] = "Impossible de modifier les marges car un fichier d'export a déjà été généré pour cette période. Seuls les administrateurs et les responsables RH peuvent modifier les marges après export.";
                    return RedirectToAction(nameof(GenererCommande), new { showQuantities = true, dateDebut, dateFin });
                }

                if (ModelState.IsValid)
                {
                    int margesModifiees = 0;

                    // Sauvegarder les marges dans la base de données
                    foreach (var jour in model)
                    {
                        foreach (var formule in jour.Formules)
                        {
                            // Calculer la marge totale (marge jour + marge nuit)
                            int margeTotale = formule.MargeJour + formule.MargeNuit;

                            // Récupérer la formule dans la base de données
                            var formuleEnBase = await _context.FormulesJour
                                .FirstOrDefaultAsync(f => f.IdFormule == formule.IdFormule && f.Supprimer == 0);

                            if (formuleEnBase != null)
                            {
                                // Mettre à jour la marge
                                formuleEnBase.Marge = margeTotale;
                                formuleEnBase.ModifiedOn = DateTime.UtcNow;
                                formuleEnBase.ModifiedBy = User.Identity?.Name ?? "System";
                                margesModifiees++;
                            }
                        }
                    }

                    // Sauvegarder les modifications
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Marges sauvegardées avec succès pour la période du {DateDebut} au {DateFin}. {MargesModifiees} formules modifiées.", dateDebut, dateFin, margesModifiees);
                    TempData["SuccessMessage"] = $"Marges sauvegardées avec succès ! {margesModifiees} formules ont été modifiées.";

                    // Rediriger vers la vue des quantités avec les marges appliquées
                    return RedirectToAction(nameof(GenererCommande), new { showQuantities = true, dateDebut, dateFin });
                }

                TempData["ErrorMessage"] = "Erreur lors de la validation du formulaire.";
                return RedirectToAction(nameof(GenererCommande), new { showQuantities = true, dateDebut, dateFin });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'ajout des marges");
                TempData["ErrorMessage"] = "Une erreur est survenue lors de l'ajout des marges.";
                return RedirectToAction(nameof(GenererCommande), new { showQuantities = true, dateDebut, dateFin });
            }
        }

        /// <summary>
        /// Exporte les quantités vers un fichier Excel
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ExportQuantites(DateTime? dateDebut, DateTime? dateFin)
        {
            try
            {
                _logger.LogInformation("ExportQuantites: Début avec dateDebut={DateDebut}, dateFin={DateFin}", dateDebut, dateFin);
                
                if (!dateDebut.HasValue || !dateFin.HasValue)
                {
                    _logger.LogWarning("ExportQuantites: Dates manquantes");
                    TempData["ErrorMessage"] = "Les dates de début et de fin sont requises.";
                    return RedirectToAction(nameof(GenererCommande), new { showQuantities = true });
                }

                _logger.LogInformation("ExportQuantites: Récupération des quotas douaniers");
                // Récupérer les quotas douaniers
                var groupeDouaniers = await _context.GroupesNonCit
                    .FirstOrDefaultAsync(g => g.Nom == "Douaniers" && g.Supprimer == 0);
                
                var quotaDouaneTotal = 0;
                if (groupeDouaniers != null)
                {
                    quotaDouaneTotal = (groupeDouaniers.QuotaJournalier ?? 0) + (groupeDouaniers.QuotaNuit ?? 0);
                    _logger.LogInformation("ExportQuantites: Quota douanier trouvé - Jour: {QuotaJour}, Nuit: {QuotaNuit}", 
                        groupeDouaniers.QuotaJournalier, groupeDouaniers.QuotaNuit);
                }
                
                _logger.LogInformation("ExportQuantites: Récupération des quantités");
                // Récupérer les quantités
                var quantitesParDate = await GetQuantitesParDate(dateDebut.Value, dateFin.Value);

                _logger.LogInformation("ExportQuantites: {Count} jours récupérés", quantitesParDate.Count);
                
                if (!quantitesParDate.Any())
                {
                    _logger.LogWarning("ExportQuantites: Aucune donnée trouvée pour la période");
                    TempData["ErrorMessage"] = "Aucune donnée à exporter pour cette période.";
                    return RedirectToAction(nameof(GenererCommande), new { showQuantities = true, dateDebut, dateFin });
                }

                // Préparer les données pour l'export
                var exportData = new List<ExportQuantiteFormuleViewModel>();
                
                foreach (var jour in quantitesParDate)
                {
                    foreach (var formule in jour.Formules)
                    {
                        var quantiteTotale = formule.QuantiteJour + formule.QuantiteNuit;
                        var marge = formule.Marge;
                        var totalAvecMarge = quantiteTotale + quotaDouaneTotal + marge;
                        
                        exportData.Add(new ExportQuantiteFormuleViewModel
                        {
                            Date = jour.Date.ToString("dd/MM/yyyy"),
                            JourSemaine = jour.Date.ToString("dddd", new System.Globalization.CultureInfo("fr-FR")),
                            TypeFormule = formule.TypeFormule ?? "",
                            Plat = formule.Plat ?? "",
                            Garniture = formule.Garniture ?? "",
                            Entree = formule.Entree ?? "",
                            Dessert = formule.Dessert ?? "",
                            Legumes = formule.Legumes ?? "",
                            Feculent = formule.Feculent ?? "",
                            QuantiteJour = formule.QuantiteJour,
                            QuantiteNuit = formule.QuantiteNuit,
                            QuotaDouane = quotaDouaneTotal,
                            Marge = marge,
                            TotalAvecMarge = totalAvecMarge
                        });
                    }
                }

                _logger.LogInformation("ExportQuantites: {Count} lignes à exporter", exportData.Count);

                var fileName = $"Quantites_Commande_Prestataire_{dateDebut.Value:yyyyMMdd}_{dateFin.Value:yyyyMMdd}.xlsx";
                _logger.LogInformation("ExportQuantites: Génération du fichier {FileName}", fileName);
                
                var fileBytes = _excelExportService.ExportToExcel(
                    exportData,
                    fileName,
                    "Quantités",
                    $"Quantités à commander au prestataire - Période du {dateDebut.Value:dd/MM/yyyy} au {dateFin.Value:dd/MM/yyyy}"
                );

                _logger.LogInformation("Export des quantités réussi pour la période du {DateDebut} au {DateFin}, {Size} octets", 
                    dateDebut, dateFin, fileBytes.Length);

                // Enregistrer l'export dans la base de données
                await SaveExportRecordAsync(dateDebut.Value, dateFin.Value, fileName, fileBytes.Length, 
                    $"Export automatique - {exportData.Count} lignes exportées");

                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'export des quantités: {Message}", ex.Message);
                _logger.LogError("Stack trace: {StackTrace}", ex.StackTrace);
                if (ex.InnerException != null)
                {
                    _logger.LogError("Inner exception: {InnerMessage}", ex.InnerException.Message);
                }
                
                TempData["ErrorMessage"] = $"Une erreur est survenue lors de l'export: {ex.Message}";
                return RedirectToAction(nameof(GenererCommande), new { showQuantities = true, dateDebut, dateFin });
            }
        }

        /// <summary>
        /// Méthode helper pour récupérer les quantités par date
        /// </summary>
        private async Task<List<QuantiteCommandePrestataireViewModel>> GetQuantitesParDate(DateTime dateDebut, DateTime dateFin)
        {
            _logger.LogInformation("GetQuantitesParDate: Début avec dates {DateDebut} à {DateFin}", dateDebut, dateFin);
            
            try
            {
                // Récupérer TOUTES les formules de la période (même sans commandes)
                _logger.LogInformation("GetQuantitesParDate: Requête base de données...");
                var formulesAvecQuantites = await _context.FormulesJour
                    .Where(f => f.Supprimer == 0 && f.Date >= dateDebut && f.Date <= dateFin)
                    .Include(f => f.Commandes.Where(c => c.Supprimer == 0))
                    .OrderBy(f => f.Date)
                    .ThenBy(f => f.NomFormule)
                    .ToListAsync();
                
                _logger.LogInformation("GetQuantitesParDate: {Count} formules trouvées en base", formulesAvecQuantites.Count);

            // Créer les formules pour chaque date
            var quantitesParDate = new List<QuantiteCommandePrestataireViewModel>();
            var dates = formulesAvecQuantites.Select(f => f.Date.Date).Distinct().OrderBy(d => d);

            foreach (var date in dates)
            {
                var formulesDuJour = formulesAvecQuantites.Where(f => f.Date.Date == date).ToList();
                var formules = new List<FormuleQuantiteViewModel>();

                foreach (var formule in formulesDuJour)
                {
                    // Calculer les quantités pour cette formule spécifique
                    var quantiteJour = formule.Commandes.Where(c => c.Supprimer == 0 && c.PeriodeService == Periode.Jour).Sum(c => c.Quantite);
                    var quantiteNuit = formule.Commandes.Where(c => c.Supprimer == 0 && c.PeriodeService == Periode.Nuit).Sum(c => c.Quantite);

                    // Déterminer le type de formule basé sur NomFormule
                    string typeFormule = "Standard";
                    if (formule.NomFormule?.Contains("Améliorée") == true || formule.NomFormule?.Contains("Amélioré") == true)
                    {
                        typeFormule = "Amélioré";
                    }
                    else if (formule.NomFormule?.Contains("Standard 1") == true)
                    {
                        typeFormule = "Standard 1";
                    }
                    else if (formule.NomFormule?.Contains("Standard 2") == true)
                    {
                        typeFormule = "Standard 2";
                    }

                    // Créer la formule avec les bonnes données selon le type
                    var formuleQuantite = new FormuleQuantiteViewModel
                    {
                        IdFormule = formule.IdFormule,
                        TypeFormule = typeFormule,
                        QuantiteJour = quantiteJour,
                        QuantiteNuit = quantiteNuit,
                        MargeJour = 0, // Sera rempli par l'utilisateur
                        MargeNuit = 0,  // Sera rempli par l'utilisateur
                        Marge = formule.Marge ?? 0 // Récupérer la marge depuis la base
                    };

                    // Remplir les champs selon le type de formule
                    if (typeFormule == "Amélioré")
                    {
                        formuleQuantite.Plat = formule.Plat ?? "";
                        formuleQuantite.Garniture = formule.Garniture ?? "";
                        formuleQuantite.Entree = formule.Entree ?? "";
                        formuleQuantite.Dessert = formule.Dessert ?? "";
                        formuleQuantite.Legumes = formule.Legumes ?? "";
                        formuleQuantite.Feculent = formule.Feculent ?? "";
                    }
                    else if (typeFormule == "Standard 1")
                    {
                        formuleQuantite.Plat = formule.PlatStandard1 ?? "";
                        formuleQuantite.Garniture = formule.GarnitureStandard1 ?? "";
                        formuleQuantite.Entree = "";
                        formuleQuantite.Dessert = "";
                        formuleQuantite.Legumes = formule.Legumes ?? "";
                        formuleQuantite.Feculent = formule.Feculent ?? "";
                    }
                    else if (typeFormule == "Standard 2")
                    {
                        formuleQuantite.Plat = formule.PlatStandard2 ?? "";
                        formuleQuantite.Garniture = formule.GarnitureStandard2 ?? "";
                        formuleQuantite.Entree = "";
                        formuleQuantite.Dessert = "";
                        formuleQuantite.Legumes = formule.Legumes ?? "";
                        formuleQuantite.Feculent = formule.Feculent ?? "";
                    }

                    formules.Add(formuleQuantite);
                }

                if (formules.Any())
                {
                    quantitesParDate.Add(new QuantiteCommandePrestataireViewModel
                    {
                        Date = date,
                        Formules = formules.OrderBy(f => f.TypeFormule).ToList()
                    });
                }
            }

            _logger.LogInformation("GetQuantitesParDate: Fin - {Count} jours avec formules retournés", quantitesParDate.Count);
            return quantitesParDate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur dans GetQuantitesParDate: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Vérifie si un export a déjà été effectué pour la période donnée
        /// </summary>
        private async Task<bool> IsExportAlreadyDoneAsync(DateTime dateDebut, DateTime dateFin)
        {
            try
            {
                var exportExists = await _context.ExportCommandesPrestataire
                    .AnyAsync(e => e.Supprimer == 0 && 
                                   e.ExportEffectue == true &&
                                   e.DateDebut.Date == dateDebut.Date && 
                                   e.DateFin.Date == dateFin.Date);
                
                _logger.LogInformation("Vérification export pour période {DateDebut} à {DateFin}: {ExportExists}", 
                    dateDebut, dateFin, exportExists);
                
                return exportExists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la vérification des exports existants");
                return false; // En cas d'erreur, on permet l'export
            }
        }

        /// <summary>
        /// Vérifie si l'utilisateur actuel peut modifier les marges (Admin ou RH)
        /// </summary>
        private bool CanModifyMarges()
        {
            try
            {
                var userRoleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
                var isAdmin = userRoleClaim == "Admin" || userRoleClaim == "Administrateur";
                var isRH = userRoleClaim == "RH" || userRoleClaim == "RH";
                
                var canModify = isAdmin || isRH;
                
                _logger.LogInformation("Vérification permission modification marges - Role: {Role}, CanModify: {CanModify}", 
                    userRoleClaim, canModify);
                
                return canModify;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la vérification des permissions");
                return false; // En cas d'erreur, on bloque par sécurité
            }
        }

        /// <summary>
        /// Enregistre un export dans la base de données
        /// </summary>
        private async Task SaveExportRecordAsync(DateTime dateDebut, DateTime dateFin, string fileName, long fileSize, string commentaires = null)
        {
            try
            {
                // Récupérer l'ID de l'utilisateur actuel
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    _logger.LogWarning("Impossible de récupérer l'ID utilisateur pour l'enregistrement de l'export");
                    return;
                }

                var exportRecord = new ExportCommandePrestataire
                {
                    Id = Guid.NewGuid(),
                    DateDebut = dateDebut,
                    DateFin = dateFin,
                    NomFichier = fileName,
                    TailleFichier = fileSize,
                    UtilisateurId = userId,
                    DateExport = DateTime.UtcNow,
                    ExportEffectue = true,
                    Commentaires = commentaires,
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = User.Identity?.Name ?? "System"
                };

                _context.ExportCommandesPrestataire.Add(exportRecord);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Export enregistré avec succès: {FileName} pour la période {DateDebut} à {DateFin}", 
                    fileName, dateDebut, dateFin);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'enregistrement de l'export");
                // On ne fait pas échouer l'export pour cette erreur
            }
        }

        /// <summary>
        /// Récupère les informations sur le dernier export pour une période donnée
        /// </summary>
        private async Task<ExportCommandePrestataire?> GetLastExportAsync(DateTime dateDebut, DateTime dateFin)
        {
            try
            {
                return await _context.ExportCommandesPrestataire
                    .Include(e => e.Utilisateur)
                    .Where(e => e.Supprimer == 0 && 
                               e.ExportEffectue == true &&
                               e.DateDebut.Date == dateDebut.Date && 
                               e.DateFin.Date == dateFin.Date)
                    .OrderByDescending(e => e.DateExport)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du dernier export");
                return null;
            }
        }
    }
}
