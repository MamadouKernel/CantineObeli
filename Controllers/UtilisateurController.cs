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
using Obeli_K.Services;

namespace Obeli_K.Controllers
{
    /// <summary>
    /// Contrôleur pour la gestion des utilisateurs du système.
    /// Permet la création, modification, suppression et import en masse des utilisateurs.
    /// Accessible uniquement aux administrateurs et RH.
    /// </summary>
    [Authorize(Roles = "Administrateur,RH")]
    public class UtilisateurController : Controller
    {
        private readonly ObeliDbContext _context;
        private readonly IUserService _userService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ILogger<UtilisateurController> _logger;

        /// <summary>
        /// Initialise une nouvelle instance du contrôleur d'utilisateurs.
        /// </summary>
        /// <param name="context">Contexte de base de données Obeli</param>
        /// <param name="userService">Service de gestion des utilisateurs</param>
        /// <param name="passwordHasher">Service de hachage des mots de passe</param>
        /// <param name="logger">Service de journalisation</param>
        public UtilisateurController(ObeliDbContext context, IUserService userService, IPasswordHasher passwordHasher, ILogger<UtilisateurController> logger)
        {
            _context = context;
            _userService = userService;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        // Populate the ViewBag with the list of directions and functions
        private async Task PopulateViewBags()
        {
            var directions = await _context.Directions
                .Where(d => d.Supprimer == 0)
                .ToListAsync();
            ViewBag.Directions = new SelectList(directions, "Id", "Nom");
            _logger.LogInformation("Chargement de {Count} directions", directions.Count);

            var fonctions = await _context.Fonctions
                .Where(f => f.Supprimer == 0)
                .ToListAsync();
            ViewBag.Fonctions = new SelectList(fonctions, "Id", "Nom");
            _logger.LogInformation("Chargement de {Count} fonctions", fonctions.Count);

            // R�les disponibles
            var roles = new List<object>
            {
                new { Value = RoleType.Admin.ToString(), Text = "Admin" },
                new { Value = RoleType.RH.ToString(), Text = "RH" },
                new { Value = RoleType.Employe.ToString(), Text = "Employ�" },
                new { Value = RoleType.PrestataireCantine.ToString(), Text = "Prestataire Cantine" }
            };
            ViewBag.Roles = new SelectList(roles, "Value", "Text");
            _logger.LogInformation("Chargement de {Count} r�les", roles.Count);
        }

        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Affiche le formulaire de cr�ation d'utilisateur
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                _logger.LogInformation("Chargement du formulaire de cr�ation d'utilisateur");
                await PopulateViewBags();
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement du formulaire de cr�ation d'utilisateur");
                TempData["ErrorMessage"] = "Une erreur est survenue lors du chargement du formulaire.";
                return RedirectToAction(nameof(Index));
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUtilisateurViewModel model)
        {
            _logger.LogInformation("=== D�BUT CR�ATION UTILISATEUR ===");
            
            if (model == null)
            {
                _logger.LogWarning("Les donn�es utilisateur sont manquantes.");
                ModelState.AddModelError("", "Les donn�es utilisateur sont manquantes.");
                await PopulateViewBags();
                return View();
            }

            _logger.LogInformation("Donn�es re�ues - Nom: {Nom}, Prenoms: {Prenoms}, UserName: {UserName}, Email: {Email}, DirectionId: {DirectionId}, FonctionId: {FonctionId}, Role: {Role}, Site: {Site}", 
                model.Nom, model.Prenoms, model.UserName, model.Email, model.DirectionId, model.FonctionId, model.Role, model.Site);
            _logger.LogInformation("Mot de passe fourni: {HasPassword}, Confirmation: {HasConfirmation}", 
                !string.IsNullOrEmpty(model.MotDePasse), !string.IsNullOrEmpty(model.ConfirmerMotDePasse));

            try
            {
                // ==== �tape 1 : Validation des champs ====

                if (string.IsNullOrWhiteSpace(model.Nom))
                    ModelState.AddModelError(nameof(model.Nom), "Le nom est obligatoire.");

                if (string.IsNullOrWhiteSpace(model.Prenoms))
                    ModelState.AddModelError(nameof(model.Prenoms), "Les pr�noms sont obligatoires.");

                if (string.IsNullOrWhiteSpace(model.UserName))
                    ModelState.AddModelError(nameof(model.UserName), "Le matricule est obligatoire.");

                if (model.DirectionId == Guid.Empty)
                    ModelState.AddModelError(nameof(model.DirectionId), "Le d�partement est obligatoire.");

                if (string.IsNullOrWhiteSpace(model.MotDePasse))
                    ModelState.AddModelError(nameof(model.MotDePasse), "Le mot de passe est requis.");
                else if (model.MotDePasse.Length < 6)
                    ModelState.AddModelError(nameof(model.MotDePasse), "Le mot de passe doit contenir au moins 6 caract�res.");

                if (model.MotDePasse != model.ConfirmerMotDePasse)
                    ModelState.AddModelError(nameof(model.ConfirmerMotDePasse), "Les mots de passe ne correspondent pas.");

                // ==== �tape 2 : V�rifications en base ====

                if (!string.IsNullOrWhiteSpace(model.UserName))
                {
                    bool userNameExists = await _context.Utilisateurs
                        .AnyAsync(u => u.UserName == model.UserName && u.Supprimer == 0);
                    if (userNameExists)
                        ModelState.AddModelError(nameof(model.UserName), "Ce matricule est d�j� utilis�.");
                }

                if (!string.IsNullOrWhiteSpace(model.Email))
                {
                    bool emailExists = await _context.Utilisateurs
                        .AnyAsync(u => u.Email == model.Email && u.Supprimer == 0);
                    if (emailExists)
                        ModelState.AddModelError(nameof(model.Email), "Cette adresse email est d�j� utilis�e.");
                }

                if (model.DirectionId != Guid.Empty)
                {
                    bool deptExists = await _context.Directions
                        .AnyAsync(d => d.Id == model.DirectionId && d.Supprimer == 0);
                    if (!deptExists)
                        ModelState.AddModelError(nameof(model.DirectionId), "Le d�partement s�lectionn� n'existe pas.");
                }

                if (model.FonctionId.HasValue && model.FonctionId != Guid.Empty)
                {
                    bool fonctionExists = await _context.Fonctions
                        .AnyAsync(f => f.Id == model.FonctionId && f.Supprimer == 0);
                    if (!fonctionExists)
                        ModelState.AddModelError(nameof(model.FonctionId), "La fonction s�lectionn�e n'existe pas.");
                }

                // ==== �tape 3 : Retour si erreurs ====

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("�chec de validation du formulaire utilisateur.");
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

                // ==== �tape 4 : Construction de l'entit� ====

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
                    DirectionId = model.DirectionId,
                    ServiceId = model.ServiceId,
                    FonctionId = model.FonctionId,
                    Site = model.Site,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = User.Identity?.Name ?? "System",
                    ModifiedAt = DateTime.UtcNow,
                    ModifiedBy = User.Identity?.Name ?? "System",
                    Supprimer = 0
                };

                // ==== �tape 5 : Sauvegarde ====

                _logger.LogInformation("Sauvegarde de l'utilisateur en base de donn�es...");
                _context.Utilisateurs.Add(nouvelUtilisateur);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Utilisateur sauvegard� avec succ�s !");

                _logger.LogInformation("Utilisateur cr�� avec succ�s : {UserName}", nouvelUtilisateur.UserName);
                TempData["SuccessMessage"] = $"L'utilisateur {nouvelUtilisateur.Nom} {nouvelUtilisateur.Prenoms} a �t� cr�� avec succ�s.";
                return RedirectToAction(nameof(List));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la cr�ation de l'utilisateur : {Message}", ex.Message);
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la cr�ation de l'utilisateur.";
                await PopulateViewBags();
                return View(model);
            }
        }



