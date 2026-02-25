using Microsoft.EntityFrameworkCore;
using Obeli_K.Data;
using Obeli_K.Models;
using Obeli_K.Services.Security;

namespace Obeli_K.Services.Users
{
    public class UserService : IUserService
    {
        private readonly ObeliDbContext _context;
        private readonly IPasswordHasher _passwordHasher;

        public UserService(ObeliDbContext context, IPasswordHasher passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<Utilisateur?> GetByUserNameAsync(string userName)
        {
            return await _context.Utilisateurs
                .Include(u => u.Direction)
                .Include(u => u.Fonction)
                .FirstOrDefaultAsync(u => u.UserName == userName && (u.Supprimer == 0));
        }

        public async Task<Utilisateur?> GetByIdAsync(Guid id)
        {
            return await _context.Utilisateurs
                .Include(u => u.Direction)
                .Include(u => u.Fonction)
                .FirstOrDefaultAsync(u => u.Id == id && (u.Supprimer == 0));
        }

        public async Task<Utilisateur> CreateAsync(Utilisateur utilisateur)
        {
            // Hasher le mot de passe
            utilisateur.MotDePasseHash = _passwordHasher.HashPassword(utilisateur.MotDePasseHash);
            
            utilisateur.CreatedAt = DateTime.UtcNow;
            utilisateur.ModifiedAt = DateTime.UtcNow;

            _context.Utilisateurs.Add(utilisateur);
            await _context.SaveChangesAsync();
            return utilisateur;
        }

        public async Task<Utilisateur?> UpdateAsync(Utilisateur utilisateur)
        {
            var existing = await GetByIdAsync(utilisateur.Id);
            if (existing == null) return null;

            // Mettre à jour les propriétés
            existing.Nom = utilisateur.Nom;
            existing.Prenoms = utilisateur.Prenoms;
            existing.Email = utilisateur.Email;
            existing.PhoneNumber = utilisateur.PhoneNumber;
            existing.Lieu = utilisateur.Lieu;
            existing.DirectionId = utilisateur.DirectionId;
            existing.FonctionId = utilisateur.FonctionId;
            existing.Role = utilisateur.Role;
            existing.Site = utilisateur.Site;
            existing.ModifiedAt = DateTime.UtcNow;
            existing.ModifiedBy = utilisateur.ModifiedBy;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var utilisateur = await GetByIdAsync(id);
            if (utilisateur == null) return false;

            // Soft delete
            utilisateur.Supprimer = 1;
            utilisateur.ModifiedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
        {
            var utilisateur = await GetByIdAsync(userId);
            if (utilisateur == null) return false;

            // Vérifier le mot de passe actuel
            if (!_passwordHasher.VerifyPassword(currentPassword, utilisateur.MotDePasseHash))
                return false;

            // Hasher et sauvegarder le nouveau mot de passe
            utilisateur.MotDePasseHash = _passwordHasher.HashPassword(newPassword);
            utilisateur.ModifiedAt = DateTime.UtcNow;
            utilisateur.MustResetPassword = false;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ResetPasswordAsync(string email, string newPassword)
        {
            var utilisateur = await _context.Utilisateurs
                .FirstOrDefaultAsync(u => u.Email == email && (u.Supprimer == 0));
            
            if (utilisateur == null) return false;

            // Hasher et sauvegarder le nouveau mot de passe
            utilisateur.MotDePasseHash = _passwordHasher.HashPassword(newPassword);
            utilisateur.ModifiedAt = DateTime.UtcNow;
            utilisateur.MustResetPassword = true;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Utilisateur>> GetAllAsync()
        {
            return await _context.Utilisateurs
                .Include(u => u.Direction)
                .Include(u => u.Fonction)
                .Where(u => u.Supprimer == 0)
                .OrderBy(u => u.Nom)
                .ThenBy(u => u.Prenoms)
                .ToListAsync();
        }

        public async Task<List<Utilisateur>> GetByRoleAsync(Models.Enums.RoleType role)
        {
            return await _context.Utilisateurs
                .Include(u => u.Direction)
                .Include(u => u.Fonction)
                .Where(u => u.Role == role && (u.Supprimer == 0))
                .OrderBy(u => u.Nom)
                .ThenBy(u => u.Prenoms)
                .ToListAsync();
        }
    }
}
