using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Obeli_K.Data;
using Obeli_K.Enums;
using Obeli_K.Models;
using Obeli_K.Models.Enums;
using Obeli_K.Models.ViewModels;
using System.Security.Claims;

namespace Obeli_K.Controllers
{
    [Authorize]
    public class PointsConsommationController : Controller
    {
        private readonly ObeliDbContext _context;
        private readonly ILogger<PointsConsommationController> _logger;

        public PointsConsommationController(ObeliDbContext context, ILogger<PointsConsommationController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Affiche la liste des points de consommation selon le rôle de l'utilisateur
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(DateTime? dateDebut, DateTime? dateFin)
        {
            try
            {
                // Vérifier les autorisations d'accès
                if (!HasAccessToPointsConsommation())
                {
                    TempData["ErrorMessage"] = "Vous n'avez pas l'autorisation d'accéder à cette fonctionnalité.";
                    return RedirectToAction("Index", "Home");
                }

                var currentUserId = GetCurrentUserId();
                var isEmploye = IsEmployee();

                // Par défaut, afficher du 17 du mois précédent au 16 du mois courant
                if (!dateDebut.HasValue)
                {
                    var aujourdhui = DateTime.Today;
                    if (aujourdhui.Day >= 17)
                    {
                        // Si on est après le 16, période du 17 du mois précédent au 16 du mois courant
                        dateDebut = new DateTime(aujourdhui.Year, aujourdhui.Month - 1, 17);
                        dateFin = new DateTime(aujourdhui.Year, aujourdhui.Month, 16);
                    }
                    else
                    {
                        // Si on est avant le 17, période du 17 du mois d'avant-dernier au 16 du mois précédent
                        var moisPrecedent = aujourdhui.AddMonths(-1);
                        dateDebut = new DateTime(moisPrecedent.Year, moisPrecedent.Month - 1, 17);
                        dateFin = new DateTime(moisPrecedent.Year, moisPrecedent.Month, 16);
                    }
                }
                if (!dateFin.HasValue)
                {
                    var aujourdhui = DateTime.Today;
                    if (aujourdhui.Day >= 17)
                    {
                        dateFin = new DateTime(aujourdhui.Year, aujourdhui.Month, 16);
                    }
                    else
                    {
                        var moisPrecedent = aujourdhui.AddMonths(-1);
                        dateFin = new DateTime(moisPrecedent.Year, moisPrecedent.Month, 16);
                    }
                }

                var query = _context.PointsConsommation
                    .AsNoTracking()
                    .Include(pc => pc.Utilisateur)
                    .Include(pc => pc.Commande)
                    .Where(pc => pc.Supprimer == 0);

                // Filtrage par période
                query = query.Where(pc => pc.DateConsommation.Date >= dateDebut.Value.Date && 
                                        pc.DateConsommation.Date <= dateFin.Value.Date);

                // Filtrage selon le rôle
                if (isEmploye && currentUserId.HasValue)
                {
                    // Employé : voir uniquement ses propres points
                    query = query.Where(pc => pc.UtilisateurId == currentUserId);
                    _logger.LogInformation("Filtrage pour employé: {UserId}", currentUserId);
                }
                // Admin/RH : voir tous les points (pas de filtrage supplémentaire)

                var pointsConsommation = await query
                    .OrderByDescending(pc => pc.DateConsommation)
                    .ThenByDescending(pc => pc.CreatedOn)
                    .ToListAsync();

                var viewModel = pointsConsommation.Select(pc => new PointConsommationListViewModel
                {
                    IdPointConsommation = pc.IdPointConsommation,
                    DateConsommation = pc.DateConsommation,
                    TypeFormule = pc.TypeFormule == "Inconnu" && pc.Commande?.FormuleJour != null 
                        ? pc.Commande.FormuleJour.NomFormule ?? "Standard"
                        : pc.TypeFormule,
                    NomPlat = pc.NomPlat,
                    QuantiteConsommee = pc.QuantiteConsommee,
                    Cout = CalculateCout(pc),
                    LieuConsommation = pc.LieuConsommation,
                    CreatedOn = pc.CreatedOn,
                    CreatedBy = pc.CreatedBy,
                    CodeCommande = pc.Commande?.CodeCommande,
                    CommandeId = pc.CommandeId,
                    StatusCommande = pc.Commande?.StatusCommande ?? 0,
                    UtilisateurNom = pc.Utilisateur?.Nom,
                    UtilisateurPrenoms = pc.Utilisateur?.Prenoms,
                    UtilisateurMatricule = pc.Utilisateur?.UserName,
                    UtilisateurNomComplet = pc.Utilisateur != null ? $"{pc.Utilisateur.Nom} {pc.Utilisateur.Prenoms}" : null,
                    UtilisateurSite = pc.Utilisateur?.Site
                }).ToList();

                // Passer les dates de filtre à la vue
                ViewBag.DateDebut = dateDebut?.ToString("yyyy-MM-dd");
                ViewBag.DateFin = dateFin?.ToString("yyyy-MM-dd");

                _logger.LogInformation("Nombre de points de consommation trouvés pour la période {DateDebut} - {DateFin}: {Count}", 
                    dateDebut, dateFin, viewModel.Count);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement des points de consommation");
                TempData["ErrorMessage"] = "Une erreur est survenue lors du chargement des points de consommation.";
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// Redirige vers la liste car les points de consommation sont créés automatiquement
        /// </summary>
        [HttpGet]
        public IActionResult Create()
        {
            TempData["InfoMessage"] = "Les points de consommation sont créés automatiquement lors de la validation des commandes par le prestataire.";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Redirige vers la liste car les points de consommation ne peuvent pas être modifiés
        /// </summary>
        [HttpGet]
        public IActionResult Edit(Guid id)
        {
            TempData["InfoMessage"] = "Les points de consommation ne peuvent pas être modifiés car ils sont générés automatiquement.";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Supprime un point de consommation (soft delete)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                if (!HasAccessToPointsConsommation())
                {
                    return Json(new { success = false, message = "Vous n'avez pas l'autorisation d'accéder à cette fonctionnalité." });
                }

                var pointConsommation = await _context.PointsConsommation
                    .FirstOrDefaultAsync(pc => pc.IdPointConsommation == id && pc.Supprimer == 0);

                if (pointConsommation == null)
                {
                    return Json(new { success = false, message = "Point de consommation introuvable." });
                }

                // Vérifier les permissions de suppression
                if (!CanEditPointConsommation(pointConsommation))
                {
                    return Json(new { success = false, message = "Vous n'avez pas l'autorisation de supprimer ce point de consommation." });
                }

                pointConsommation.Supprimer = 1;
                pointConsommation.ModifiedOn = DateTime.UtcNow;
                pointConsommation.ModifiedBy = User.Identity?.Name ?? "System";

                await _context.SaveChangesAsync();

                _logger.LogInformation("Point de consommation supprimé avec succès: {Id}", pointConsommation.IdPointConsommation);
                return Json(new { success = true, message = "Point de consommation supprimé avec succès." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression du point de consommation {Id}", id);
                return Json(new { success = false, message = "Une erreur est survenue lors de la suppression." });
            }
        }

        #region Méthodes privées

        /// <summary>
        /// Vérifie si l'utilisateur a accès aux points de consommation
        /// </summary>
        private bool HasAccessToPointsConsommation()
        {
            return User.IsInRole("Administrateur") || 
                   User.IsInRole("RessourcesHumaines") || 
                   User.IsInRole("Employe");
        }

        /// <summary>
        /// Vérifie si l'utilisateur est un employé (pas admin/RH)
        /// </summary>
        private bool IsEmployee()
        {
            return User.IsInRole("Employe") && 
                   !User.IsInRole("Administrateur") && 
                   !User.IsInRole("RessourcesHumaines");
        }

        /// <summary>
        /// Crée des points de consommation manquants pour les commandes consommées
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Administrateur,RessourcesHumaines")]
        public async Task<IActionResult> CreerPointsManquants()
        {
            try
            {
                // Trouver les commandes consommées qui n'ont pas de point de consommation
                var commandesConsommeesSansPoint = await _context.Commandes
                    .Include(c => c.Utilisateur)
                    .Include(c => c.FormuleJour)
                    .ThenInclude(f => f.NomFormuleNavigation)
                    .Where(c => c.Supprimer == 0 
                             && c.StatusCommande == (int)Enums.StatutCommande.Consommee
                             && !_context.PointsConsommation.Any(pc => pc.CommandeId == c.IdCommande && pc.Supprimer == 0))
                    .ToListAsync();

                int pointsCrees = 0;
                foreach (var commande in commandesConsommeesSansPoint)
                {
                    // Créer le point de consommation manquant
                    var typeFormule = commande.FormuleJour?.NomFormule ?? "Inconnu";
                    var nomPlat = GetNomPlatFromFormule(commande.FormuleJour);

                    var pointConsommation = new PointConsommation
                    {
                        IdPointConsommation = Guid.NewGuid(),
                        UtilisateurId = commande.UtilisateurId ?? Guid.Empty,
                        CommandeId = commande.IdCommande,
                        DateConsommation = commande.DateConsommation ?? DateTime.Today,
                        TypeFormule = typeFormule,
                        NomPlat = nomPlat,
                        QuantiteConsommee = commande.Quantite,
                        LieuConsommation = "Restaurant CIT",
                        CreatedOn = DateTime.UtcNow,
                        CreatedBy = User.Identity?.Name ?? "System",
                        Supprimer = 0
                    };

                    _context.PointsConsommation.Add(pointConsommation);
                    pointsCrees++;
                }

                if (pointsCrees > 0)
                {
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"{pointsCrees} point(s) de consommation créé(s) pour les commandes manquantes.";
                }
                else
                {
                    TempData["InfoMessage"] = "Aucun point de consommation manquant trouvé.";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création des points de consommation manquants");
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la création des points manquants.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Met à jour les types de formule "Inconnu" avec les bonnes valeurs
        /// </summary>
        [HttpPost]
        [Authorize] // Autoriser tous les utilisateurs authentifiés
        public async Task<IActionResult> CorrigerTypesFormule()
        {
            try
            {
                // Trouver les points de consommation avec TypeFormule = "Inconnu"
                var pointsAvecTypeInconnu = await _context.PointsConsommation
                    .Include(pc => pc.Commande)
                    .ThenInclude(c => c.FormuleJour)
                    .Where(pc => pc.Supprimer == 0 && pc.TypeFormule == "Inconnu")
                    .ToListAsync();

                int pointsCorriges = 0;
                foreach (var point in pointsAvecTypeInconnu)
                {
                    if (point.Commande?.FormuleJour != null)
                    {
                        var nouveauTypeFormule = point.Commande.FormuleJour.NomFormule ?? "Standard";
                        point.TypeFormule = nouveauTypeFormule;
                        point.ModifiedOn = DateTime.UtcNow;
                        point.ModifiedBy = User.Identity?.Name ?? "System";
                        pointsCorriges++;
                    }
                }

                if (pointsCorriges > 0)
                {
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"{pointsCorriges} type(s) de formule corrigé(s).";
                }
                else
                {
                    TempData["InfoMessage"] = "Aucun type de formule 'Inconnu' trouvé.";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la correction des types de formule");
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la correction des types de formule.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Obtient le nom du plat à partir de la formule
        /// </summary>
        private string GetNomPlatFromFormule(FormuleJour? formule)
        {
            if (formule == null) return "Plat inconnu";

            return formule.NomFormule?.ToUpper() switch
            {
                "AMÉLIORÉ" => formule.Plat ?? "Plat amélioré",
                "STANDARD 1" => formule.PlatStandard1 ?? "Plat standard 1",
                "STANDARD 2" => formule.PlatStandard2 ?? "Plat standard 2",
                _ => formule.Plat ?? formule.PlatStandard1 ?? formule.PlatStandard2 ?? "Plat du jour"
            };
        }

        /// <summary>
        /// Vérifie si l'utilisateur peut modifier/supprimer un point de consommation
        /// </summary>
        private bool CanEditPointConsommation(PointConsommation pointConsommation)
        {
            var currentUserId = GetCurrentUserId();
            var isEmploye = IsEmployee();

            if (isEmploye && currentUserId.HasValue)
            {
                // Employé : peut modifier uniquement ses propres points
                return pointConsommation.UtilisateurId == currentUserId.Value;
            }
            else
            {
                // Admin/RH : peuvent modifier tous les points
                return true;
            }
        }

        /// <summary>
        /// Récupère l'ID de l'utilisateur connecté
        /// </summary>
        private Guid? GetCurrentUserId()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var userId))
                {
                    return userId;
                }

                var userNameClaim = User.FindFirst("UserName")?.Value;
                if (!string.IsNullOrEmpty(userNameClaim))
                {
                    var utilisateur = _context.Utilisateurs
                        .AsNoTracking()
                        .FirstOrDefault(u => u.UserName == userNameClaim && u.Supprimer == 0);
                    return utilisateur?.Id;
                }
            }
            return null;
        }


        /// <summary>
        /// Calcule le prix d'une formule selon son type
        /// </summary>
        private decimal GetPrixFormule(string? typeFormule)
        {
            return typeFormule?.ToUpper() switch
            {
                "STANDARD1" or "STANDARD 1" or "STANDARD" => 550,
                "STANDARD2" or "STANDARD 2" => 550,
                "AMELIORE" or "AMÉLIORÉ" or "AMELIOREE" or "AMÉLIORÉE" => 2800,
                _ => 0
            };
        }

        /// <summary>
        /// Calcule le coût d'un point de consommation
        /// Coût = nombre de plats consommés × prix unitaire (seulement si statut = Consommée)
        /// </summary>
        private decimal CalculateCout(PointConsommation pc)
        {
            // Seulement calculer le coût si la commande est consommée (statut = 1)
            if (pc.Commande?.StatusCommande == 1) // StatutCommande.Consommee
            {
                // Utiliser le type de formule de la commande si le point a "Inconnu"
                var typeFormulePourCalcul = pc.TypeFormule;
                if (typeFormulePourCalcul == "Inconnu" && pc.Commande?.FormuleJour != null)
                {
                    typeFormulePourCalcul = pc.Commande.FormuleJour.NomFormule ?? "Standard";
                }
                
                var prixUnitaire = GetPrixFormule(typeFormulePourCalcul);
                return pc.QuantiteConsommee * prixUnitaire;
            }
            
            return 0; // Pas de coût si pas consommé
        }

        /// <summary>
        /// Exporte les points de consommation en Excel
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExporterExcel(DateTime? dateDebut, DateTime? dateFin)
        {
            try
            {
                // Vérifier les autorisations d'accès
                if (!HasAccessToPointsConsommation())
                {
                    TempData["ErrorMessage"] = "Vous n'avez pas l'autorisation d'accéder à cette fonctionnalité.";
                    return RedirectToAction("Index", "Home");
                }

                var currentUserId = GetCurrentUserId();
                var isEmploye = IsEmployee();

                // Utiliser les dates par défaut si non fournies
                if (!dateDebut.HasValue || !dateFin.HasValue)
                {
                    var aujourdhui = DateTime.Today;
                    if (aujourdhui.Day >= 17)
                    {
                        dateDebut = new DateTime(aujourdhui.Year, aujourdhui.Month - 1, 17);
                        dateFin = new DateTime(aujourdhui.Year, aujourdhui.Month, 16);
                    }
                    else
                    {
                        var moisPrecedent = aujourdhui.AddMonths(-1);
                        dateDebut = new DateTime(moisPrecedent.Year, moisPrecedent.Month - 1, 17);
                        dateFin = new DateTime(moisPrecedent.Year, moisPrecedent.Month, 16);
                    }
                }

                var query = _context.PointsConsommation
                    .AsNoTracking()
                    .Include(pc => pc.Utilisateur)
                    .Include(pc => pc.Commande)
                    .Where(pc => pc.Supprimer == 0);

                // Filtrage par période
                query = query.Where(pc => pc.DateConsommation.Date >= dateDebut.Value.Date && 
                                        pc.DateConsommation.Date <= dateFin.Value.Date);

                // Filtrage selon le rôle
                if (isEmploye && currentUserId.HasValue)
                {
                    query = query.Where(pc => pc.UtilisateurId == currentUserId);
                }

                var pointsConsommation = await query
                    .OrderByDescending(pc => pc.DateConsommation)
                    .ThenByDescending(pc => pc.CreatedOn)
                    .ToListAsync();

                if (!pointsConsommation.Any())
                {
                    TempData["InfoMessage"] = "Aucun point de consommation trouvé pour l'export.";
                    return RedirectToAction("Index");
                }

                using var workbook = new ClosedXML.Excel.XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Points de Consommation");

                // En-têtes
                var headers = new[] { "Date", "Heure", "Utilisateur", "Matricule", "Type Formule", "Plat", "Commande", "Lieu", "Site", "Statut", "Coût", "Créé le", "Créé par" };
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cell(1, i + 1).Value = headers[i];
                }
                worksheet.Row(1).Style.Font.Bold = true;
                worksheet.Row(1).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightBlue;

                // Données
                int row = 2;
                foreach (var pc in pointsConsommation)
                {
                    worksheet.Cell(row, 1).Value = pc.DateConsommation.ToString("dd/MM/yyyy");
                    worksheet.Cell(row, 2).Value = pc.DateConsommation.ToString("HH:mm");
                    worksheet.Cell(row, 3).Value = pc.Utilisateur != null ? $"{pc.Utilisateur.Nom} {pc.Utilisateur.Prenoms}" : "";
                    worksheet.Cell(row, 4).Value = pc.Utilisateur?.UserName ?? "";
                    worksheet.Cell(row, 5).Value = pc.TypeFormule ?? "";
                    worksheet.Cell(row, 6).Value = pc.NomPlat ?? "";
                    worksheet.Cell(row, 7).Value = pc.Commande?.CodeCommande ?? "";
                    worksheet.Cell(row, 8).Value = pc.LieuConsommation ?? "";
                    worksheet.Cell(row, 9).Value = pc.Utilisateur?.Site?.ToString() ?? "";
                    
                    // Statut
                    var statut = pc.Commande?.StatusCommande switch
                    {
                        0 => "Précommandée",
                        1 => "Consommée",
                        2 => "Annulée",
                        _ => "Inconnu"
                    };
                    worksheet.Cell(row, 10).Value = statut;
                    
                    worksheet.Cell(row, 11).Value = CalculateCout(pc);
                    worksheet.Cell(row, 12).Value = pc.CreatedOn.ToString("dd/MM/yyyy HH:mm");
                    worksheet.Cell(row, 13).Value = pc.CreatedBy ?? "";
                    
                    row++;
                }

                // Ajuster la largeur des colonnes
                worksheet.Columns().AdjustToContents();

                // Générer le fichier en mémoire
                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;

                var fileName = $"Points_Consommation_{dateDebut:yyyyMMdd}_{dateFin:yyyyMMdd}.xlsx";
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'export Excel des points de consommation");
                TempData["ErrorMessage"] = "Une erreur est survenue lors de l'export Excel.";
                return RedirectToAction("Index");
            }
        }

        #endregion
    }
}
