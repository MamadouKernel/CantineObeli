using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Obeli_K.Data;
using Obeli_K.Models;

namespace Obeli_K.Controllers
{
    [Authorize(Roles = "Administrateur,RessourcesHumaines")]
    public class DepartementController : Controller
    {
        private readonly ObeliDbContext _context;
        private readonly ILogger<DepartementController> _logger;

        public DepartementController(ObeliDbContext context, ILogger<DepartementController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Affiche la page d'accueil des départements
        /// </summary>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Affiche la liste des départements
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> List()
        {
            try
            {
                var departements = await _context.Departements
                    .Where(d => d.Supprimer == 0)
                    .OrderBy(d => d.Nom)
                    .Select(d => new
                    {
                        d.Id,
                        d.Nom,
                        d.Description,
                        d.CreatedOn,
                        d.CreatedBy,
                        d.ModifiedOn,
                        d.ModifiedBy,
                        // Compter le nombre d'utilisateurs actifs dans ce département
                        NombreUtilisateurs = _context.Utilisateurs.Count(u => u.DepartementId == d.Id && u.Supprimer == 0)
                    })
                    .ToListAsync();

                ViewBag.Departements = departements;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement de la liste des départements");
                TempData["ErrorMessage"] = "Une erreur est survenue lors du chargement des départements.";
                return View(new List<object>());
            }
        }

        /// <summary>
        /// Affiche le formulaire de création de département
        /// </summary>
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Traite la création d'un nouveau département
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Departement departement)
        {
            try
            {
                // Validation du nom (obligatoire)
                if (string.IsNullOrWhiteSpace(departement.Nom))
                {
                    ModelState.AddModelError("Nom", "Le nom du département est obligatoire.");
                }

                // Vérifier si le nom existe déjà
                if (await _context.Departements.AnyAsync(d => d.Nom == departement.Nom && d.Supprimer == 0))
                {
                    ModelState.AddModelError("Nom", "Un département avec ce nom existe déjà.");
                }

                if (!ModelState.IsValid)
                {
                    return View(departement);
                }

                // Créer le département
                departement.Id = Guid.NewGuid();
                departement.CreatedOn = DateTime.UtcNow;
                departement.CreatedBy = User.Identity?.Name ?? "System";
                departement.Supprimer = 0;

                _context.Departements.Add(departement);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Nouveau département créé: {departement.Nom} par {User.Identity?.Name}");
                TempData["SuccessMessage"] = $"Le département '{departement.Nom}' a été créé avec succès.";

                return RedirectToAction(nameof(List));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création du département");
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la création du département.";
                return View(departement);
            }
        }

        /// <summary>
        /// Affiche les détails d'un département
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var departement = await _context.Departements
                    .FirstOrDefaultAsync(d => d.Id == id && d.Supprimer == 0);

                if (departement == null)
                {
                    TempData["ErrorMessage"] = "Département introuvable.";
                    return RedirectToAction(nameof(List));
                }

