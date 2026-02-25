using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Obeli_K.Data;
using Obeli_K.Models;
using Obeli_K.Models.ViewModels;
using Obeli_K.Enums;
using System.Security.Claims;

namespace Obeli_K.Controllers
{
    [Authorize(Roles = "Administrateur,RH")]
    public class VisiteurController : Controller
    {
        private readonly ObeliDbContext _context;
        private readonly ILogger<VisiteurController> _logger;

        public VisiteurController(ObeliDbContext context, ILogger<VisiteurController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Affiche la liste des formules disponibles pour les visiteurs
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> List()
        {
            try
            {
                var model = new VisiteurSelectionViewModel
                {
                    DateDebut = DateTime.Today,
                    DateFin = DateTime.Today.AddDays(7)
                };

                // Charger les directions
                ViewBag.Directions = await _context.Directions
                    .Where(d => d.Supprimer == 0)
                    .OrderBy(d => d.Nom)
                    .ToListAsync();

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement de la liste des visiteurs");
                TempData["ErrorMessage"] = "Une erreur est survenue lors du chargement de la page.";
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// Affiche la liste des commandes de visiteurs avec filtrage par direction
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Commands(int page = 1, int pageSize = 5, Guid? DirectionId = null, DateTime? dateDebut = null, DateTime? dateFin = null)
        {
            try
            {
                // Charger les directions pour le filtre
                ViewBag.Directions = await _context.Directions
                    .Where(d => d.Supprimer == 0)
                    .OrderBy(d => d.Nom)
                    .ToListAsync();

                // Construire la requête pour les commandes de visiteurs
                var query = _context.Commandes
                    .Include(c => c.FormuleJour)
                    .Include(c => c.Utilisateur)
                    .Where(c => c.TypeClient == TypeClientCommande.Visiteur && c.Supprimer == 0);

                // Appliquer les filtres
                if (DirectionId.HasValue)
                {
                    // Filtrer par direction via le code de commande (format: DIRECTION-VIS-...)
                    var direction = await _context.Directions
                        .FirstOrDefaultAsync(d => d.Id == DirectionId);
                    
                    if (direction != null)
                    {
                        var codeDirection = GenerateCodeDirection(direction.Nom);
                        query = query.Where(c => c.CodeCommande != null && c.CodeCommande.StartsWith(codeDirection));
                    }
                }

                if (dateDebut.HasValue)
                {
                    query = query.Where(c => c.DateConsommation >= dateDebut.Value);
                }

                if (dateFin.HasValue)
                {
                    query = query.Where(c => c.DateConsommation <= dateFin.Value);
                }

                // Compter le total
                var totalCount = await query.CountAsync();

                // Pagination
                var commandes = await query
                    .OrderByDescending(c => c.DateConsommation)
                    .ThenByDescending(c => c.Date)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(c => new
                    {
                        c.IdCommande,
                        c.CodeCommande,
                        c.Date,
                        c.DateConsommation,
                        c.StatusCommande,
                        c.Quantite,
                        c.VisiteurNom,
                        c.VisiteurTelephone,
                        c.CreatedOn,
                        c.CreatedBy,
                        FormuleNom = c.FormuleJour != null ? c.FormuleJour.NomFormule : "N/A",
                        FormuleDate = c.FormuleJour != null ? c.FormuleJour.Date : (DateTime?)null
                    })
                    .ToListAsync();

                // Créer le modèle de pagination
                var pagination = PaginationViewModel.Create(
                    commandes, 
                    page, 
                    pageSize, 
                    HttpContext, 
                    "Commands", 
                    "Visiteur", 
                    new { DirectionId, dateDebut, dateFin });

                var model = new
                {
                    Commandes = commandes,
                    Pagination = pagination,
                    DirectionId = DirectionId,
                    DateDebut = dateDebut,
                    DateFin = dateFin
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement des commandes de visiteurs");
                TempData["ErrorMessage"] = "Une erreur est survenue lors du chargement des commandes.";
                return RedirectToAction("List");
            }
        }

        /// <summary>
        /// Affiche le formulaire de création de commandes pour visiteurs
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                var model = new CreateCommandeVisiteurViewModel
                {
                    DateDebut = DateTime.Today,
                    DateFin = DateTime.Today.AddDays(7),
                    NombreVisiteurs = 1,
                    TypeFormule = "Standard 1",
                    PeriodeService = Periode.Jour
                };

                // Charger les directions
                ViewBag.Directions = await _context.Directions
                    .Where(d => d.Supprimer == 0)
                    .OrderBy(d => d.Nom)
                    .ToListAsync();

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement du formulaire visiteur");
                TempData["ErrorMessage"] = "Une erreur est survenue lors du chargement du formulaire.";
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// Crée une commande pour un visiteur (AJAX)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCommande([FromBody] CreateCommandeVisiteurRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.VisiteurNom))
                {
                    return Json(new { success = false, message = "Le nom du visiteur est obligatoire" });
                }

                if (request.IdFormule == Guid.Empty)
                {
                    return Json(new { success = false, message = "L'ID de la formule est invalide" });
                }

                if (request.DirectionId == Guid.Empty)
                {
                    return Json(new { success = false, message = "Le direction est obligatoire" });
                }

                // Validation : Les commandes pour visiteurs doivent être créées au moins 48h à l'avance
                var maintenant = DateTime.Now;
                var delaiMinimum = maintenant.AddHours(48);
                
                if (request.DateConsommation < delaiMinimum)
                {
                    var dateLimite = delaiMinimum.ToString("dd/MM/yyyy à HH:mm");
                    return Json(new { 
                        success = false, 
                        message = $"Les commandes pour visiteurs doivent être créées au moins 48h à l'avance. Date limite pour le {request.DateConsommation:dd/MM/yyyy}: {dateLimite}" 
                    });
                }

                // Récupérer la formule
                var formule = await _context.FormulesJour
                    .FirstOrDefaultAsync(f => f.IdFormule == request.IdFormule && f.Supprimer == 0);

                if (formule == null)
                {
                    return Json(new { success = false, message = "Formule introuvable" });
                }

                // Récupérer la direction
                var direction = await _context.Directions
                    .FirstOrDefaultAsync(d => d.Id == request.DirectionId && d.Supprimer == 0);

                if (direction == null)
                {
                    return Json(new { success = false, message = "Direction introuvable" });
                }

                // Générer un code unique automatiquement pour le visiteur avec la direction et la date
                var codeDirection = GenerateCodeDirection(direction.Nom);
                var codeVisiteur = GenerateCodeVisiteur(request.VisiteurNom);
                var dateConsommation = request.DateConsommation.ToString("yyyyMMdd");
                var codeCommande = await GenerateUniqueCodeCommande($"{codeDirection}-VIS-{codeVisiteur}-{dateConsommation}");

                // Créer la commande
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
                var userName = User.Identity?.Name ?? "System";

                var commande = new Commande
                {
                    IdCommande = Guid.NewGuid(),
                    CodeCommande = codeCommande,
                    Date = DateTime.UtcNow,
                    DateConsommation = request.DateConsommation,
                    StatusCommande = 0, // Précommandé
                    Quantite = 1,
                    PeriodeService = Periode.Jour,
                    UtilisateurId = null, // Pas d'utilisateur CIT pour les visiteurs
                    IdFormule = formule.IdFormule,
                    Montant = 0, // Les visiteurs ne paient pas
                    VisiteurNom = request.VisiteurNom,
                    VisiteurTelephone = request.VisiteurTelephone,
                    // DirectionId supprimé - Table Direction non utilisée
                    TypeClient = TypeClientCommande.Visiteur,
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = userName,
                    Supprimer = 0
                };

                _context.Commandes.Add(commande);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Commande visiteur créée: {CodeCommande} pour {VisiteurNom}", 
                    codeCommande, request.VisiteurNom);

                return Json(new { 
                    success = true, 
                    message = $"Commande créée avec succès pour {request.VisiteurNom}. Code: {codeCommande}" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création de la commande visiteur");
                return Json(new { success = false, message = "Une erreur est survenue lors de la création de la commande" });
            }
        }

        /// <summary>
        /// Récupère les formules disponibles pour une période donnée (AJAX)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> GetFormules([FromBody] GetFormulesRequest request)
        {
            try
            {
                var formules = await _context.FormulesJour
                    .Where(f => f.Supprimer == 0 && 
                               f.Date >= request.DateDebut && 
                               f.Date <= request.DateFin)
                    .OrderBy(f => f.Date)
                    .ThenBy(f => f.NomFormule)
                    .Select(f => new
                    {
                        idFormule = f.IdFormule,
                        date = f.Date.ToString("yyyy-MM-dd"),
                        nom = f.NomFormule,
                        plat = f.Plat,
                        garniture = f.Garniture,
                        entree = f.Entree,
                        dessert = f.Dessert,
                        platStandard1 = f.PlatStandard1,
                        garnitureStandard1 = f.GarnitureStandard1,
                        platStandard2 = f.PlatStandard2,
                        garnitureStandard2 = f.GarnitureStandard2
                    })
                    .ToListAsync();

                return Json(new { success = true, formules = formules });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des formules");
                return Json(new { success = false, error = "Erreur lors de la récupération des formules" });
            }
        }

        /// <summary>
        /// Crée les commandes pour les visiteurs
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCommandeVisiteurViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.Directions = await _context.Directions
                        .Where(d => d.Supprimer == 0)
                        .OrderBy(d => d.Nom)
                        .ToListAsync();
                    return View(model);
                }

                // Validation des dates
                if (model.DateFin < model.DateDebut)
                {
                    ModelState.AddModelError("DateFin", "La date de fin doit être supérieure ou égale à la date de début");
                    ViewBag.Directions = await _context.Directions
                        .Where(d => d.Supprimer == 0)
                        .OrderBy(d => d.Nom)
                        .ToListAsync();
                    return View(model);
                }

                // Validation : Les commandes pour visiteurs doivent être créées au moins 48h à l'avance
                var maintenant = DateTime.Now;
                var delaiMinimum = maintenant.AddHours(48);
                
                if (model.DateDebut < delaiMinimum)
                {
                    var dateLimite = delaiMinimum.ToString("dd/MM/yyyy à HH:mm");
                    ModelState.AddModelError("DateDebut", $"Les commandes pour visiteurs doivent être créées au moins 48h à l'avance. Date limite: {dateLimite}");
                    ViewBag.Directions = await _context.Directions
                        .Where(d => d.Supprimer == 0)
                        .OrderBy(d => d.Nom)
                        .ToListAsync();
                    return View(model);
                }

                // Récupérer la direction
                var direction = await _context.Directions
                    .FirstOrDefaultAsync(d => d.Id == model.DirectionId && d.Supprimer == 0);

                if (direction == null)
                {
                    ModelState.AddModelError("DirectionId", "Direction introuvable");
                    ViewBag.Directions = await _context.Directions
                        .Where(d => d.Supprimer == 0)
                        .OrderBy(d => d.Nom)
                        .ToListAsync();
                    return View(model);
                }

                // Générer le code de commande avec la date
                var codeDirection = GenerateCodeDirection(direction.Nom);
                var dateDebut = model.DateDebut.ToString("yyyyMMdd");
                var codeCommande = await GenerateUniqueCodeCommande($"{codeDirection}-GRP-{dateDebut}");

                // Récupérer les formules de la période
                var formules = await _context.FormulesJour
                    .Where(f => f.Supprimer == 0 && 
                               f.Date >= model.DateDebut && 
                               f.Date <= model.DateFin)
                    .OrderBy(f => f.Date)
                    .ToListAsync();

                if (!formules.Any())
                {
                    TempData["ErrorMessage"] = "Aucune formule disponible pour cette période.";
                    ViewBag.Directions = await _context.Directions
                        .Where(d => d.Supprimer == 0)
                        .OrderBy(d => d.Nom)
                        .ToListAsync();
                    return View(model);
                }

                // Créer les commandes
                var commandesCreees = 0;
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
                var userName = User.Identity?.Name ?? "System";

                foreach (var date in GetDateRange(model.DateDebut, model.DateFin))
                {
                    // Trouver la formule correspondante au type choisi
                    var formule = formules.FirstOrDefault(f => 
                        f.Date.Date == date.Date && 
                        IsMatchingFormuleType(f, model.TypeFormule));

                    if (formule != null)
                    {
                        var commande = new Commande
                        {
                            IdCommande = Guid.NewGuid(),
                            CodeCommande = codeCommande,
                            Date = DateTime.UtcNow,
                            DateConsommation = date,
                            StatusCommande = 0, // Précommandé
                            Quantite = model.NombreVisiteurs,
                            PeriodeService = model.PeriodeService,
                            UtilisateurId = null, // Pas d'utilisateur CIT pour les visiteurs
                            IdFormule = formule.IdFormule,
                            Montant = 0, // Les visiteurs ne paient pas
                            VisiteurNom = $"Groupe {direction.Nom} ({model.NombreVisiteurs} {(model.NombreVisiteurs > 1 ? "personnes" : "personne")})",
                            // DirectionId supprimé - Table Direction non utilisée
                            TypeClient = TypeClientCommande.Visiteur,
                            CreatedOn = DateTime.UtcNow,
                            CreatedBy = userName,
                            Supprimer = 0
                        };

                        _context.Commandes.Add(commande);
                        commandesCreees++;
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Commandes visiteurs créées: {Count} commandes pour {NombreVisiteurs} visiteurs de la direction {Direction}", 
                    commandesCreees, model.NombreVisiteurs, direction.Nom);

                TempData["SuccessMessage"] = $"✅ {commandesCreees} commande(s) créée(s) avec succès pour {model.NombreVisiteurs} visiteur(s) de la direction {direction.Nom}. Code: {codeCommande}";
                return RedirectToAction("Create");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création des commandes visiteurs");
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la création des commandes.";
                ViewBag.Directions = await _context.Directions
                    .Where(d => d.Supprimer == 0)
                    .OrderBy(d => d.Nom)
                    .ToListAsync();
                return View(model);
            }
        }

        /// <summary>
        /// <summary>
        /// Génère un code de direction à partir du nom
        /// </summary>
        private string GenerateCodeDirection(string nomDirection)
        {
            // Prendre les 3 premières lettres ou utiliser un acronyme
            var mots = nomDirection.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            
            if (mots.Length > 1)
            {
                // Prendre la première lettre de chaque mot (max 3)
                return string.Join("", mots.Take(3).Select(m => m[0])).ToLower();
            }
            else
            {
                // Prendre les 3 premières lettres
                return nomDirection.Substring(0, Math.Min(3, nomDirection.Length)).ToLower();
            }
        }


        /// <summary>
        /// Vérifie si une formule correspond au type demandé
        /// </summary>
        private bool IsMatchingFormuleType(FormuleJour formule, string typeFormule)
        {
            if (string.IsNullOrEmpty(formule.NomFormule))
                return false;

            return typeFormule.ToLower() switch
            {
                "amélioré" => formule.NomFormule.Contains("Amélioré", StringComparison.OrdinalIgnoreCase) ||
                             formule.NomFormule.Contains("Améliorée", StringComparison.OrdinalIgnoreCase),
                "standard 1" => formule.NomFormule.Contains("Standard 1", StringComparison.OrdinalIgnoreCase),
                "standard 2" => formule.NomFormule.Contains("Standard 2", StringComparison.OrdinalIgnoreCase),
                _ => false
            };
        }

        /// <summary>
        /// Génère une liste de dates entre deux dates
        /// </summary>
        private IEnumerable<DateTime> GetDateRange(DateTime startDate, DateTime endDate)
        {
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                yield return date;
            }
        }

