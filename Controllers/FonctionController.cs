using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Obeli_K.Data;
using Obeli_K.Models;

namespace Obeli_K.Controllers
{
    [Authorize(Roles = "Administrateur,RessourcesHumaines")]
    public class FonctionController : Controller
    {
        private readonly ObeliDbContext _context;
        private readonly ILogger<FonctionController> _logger;

        public FonctionController(ObeliDbContext context, ILogger<FonctionController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Affiche la page d'accueil des fonctions
        /// </summary>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Affiche la liste des fonctions
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> List()
        {
            try
            {
                var fonctions = await _context.Fonctions
                    .Where(f => f.Supprimer == 0)
                    .OrderBy(f => f.Nom)
                    .Select(f => new
                    {
                        f.Id,
                        f.Nom,
                        f.Description,
                        f.CreatedOn,
                        f.CreatedBy,
                        f.ModifiedOn,
                        f.ModifiedBy,
                        // Compter le nombre d'utilisateurs actifs avec cette fonction
                        NombreUtilisateurs = _context.Utilisateurs.Count(u => u.FonctionId == f.Id && u.Supprimer == 0)
                    })
                    .ToListAsync();

                ViewBag.Fonctions = fonctions;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement de la liste des fonctions");
                TempData["ErrorMessage"] = "Une erreur est survenue lors du chargement des fonctions.";
                return View(new List<object>());
            }
        }

        /// <summary>
        /// Affiche le formulaire de création de fonction
        /// </summary>
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Traite la création d'une nouvelle fonction
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Fonction fonction)
        {
            try
            {
                // Validation du nom (obligatoire)
                if (string.IsNullOrWhiteSpace(fonction.Nom))
                {
                    ModelState.AddModelError("Nom", "Le nom de la fonction est obligatoire.");
                }

                // Vérifier si le nom existe déjà
                if (await _context.Fonctions.AnyAsync(f => f.Nom == fonction.Nom && f.Supprimer == 0))
                {
                    ModelState.AddModelError("Nom", "Une fonction avec ce nom existe déjà.");
                }

                if (!ModelState.IsValid)
                {
                    return View(fonction);
                }

                // Créer la fonction
                fonction.Id = Guid.NewGuid();
                fonction.CreatedOn = DateTime.UtcNow;
                fonction.CreatedBy = User.Identity?.Name ?? "System";
                fonction.Supprimer = 0;

                _context.Fonctions.Add(fonction);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Nouvelle fonction créée: {fonction.Nom} par {User.Identity?.Name}");
                TempData["SuccessMessage"] = $"La fonction '{fonction.Nom}' a été créée avec succès.";

                return RedirectToAction(nameof(List));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création de la fonction");
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la création de la fonction.";
                return View(fonction);
            }
        }

        /// <summary>
        /// Affiche les détails d'une fonction
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var fonction = await _context.Fonctions
                    .FirstOrDefaultAsync(f => f.Id == id && f.Supprimer == 0);

                if (fonction == null)
                {
                    TempData["ErrorMessage"] = "Fonction introuvable.";
                    return RedirectToAction(nameof(List));
                }

