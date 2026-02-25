using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Obeli_K.Data;
using Obeli_K.Enums;
using Obeli_K.Models;
using Obeli_K.Models.ViewModels;
using Obeli_K.Services;
using Obeli_K.Services.Configuration;

namespace Obeli_K.Controllers
{
    /// <summary>
    /// Contrôleur pour la gestion des points de consommation et de la facturation.
    /// Gère l'affichage des points de consommation CIT et les opérations de facturation.
    /// Accessible aux administrateurs et RH.
    /// </summary>
    [Authorize]
    public class PointsConsommationController : Controller
    {
        private readonly ObeliDbContext _context;
        private readonly ILogger<PointsConsommationController> _logger;
        private readonly IFacturationService _facturationService;
        private readonly IConfigurationService _configService;
        private readonly ExcelExportService _excelExportService;

        /// <summary>
        /// Initialise une nouvelle instance du contrôleur de points de consommation.
        /// </summary>
        /// <param name="context">Contexte de base de données Obeli</param>
        /// <param name="logger">Service de journalisation</param>
        /// <param name="facturationService">Service de facturation</param>
        /// <param name="configService">Service de configuration</param>
        /// <param name="excelExportService">Service d'export Excel</param>
        public PointsConsommationController(
            ObeliDbContext context,
            ILogger<PointsConsommationController> logger,
            IFacturationService facturationService,
            IConfigurationService configService,
            ExcelExportService excelExportService)
        {
            _context = context;
            _logger = logger;
            _facturationService = facturationService;
            _configService = configService;
            _excelExportService = excelExportService;
        }

        /// <summary>
        /// Action Index : Redirige vers la vue appropriée selon le rôle de l'utilisateur
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            // Si l'utilisateur est Admin ou RH, rediriger vers PointConsommationCIT
            if (User.IsInRole("Administrateur") || User.IsInRole("RH"))
            {
                return RedirectToAction("PointConsommationCIT");
            }
            // Sinon, rediriger vers MesPointsConsommation
            return RedirectToAction("MesPointsConsommation");
        }

