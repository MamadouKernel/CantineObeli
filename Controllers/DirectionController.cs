using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Obeli_K.Data;
using Obeli_K.Models;
using Obeli_K.Models.ViewModels;

namespace Obeli_K.Controllers
{
    [Authorize(Roles = "Administrateur,RH")]
    public class DirectionController : Controller
    {
        private readonly ObeliDbContext _context;
        private readonly ILogger<DirectionController> _logger;

        public DirectionController(ObeliDbContext context, ILogger<DirectionController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Affiche la page d'accueil des directions
        /// </summary>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Affiche la liste des directions
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> List(int page = 1, int pageSize = 5)
        {
            try
            {
                var query = _context.Directions
                    .Where(d => d.Supprimer == 0)
                    .OrderBy(d => d.Nom);

                var totalCount = await query.CountAsync();

                var directions = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(d => new
                    {
                        d.Id,
                        d.Nom,
                        d.Description,
                        d.Code,
                        d.Responsable,
                        d.Email,
                        d.CreatedOn,
                        d.CreatedBy,
                        d.ModifiedOn,
                        d.ModifiedBy,
                        NombreServices = _context.Services.Count(s => s.DirectionId == d.Id && s.Supprimer == 0)
                    })
                    .ToListAsync();

                var pagination = new PaginationViewModel(HttpContext, "List", "Direction")
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalItems = totalCount
                };

                var pagedModel = new PagedViewModel<object>
                {
                    Items = directions,
                    Pagination = pagination
                };

                ViewBag.Directions = directions;
                return View(pagedModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement de la liste des directions");
                TempData["ErrorMessage"] = "Une erreur est survenue lors du chargement des directions.";
                return View(new PagedViewModel<object>());
            }
        }

        /// <summary>
        /// Affiche le formulaire de création de direction
        /// </summary>
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Traite la création d'une nouvelle direction
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Direction direction)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(direction.Nom))
                {
                    ModelState.AddModelError("Nom", "Le nom de la direction est obligatoire.");
                }

                if (await _context.Directions.AnyAsync(d => d.Nom == direction.Nom && d.Supprimer == 0))
                {
                    ModelState.AddModelError("Nom", "Une direction avec ce nom existe déjà.");
                }

                if (!string.IsNullOrWhiteSpace(direction.Code) && 
                    await _context.Directions.AnyAsync(d => d.Code == direction.Code && d.Supprimer == 0))
                {
                    ModelState.AddModelError("Code", "Une direction avec ce code existe déjà.");
                }

                if (!ModelState.IsValid)
                {
                    return View(direction);
                }

                direction.Id = Guid.NewGuid();
                direction.CreatedOn = DateTime.UtcNow;
                direction.CreatedBy = User.Identity?.Name ?? "System";
                direction.Supprimer = 0;

                _context.Directions.Add(direction);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Nouvelle direction créée: {direction.Nom} par {User.Identity?.Name}");
                TempData["SuccessMessage"] = $"La direction '{direction.Nom}' a été créée avec succès.";