                // Récupérer les utilisateurs de ce département
                var utilisateurs = await _context.Utilisateurs
                    .Include(u => u.Fonction)
                    .Where(u => u.DepartementId == id && u.Supprimer == 0)
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
                        u.CreatedAt
                    })
                    .ToListAsync();

                ViewBag.Utilisateurs = utilisateurs;
                return View(departement);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement des détails du département {DepartementId}", id);
                TempData["ErrorMessage"] = "Une erreur est survenue lors du chargement des détails.";
                return RedirectToAction(nameof(List));
            }
        }

        /// <summary>
        /// Affiche le formulaire d'édition d'un département
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var departement = await _context.Departements
                    .FirstOrDefaultAsync(d => d.Id == id && d.Supprimer == 0);

                if (departement == null)
                {
                    TempData["ErrorMessage"] = "Département introuvable.";
                    return RedirectToAction(nameof(List));
                }

                return View(departement);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement du département pour édition {DepartementId}", id);
                TempData["ErrorMessage"] = "Une erreur est survenue lors du chargement du département.";
                return RedirectToAction(nameof(List));
            }
        }

        /// <summary>
        /// Traite la modification d'un département
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, Departement departement)
        {
            try
            {
                if (id != departement.Id)
                {
                    TempData["ErrorMessage"] = "Identifiant département invalide.";
                    return RedirectToAction(nameof(List));
                }

                var existingDepartement = await _context.Departements
                    .FirstOrDefaultAsync(d => d.Id == id && d.Supprimer == 0);

                if (existingDepartement == null)
                {
                    TempData["ErrorMessage"] = "Département introuvable.";
                    return RedirectToAction(nameof(List));
                }

                // Validation du nom (obligatoire)
                if (string.IsNullOrWhiteSpace(departement.Nom))
                {
                    ModelState.AddModelError("Nom", "Le nom du département est obligatoire.");
                }

                // Vérifier si le nom existe déjà (sauf pour le département actuel)
                if (await _context.Departements.AnyAsync(d => d.Nom == departement.Nom && d.Id != id && d.Supprimer == 0))
                {
                    ModelState.AddModelError("Nom", "Un département avec ce nom existe déjà.");
                }

                if (!ModelState.IsValid)
                {
                    return View(departement);
                }

                // Mettre à jour les propriétés
                existingDepartement.Nom = departement.Nom;
                existingDepartement.Description = departement.Description;
                existingDepartement.ModifiedOn = DateTime.UtcNow;
                existingDepartement.ModifiedBy = User.Identity?.Name ?? "System";

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Département modifié: {existingDepartement.Nom} par {User.Identity?.Name}");
                TempData["SuccessMessage"] = $"Le département '{existingDepartement.Nom}' a été modifié avec succès.";

                return RedirectToAction(nameof(List));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la modification du département {DepartementId}", id);
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la modification du département.";
                return View(departement);
            }
        }

        /// <summary>
        /// Supprime un département (soft delete)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var departement = await _context.Departements
                    .FirstOrDefaultAsync(d => d.Id == id && d.Supprimer == 0);

                if (departement == null)
                {
                    TempData["ErrorMessage"] = "Département introuvable.";
                    return RedirectToAction(nameof(List));
                }

                // Vérifier s'il y a des utilisateurs dans ce département
                var utilisateursCount = await _context.Utilisateurs
                    .CountAsync(u => u.DepartementId == id && u.Supprimer == 0);

                if (utilisateursCount > 0)
                {
                    TempData["ErrorMessage"] = $"Impossible de supprimer ce département car il contient {utilisateursCount} utilisateur(s). Veuillez d'abord réassigner ou supprimer les utilisateurs.";
                    return RedirectToAction(nameof(List));
                }

                // Soft delete
                departement.Supprimer = 1;
                departement.ModifiedOn = DateTime.UtcNow;
                departement.ModifiedBy = User.Identity?.Name ?? "System";

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Département supprimé: {departement.Nom} par {User.Identity?.Name}");
                TempData["SuccessMessage"] = $"Le département '{departement.Nom}' a été supprimé avec succès.";

                return RedirectToAction(nameof(List));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression du département {DepartementId}", id);
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la suppression du département.";
                return RedirectToAction(nameof(List));
            }
        }

        /// <summary>
        /// API pour récupérer les départements (pour les dropdowns)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetDepartements()
        {
            try
            {
                var departements = await _context.Departements
                    .Where(d => d.Supprimer == 0)
                    .OrderBy(d => d.Nom)
                    .Select(d => new
                    {
                        d.Id,
                        d.Nom,
                        d.Description
                    })
                    .ToListAsync();

                return Json(new { success = true, data = departements });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des départements");
                return Json(new { success = false, message = "Erreur lors de la récupération des départements" });
            }
        }
    }
}