        /// <summary>
        /// Affiche les points de consommation de l'utilisateur connecté (y compris les facturations)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> MesPointsConsommation(DateTime? dateDebut, DateTime? dateFin)
        {
            try
            {
                _logger.LogInformation("🔍 MesPointsConsommation - Début du traitement");

                // Récupérer l'ID de l'utilisateur connecté
                var utilisateurId = GetCurrentUserId();
                if (utilisateurId == Guid.Empty)
                {
                    TempData["ErrorMessage"] = "Utilisateur non identifié.";
                    return RedirectToAction("Login", "Auth");
                }

                // Définir les dates par défaut
                var dateDebutValue = dateDebut ?? DateTime.Today.AddDays(-30);
                var dateFinValue = dateFin ?? DateTime.Today;

                _logger.LogInformation("📅 Période sélectionnée: {DateDebut} à {DateFin} pour l'utilisateur {UserId}",
                    dateDebutValue.ToString("yyyy-MM-dd"), dateFinValue.ToString("yyyy-MM-dd"), utilisateurId);

                // Vérifier si la facturation est activée
                var facturationActive = await _configService.GetConfigurationAsync("FACTURATION_NON_CONSOMMEES_ACTIVE");
                var isFacturationActive = !string.IsNullOrEmpty(facturationActive) && facturationActive.ToLower() == "true";

                _logger.LogInformation("🔍 Facturation active: {Active}", isFacturationActive);

                // Récupérer les points de consommation de l'utilisateur selon la logique
                List<PointConsommation> pointsConsommation;

                if (isFacturationActive)
                {
                    // Facturation activée : Récupérer TOUS les points (consommés + facturations)
                    pointsConsommation = await _context.PointsConsommation
                        .Include(pc => pc.Commande)
                            .ThenInclude(c => c.FormuleJour)
                        .Where(pc => pc.UtilisateurId == utilisateurId
                                  && pc.Supprimer == 0
                                  && pc.DateConsommation >= dateDebutValue.Date
                                  && pc.DateConsommation <= dateFinValue.Date)
                        .OrderByDescending(pc => pc.DateConsommation)
                        .ToListAsync();

                    _logger.LogInformation("📊 Mode facturation activée: Récupération de tous les points de consommation");
                }
                else
                {
                    // Facturation désactivée : Seulement les consommations réelles (pas les facturations)
                    pointsConsommation = await _context.PointsConsommation
                        .Include(pc => pc.Commande)
                            .ThenInclude(c => c.FormuleJour)
                        .Where(pc => pc.UtilisateurId == utilisateurId
                                  && pc.Supprimer == 0
                                  && pc.DateConsommation >= dateDebutValue.Date
                                  && pc.DateConsommation <= dateFinValue.Date
                                  && !pc.LieuConsommation.Contains("FACTURATION")) // Exclure les facturations
                        .OrderByDescending(pc => pc.DateConsommation)
                        .ToListAsync();

                    _logger.LogInformation("📊 Mode facturation désactivée: Récupération des consommations réelles uniquement");
                }

                _logger.LogInformation("📊 Points de consommation trouvés: {Count}", pointsConsommation.Count);

                // Calculer les statistiques
                var montantTotal = pointsConsommation.Sum(pc => CalculerCout(pc));
                var facturations = pointsConsommation.Where(pc => pc.LieuConsommation?.Contains("FACTURATION") == true).ToList();
                var consommationsReelles = pointsConsommation.Where(pc => pc.LieuConsommation?.Contains("FACTURATION") != true).ToList();

                ViewBag.DateDebut = dateDebutValue.ToString("yyyy-MM-dd");
                ViewBag.DateFin = dateFinValue.ToString("yyyy-MM-dd");
                ViewBag.TotalPoints = pointsConsommation.Count;
                ViewBag.MontantTotal = montantTotal;
                ViewBag.Facturations = facturations.Count;
                ViewBag.ConsommationsReelles = consommationsReelles.Count;
                ViewBag.FacturationActive = isFacturationActive;

                _logger.LogInformation("✅ MesPointsConsommation - Traitement terminé: {Count} points",
                    pointsConsommation.Count);

                return View(pointsConsommation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors du chargement des points de consommation de l'utilisateur");
                TempData["ErrorMessage"] = "Erreur lors du chargement de vos points de consommation.";
                return View(new List<PointConsommation>());
            }
        }

        /// <summary>
        /// Affiche les points de consommation de tous les utilisateurs (CIT) - GROUP BY utilisateur
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Administrateur,RH")]
        public async Task<IActionResult> PointConsommationCIT(DateTime? dateDebut, DateTime? dateFin, string? matricule = null)
        {
            try
            {
                _logger.LogInformation("🔍 PointConsommationCIT - Début du traitement");

                // Définir les dates par défaut
                var dateDebutValue = dateDebut ?? DateTime.Today.AddDays(-30);
                var dateFinValue = dateFin ?? DateTime.Today;

                _logger.LogInformation("📅 Période sélectionnée: {DateDebut} à {DateFin}",
                    dateDebutValue.ToString("yyyy-MM-dd"), dateFinValue.ToString("yyyy-MM-dd"));

                // Récupérer les utilisateurs actifs (filtrés par matricule si fourni)
                var queryUtilisateurs = _context.Utilisateurs.Where(u => u.Supprimer == 0);
                
                if (!string.IsNullOrWhiteSpace(matricule))
                {
                    queryUtilisateurs = queryUtilisateurs.Where(u => u.UserName == matricule.Trim());
                }
                
                var tousUtilisateurs = await queryUtilisateurs
                    .Select(u => new
                    {
                        u.Id,
                        u.Nom,
                        u.Prenoms,
                        NomComplet = u.Nom + " " + u.Prenoms,
                        u.Email,
                        u.UserName
                    })
                    .ToListAsync();

                _logger.LogInformation("👥 Nombre d'utilisateurs actifs: {Count}", tousUtilisateurs.Count);

                // Récupérer les points de consommation pour la période
                var pointsConsommation = await _context.PointsConsommation
                    .Include(pc => pc.Utilisateur)
                    .Include(pc => pc.Commande)
                        .ThenInclude(c => c.FormuleJour)
                    .Where(pc => pc.Supprimer == 0
                              && pc.DateConsommation >= dateDebutValue.Date
                              && pc.DateConsommation <= dateFinValue.Date)
                    .ToListAsync();

                _logger.LogInformation("📊 Points de consommation trouvés: {Count}", pointsConsommation.Count);

                // Grouper par utilisateur et calculer les montants par statut et type de formule
                var resultats = tousUtilisateurs.Select(u =>
                {
                    var pointsUtilisateur = pointsConsommation.Where(pc => pc.UtilisateurId == u.Id).ToList();
                    
                    // Calculer les quantités par type de formule et statut
                    int standardConsommee = 0;
                    int standardNonRecuperee = 0;
                    int standardIndisponible = 0;
                    int amelioreeConsommee = 0;
                    int amelioreeNonRecuperee = 0;
                    int amelioreeIndisponible = 0;

                    foreach (var pc in pointsUtilisateur)
                    {
                        // Vérifier si le point est facturable (a un montant > 0)
                        var montant = CalculerCout(pc);
                        if (montant > 0)
                        {
                            var typeFormule = pc.TypeFormule?.ToLower() ?? "standard";
                            var statutCommande = pc.Commande?.StatusCommande ?? -1;
                            var isStandard = typeFormule.Contains("standard");
                            var isAmeliore = typeFormule.Contains("amélioré") || typeFormule.Contains("ameliore") || typeFormule.Contains("ameliorée");
                            var quantite = pc.QuantiteConsommee;

                            // Déterminer le statut et compter les quantités
                            if (statutCommande == (int)StatutCommande.Consommee)
                            {
                                if (isStandard) standardConsommee += quantite;
                                else if (isAmeliore) amelioreeConsommee += quantite;
                            }
                            else if (statutCommande == (int)StatutCommande.NonRecuperer)
                            {
                                if (isStandard) standardNonRecuperee += quantite;
                                else if (isAmeliore) amelioreeNonRecuperee += quantite;
                            }
                            else if (statutCommande == (int)StatutCommande.Indisponible)
                            {
                                if (isStandard) standardIndisponible += quantite;
                                else if (isAmeliore) amelioreeIndisponible += quantite;
                            }
                            // Pour les facturations (statut Précommandée avec FACTURATION)
                            else if (statutCommande == (int)StatutCommande.Precommander && 
                                     pc.LieuConsommation?.Contains("FACTURATION") == true)
                            {
                                // Les facturations sont considérées comme "non récupérées"
                                if (isStandard) standardNonRecuperee += quantite;
                                else if (isAmeliore) amelioreeNonRecuperee += quantite;
                            }
                        }
                    }

                    var total = standardConsommee + standardNonRecuperee + standardIndisponible +
                               amelioreeConsommee + amelioreeNonRecuperee + amelioreeIndisponible;

                    // Calcul du montant total selon la formule :
                    // ((StandardNonRecuperee + StandardConsommee) * 550) + ((AmelioreeNonRecuperee + AmelioreeConsommee) * 2800)
                    var montantTotal = ((standardNonRecuperee + standardConsommee) * 550m) +
                                     ((amelioreeNonRecuperee + amelioreeConsommee) * 2800m);

                    return new PointConsommationCITViewModel
                    {
                        UtilisateurId = u.Id,
                        UtilisateurNom = u.Nom,
                        UtilisateurPrenoms = u.Prenoms,
                        UtilisateurNomComplet = u.NomComplet,
                        Email = u.Email,
                        Matricule = u.UserName,
                        NombreConsommations = pointsUtilisateur.Count,
                        StandardConsommee = standardConsommee,
                        StandardNonRecuperee = standardNonRecuperee,
                        StandardIndisponible = standardIndisponible,
                        AmelioreeConsommee = amelioreeConsommee,
                        AmelioreeNonRecuperee = amelioreeNonRecuperee,
                        AmelioreeIndisponible = amelioreeIndisponible,
                        Total = total,
                        MontantTotal = montantTotal
                    };
                })
                .OrderBy(r => r.UtilisateurNomComplet)
                .ToList();

                ViewBag.DateDebut = dateDebutValue.ToString("yyyy-MM-dd");
                ViewBag.DateFin = dateFinValue.ToString("yyyy-MM-dd");
                ViewBag.Matricule = matricule;
                ViewBag.TotalUtilisateurs = resultats.Count;
                ViewBag.TotalConsommations = resultats.Sum(r => r.NombreConsommations);
                ViewBag.MontantGlobal = resultats.Sum(r => r.MontantTotal);

                _logger.LogInformation("✅ PointConsommationCIT - Traitement terminé: {Count} utilisateurs",
                    resultats.Count);

                // Créer le PagedViewModel
                var pagination = new PaginationViewModel(HttpContext, "PointConsommationCIT", "PointsConsommation", new { dateDebut, dateFin, matricule })
                {
                    CurrentPage = 1,
                    PageSize = resultats.Count > 0 ? resultats.Count : 10,
                    TotalItems = resultats.Count
                };

                var pagedViewModel = new PagedViewModel<PointConsommationCITViewModel>
                {
                    Items = resultats,
                    Pagination = pagination
                };

                return View(pagedViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors du calcul des points de consommation CIT");
                TempData["ErrorMessage"] = "Erreur lors du calcul des points de consommation.";
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// Recherche des utilisateurs par matricule (pour le filtre de la liste des points de consommation)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Administrateur,RH")]
        public async Task<IActionResult> SearchUsersByMatricule(string term)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
                {
                    return Json(new List<object>());
                }

                var users = await _context.Utilisateurs
                    .Where(u => u.Supprimer == 0 && 
                           (u.UserName.Contains(term) || 
                            u.Nom.Contains(term) || 
                            u.Prenoms.Contains(term)))
                    .OrderBy(u => u.UserName)
                    .Take(20)
                    .Select(u => new
                    {
                        id = u.UserName,
                        text = $"{u.Nom} {u.Prenoms} ({u.UserName})",
                        matricule = u.UserName
                    })
                    .ToListAsync();

                return Json(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la recherche d'utilisateurs par matricule");
                return Json(new List<object>());
            }
        }

        /// <summary>
        /// Calcule le coût d'un point de consommation
        /// </summary>
        private decimal CalculerCout(PointConsommation pc)
        {
            // Si la commande est consommée, facturée ou précommandée avec facturation
            if (pc.Commande?.StatusCommande == 1 || pc.Commande?.StatusCommande == 3 ||
                (pc.Commande?.StatusCommande == 0 && pc.LieuConsommation?.Contains("FACTURATION") == true))
            {
                // Pour les commandes facturées, extraire le montant du lieu de consommation
                if (pc.LieuConsommation?.Contains("FACTURATION") == true)
                {
                    var match = System.Text.RegularExpressions.Regex.Match(pc.LieuConsommation, @"\((\d+(?:\.\d+)?)\s*F\s*CFA\)");
                    if (match.Success && decimal.TryParse(match.Groups[1].Value, out var montantFacture))
                    {
                        return montantFacture;
                    }
                }

                // Calcul standard
                var typeFormule = pc.TypeFormule ?? "Standard";
                var prixUnitaire = GetPrixFormule(typeFormule);
                return pc.QuantiteConsommee * prixUnitaire;
            }

            return 0;
        }

        /// <summary>
        /// Retourne le prix d'une formule
        /// </summary>
        private decimal GetPrixFormule(string nomFormule)
        {
            return nomFormule?.ToLower() switch
            {
                "amélioré" or "ameliore" or "améliorée" or "ameliorée" => 2800m,
                "standard" or "standard 1" or "standard 2" => 550m,
                _ => 550m
            };
        }

        /// <summary>
        /// Affiche le formulaire de création d'un point de consommation
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Administrateur,RH,PrestataireCantine")]
        public async Task<IActionResult> Create()
        {
            // Récupérer les utilisateurs pour le dropdown
            var utilisateurs = await _context.Utilisateurs
                .Where(u => u.Supprimer == 0)
                .OrderBy(u => u.Nom)
                .Select(u => new
                {
                    u.Id,
                    NomComplet = u.Nom + " " + u.Prenoms
                })
                .ToListAsync();

            ViewBag.Utilisateurs = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(utilisateurs, "Id", "NomComplet");

            return View();
        }

        /// <summary>
        /// Affiche le formulaire d'édition d'un point de consommation
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Administrateur,RH,PrestataireCantine")]
        public async Task<IActionResult> Edit(Guid id)
        {
            var pointConsommation = await _context.PointsConsommation
                .Include(pc => pc.Utilisateur)
                .Include(pc => pc.Commande)
                .FirstOrDefaultAsync(pc => pc.IdPointConsommation == id && pc.Supprimer == 0);

            if (pointConsommation == null)
            {
                TempData["ErrorMessage"] = "Point de consommation non trouvé.";
                return RedirectToAction("PointConsommationCIT");
            }

            // Récupérer les utilisateurs pour le dropdown
            var utilisateurs = await _context.Utilisateurs
                .Where(u => u.Supprimer == 0)
                .OrderBy(u => u.Nom)
                .Select(u => new
                {
                    u.Id,
                    NomComplet = u.Nom + " " + u.Prenoms
                })
                .ToListAsync();

            ViewBag.Utilisateurs = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(utilisateurs, "Id", "NomComplet", pointConsommation.UtilisateurId);

            return View(pointConsommation);
        }

        /// <summary>
        /// Déclenche manuellement la facturation des commandes non consommées
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Administrateur,RH")]
        public async Task<IActionResult> DeclencherFacturation()
        {
            try
            {
                _logger.LogInformation("🔄 Déclenchement manuel de la facturation");

                // Vérifier si la facturation est activée
                var facturationActiveConfig = await _configService.GetConfigurationAsync("FACTURATION_NON_CONSOMMEES_ACTIVE");
                var isFacturationActive = facturationActiveConfig == "true";
                
                if (!isFacturationActive)
                {
                    TempData["WarningMessage"] = "La facturation des commandes non consommées n'est pas activée.";
                    return RedirectToAction("PointConsommationCIT");
                }

                // Récupérer les commandes non consommées
                var commandesNonConsommees = await _facturationService.GetCommandesNonConsommeesAsync();
                
                if (!commandesNonConsommees.Any())
                {
                    TempData["InfoMessage"] = "Aucune commande non consommée à facturer.";
                    return RedirectToAction("PointConsommationCIT");
                }

                // Calculer la facturation
                var resultatFacturation = await _facturationService.CalculerFacturationAsync(commandesNonConsommees);

                // Appliquer la facturation
                await _facturationService.AppliquerFacturationAsync(commandesNonConsommees, resultatFacturation);

                TempData["SuccessMessage"] = "Facturation déclenchée avec succès !";
                _logger.LogInformation("✅ Facturation manuelle terminée avec succès");

                return RedirectToAction("PointConsommationCIT");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors du déclenchement de la facturation");
                TempData["ErrorMessage"] = "Erreur lors du déclenchement de la facturation.";
                return RedirectToAction("PointConsommationCIT");
            }
        }

        /// <summary>
        /// Synchronise la facturation pour forcer une mise à jour complète
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Administrateur,RH")]
        public async Task<IActionResult> SynchroniserFacturation()
        {
            try
            {
                _logger.LogInformation("🔄 Synchronisation de la facturation");

                // Vérifier si la facturation est activée
                var facturationActiveConfig = await _configService.GetConfigurationAsync("FACTURATION_NON_CONSOMMEES_ACTIVE");
                var isFacturationActive = facturationActiveConfig == "true";
                
                if (!isFacturationActive)
                {
                    TempData["WarningMessage"] = "La facturation des commandes non consommées n'est pas activée.";
                    return RedirectToAction("PointConsommationCIT");
                }

                // Récupérer les commandes non consommées
                var commandesNonConsommees = await _facturationService.GetCommandesNonConsommeesAsync();
                
                if (!commandesNonConsommees.Any())
                {
                    TempData["InfoMessage"] = "Aucune commande non consommée à synchroniser.";
                    return RedirectToAction("PointConsommationCIT");
                }

                // Calculer la facturation
                var resultatFacturation = await _facturationService.CalculerFacturationAsync(commandesNonConsommees);

                // Forcer la synchronisation complète
                await _facturationService.AppliquerFacturationAsync(commandesNonConsommees, resultatFacturation);

                TempData["SuccessMessage"] = "Synchronisation de la facturation terminée avec succès !";
                _logger.LogInformation("✅ Synchronisation de la facturation terminée");

                return RedirectToAction("PointConsommationCIT");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors de la synchronisation de la facturation");
                TempData["ErrorMessage"] = "Erreur lors de la synchronisation de la facturation.";
                return RedirectToAction("PointConsommationCIT");
            }
        }

        /// <summary>
        /// Exporte les données de consommation CIT en Excel
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Administrateur,RH")]
        public async Task<IActionResult> ExportExcelCIT(DateTime? dateDebut, DateTime? dateFin)
        {
            try
            {
                _logger.LogInformation("📊 Export Excel des points de consommation CIT");

                // Définir les dates par défaut
                var dateDebutValue = dateDebut ?? DateTime.Today.AddDays(-30);
                var dateFinValue = dateFin ?? DateTime.Today;

                // Récupérer les données (même logique que PointConsommationCIT)
                var tousUtilisateurs = await _context.Utilisateurs
                    .Where(u => u.Supprimer == 0)
                    .Select(u => new
                    {
                        u.Id,
                        u.Nom,
                        u.Prenoms,
                        NomComplet = u.Nom + " " + u.Prenoms,
                        u.Email,
                        u.UserName
                    })
                    .ToListAsync();

                var pointsConsommation = await _context.PointsConsommation
                    .Include(pc => pc.Utilisateur)
                    .Include(pc => pc.Commande)
                        .ThenInclude(c => c.FormuleJour)
                    .Where(pc => pc.Supprimer == 0
                              && pc.DateConsommation >= dateDebutValue.Date
                              && pc.DateConsommation <= dateFinValue.Date)
                    .ToListAsync();

                var resultats = tousUtilisateurs.Select(u =>
                {
                    var pointsUtilisateur = pointsConsommation.Where(pc => pc.UtilisateurId == u.Id).ToList();
                    var montantTotal = pointsUtilisateur.Sum(pc => CalculerCout(pc));

                    return new
                    {
                        UtilisateurNomComplet = u.NomComplet,
                        Email = u.Email,
                        Matricule = u.UserName,
                        NombreConsommations = pointsUtilisateur.Count,
                        MontantTotal = montantTotal
                    };
                })
                .OrderByDescending(r => r.MontantTotal)
                .ThenBy(r => r.UtilisateurNomComplet)
                .ToList();

                var fileName = $"PointsConsommationCIT_{dateDebutValue:yyyyMMdd}_{dateFinValue:yyyyMMdd}.xlsx";

                var fileBytes = _excelExportService.ExportToExcel(
                    resultats,
                    fileName,
                    "Points de Consommation CIT",
                    $"Points de consommation CIT du {dateDebutValue:dd/MM/yyyy} au {dateFinValue:dd/MM/yyyy}"
                );

                _logger.LogInformation("✅ Export Excel terminé: {FileName}", fileName);

                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors de l'export Excel des points de consommation CIT");
                TempData["ErrorMessage"] = "Erreur lors de l'export Excel.";
                return RedirectToAction("PointConsommationCIT");
            }
        }

        /// <summary>
        /// Récupère l'ID de l'utilisateur connecté
        /// </summary>
        private Guid GetCurrentUserId()
        {
            try
            {
                var userIdClaim = User.FindFirst("UserId");
                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    return userId;
                }

                // Fallback: chercher par nom d'utilisateur
                var userName = User.Identity?.Name;
                if (!string.IsNullOrEmpty(userName))
                {
                    var utilisateur = _context.Utilisateurs
                        .FirstOrDefault(u => u.Email == userName && u.Supprimer == 0);
                    return utilisateur?.Id ?? Guid.Empty;
                }

                return Guid.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors de la récupération de l'ID utilisateur");
                return Guid.Empty;
            }
        }
    }
}