        /// <summary>
        /// Génère un code unique pour un visiteur basé sur son nom
        /// </summary>
        private string GenerateCodeVisiteur(string nomVisiteur)
        {
            // Prendre les 3 premières lettres du nom et ajouter un numéro séquentiel
            var nomNettoye = nomVisiteur.Replace(" ", "").ToUpper();
            var prefixe = nomNettoye.Length >= 3 ? nomNettoye.Substring(0, 3) : nomNettoye.PadRight(3, 'X');
            
            // Générer un numéro séquentiel basé sur la date et l'heure
            var timestamp = DateTime.Now.ToString("HHmmss");
            var numero = timestamp.Substring(timestamp.Length - 3); // 3 derniers chiffres
            
            return $"{prefixe}{numero}";
        }

        /// <summary>
        /// Génère un code de commande unique en vérifiant l'unicité dans la base de données
        /// </summary>
        private async Task<string> GenerateUniqueCodeCommande(string baseCode)
        {
            var codeCommande = baseCode;
            var counter = 1;
            
            // Vérifier si le code existe déjà
            while (await _context.Commandes.AnyAsync(c => c.CodeCommande == codeCommande && c.Supprimer == 0))
            {
                codeCommande = $"{baseCode}-{counter:D2}";
                counter++;
                
                // Sécurité : éviter les boucles infinies
                if (counter > 99)
                {
                    // Si on dépasse 99, ajouter un timestamp unique
                    var timestamp = DateTime.Now.ToString("HHmmss");
                    codeCommande = $"{baseCode}-{timestamp}";
                    break;
                }
            }
            
            return codeCommande;
        }
    }

}

/// <summary>
/// Modèle de requête pour créer une commande visiteur
/// </summary>
public class CreateCommandeVisiteurRequest
{
    public string VisiteurNom { get; set; } = string.Empty;
    public string? VisiteurTelephone { get; set; }
    public Guid DirectionId { get; set; }
    public Guid IdFormule { get; set; }
    public DateTime DateConsommation { get; set; }
}

/// <summary>
/// Modèle de requête pour récupérer les formules
/// </summary>
public class GetFormulesRequest
{
    public DateTime DateDebut { get; set; }
    public DateTime DateFin { get; set; }
}