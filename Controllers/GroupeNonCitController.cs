using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Obeli_K.Data;
using Obeli_K.Models;
using Obeli_K.Enums;
using Obeli_K.Services;

namespace Obeli_K.Controllers
{
    [Authorize(Roles = "Administrateur,RH")]
    public class GroupeNonCitController : Controller
    {
        private readonly ObeliDbContext _context;
        private readonly ILogger<GroupeNonCitController> _logger;
        private readonly GroupeNonCitInitializationService _initializationService;

        public GroupeNonCitController(ObeliDbContext context, ILogger<GroupeNonCitController> logger, GroupeNonCitInitializationService initializationService)
        {
            _context = context;
            _logger = logger;
            _initializationService = initializationService;
        }

        // GET: GroupeNonCit/Index
        public async Task<IActionResult> Index()
        {
            try
            {
                // Supprimer automatiquement tous les groupes sauf "Douaniers"
                var groupesNonDouaniers = await _context.GroupesNonCit
                    .Where(g => g.Supprimer == 0 && 
                           g.Nom.ToLower() != "douaniers")
                    .ToListAsync();

                if (groupesNonDouaniers.Any())
                {
                    foreach (var groupe in groupesNonDouaniers)
                    {
                        // Vérifier s'il a des commandes
                        var aDesCommandes = await _context.Commandes
                            .AnyAsync(c => c.GroupeNonCitId == groupe.Id && c.Supprimer == 0);

                        if (!aDesCommandes)
                        {
                            groupe.Supprimer = 1;
                            groupe.ModifiedOn = DateTime.UtcNow;
                            groupe.ModifiedBy = "System_AutoCleanup";
                            _logger.LogInformation("🗑️ Groupe non-Douaniers supprimé automatiquement : {Nom}", groupe.Nom);
                        }
                        else
                        {
                            _logger.LogWarning("⚠️ Groupe {Nom} non supprimé car il a des commandes associées", groupe.Nom);
                        }
                    }

                    if (groupesNonDouaniers.Any(g => g.Supprimer == 1))
                    {
                        await _context.SaveChangesAsync();
                    }
                }

                // Ne plus initialiser automatiquement - les groupes sont créés manuellement par les RH/Admin
                var groupes = await _context.GroupesNonCit
                    .Where(g => g.Supprimer == 0)
                    .OrderBy(g => g.Nom)
                    .ToListAsync();

                _logger.LogInformation("📋 Chargement de {Count} groupes non-CIT", groupes.Count);
                
                if (!groupes.Any())
                {
                    TempData["InfoMessage"] = "Aucun groupe non-CIT trouvé. Utilisez le bouton 'Créer un nouveau groupe' pour en ajouter un.";
                }
                
                return View(groupes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors du chargement des groupes non-CIT");
                TempData["ErrorMessage"] = "Erreur lors du chargement des groupes non-CIT.";
                return View(new List<GroupeNonCit>());
            }
        }

        // GET: GroupeNonCit/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var groupe = await _context.GroupesNonCit
                    .FirstOrDefaultAsync(g => g.Id == id && g.Supprimer == 0);

                if (groupe == null)
                {
                    return NotFound();
                }

                return View(groupe);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors du chargement du groupe {Id}", id);
                TempData["ErrorMessage"] = "Erreur lors du chargement du groupe.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: GroupeNonCit/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, GroupeNonCit groupe)
        {
            if (id != groupe.Id)
            {
                return NotFound();
            }

            try
            {
                if (ModelState.IsValid)
                {
                    // Récupérer le groupe existant
                    var groupeExistant = await _context.GroupesNonCit
                        .FirstOrDefaultAsync(g => g.Id == id && g.Supprimer == 0);

                    if (groupeExistant == null)
                    {
                        return NotFound();
                    }

                    // Mettre à jour les propriétés
                    groupeExistant.Nom = groupe.Nom;
                    groupeExistant.Description = groupe.Description;
                    groupeExistant.QuotaJournalier = groupe.QuotaJournalier;
                    groupeExistant.QuotaNuit = groupe.QuotaNuit;
                    groupeExistant.RestrictionFormuleStandard = groupe.RestrictionFormuleStandard;
                    groupeExistant.CodeGroupe = groupe.CodeGroupe;

                    groupeExistant.ModifiedOn = DateTime.UtcNow;
                    groupeExistant.ModifiedBy = User.Identity?.Name ?? "System";

                    await _context.SaveChangesAsync();

                    _logger.LogInformation("✅ Groupe non-CIT {Id} modifié", id);

                    TempData["SuccessMessage"] = "Groupe non-CIT modifié avec succès.";
                    return RedirectToAction(nameof(Index));
                }

                return View(groupe);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors de la modification du groupe {Id}", id);
                TempData["ErrorMessage"] = "Erreur lors de la modification du groupe non-CIT.";
                return View(groupe);
            }
        }

        // POST: GroupeNonCit/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var groupe = await _context.GroupesNonCit
                    .FirstOrDefaultAsync(g => g.Id == id && g.Supprimer == 0);

                if (groupe == null)
                {
                    TempData["ErrorMessage"] = "Groupe non trouvé.";
                    return RedirectToAction(nameof(Index));
                }

                // Vérifier si le groupe a des commandes associées
                var aDesCommandes = await _context.Commandes
                    .AnyAsync(c => c.GroupeNonCitId == groupe.Id && c.Supprimer == 0);

                if (aDesCommandes)
                {
                    TempData["ErrorMessage"] = $"Impossible de supprimer le groupe '{groupe.Nom}' car il a des commandes associées. Supprimez d'abord les commandes ou utilisez la modification pour désactiver le groupe.";
                    return RedirectToAction(nameof(Index));
                }

                // Soft delete
                groupe.Supprimer = 1;
                groupe.ModifiedOn = DateTime.UtcNow;
                groupe.ModifiedBy = User.Identity?.Name ?? "System";

                await _context.SaveChangesAsync();

                _logger.LogInformation("✅ Groupe non-CIT {Id} ({Nom}) supprimé", id, groupe.Nom);
                TempData["SuccessMessage"] = $"Groupe '{groupe.Nom}' supprimé avec succès.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors de la suppression du groupe {Id}", id);
                TempData["ErrorMessage"] = "Erreur lors de la suppression du groupe.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: GroupeNonCit/Create
        public IActionResult Create()
        {
            return View(new GroupeNonCit());
        }

