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

        private static string Sha256Base64(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToBase64String(bytes);
        }
    }
}