        /// <summary>
        /// Traite la cr�ation d'un nouvel utilisateur
        /// </summary>
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create(Utilisateur utilisateur, string motDePasse, string confirmerMotDePasse)
        //{
        //    _logger.LogInformation("=== CR�ATION UTILISATEUR D�BUT ===");

        //    try
        //    {
        //        // 1. Validation basique des champs obligatoires
        //        if (string.IsNullOrWhiteSpace(utilisateur.Nom))
        //            ModelState.AddModelError("Nom", "Le nom est obligatoire.");

        //        if (string.IsNullOrWhiteSpace(utilisateur.Prenoms))
        //            ModelState.AddModelError("Prenoms", "Les pr�noms sont obligatoires.");

        //        if (string.IsNullOrWhiteSpace(utilisateur.UserName))
        //            ModelState.AddModelError("UserName", "Le matricule est obligatoire.");

        //        if (utilisateur.DirectionId == Guid.Empty)
        //            ModelState.AddModelError("DirectionId", "Le d�partement est obligatoire.");

        //        if (string.IsNullOrWhiteSpace(motDePasse))
        //            ModelState.AddModelError("motDePasse", "Le mot de passe est requis.");
        //        else if (motDePasse.Length < 6)
        //            ModelState.AddModelError("motDePasse", "Le mot de passe doit contenir au moins 6 caract�res.");

