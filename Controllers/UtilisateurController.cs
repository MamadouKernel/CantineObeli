using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Obeli_K.Data;
using Obeli_K.Models;
using Obeli_K.Models.Enums;
using Obeli_K.Models.ViewModels;
using Obeli_K.Services.Security;
using Obeli_K.Services.Users;

namespace Obeli_K.Controllers
{
    [Authorize(Roles = "Administrateur,RessourcesHumaines")]
    public class UtilisateurController : Controller
    {
        private readonly ObeliDbContext _context;
        private readonly IUserService _userService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ILogger<UtilisateurController> _logger;

        public UtilisateurController(ObeliDbContext context, IUserService userService, IPasswordHasher passwordHasher, ILogger<UtilisateurController> logger)
        {
            _context = context;
            _userService = userService;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        // Populate the ViewBag with the list of departments and functions
        private async Task PopulateViewBags()
        {
            var departements = await _context.Departements
                .Where(d => d.Supprimer == 0)
                .ToListAsync();
            ViewBag.Departements = new SelectList(departements, "Id", "Nom");
            _logger.LogInformation("Chargement de {Count} départements", departements.Count);

            var fonctions = await _context.Fonctions
                .Where(f => f.Supprimer == 0)
                .ToListAsync();
            ViewBag.Fonctions = new SelectList(fonctions, "Id", "Nom");
            _logger.LogInformation("Chargement de {Count} fonctions", fonctions.Count);

            // Rôles disponibles
            var roles = new List<object>
            {
                new { Value = RoleType.Admin.ToString(), Text = "Admin" },
                new { Value = RoleType.RH.ToString(), Text = "RH" },
                new { Value = RoleType.Employe.ToString(), Text = "Employé" },
                new { Value = RoleType.PrestataireCantine.ToString(), Text = "Prestataire Cantine" }
            };
            ViewBag.Roles = new SelectList(roles, "Value", "Text");
            _logger.LogInformation("Chargement de {Count} rôles", roles.Count);
        }

        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Affiche le formulaire de création d'utilisateur
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                _logger.LogInformation("Chargement du formulaire de création d'utilisateur");
                await PopulateViewBags();
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement du formulaire de création d'utilisateur");
                TempData["ErrorMessage"] = "Une erreur est survenue lors du chargement du formulaire.";
                return RedirectToAction(nameof(Index));
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUtilisateurViewModel model)
        {
            _logger.LogInformation("=== DÉBUT CRÉATION UTILISATEUR ===");
            
            if (model == null)
            {
                _logger.LogWarning("Les données utilisateur sont manquantes.");
                ModelState.AddModelError("", "Les données utilisateur sont manquantes.");
                await PopulateViewBags();
                return View();
            }

            _logger.LogInformation("Données reçues - Nom: {Nom}, Prenoms: {Prenoms}, UserName: {UserName}, Email: {Email}, DepartementId: {DepartementId}, FonctionId: {FonctionId}, Role: {Role}, Site: {Site}", 
                model.Nom, model.Prenoms, model.UserName, model.Email, model.DepartementId, model.FonctionId, model.Role, model.Site);
            _logger.LogInformation("Mot de passe fourni: {HasPassword}, Confirmation: {HasConfirmation}", 
                !string.IsNullOrEmpty(model.MotDePasse), !string.IsNullOrEmpty(model.ConfirmerMotDePasse));

            try
            {
                // ==== Étape 1 : Validation des champs ====

                if (string.IsNullOrWhiteSpace(model.Nom))
                    ModelState.AddModelError(nameof(model.Nom), "Le nom est obligatoire.");

                if (string.IsNullOrWhiteSpace(model.Prenoms))
                    ModelState.AddModelError(nameof(model.Prenoms), "Les prénoms sont obligatoires.");

                if (string.IsNullOrWhiteSpace(model.UserName))
                    ModelState.AddModelError(nameof(model.UserName), "Le matricule est obligatoire.");

                if (model.DepartementId == Guid.Empty)
                    ModelState.AddModelError(nameof(model.DepartementId), "Le département est obligatoire.");

                if (string.IsNullOrWhiteSpace(model.MotDePasse))
                    ModelState.AddModelError(nameof(model.MotDePasse), "Le mot de passe est requis.");
                else if (model.MotDePasse.Length < 6)
                    ModelState.AddModelError(nameof(model.MotDePasse), "Le mot de passe doit contenir au moins 6 caractères.");

                if (model.MotDePasse != model.ConfirmerMotDePasse)
                    ModelState.AddModelError(nameof(model.ConfirmerMotDePasse), "Les mots de passe ne correspondent pas.");

                // ==== Étape 2 : Vérifications en base ====

                if (!string.IsNullOrWhiteSpace(model.UserName))
                {
                    bool userNameExists = await _context.Utilisateurs
                        .AnyAsync(u => u.UserName == model.UserName && u.Supprimer == 0);
                    if (userNameExists)
                        ModelState.AddModelError(nameof(model.UserName), "Ce matricule est déjà utilisé.");
                }

                if (!string.IsNullOrWhiteSpace(model.Email))
                {
                    bool emailExists = await _context.Utilisateurs
                        .AnyAsync(u => u.Email == model.Email && u.Supprimer == 0);
                    if (emailExists)
                        ModelState.AddModelError(nameof(model.Email), "Cette adresse email est déjà utilisée.");
                }

                if (model.DepartementId != Guid.Empty)
                {
                    bool deptExists = await _context.Departements
                        .AnyAsync(d => d.Id == model.DepartementId && d.Supprimer == 0);
                    if (!deptExists)
                        ModelState.AddModelError(nameof(model.DepartementId), "Le département sélectionné n'existe pas.");
                }

                if (model.FonctionId.HasValue && model.FonctionId != Guid.Empty)
                {
                    bool fonctionExists = await _context.Fonctions
                        .AnyAsync(f => f.Id == model.FonctionId && f.Supprimer == 0);
                    if (!fonctionExists)
                        ModelState.AddModelError(nameof(model.FonctionId), "La fonction sélectionnée n'existe pas.");
                }

                // ==== Étape 3 : Retour si erreurs ====

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Échec de validation du formulaire utilisateur.");
                    foreach (var error in ModelState)
                    {
                        if (error.Value.Errors.Count > 0)
                        {
                            _logger.LogWarning("Erreur de validation - {Key}: {Errors}", 
                                error.Key, string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage)));
                        }
                    }
                    await PopulateViewBags();
                    return View(model);
                }