        // POST: GroupeNonCit/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GroupeNonCit groupe)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Vérifier si un groupe avec le même nom existe déjà
                    var groupeExistant = await _context.GroupesNonCit
                        .FirstOrDefaultAsync(g => g.Nom.ToLower() == groupe.Nom.ToLower() && g.Supprimer == 0);

                    if (groupeExistant != null)
                    {
                        ModelState.AddModelError(nameof(groupe.Nom), "Un groupe avec ce nom existe déjà.");
                        return View(groupe);
                    }

                    // Créer le nouveau groupe
                    groupe.Id = Guid.NewGuid();
                    groupe.CreatedOn = DateTime.UtcNow;
                    groupe.CreatedBy = User.Identity?.Name ?? "System";
                    groupe.Supprimer = 0;

                    _context.GroupesNonCit.Add(groupe);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("✅ Nouveau groupe non-CIT créé : {Nom} (ID: {Id})", groupe.Nom, groupe.Id);

                    TempData["SuccessMessage"] = $"Groupe '{groupe.Nom}' créé avec succès.";
                    return RedirectToAction(nameof(Index));
                }

                return View(groupe);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors de la création du groupe");
                TempData["ErrorMessage"] = "Erreur lors de la création du groupe non-CIT.";
                return View(groupe);
            }
        }

        // GET: GroupeNonCit/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var groupe = await _context.GroupesNonCit
                    .FirstOrDefaultAsync(g => g.Id == id && g.Supprimer == 0);

                if (groupe == null)
                {
                    return NotFound();
                }

                // Calculer les statistiques de consommation pour aujourd'hui
                var aujourdhui = DateTime.Today;
                var platsConsommesJour = await _context.Commandes
                    .Where(c => c.GroupeNonCitId == groupe.Id && 
                               c.DateConsommation.HasValue && c.DateConsommation.Value.Date == aujourdhui && 
                               c.PeriodeService == Periode.Jour &&
                               c.Supprimer == 0)
                    .SumAsync(c => c.Quantite);
                
                var platsConsommesNuit = await _context.Commandes
                    .Where(c => c.GroupeNonCitId == groupe.Id && 
                               c.DateConsommation.HasValue && c.DateConsommation.Value.Date == aujourdhui && 
                               c.PeriodeService == Periode.Nuit &&
                               c.Supprimer == 0)
                    .SumAsync(c => c.Quantite);

                ViewBag.PlatsConsommesJour = platsConsommesJour;
                ViewBag.PlatsConsommesNuit = platsConsommesNuit;
                ViewBag.DateAujourdhui = aujourdhui;

                return View(groupe);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors du chargement des détails du groupe {Id}", id);
                TempData["ErrorMessage"] = "Erreur lors du chargement des détails du groupe.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