        //        if (motDePasse != confirmerMotDePasse)
        //            ModelState.AddModelError("confirmerMotDePasse", "Les mots de passe ne correspondent pas.");

        //        // 2. V�rifier l'unicit� du matricule
        //        if (!string.IsNullOrWhiteSpace(utilisateur.UserName))
        //        {
        //            var matriculeExiste = await _context.Utilisateurs
        //                .AnyAsync(u => u.UserName == utilisateur.UserName && u.Supprimer == 0);
        //            if (matriculeExiste)
        //                ModelState.AddModelError("UserName", "Ce matricule est d�j� utilis�.");
        //        }

        //        // 3. V�rifier l'unicit� de l'email (si fourni)
        //        if (!string.IsNullOrWhiteSpace(utilisateur.Email))
        //        {
        //            var emailExiste = await _context.Utilisateurs
        //                .AnyAsync(u => u.Email == utilisateur.Email && u.Supprimer == 0);
        //            if (emailExiste)
        //                ModelState.AddModelError("Email", "Cette adresse email est d�j� utilis�e.");
        //        }

        //        // 4. V�rifier que le d�partement existe
        //        if (utilisateur.DirectionId != Guid.Empty)
        //        {
        //            var DirectionExiste = await _context.Directions
        //                .AnyAsync(d => d.Id == utilisateur.DirectionId && d.Supprimer == 0);
        //            if (!DirectionExiste)
        //                ModelState.AddModelError("DirectionId", "Le d�partement s�lectionn� n'existe pas.");
        //        }

        //        // 5. V�rifier que la fonction existe (si fournie)
        //        if (utilisateur.FonctionId.HasValue && utilisateur.FonctionId != Guid.Empty)
        //        {
        //            var fonctionExiste = await _context.Fonctions
        //                .AnyAsync(f => f.Id == utilisateur.FonctionId && f.Supprimer == 0);
        //            if (!fonctionExiste)
        //                ModelState.AddModelError("FonctionId", "La fonction s�lectionn�e n'existe pas.");
        //        }

        //        // 6. Si erreurs de validation, retourner � la vue
        //        if (!ModelState.IsValid)
        //        {
        //            _logger.LogWarning("Erreurs de validation d�tect�es");
        //            await PopulateViewBags();
        //            return View(utilisateur);
        //        }

        //        // 7. Cr�er l'utilisateur
        //        var nouvelUtilisateur = new Utilisateur
        //        {
        //            // ID g�n�r� automatiquement par le mod�le
        //            Nom = utilisateur.Nom.Trim(),
        //            Prenoms = utilisateur.Prenoms.Trim(),
        //            UserName = utilisateur.UserName.Trim(),
        //            Email = string.IsNullOrWhiteSpace(utilisateur.Email) ? null : utilisateur.Email.Trim(),
        //            PhoneNumber = string.IsNullOrWhiteSpace(utilisateur.PhoneNumber) ? null : utilisateur.PhoneNumber.Trim(),
        //            Lieu = string.IsNullOrWhiteSpace(utilisateur.Lieu) ? null : utilisateur.Lieu.Trim(),
        //            CodeCommande = string.IsNullOrWhiteSpace(utilisateur.CodeCommande) ? null : utilisateur.CodeCommande.Trim(),

        //            // S�curit�
        //            MotDePasseHash = _passwordHasher.HashPassword(motDePasse),
        //            MustResetPassword = true,

        //            // R�les et affectations
        //            Role = utilisateur.Role,
        //            DirectionId = utilisateur.DirectionId,
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