                return RedirectToAction(nameof(List));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création de la direction");
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la création de la direction.";
                return View(direction);
            }
        }

        /// <summary>
        /// Affiche les détails d'une direction
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var direction = await _context.Directions
                    .FirstOrDefaultAsync(d => d.Id == id && d.Supprimer == 0);

                if (direction == null)
                {
                    TempData["ErrorMessage"] = "Direction introuvable.";
                    return RedirectToAction(nameof(List));
                }

                var services = await _context.Services
                    .Where(s => s.DirectionId == id && s.Supprimer == 0)
                    .OrderBy(s => s.Nom)
                    .Select(s => new
                    {
                        s.Id,
                        s.Nom,
                        s.Description,
                        s.Code,
                        s.Responsable,
                        s.Email,
                        NombreUtilisateurs = _context.Utilisateurs.Count(u => u.ServiceId == s.Id && u.Supprimer == 0)
                    })
                    .ToListAsync();

                ViewBag.Services = services;
                return View(direction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement des détails de la direction {DirectionId}", id);
                TempData["ErrorMessage"] = "Une erreur est survenue lors du chargement des détails.";
                return RedirectToAction(nameof(List));
            }
        }

        /// <summary>
        /// Affiche le formulaire d'édition d'une direction
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var direction = await _context.Directions
                    .FirstOrDefaultAsync(d => d.Id == id && d.Supprimer == 0);

                if (direction == null)
                {
                    TempData["ErrorMessage"] = "Direction introuvable.";
                    return RedirectToAction(nameof(List));
                }

                return View(direction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement de la direction pour édition {DirectionId}", id);
                TempData["ErrorMessage"] = "Une erreur est survenue lors du chargement de la direction.";
                return RedirectToAction(nameof(List));
            }
        }

        /// <summary>
        /// Traite la modification d'une direction
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, Direction direction)
        {
            try
            {
                if (id != direction.Id)
                {
                    TempData["ErrorMessage"] = "Identifiant direction invalide.";
                    return RedirectToAction(nameof(List));
                }

                var existingDirection = await _context.Directions
                    .FirstOrDefaultAsync(d => d.Id == id && d.Supprimer == 0);

                if (existingDirection == null)
                {
                    TempData["ErrorMessage"] = "Direction introuvable.";
                    return RedirectToAction(nameof(List));
                }

                if (string.IsNullOrWhiteSpace(direction.Nom))
                {
                    ModelState.AddModelError("Nom", "Le nom de la direction est obligatoire.");
                }

                if (await _context.Directions.AnyAsync(d => d.Nom == direction.Nom && d.Id != id && d.Supprimer == 0))
                {
                    ModelState.AddModelError("Nom", "Une direction avec ce nom existe déjà.");
                }

                if (!string.IsNullOrWhiteSpace(direction.Code) && 
                    await _context.Directions.AnyAsync(d => d.Code == direction.Code && d.Id != id && d.Supprimer == 0))
                {
                    ModelState.AddModelError("Code", "Une direction avec ce code existe déjà.");
                }

                if (!ModelState.IsValid)
                {
                    return View(direction);
                }

                existingDirection.Nom = direction.Nom;
                existingDirection.Description = direction.Description;
                existingDirection.Code = direction.Code;
                existingDirection.Responsable = direction.Responsable;
                existingDirection.Email = direction.Email;
                existingDirection.ModifiedOn = DateTime.UtcNow;
                existingDirection.ModifiedBy = User.Identity?.Name ?? "System";

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Direction modifiée: {existingDirection.Nom} par {User.Identity?.Name}");
                TempData["SuccessMessage"] = $"La direction '{existingDirection.Nom}' a été modifiée avec succès.";

                return RedirectToAction(nameof(List));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la modification de la direction {DirectionId}", id);
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la modification de la direction.";
                return View(direction);
            }
        }

        /// <summary>
        /// Supprime une direction (soft delete)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var direction = await _context.Directions
                    .FirstOrDefaultAsync(d => d.Id == id && d.Supprimer == 0);

                if (direction == null)
                {
                    TempData["ErrorMessage"] = "Direction introuvable.";
                    return RedirectToAction(nameof(List));
                }

                var directionsCount = await _context.Services
                    .CountAsync(s => s.DirectionId == id && s.Supprimer == 0);

                if (directionsCount > 0)
                {
                    TempData["ErrorMessage"] = $"Impossible de supprimer cette direction car elle contient {directionsCount} service(s). Veuillez d'abord réassigner ou supprimer les services.";
                    return RedirectToAction(nameof(List));
                }

                direction.Supprimer = 1;
                direction.ModifiedOn = DateTime.UtcNow;
                direction.ModifiedBy = User.Identity?.Name ?? "System";

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Direction supprimée: {direction.Nom} par {User.Identity?.Name}");
                TempData["SuccessMessage"] = $"La direction '{direction.Nom}' a été supprimée avec succès.";

                return RedirectToAction(nameof(List));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression de la direction {DirectionId}", id);
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la suppression de la direction.";
                return RedirectToAction(nameof(List));
            }
        }

        /// <summary>
        /// API pour récupérer les directions (pour les dropdowns)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetDirections()
        {
            try
            {
                var directions = await _context.Directions
                    .Where(d => d.Supprimer == 0)
                    .OrderBy(d => d.Nom)
                    .Select(d => new
                    {
                        d.Id,
                        d.Nom,
                        d.Description,
                        d.Code
                    })
                    .ToListAsync();

                return Json(new { success = true, data = directions });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des directions");
                return Json(new { success = false, message = "Erreur lors de la récupération des directions" });
            }
        }
    }
}
