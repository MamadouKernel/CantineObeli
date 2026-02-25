using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Obeli_K.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Obeli_K.Controllers
{
    [Authorize]
    public class DiagnosticUserController : Controller
    {
        private readonly ObeliDbContext _context;
        private readonly ILogger<DiagnosticUserController> _logger;

        public DiagnosticUserController(ObeliDbContext context, ILogger<DiagnosticUserController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var diagnostic = new
                {
                    // Informations de session
                    SessionInfo = new
                    {
                        IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
                        Name = User.Identity?.Name,
                        AuthenticationType = User.Identity?.AuthenticationType,
                        Claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
                    },

                    // ID utilisateur depuis les claims
                    UserIdFromClaims = GetUserIdFromClaims(),

                    // Utilisateur dans la base de données
                    UserInDatabase = await GetUserFromDatabase(),

                    // Tous les utilisateurs (pour comparaison)
                    AllUsers = await GetAllUsers()
                };

                return Json(diagnostic, new System.Text.Json.JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors du diagnostic utilisateur");
                return Json(new { error = ex.Message });
            }
        }

        private string? GetUserIdFromClaims()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("🔍 ID utilisateur depuis les claims: {UserId}", userIdClaim);
            return userIdClaim;
        }

        private async Task<object?> GetUserFromDatabase()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var userId))
                {
                    var user = await _context.Utilisateurs
                        .AsNoTracking()
                        .FirstOrDefaultAsync(u => u.Id == userId && u.Supprimer == 0);

                    if (user != null)
                    {
                        _logger.LogInformation("✅ Utilisateur trouvé en base: {UserName} ({UserId})", user.UserName, user.Id);
                        return new
                        {
                            Id = user.Id.ToString(),
                            UserName = user.UserName,
                            Nom = user.Nom,
                            Prenoms = user.Prenoms,
                            Email = user.Email,
                            Role = user.Role,
                            Site = user.Site?.ToString(),
                            Supprimer = user.Supprimer
                        };
                    }
                    else
                    {
                        _logger.LogWarning("❌ Utilisateur non trouvé en base pour l'ID: {UserId}", userId);
                        return new { error = $"Utilisateur non trouvé pour l'ID {userId}" };
                    }
                }

                // Fallback : chercher par UserName
                var userNameClaim = User.FindFirst("UserName")?.Value ?? User.Identity?.Name;
                if (!string.IsNullOrEmpty(userNameClaim))
                {
                    var user = await _context.Utilisateurs
                        .AsNoTracking()
                        .FirstOrDefaultAsync(u => u.UserName == userNameClaim && u.Supprimer == 0);

                    if (user != null)
                    {
                        _logger.LogInformation("✅ Utilisateur trouvé par UserName: {UserName} ({UserId})", user.UserName, user.Id);
                        return new
                        {
                            Id = user.Id.ToString(),
                            UserName = user.UserName,
                            Nom = user.Nom,
                            Prenoms = user.Prenoms,
                            Email = user.Email,
                            Role = user.Role,
                            Site = user.Site?.ToString(),
                            Supprimer = user.Supprimer
                        };
                    }
                    else
                    {
                        _logger.LogWarning("❌ Utilisateur non trouvé en base pour le UserName: {UserName}", userNameClaim);
                        return new { error = $"Utilisateur non trouvé pour le UserName {userNameClaim}" };
                    }
                }

                return new { error = "Aucun identifiant utilisateur trouvé" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors de la recherche utilisateur en base");
                return new { error = ex.Message };
            }
        }

        private async Task<object> GetAllUsers()
        {
            try
            {
                var users = await _context.Utilisateurs
                    .AsNoTracking()
                    .Where(u => u.Supprimer == 0)
                    .Select(u => new
                    {
                        Id = u.Id.ToString(),
                        UserName = u.UserName,
                        Nom = u.Nom,
                        Prenoms = u.Prenoms,
                        Role = u.Role,
                        Site = u.Site != null ? u.Site.ToString() : null
                    })
                    .ToListAsync();

                _logger.LogInformation("📊 {Count} utilisateurs trouvés en base", users.Count);
                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors de la récupération de tous les utilisateurs");
                return new { error = ex.Message };
            }
        }
    }
}
