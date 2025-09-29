using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Obeli_K.Models;
using Obeli_K.Models.Enums;
using Obeli_K.Data;

namespace Obeli_K.Controllers
{
    public class AuthController : Controller
    {
        private readonly ObeliDbContext _db;

        public AuthController(ObeliDbContext db) => _db = db;

        [HttpGet]
        public IActionResult Login()
        {
            ClaimsPrincipal claimsPrincipal = HttpContext.User;
            if (claimsPrincipal.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var u = await _db.Utilisateurs
                    .FirstOrDefaultAsync(x => x.UserName == model.Matricule);
            if (u == null || !BCrypt.Net.BCrypt.Verify(model.MotDePasse, u.MotDePasseHash))
            {
                ModelState.AddModelError("", "Matricule ou mot de passe invalide.");
                TempData["error"] = "Matricule ou mot de passe invalide.";
                return View(model);
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, u.Id.ToString()),
                new(ClaimTypes.Name, $"{u.Nom} {u.Prenoms}"),
                new("UserName", u.UserName)
            };

            // Assigner le bon nom de rôle selon l'enum
            switch (u.Role)
            {
                case RoleType.Admin:
                    claims.Add(new(ClaimTypes.Role, "Administrateur"));
                    break;
                case RoleType.RH:
                    claims.Add(new(ClaimTypes.Role, "RessourcesHumaines"));
                    break;
                case RoleType.Employe:
                    claims.Add(new(ClaimTypes.Role, "Employe"));
                    break;
                case RoleType.PrestataireCantine:
                    claims.Add(new(ClaimTypes.Role, "PrestataireCantine"));
                    break;
            }

            // Si c'est un administrateur, ajouter automatiquement tous les rôles
            if (u.Role == RoleType.Admin)
            {
                claims.Add(new(ClaimTypes.Role, "RessourcesHumaines"));
                claims.Add(new(ClaimTypes.Role, "PrestataireCantine"));
                claims.Add(new(ClaimTypes.Role, "Employe"));
                claims.Add(new(ClaimTypes.Role, "Visiteur"));
            }

            var id = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return Redirect(model.ReturnUrl);

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }

        // ======== Mot de passe oublié ========
        [HttpGet]
        public IActionResult Forgot() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Forgot(string userNameOrEmail)
        {
            var u = await _db.Utilisateurs.FirstOrDefaultAsync(x =>
                x.UserName == userNameOrEmail || x.Email == userNameOrEmail);

            // Toujours répondre OK pour ne pas divulguer l’existence d’un compte
            if (u != null)
            {
                var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
                var tokenHash = Sha256Base64(rawToken);

                u.ResetTokenHash = tokenHash;
                u.ResetExpireLeUtc = DateTime.UtcNow.AddHours(2);
                u.ResetUtilise = false;
                await _db.SaveChangesAsync();

                // Ici tu peux envoyer par email/SMS. Pour l’instant on affiche un lien de reset.
                TempData["ResetLink"] = Url.Action(nameof(Reset), "Auth", new { token = rawToken }, Request.Scheme);
            }

            TempData["ok"] = "Si le compte existe, un lien de réinitialisation a été généré.";
            return RedirectToAction(nameof(Forgot));
        }

        [HttpGet]
        public IActionResult Reset(string token)
        {
            ViewBag.Token = token;
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Reset(string token, string nouveauMotDePasse, string confirmation)
        {
            if (string.IsNullOrWhiteSpace(nouveauMotDePasse) || nouveauMotDePasse != confirmation)
            {
                ModelState.AddModelError("", "Les mots de passe ne correspondent pas.");
                ViewBag.Token = token;
                return View();
            }

            var hash = Sha256Base64(token);
            var u = await _db.Utilisateurs.FirstOrDefaultAsync(x =>
                x.ResetTokenHash == hash && x.ResetExpireLeUtc > DateTime.UtcNow && !x.ResetUtilise);

            if (u == null)
            {
                ModelState.AddModelError("", "Jeton invalide ou expiré.");
                ViewBag.Token = token;
                return View();
            }

            u.MotDePasseHash = BCrypt.Net.BCrypt.HashPassword(nouveauMotDePasse, workFactor: 12);
            u.ResetUtilise = true;
            u.MustResetPassword = false;
            u.ModifiedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            TempData["ok"] = "Mot de passe réinitialisé. Connectez-vous.";
            return RedirectToAction(nameof(Login));
        }

        // ======== Changement de mot de passe ========
        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [Authorize]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string motDePasseActuel, string nouveauMotDePasse, string confirmation)
        {
            if (string.IsNullOrWhiteSpace(motDePasseActuel) || 
                string.IsNullOrWhiteSpace(nouveauMotDePasse) || 
                nouveauMotDePasse != confirmation)
            {
                ModelState.AddModelError("", "Tous les champs sont obligatoires et les nouveaux mots de passe doivent correspondre.");
                return View();
            }

            if (nouveauMotDePasse.Length < 8)
            {
                ModelState.AddModelError("", "Le nouveau mot de passe doit contenir au moins 8 caractères.");
                return View();
            }

            // Récupérer l'utilisateur connecté
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var id))
            {
                return RedirectToAction(nameof(Login));
            }

            var utilisateur = await _db.Utilisateurs.FindAsync(id);
            if (utilisateur == null)
            {
                return RedirectToAction(nameof(Login));
            }