                // Récupérer les utilisateurs avec cette fonction
                var utilisateurs = await _context.Utilisateurs
                    .Include(u => u.Departement)
                    .Where(u => u.FonctionId == id && u.Supprimer == 0)
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
                        DepartementNom = u.Departement != null ? u.Departement.Nom : "N/A",
                        u.CreatedAt
                    })
                    .ToListAsync();

                ViewBag.Utilisateurs = utilisateurs;
                return View(fonction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement des détails de la fonction {FonctionId}", id);
                TempData["ErrorMessage"] = "Une erreur est survenue lors du chargement des détails.";
                return RedirectToAction(nameof(List));
            }
        }

        /// <summary>
        /// Affiche le formulaire d'édition d'une fonction
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var fonction = await _context.Fonctions
                    .FirstOrDefaultAsync(f => f.Id == id && f.Supprimer == 0);

                if (fonction == null)
                {
                    TempData["ErrorMessage"] = "Fonction introuvable.";
                    return RedirectToAction(nameof(List));
                }

                return View(fonction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement de la fonction pour édition {FonctionId}", id);
                TempData["ErrorMessage"] = "Une erreur est survenue lors du chargement de la fonction.";
                return RedirectToAction(nameof(List));
            }
        }

        /// <summary>
        /// Traite la modification d'une fonction
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, Fonction fonction)
        {
            try
            {
                if (id != fonction.Id)
                {
                    TempData["ErrorMessage"] = "Identifiant fonction invalide.";
                    return RedirectToAction(nameof(List));
                }

                var existingFonction = await _context.Fonctions
                    .FirstOrDefaultAsync(f => f.Id == id && f.Supprimer == 0);

                if (existingFonction == null)
                {
                    TempData["ErrorMessage"] = "Fonction introuvable.";
                    return RedirectToAction(nameof(List));
                }

                // Validation du nom (obligatoire)
                if (string.IsNullOrWhiteSpace(fonction.Nom))
                {
                    ModelState.AddModelError("Nom", "Le nom de la fonction est obligatoire.");
                }

                // Vérifier si le nom existe déjà (sauf pour la fonction actuelle)
                if (await _context.Fonctions.AnyAsync(f => f.Nom == fonction.Nom && f.Id != id && f.Supprimer == 0))
                {
                    ModelState.AddModelError("Nom", "Une fonction avec ce nom existe déjà.");
                }

                if (!ModelState.IsValid)
                {
                    return View(fonction);
                }

                // Mettre à jour les propriétés
                existingFonction.Nom = fonction.Nom;
                existingFonction.Description = fonction.Description;
                existingFonction.ModifiedOn = DateTime.UtcNow;
                existingFonction.ModifiedBy = User.Identity?.Name ?? "System";

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Fonction modifiée: {existingFonction.Nom} par {User.Identity?.Name}");
                TempData["SuccessMessage"] = $"La fonction '{existingFonction.Nom}' a été modifiée avec succès.";

                return RedirectToAction(nameof(List));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la modification de la fonction {FonctionId}", id);
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la modification de la fonction.";
                return View(fonction);
            }
        }

        /// <summary>
        /// Supprime une fonction (soft delete)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var fonction = await _context.Fonctions
                    .FirstOrDefaultAsync(f => f.Id == id && f.Supprimer == 0);

                if (fonction == null)
                {
                    TempData["ErrorMessage"] = "Fonction introuvable.";
                    return RedirectToAction(nameof(List));
                }

                // Vérifier s'il y a des utilisateurs avec cette fonction
                var utilisateursCount = await _context.Utilisateurs
                    .CountAsync(u => u.FonctionId == id && u.Supprimer == 0);

                if (utilisateursCount > 0)
                {
                    TempData["ErrorMessage"] = $"Impossible de supprimer cette fonction car elle est assignée à {utilisateursCount} utilisateur(s). Veuillez d'abord réassigner ou supprimer les utilisateurs.";
                    return RedirectToAction(nameof(List));
                }

                // Soft delete
                fonction.Supprimer = 1;
                fonction.ModifiedOn = DateTime.UtcNow;
                fonction.ModifiedBy = User.Identity?.Name ?? "System";

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Fonction supprimée: {fonction.Nom} par {User.Identity?.Name}");
                TempData["SuccessMessage"] = $"La fonction '{fonction.Nom}' a été supprimée avec succès.";

                return RedirectToAction(nameof(List));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression de la fonction {FonctionId}", id);
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la suppression de la fonction.";
                return RedirectToAction(nameof(List));
            }
        }

        /// <summary>
        /// API pour récupérer les fonctions (pour les dropdowns)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetFonctions()
        {
            try
            {
                var fonctions = await _context.Fonctions
                    .Where(f => f.Supprimer == 0)
                    .OrderBy(f => f.Nom)
                    .Select(f => new
                    {
                        f.Id,
                        f.Nom,
                        f.Description
                    })
                    .ToListAsync();

                return Json(new { success = true, data = fonctions });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des fonctions");
                return Json(new { success = false, message = "Erreur lors de la récupération des fonctions" });
            }
        }
    }
}