                // ==== Étape 4 : Construction de l'entité ====

                var nouvelUtilisateur = new Utilisateur
                {
                    Nom = model.Nom.Trim(),
                    Prenoms = model.Prenoms.Trim(),
                    UserName = model.UserName.Trim(),
                    Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim(),
                    PhoneNumber = string.IsNullOrWhiteSpace(model.PhoneNumber) ? null : model.PhoneNumber.Trim(),
                    Lieu = string.IsNullOrWhiteSpace(model.Lieu) ? null : model.Lieu.Trim(),
                    CodeCommande = string.IsNullOrWhiteSpace(model.CodeCommande) ? null : model.CodeCommande.Trim(),
                    MotDePasseHash = _passwordHasher.HashPassword(model.MotDePasse),
                    MustResetPassword = true,
                    Role = model.Role,
                    DepartementId = model.DepartementId,
                    FonctionId = model.FonctionId,
                    Site = model.Site,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = User.Identity?.Name ?? "System",
                    ModifiedAt = DateTime.UtcNow,
                    ModifiedBy = User.Identity?.Name ?? "System",
                    Supprimer = 0
                };

                // ==== Étape 5 : Sauvegarde ====

                _logger.LogInformation("Sauvegarde de l'utilisateur en base de données...");
                _context.Utilisateurs.Add(nouvelUtilisateur);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Utilisateur sauvegardé avec succès !");

