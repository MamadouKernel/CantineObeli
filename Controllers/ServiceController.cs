using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Obeli_K.Data;
using Obeli_K.Models;
using Obeli_K.Models.ViewModels;

namespace Obeli_K.Controllers
{
    [Authorize(Roles = "Administrateur,RH")]
    public class ServiceController : Controller
    {
        private readonly ObeliDbContext _context;
        private readonly ILogger<ServiceController> _logger;

        public ServiceController(ObeliDbContext context, ILogger<ServiceController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Affiche la page d'accueil des services
        /// </summary>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Affiche la liste des services
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> List(int page = 1, int pageSize = 5)
        {
            try
            {
                var query = _context.Services
                    .Include(s => s.Direction)
                    .Where(s => s.Supprimer == 0)
                    .OrderBy(s => s.Nom);

                var totalCount = await query.CountAsync();

                var services = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(s => new
                    {
                        s.Id,
                        s.Nom,
                        s.Description,
                        s.Code,
                        s.Responsable,
                        s.Email,
                        s.DirectionId,
                        DirectionNom = s.Direction != null ? s.Direction.Nom : "N/A",
                        s.CreatedOn,
                        s.CreatedBy,
                        s.ModifiedOn,
                        s.ModifiedBy,
                        NombreUtilisateurs = _context.Utilisateurs.Count(u => u.ServiceId == s.Id && u.Supprimer == 0)
                    })
                    .ToListAsync();

                var pagination = new PaginationViewModel(HttpContext, "List", "Service")
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalItems = totalCount
                };

                var pagedModel = new PagedViewModel<object>
                {
                    Items = services,
                    Pagination = pagination
                };

                ViewBag.Services = services;
                return View(pagedModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement de la liste des services");
                TempData["ErrorMessage"] = "Une erreur est survenue lors du chargement des services.";
                return View(new PagedViewModel<object>());
            }
        }

        /// <summary>
        /// Affiche le formulaire de création de service
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Directions = await _context.Directions
                .Where(d => d.Supprimer == 0)
                .OrderBy(d => d.Nom)
                .Select(d => new { d.Id, d.Nom })
                .ToListAsync();

            return View();
        }

