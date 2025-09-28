using Obeli_K.Models;

namespace Obeli_K.Services.Users
{
    public interface IUserService
    {
        Task<Utilisateur?> GetByUserNameAsync(string userName);
        Task<Utilisateur?> GetByIdAsync(Guid id);
        Task<Utilisateur> CreateAsync(Utilisateur utilisateur);
        Task<Utilisateur?> UpdateAsync(Utilisateur utilisateur);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
        Task<bool> ResetPasswordAsync(string email, string newPassword);
        Task<List<Utilisateur>> GetAllAsync();
        Task<List<Utilisateur>> GetByRoleAsync(Models.Enums.RoleType role);
    }
}