                _logger.LogInformation("Utilisateur créé avec succès : {UserName}", nouvelUtilisateur.UserName);
                TempData["SuccessMessage"] = $"L'utilisateur {nouvelUtilisateur.Nom} {nouvelUtilisateur.Prenoms} a été créé avec succès.";
                return RedirectToAction(nameof(List));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création de l'utilisateur : {Message}", ex.Message);
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la création de l'utilisateur.";
                await PopulateViewBags();
                return View(model);
            }
        }



        /// <summary>
        /// Traite la création d'un nouvel utilisateur
        /// </summary>
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create(Utilisateur utilisateur, string motDePasse, string confirmerMotDePasse)
        //{
        //    _logger.LogInformation("=== CRÉATION UTILISATEUR DÉBUT ===");

        //    try
        //    {
        //        // 1. Validation basique des champs obligatoires
        //        if (string.IsNullOrWhiteSpace(utilisateur.Nom))
        //            ModelState.AddModelError("Nom", "Le nom est obligatoire.");

        //        if (string.IsNullOrWhiteSpace(utilisateur.Prenoms))
        //            ModelState.AddModelError("Prenoms", "Les prénoms sont obligatoires.");

        //        if (string.IsNullOrWhiteSpace(utilisateur.UserName))
        //            ModelState.AddModelError("UserName", "Le matricule est obligatoire.");

        //        if (utilisateur.DepartementId == Guid.Empty)
        //            ModelState.AddModelError("DepartementId", "Le département est obligatoire.");

        //        if (string.IsNullOrWhiteSpace(motDePasse))
        //            ModelState.AddModelError("motDePasse", "Le mot de passe est requis.");
        //        else if (motDePasse.Length < 6)
        //            ModelState.AddModelError("motDePasse", "Le mot de passe doit contenir au moins 6 caractères.");

        //        if (motDePasse != confirmerMotDePasse)
        //            ModelState.AddModelError("confirmerMotDePasse", "Les mots de passe ne correspondent pas.");

        //        // 2. Vérifier l'unicité du matricule
        //        if (!string.IsNullOrWhiteSpace(utilisateur.UserName))
        //        {
        //            var matriculeExiste = await _context.Utilisateurs
        //                .AnyAsync(u => u.UserName == utilisateur.UserName && u.Supprimer == 0);
        //            if (matriculeExiste)
        //                ModelState.AddModelError("UserName", "Ce matricule est déjà utilisé.");
        //        }

        //        // 3. Vérifier l'unicité de l'email (si fourni)
        //        if (!string.IsNullOrWhiteSpace(utilisateur.Email))
        //        {
        //            var emailExiste = await _context.Utilisateurs
        //                .AnyAsync(u => u.Email == utilisateur.Email && u.Supprimer == 0);
        //            if (emailExiste)
        //                ModelState.AddModelError("Email", "Cette adresse email est déjà utilisée.");
        //        }

        //        // 4. Vérifier que le département existe
        //        if (utilisateur.DepartementId != Guid.Empty)
        //        {
        //            var departementExiste = await _context.Departements
        //                .AnyAsync(d => d.Id == utilisateur.DepartementId && d.Supprimer == 0);
        //            if (!departementExiste)
        //                ModelState.AddModelError("DepartementId", "Le département sélectionné n'existe pas.");
        //        }

        //        // 5. Vérifier que la fonction existe (si fournie)
        //        if (utilisateur.FonctionId.HasValue && utilisateur.FonctionId != Guid.Empty)
        //        {
        //            var fonctionExiste = await _context.Fonctions
        //                .AnyAsync(f => f.Id == utilisateur.FonctionId && f.Supprimer == 0);
        //            if (!fonctionExiste)
        //                ModelState.AddModelError("FonctionId", "La fonction sélectionnée n'existe pas.");
        //        }

        //        // 6. Si erreurs de validation, retourner à la vue
        //        if (!ModelState.IsValid)
        //        {
        //            _logger.LogWarning("Erreurs de validation détectées");
        //            await PopulateViewBags();
        //            return View(utilisateur);
        //        }

        //        // 7. Créer l'utilisateur
        //        var nouvelUtilisateur = new Utilisateur
        //        {
        //            // ID généré automatiquement par le modèle
        //            Nom = utilisateur.Nom.Trim(),
        //            Prenoms = utilisateur.Prenoms.Trim(),
        //            UserName = utilisateur.UserName.Trim(),
        //            Email = string.IsNullOrWhiteSpace(utilisateur.Email) ? null : utilisateur.Email.Trim(),
        //            PhoneNumber = string.IsNullOrWhiteSpace(utilisateur.PhoneNumber) ? null : utilisateur.PhoneNumber.Trim(),
        //            Lieu = string.IsNullOrWhiteSpace(utilisateur.Lieu) ? null : utilisateur.Lieu.Trim(),
        //            CodeCommande = string.IsNullOrWhiteSpace(utilisateur.CodeCommande) ? null : utilisateur.CodeCommande.Trim(),

        //            // Sécurité
        //            MotDePasseHash = _passwordHasher.HashPassword(motDePasse),
        //            MustResetPassword = true,

        //            // Rôles et affectations
        //            Role = utilisateur.Role,
        //            DepartementId = utilisateur.DepartementId,
        //            FonctionId = utilisateur.FonctionId,
        //            Site = utilisateur.Site,

        //            // Audit
        //            CreatedAt = DateTime.UtcNow,
        //            CreatedBy = User.Identity?.Name ?? "System",
        //            ModifiedAt = DateTime.UtcNow,
        //            ModifiedBy = User.Identity?.Name ?? "System",

        //            // Soft delete
        //            Supprimer = 0
        //        };

        //        _context.Utilisateurs.Add(nouvelUtilisateur);
        //        await _context.SaveChangesAsync();

        //        _logger.LogInformation("Utilisateur créé avec succès - ID: {Id}, Nom: {Nom}", nouvelUtilisateur.Id, nouvelUtilisateur.Nom);
        //        TempData["SuccessMessage"] = $"L'utilisateur {nouvelUtilisateur.Nom} {nouvelUtilisateur.Prenoms} a été créé avec succès.";
        //        return RedirectToAction(nameof(List));
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Erreur lors de la création de l'utilisateur: {Message}", ex.Message);
        //        TempData["ErrorMessage"] = $"Erreur: {ex.Message}";
        //        await PopulateViewBags();
        //        return View(utilisateur);
        //    }
        //}

        /// <summary>
        /// Affiche la liste des utilisateurs
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> List()
        {
            try
            {
                var utilisateurs = await _context.Utilisateurs
                    .Include(u => u.Departement)
                    .Include(u => u.Fonction)
                    .Where(u => u.Supprimer == 0)
                    .OrderBy(u => u.Nom)
                    .ThenBy(u => u.Prenoms)
                    .Select(u => new
                    {
                        u.Id,
                        u.Nom,
                        u.Prenoms,
                        u.UserName,
                        u.CodeCommande,
                        u.Email,
                        u.PhoneNumber,
                        u.Role,
                        u.Lieu,
                        u.Site,
                        DepartementNom = u.Departement != null ? u.Departement.Nom : "N/A",
                        FonctionNom = u.Fonction != null ? u.Fonction.Nom : "N/A",
                        u.CreatedAt
                    })
                    .ToListAsync();

                ViewBag.Utilisateurs = utilisateurs;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement de la liste des utilisateurs");
                TempData["ErrorMessage"] = "Une erreur est survenue lors du chargement des utilisateurs.";
                return View(new List<object>());
            }
        }

        /// <summary>
        /// Affiche les détails d'un utilisateur
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var utilisateur = await _context.Utilisateurs
                    .Include(u => u.Departement)
                    .Include(u => u.Fonction)
                    .FirstOrDefaultAsync(u => u.Id == id && u.Supprimer == 0);

                if (utilisateur == null)
                {
                    TempData["ErrorMessage"] = "Utilisateur introuvable.";
                    return RedirectToAction(nameof(List));
                }

                return View(utilisateur);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement des détails de l'utilisateur {UserId}", id);
                TempData["ErrorMessage"] = "Une erreur est survenue lors du chargement des détails.";
                return RedirectToAction(nameof(List));
            }
        }

        /// <summary>
        /// Affiche le formulaire d'édition d'un utilisateur
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var utilisateur = await _context.Utilisateurs
                    .FirstOrDefaultAsync(u => u.Id == id && u.Supprimer == 0);

                if (utilisateur == null)
                {
                    TempData["ErrorMessage"] = "Utilisateur introuvable.";
                    return RedirectToAction(nameof(List));
                }

                // Mapper vers le ViewModel
                var model = new EditUtilisateurViewModel
                {
                    Id = utilisateur.Id,
                    Nom = utilisateur.Nom,
                    Prenoms = utilisateur.Prenoms,
                    UserName = utilisateur.UserName,
                    Email = utilisateur.Email,
                    PhoneNumber = utilisateur.PhoneNumber,
                    Lieu = utilisateur.Lieu,
                    CodeCommande = utilisateur.CodeCommande,
                    Role = utilisateur.Role,
                    DepartementId = utilisateur.DepartementId,
                    FonctionId = utilisateur.FonctionId,
                    Site = utilisateur.Site
                };

                await PopulateViewBags();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement de l'utilisateur pour édition {UserId}", id);
                TempData["ErrorMessage"] = "Une erreur est survenue lors du chargement de l'utilisateur.";
                return RedirectToAction(nameof(List));
            }
        }

        /// <summary>
        /// Traite la modification d'un utilisateur
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, EditUtilisateurViewModel model)
        {
            _logger.LogInformation("=== DÉBUT MODIFICATION UTILISATEUR ===");
            _logger.LogInformation("Modification utilisateur ID: {Id}, Nom: {Nom}, UserName: {UserName}", 
                id, model?.Nom, model?.UserName);

            try
            {
                if (model == null)
                {
                    _logger.LogWarning("Les données utilisateur sont manquantes.");
                    TempData["ErrorMessage"] = "Les données utilisateur sont manquantes.";
                    return RedirectToAction(nameof(List));
                }

                if (id != model.Id)
                {
                    _logger.LogWarning("Identifiant utilisateur invalide. ID attendu: {ExpectedId}, ID reçu: {ReceivedId}", id, model.Id);
                    TempData["ErrorMessage"] = "Identifiant utilisateur invalide.";
                    return RedirectToAction(nameof(List));
                }

                var existingUser = await _context.Utilisateurs
                    .FirstOrDefaultAsync(u => u.Id == id && u.Supprimer == 0);

                if (existingUser == null)
                {
                    _logger.LogWarning("Utilisateur introuvable avec l'ID: {Id}", id);
                    TempData["ErrorMessage"] = "Utilisateur introuvable.";
                    return RedirectToAction(nameof(List));
                }

                // Validation des champs obligatoires
                if (string.IsNullOrWhiteSpace(model.Nom))
                    ModelState.AddModelError(nameof(model.Nom), "Le nom est obligatoire.");

                if (string.IsNullOrWhiteSpace(model.Prenoms))
                    ModelState.AddModelError(nameof(model.Prenoms), "Les prénoms sont obligatoires.");

                if (string.IsNullOrWhiteSpace(model.UserName))
                    ModelState.AddModelError(nameof(model.UserName), "Le matricule est obligatoire.");

                if (model.DepartementId == Guid.Empty)
                    ModelState.AddModelError(nameof(model.DepartementId), "Le département est obligatoire.");

                // Validation du changement de mot de passe (si fourni)
                if (!string.IsNullOrWhiteSpace(model.NouveauMotDePasse))
                {
                    if (model.NouveauMotDePasse.Length < 6)
                        ModelState.AddModelError(nameof(model.NouveauMotDePasse), "Le mot de passe doit contenir au moins 6 caractères.");

                    if (model.NouveauMotDePasse != model.ConfirmerNouveauMotDePasse)
                        ModelState.AddModelError(nameof(model.ConfirmerNouveauMotDePasse), "Les mots de passe ne correspondent pas.");
                }

                // Vérifier si l'email existe déjà (sauf pour l'utilisateur actuel)
                if (!string.IsNullOrWhiteSpace(model.Email) && 
                    await _context.Utilisateurs.AnyAsync(u => u.Email == model.Email && u.Id != id && u.Supprimer == 0))
                {
                    ModelState.AddModelError(nameof(model.Email), "Cette adresse email est déjà utilisée.");
                }

                // Vérifier si le matricule (UserName) existe déjà (sauf pour l'utilisateur actuel)
                if (!string.IsNullOrWhiteSpace(model.UserName) && 
                    await _context.Utilisateurs.AnyAsync(u => u.UserName == model.UserName && u.Id != id && u.Supprimer == 0))
                {
                    ModelState.AddModelError(nameof(model.UserName), "Ce matricule est déjà utilisé.");
                }

                // Vérifier que le département existe
                if (model.DepartementId != Guid.Empty)
                {
                    bool deptExists = await _context.Departements
                        .AnyAsync(d => d.Id == model.DepartementId && d.Supprimer == 0);
                    if (!deptExists)
                        ModelState.AddModelError(nameof(model.DepartementId), "Le département sélectionné n'existe pas.");
                }

                // Vérifier que la fonction existe (si fournie)
                if (model.FonctionId.HasValue && model.FonctionId != Guid.Empty)
                {
                    bool fonctionExists = await _context.Fonctions
                        .AnyAsync(f => f.Id == model.FonctionId && f.Supprimer == 0);
                    if (!fonctionExists)
                        ModelState.AddModelError(nameof(model.FonctionId), "La fonction sélectionnée n'existe pas.");
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Échec de validation du formulaire de modification utilisateur.");
                    foreach (var error in ModelState)
                    {
                        if (error.Value.Errors.Count > 0)
                        {
                            _logger.LogWarning("Erreur de validation - {Key}: {Errors}", 
                                error.Key, string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage)));
                        }
                    }
                    await PopulateViewBags();
                    return View(model);
                }

                // Mettre à jour les propriétés
                existingUser.Nom = model.Nom.Trim();
                existingUser.Prenoms = model.Prenoms.Trim();
                existingUser.UserName = model.UserName.Trim();
                existingUser.Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim();
                existingUser.PhoneNumber = string.IsNullOrWhiteSpace(model.PhoneNumber) ? null : model.PhoneNumber.Trim();
                existingUser.Lieu = string.IsNullOrWhiteSpace(model.Lieu) ? null : model.Lieu.Trim();
                existingUser.CodeCommande = string.IsNullOrWhiteSpace(model.CodeCommande) ? null : model.CodeCommande.Trim();
                existingUser.Role = model.Role;
                existingUser.DepartementId = model.DepartementId;
                existingUser.FonctionId = model.FonctionId;
                existingUser.Site = model.Site;
                existingUser.ModifiedAt = DateTime.UtcNow;
                existingUser.ModifiedBy = User.Identity?.Name ?? "System";

                // Mettre à jour le mot de passe si fourni
                if (!string.IsNullOrWhiteSpace(model.NouveauMotDePasse))
                {
                    existingUser.MotDePasseHash = _passwordHasher.HashPassword(model.NouveauMotDePasse);
                    existingUser.MustResetPassword = false; // L'utilisateur a défini un nouveau mot de passe
                    _logger.LogInformation("Mot de passe mis à jour pour l'utilisateur {UserName}", existingUser.UserName);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Utilisateur modifié avec succès: {UserName} par {ModifiedBy}", 
                    existingUser.UserName, User.Identity?.Name);
                TempData["SuccessMessage"] = $"L'utilisateur {existingUser.Nom} {existingUser.Prenoms} a été modifié avec succès.";

                return RedirectToAction(nameof(List));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la modification de l'utilisateur {UserId}: {Message}", id, ex.Message);
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la modification de l'utilisateur.";
                await PopulateViewBags();
                return View(model);
            }
        }

        /// <summary>
        /// Supprime un utilisateur (soft delete)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var utilisateur = await _context.Utilisateurs
                    .FirstOrDefaultAsync(u => u.Id == id && u.Supprimer == 0);

                if (utilisateur == null)
                {
                    TempData["ErrorMessage"] = "Utilisateur introuvable.";
                    return RedirectToAction(nameof(List));
                }

                // Soft delete
                utilisateur.Supprimer = 1;
                utilisateur.ModifiedAt = DateTime.UtcNow;
                utilisateur.ModifiedBy = User.Identity?.Name ?? "System";

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Utilisateur supprimé: {utilisateur.UserName} par {User.Identity?.Name}");
                TempData["SuccessMessage"] = $"L'utilisateur {utilisateur.Nom} {utilisateur.Prenoms} a été supprimé avec succès.";

                return RedirectToAction(nameof(List));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression de l'utilisateur {UserId}", id);
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la suppression de l'utilisateur.";
                return RedirectToAction(nameof(List));
            }
        }
    }
}