        /// <summary>
        /// Traite la création d'un nouveau service
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Service service)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(service.Nom))
                {
                    ModelState.AddModelError("Nom", "Le nom du service est obligatoire.");
                }

                if (await _context.Services.AnyAsync(s => s.Nom == service.Nom && s.Supprimer == 0))
                {
                    ModelState.AddModelError("Nom", "Un service avec ce nom existe déjà.");
                }

                if (!string.IsNullOrWhiteSpace(service.Code) && 
                    await _context.Services.AnyAsync(s => s.Code == service.Code && s.Supprimer == 0))
                {
                    ModelState.AddModelError("Code", "Un service avec ce code existe déjà.");
                }

                if (!ModelState.IsValid)
                {
                    ViewBag.Directions = await _context.Directions
                        .Where(d => d.Supprimer == 0)
                        .OrderBy(d => d.Nom)
                        .Select(d => new { d.Id, d.Nom })
                        .ToListAsync();
                    return View(service);
                }

                service.Id = Guid.NewGuid();
                service.CreatedOn = DateTime.UtcNow;
                service.CreatedBy = User.Identity?.Name ?? "System";
                service.Supprimer = 0;

                _context.Services.Add(service);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Nouveau service créé: {service.Nom} par {User.Identity?.Name}");
                TempData["SuccessMessage"] = $"Le service '{service.Nom}' a été créé avec succès.";

                return RedirectToAction(nameof(List));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création du service");
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la création du service.";
                ViewBag.Directions = await _context.Directions
                    .Where(d => d.Supprimer == 0)
                    .OrderBy(d => d.Nom)
                    .Select(d => new { d.Id, d.Nom })
                    .ToListAsync();
                return View(service);
            }
        }

        /// <summary>
        /// Affiche les détails d'un service
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var service = await _context.Services
                    .Include(s => s.Direction)
                    .FirstOrDefaultAsync(s => s.Id == id && s.Supprimer == 0);

                if (service == null)
                {
                    TempData["ErrorMessage"] = "Service introuvable.";
                    return RedirectToAction(nameof(List));
                }

                var utilisateurs = await _context.Utilisateurs
                    .Include(u => u.Fonction)
                    .Include(u => u.Direction)
                    .Where(u => u.ServiceId == id && u.Supprimer == 0)
                    .OrderBy(u => u.Nom)
                    .ThenBy(u => u.Prenoms)
                    .Select(u => new
                    {
                        u.Id,
                        u.Nom,
                        u.Prenoms,
                        u.UserName,
                        u.Email,
                        u.PhoneNumber,
                        u.Role,
                        u.Lieu,
                        u.Site,
                        FonctionNom = u.Fonction != null ? u.Fonction.Nom : "N/A",
                        DirectionNom = u.Direction != null ? u.Direction.Nom : "N/A",
                        u.CreatedAt
                    })
                    .ToListAsync();

                ViewBag.Utilisateurs = utilisateurs;
                return View(service);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement des détails du service {ServiceId}", id);
                TempData["ErrorMessage"] = "Une erreur est survenue lors du chargement des détails.";
                return RedirectToAction(nameof(List));
            }
        }

        /// <summary>
        /// Affiche le formulaire d'édition d'un service
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var service = await _context.Services
                    .FirstOrDefaultAsync(s => s.Id == id && s.Supprimer == 0);

                if (service == null)
                {
                    TempData["ErrorMessage"] = "Service introuvable.";
                    return RedirectToAction(nameof(List));
                }

                ViewBag.Directions = await _context.Directions
                    .Where(d => d.Supprimer == 0)
                    .OrderBy(d => d.Nom)
                    .Select(d => new { d.Id, d.Nom })
                    .ToListAsync();

                return View(service);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement du service pour édition {ServiceId}", id);
                TempData["ErrorMessage"] = "Une erreur est survenue lors du chargement du service.";
                return RedirectToAction(nameof(List));
            }
        }

        /// <summary>
        /// Traite la modification d'un service
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, Service service)
        {
            try
            {
                if (id != service.Id)
                {
                    TempData["ErrorMessage"] = "Identifiant service invalide.";
                    return RedirectToAction(nameof(List));
                }

                var existingService = await _context.Services
                    .FirstOrDefaultAsync(s => s.Id == id && s.Supprimer == 0);

                if (existingService == null)
                {
                    TempData["ErrorMessage"] = "Service introuvable.";
                    return RedirectToAction(nameof(List));
                }

                if (string.IsNullOrWhiteSpace(service.Nom))
                {
                    ModelState.AddModelError("Nom", "Le nom du service est obligatoire.");
                }

                if (await _context.Services.AnyAsync(s => s.Nom == service.Nom && s.Id != id && s.Supprimer == 0))
                {
                    ModelState.AddModelError("Nom", "Un service avec ce nom existe déjà.");
                }

                if (!string.IsNullOrWhiteSpace(service.Code) && 
                    await _context.Services.AnyAsync(s => s.Code == service.Code && s.Id != id && s.Supprimer == 0))
                {
                    ModelState.AddModelError("Code", "Un service avec ce code existe déjà.");
                }

                if (!ModelState.IsValid)
                {
                    ViewBag.Directions = await _context.Directions
                        .Where(d => d.Supprimer == 0)
                        .OrderBy(d => d.Nom)
                        .Select(d => new { d.Id, d.Nom })
                        .ToListAsync();
                    return View(service);
                }

                existingService.Nom = service.Nom;
                existingService.Description = service.Description;
                existingService.Code = service.Code;
                existingService.Responsable = service.Responsable;
                existingService.Email = service.Email;
                existingService.DirectionId = service.DirectionId;
                existingService.ModifiedOn = DateTime.UtcNow;
                existingService.ModifiedBy = User.Identity?.Name ?? "System";

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Service modifié: {existingService.Nom} par {User.Identity?.Name}");
                TempData["SuccessMessage"] = $"Le service '{existingService.Nom}' a été modifié avec succès.";

                return RedirectToAction(nameof(List));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la modification du service {ServiceId}", id);
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la modification du service.";
                ViewBag.Directions = await _context.Directions
                    .Where(d => d.Supprimer == 0)
                    .OrderBy(d => d.Nom)
                    .Select(d => new { d.Id, d.Nom })
                    .ToListAsync();
                return View(service);
            }
        }

        /// <summary>
        /// Supprime un service (soft delete)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var service = await _context.Services
                    .FirstOrDefaultAsync(s => s.Id == id && s.Supprimer == 0);

                if (service == null)
                {
                    TempData["ErrorMessage"] = "Service introuvable.";
                    return RedirectToAction(nameof(List));
                }

                var utilisateursCount = await _context.Utilisateurs
                    .CountAsync(u => u.ServiceId == id && u.Supprimer == 0);

                if (utilisateursCount > 0)
                {
                    TempData["ErrorMessage"] = $"Impossible de supprimer ce service car il contient {utilisateursCount} utilisateur(s). Veuillez d'abord réassigner ou supprimer les utilisateurs.";
                    return RedirectToAction(nameof(List));
                }

                service.Supprimer = 1;
                service.ModifiedOn = DateTime.UtcNow;
                service.ModifiedBy = User.Identity?.Name ?? "System";

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Service supprimé: {service.Nom} par {User.Identity?.Name}");
                TempData["SuccessMessage"] = $"Le service '{service.Nom}' a été supprimé avec succès.";

                return RedirectToAction(nameof(List));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression du service {ServiceId}", id);
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la suppression du service.";
                return RedirectToAction(nameof(List));
            }
        }

        /// <summary>
        /// API pour récupérer les services (pour les dropdowns)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetServices(Guid? directionId = null)
        {
            try
            {
                var query = _context.Services
                    .Where(s => s.Supprimer == 0);

                if (directionId.HasValue)
                {
                    query = query.Where(s => s.DirectionId == directionId.Value);
                }

                var services = await query
                    .OrderBy(s => s.Nom)
                    .Select(s => new
                    {
                        s.Id,
                        s.Nom,
                        s.Description,
                        s.Code,
                        s.DirectionId
                    })
                    .ToListAsync();

                return Json(new { success = true, data = services });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des services");
                return Json(new { success = false, message = "Erreur lors de la récupération des services" });
            }
        }
    }
}