        //        _logger.LogInformation("Utilisateur cr�� avec succ�s - ID: {Id}, Nom: {Nom}", nouvelUtilisateur.Id, nouvelUtilisateur.Nom);
        //        TempData["SuccessMessage"] = $"L'utilisateur {nouvelUtilisateur.Nom} {nouvelUtilisateur.Prenoms} a �t� cr�� avec succ�s.";
        //        return RedirectToAction(nameof(List));
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Erreur lors de la cr�ation de l'utilisateur: {Message}", ex.Message);
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
                    .Include(u => u.Direction)
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
                        DirectionNom = u.Direction != null ? u.Direction.Nom : "N/A",
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
        /// Affiche les d�tails d'un utilisateur
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var utilisateur = await _context.Utilisateurs
                    .Include(u => u.Direction)
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
                _logger.LogError(ex, "Erreur lors du chargement des d�tails de l'utilisateur {UserId}", id);
                TempData["ErrorMessage"] = "Une erreur est survenue lors du chargement des d�tails.";
                return RedirectToAction(nameof(List));
            }
        }

        /// <summary>
        /// Affiche le formulaire d'�dition d'un utilisateur
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
                    DirectionId = utilisateur.DirectionId,
                    ServiceId = utilisateur.ServiceId,
                    FonctionId = utilisateur.FonctionId,
                    Site = utilisateur.Site
                };

                await PopulateViewBags();
                
                // Charger les services si une direction est sélectionnée
                if (utilisateur.DirectionId.HasValue)
                {
                    var services = await _context.Services
                        .Where(s => s.DirectionId == utilisateur.DirectionId && s.Supprimer == 0)
                        .ToListAsync();
                    ViewBag.Services = new SelectList(services, "Id", "Nom");
                }
                
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement de l'utilisateur pour �dition {UserId}", id);
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
            _logger.LogInformation("=== D�BUT MODIFICATION UTILISATEUR ===");
            _logger.LogInformation("Modification utilisateur ID: {Id}, Nom: {Nom}, UserName: {UserName}", 
                id, model?.Nom, model?.UserName);

            try
            {
                if (model == null)
                {
                    _logger.LogWarning("Les donn�es utilisateur sont manquantes.");
                    TempData["ErrorMessage"] = "Les donn�es utilisateur sont manquantes.";
                    return RedirectToAction(nameof(List));
                }

                if (id != model.Id)
                {
                    _logger.LogWarning("Identifiant utilisateur invalide. ID attendu: {ExpectedId}, ID re�u: {ReceivedId}", id, model.Id);
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
                    ModelState.AddModelError(nameof(model.Prenoms), "Les pr�noms sont obligatoires.");

                if (string.IsNullOrWhiteSpace(model.UserName))
                    ModelState.AddModelError(nameof(model.UserName), "Le matricule est obligatoire.");

                if (model.DirectionId == Guid.Empty)
                    ModelState.AddModelError(nameof(model.DirectionId), "Le d�partement est obligatoire.");

                // Validation du changement de mot de passe (optionnel)
                if (!string.IsNullOrWhiteSpace(model.NouveauMotDePasse))
                {
                    if (model.NouveauMotDePasse.Length < 6)
                        ModelState.AddModelError(nameof(model.NouveauMotDePasse), "Le mot de passe doit contenir au moins 6 caract�res.");

                    if (model.NouveauMotDePasse != model.ConfirmerNouveauMotDePasse)
                        ModelState.AddModelError(nameof(model.ConfirmerNouveauMotDePasse), "Les mots de passe ne correspondent pas.");
                }

                // V�rifier si l'email existe d�j� (sauf pour l'utilisateur actuel)
                if (!string.IsNullOrWhiteSpace(model.Email) && 
                    await _context.Utilisateurs.AnyAsync(u => u.Email == model.Email && u.Id != id && u.Supprimer == 0))
                {
                    ModelState.AddModelError(nameof(model.Email), "Cette adresse email est d�j� utilis�e.");
                }

                // V�rifier si le matricule (UserName) existe d�j� (sauf pour l'utilisateur actuel)
                if (!string.IsNullOrWhiteSpace(model.UserName) && 
                    await _context.Utilisateurs.AnyAsync(u => u.UserName == model.UserName && u.Id != id && u.Supprimer == 0))
                {
                    ModelState.AddModelError(nameof(model.UserName), "Ce matricule est d�j� utilis�.");
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("�chec de validation du formulaire de modification utilisateur.");
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

                // Mettre � jour les propri�t�s
                existingUser.Nom = model.Nom.Trim();
                existingUser.Prenoms = model.Prenoms.Trim();
                existingUser.UserName = model.UserName.Trim();
                existingUser.Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim();
                existingUser.PhoneNumber = string.IsNullOrWhiteSpace(model.PhoneNumber) ? null : model.PhoneNumber.Trim();
                existingUser.Lieu = string.IsNullOrWhiteSpace(model.Lieu) ? null : model.Lieu.Trim();
                existingUser.Role = model.Role;
                existingUser.DirectionId = model.DirectionId;
                existingUser.ServiceId = model.ServiceId;
                existingUser.FonctionId = model.FonctionId;
                existingUser.Site = model.Site;
                existingUser.ModifiedAt = DateTime.UtcNow;
                existingUser.ModifiedBy = User.Identity?.Name ?? "System";

                // Mettre � jour le mot de passe si fourni
                if (!string.IsNullOrWhiteSpace(model.NouveauMotDePasse))
                {
                    existingUser.MotDePasseHash = _passwordHasher.HashPassword(model.NouveauMotDePasse);
                    existingUser.MustResetPassword = false; // L'utilisateur a d�fini un nouveau mot de passe
                    _logger.LogInformation("Mot de passe mis � jour pour l'utilisateur {UserName}", existingUser.UserName);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Utilisateur modifi� avec succ�s: {UserName} par {ModifiedBy}", 
                    existingUser.UserName, User.Identity?.Name);
                TempData["SuccessMessage"] = $"L'utilisateur {existingUser.Nom} {existingUser.Prenoms} a �t� modifi� avec succ�s.";

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

                _logger.LogInformation($"Utilisateur supprim�: {utilisateur.UserName} par {User.Identity?.Name}");
                TempData["SuccessMessage"] = $"L'utilisateur {utilisateur.Nom} {utilisateur.Prenoms} a �t� supprim� avec succ�s.";

                return RedirectToAction(nameof(List));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression de l'utilisateur {UserId}", id);
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la suppression de l'utilisateur.";
                return RedirectToAction(nameof(List));
            }
        }

        /// <summary>
        /// Affiche le formulaire de réinitialisation de mot de passe
        /// </summary>
        [HttpGet]
        public IActionResult ResetPassword()
        {
            return View();
        }

        /// <summary>
        /// Traite la réinitialisation de mot de passe
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Les données sont manquantes.");
                return View();
            }

            try
            {
                var matricules = model.Matricules
                    ?.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(m => m.Trim())
                    .Where(m => !string.IsNullOrWhiteSpace(m))
                    .ToList();

                if (matricules == null || !matricules.Any())
                {
                    ModelState.AddModelError(nameof(model.Matricules), "Aucun matricule valide trouvé.");
                    return View(model);
                }

                var utilisateurs = await _context.Utilisateurs
                    .Where(u => matricules.Contains(u.UserName) && u.Supprimer == 0)
                    .ToListAsync();

                if (!utilisateurs.Any())
                {
                    return View(model);
                }

                var defaultPassword = model.NouveauMotDePasse ?? "Welcome2024!";
                var hashedPassword = _passwordHasher.HashPassword(defaultPassword);
                var updatedCount = 0;

                foreach (var utilisateur in utilisateurs)
                {
                    utilisateur.MotDePasseHash = hashedPassword;
                    utilisateur.MustResetPassword = true;
                    utilisateur.ModifiedAt = DateTime.UtcNow;
                    utilisateur.ModifiedBy = User.Identity?.Name ?? "System";
                    updatedCount++;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("{Count} mots de passe réinitialisés par {User}", updatedCount, User.Identity?.Name);
                TempData["SuccessMessage"] = $"{updatedCount} mot(s) de passe réinitialisé(s) avec succès.";

                return RedirectToAction(nameof(List));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la réinitialisation des mots de passe");
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la réinitialisation des mots de passe.";
                return View(model);
            }
        }
    }
}