            // Vérifier l'ancien mot de passe
            if (!BCrypt.Net.BCrypt.Verify(motDePasseActuel, utilisateur.MotDePasseHash))
            {
                ModelState.AddModelError("", "Le mot de passe actuel est incorrect.");
                return View();
            }

            // Mettre à jour le mot de passe
            utilisateur.MotDePasseHash = BCrypt.Net.BCrypt.HashPassword(nouveauMotDePasse, 12);
            utilisateur.MustResetPassword = false;
            utilisateur.ModifiedAt = DateTime.UtcNow;
            utilisateur.ModifiedBy = utilisateur.UserName;

            await _db.SaveChangesAsync();

            TempData["ok"] = "Votre mot de passe a été modifié avec succès !";
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var id))
            {
                return RedirectToAction(nameof(Login));
            }

            var utilisateur = await _db.Utilisateurs
                .Include(u => u.Fonction)
                .Include(u => u.Departement)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (utilisateur == null)
            {
                return RedirectToAction(nameof(Login));
            }

            return View(utilisateur);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EditProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var id))
            {
                return RedirectToAction(nameof(Login));
            }

            var utilisateur = await _db.Utilisateurs
                .Include(u => u.Fonction)
                .Include(u => u.Departement)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (utilisateur == null)
            {
                return RedirectToAction(nameof(Login));
            }

            // Préparer les listes pour les dropdowns
            ViewBag.Fonctions = await _db.Fonctions
                .Where(f => f.Supprimer == 0)
                .OrderBy(f => f.Nom)
                .Select(f => new { Value = f.Id.ToString(), Text = f.Nom })
                .ToListAsync();

            ViewBag.Departements = await _db.Departements
                .Where(d => d.Supprimer == 0)
                .OrderBy(d => d.Nom)
                .Select(d => new { Value = d.Id.ToString(), Text = d.Nom })
                .ToListAsync();

            return View(utilisateur);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(Utilisateur model)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var id))
            {
                return RedirectToAction(nameof(Login));
            }

            var utilisateur = await _db.Utilisateurs.FindAsync(id);
            if (utilisateur == null)
            {
                return RedirectToAction(nameof(Login));
            }

            // Validation des champs obligatoires
            if (string.IsNullOrWhiteSpace(model.Nom) || string.IsNullOrWhiteSpace(model.Prenoms) || string.IsNullOrWhiteSpace(model.UserName))
            {
                ModelState.AddModelError("", "Le nom, prénoms et matricule sont obligatoires.");
                await PopulateEditProfileViewBags();
                return View(model);
            }

            // Vérifier si le matricule est déjà utilisé par un autre utilisateur
            if (model.UserName != utilisateur.UserName)
            {
                var existingUser = await _db.Utilisateurs
                    .FirstOrDefaultAsync(u => u.UserName == model.UserName && u.Id != id && u.Supprimer == 0);
                
                if (existingUser != null)
                {
                    ModelState.AddModelError("UserName", "Ce matricule est déjà utilisé par un autre utilisateur.");
                    await PopulateEditProfileViewBags();
                    return View(model);
                }
            }

            // Vérifier si l'email est déjà utilisé par un autre utilisateur (si fourni)
            if (!string.IsNullOrWhiteSpace(model.Email) && model.Email != utilisateur.Email)
            {
                var existingEmail = await _db.Utilisateurs
                    .FirstOrDefaultAsync(u => u.Email == model.Email && u.Id != id && u.Supprimer == 0);
                
                if (existingEmail != null)
                {
                    ModelState.AddModelError("Email", "Cet email est déjà utilisé par un autre utilisateur.");
                    await PopulateEditProfileViewBags();
                    return View(model);
                }
            }

            // Mettre à jour les informations (sauf le mot de passe)
            utilisateur.Nom = model.Nom.Trim();
            utilisateur.Prenoms = model.Prenoms.Trim();
            utilisateur.UserName = model.UserName.Trim();
            utilisateur.Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim();
            utilisateur.PhoneNumber = string.IsNullOrWhiteSpace(model.PhoneNumber) ? null : model.PhoneNumber.Trim();
            utilisateur.Lieu = string.IsNullOrWhiteSpace(model.Lieu) ? null : model.Lieu.Trim();
            utilisateur.DepartementId = model.DepartementId;
            utilisateur.FonctionId = model.FonctionId;
            utilisateur.Site = model.Site;
            utilisateur.ModifiedAt = DateTime.UtcNow;
            utilisateur.ModifiedBy = utilisateur.UserName;

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Vos informations ont été mises à jour avec succès !";
            return RedirectToAction(nameof(Profile));
        }

        private async Task PopulateEditProfileViewBags()
        {
            ViewBag.Fonctions = await _db.Fonctions
                .Where(f => f.Supprimer == 0)
                .OrderBy(f => f.Nom)
                .Select(f => new { Value = f.Id.ToString(), Text = f.Nom })
                .ToListAsync();

            ViewBag.Departements = await _db.Departements
                .Where(d => d.Supprimer == 0)
                .OrderBy(d => d.Nom)
                .Select(d => new { Value = d.Id.ToString(), Text = d.Nom })
                .ToListAsync();
        }

        private static string Sha256Base64(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToBase64String(bytes);
        }
    }
}
