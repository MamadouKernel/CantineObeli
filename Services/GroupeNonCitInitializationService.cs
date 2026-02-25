using Microsoft.EntityFrameworkCore;
using Obeli_K.Data;
using Obeli_K.Models;
using Obeli_K.Enums;

namespace Obeli_K.Services
{
    /// <summary>
    /// Service d'initialisation automatique des groupes non-CIT
    /// </summary>
    public class GroupeNonCitInitializationService
    {
        private readonly ObeliDbContext _context;
        private readonly ILogger<GroupeNonCitInitializationService> _logger;

        public GroupeNonCitInitializationService(ObeliDbContext context, ILogger<GroupeNonCitInitializationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Initialise tous les groupes non-CIT prédéfinis
        /// </summary>
        public async Task InitializeGroupsAsync()
        {
            try
            {
                _logger.LogInformation("🔧 Initialisation des groupes non-CIT...");

                // Vérifier si des groupes existent déjà
                var existingGroups = await _context.GroupesNonCit
                    .Where(g => g.Supprimer == 0)
                    .ToListAsync();

                if (existingGroups.Any())
                {
                    _logger.LogInformation("✅ {Count} groupes non-CIT existent déjà", existingGroups.Count);
                    return;
                }

                // Créer les groupes prédéfinis
                var groupsToCreate = new List<GroupeNonCit>
                {
                    CreateGroupFromConfig(
                        GroupeNonCitConfig.Douaniers.Nom,
                        GroupeNonCitConfig.Douaniers.Description,
                        GroupeNonCitConfig.Douaniers.CodeGroupe,
                        GroupeNonCitConfig.Douaniers.QuotaJournalier,
                        GroupeNonCitConfig.Douaniers.QuotaNuit,
                        GroupeNonCitConfig.Douaniers.RestrictionFormuleStandard
                    ),
                    CreateGroupFromConfig(
                        GroupeNonCitConfig.ForcesOrdre.Nom,
                        GroupeNonCitConfig.ForcesOrdre.Description,
                        GroupeNonCitConfig.ForcesOrdre.CodeGroupe,
                        GroupeNonCitConfig.ForcesOrdre.QuotaJournalier,
                        GroupeNonCitConfig.ForcesOrdre.QuotaNuit,
                        GroupeNonCitConfig.ForcesOrdre.RestrictionFormuleStandard
                    ),
                    CreateGroupFromConfig(
                        GroupeNonCitConfig.Securite.Nom,
                        GroupeNonCitConfig.Securite.Description,
                        GroupeNonCitConfig.Securite.CodeGroupe,
                        GroupeNonCitConfig.Securite.QuotaJournalier,
                        GroupeNonCitConfig.Securite.QuotaNuit,
                        GroupeNonCitConfig.Securite.RestrictionFormuleStandard
                    ),
                    CreateGroupFromConfig(
                        GroupeNonCitConfig.VisiteursOfficiels.Nom,
                        GroupeNonCitConfig.VisiteursOfficiels.Description,
                        GroupeNonCitConfig.VisiteursOfficiels.CodeGroupe,
                        GroupeNonCitConfig.VisiteursOfficiels.QuotaJournalier,
                        GroupeNonCitConfig.VisiteursOfficiels.QuotaNuit,
                        GroupeNonCitConfig.VisiteursOfficiels.RestrictionFormuleStandard
                    )
                };

                _context.GroupesNonCit.AddRange(groupsToCreate);
                await _context.SaveChangesAsync();

                _logger.LogInformation("✅ {Count} groupes non-CIT créés avec succès", groupsToCreate.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors de l'initialisation des groupes non-CIT");
                throw;
            }
        }

        /// <summary>
        /// Crée un groupe à partir de sa configuration
        /// </summary>
        private GroupeNonCit CreateGroupFromConfig(
            string nom, 
            string description, 
            string codeGroupe, 
            int quotaJournalier, 
            int quotaNuit, 
            bool restrictionFormuleStandard)
        {
            return new GroupeNonCit
            {
                Id = Guid.NewGuid(),
                Nom = nom,
                Description = description,
                CodeGroupe = codeGroupe,
                QuotaJournalier = quotaJournalier,
                QuotaNuit = quotaNuit,
                RestrictionFormuleStandard = restrictionFormuleStandard,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "System_AutoInit",
                Supprimer = 0
            };
        }

        /// <summary>
        /// Vérifie si un groupe spécifique existe
        /// </summary>
        public async Task<bool> GroupExistsAsync(string nomGroupe)
        {
            return await _context.GroupesNonCit
                .AnyAsync(g => g.Nom == nomGroupe && g.Supprimer == 0);
        }

        /// <summary>
        /// Crée un groupe spécifique s'il n'existe pas
        /// </summary>
        public async Task<GroupeNonCit?> CreateGroupIfNotExistsAsync(GroupeNonCitEnum groupeEnum)
        {
            try
            {
                var (nom, description, codeGroupe, quotaJournalier, quotaNuit, restriction) = GetGroupConfig(groupeEnum);

                // Vérifier si le groupe existe déjà
                var existingGroup = await _context.GroupesNonCit
                    .FirstOrDefaultAsync(g => g.Nom == nom && g.Supprimer == 0);

                if (existingGroup != null)
                {
                    _logger.LogInformation("✅ Groupe {Nom} existe déjà", nom);
                    return existingGroup;
                }

                // Créer le groupe
                var newGroup = CreateGroupFromConfig(nom, description, codeGroupe, quotaJournalier, quotaNuit, restriction);
                
                _context.GroupesNonCit.Add(newGroup);
                await _context.SaveChangesAsync();

                _logger.LogInformation("✅ Groupe {Nom} créé avec succès", nom);
                return newGroup;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors de la création du groupe {GroupeEnum}", groupeEnum);
                return null;
            }
        }

        /// <summary>
        /// Récupère la configuration d'un groupe à partir de l'enum
        /// </summary>
        private (string nom, string description, string codeGroupe, int quotaJournalier, int quotaNuit, bool restriction) GetGroupConfig(GroupeNonCitEnum groupeEnum)
        {
            return groupeEnum switch
            {
                GroupeNonCitEnum.Douaniers => (
                    GroupeNonCitConfig.Douaniers.Nom,
                    GroupeNonCitConfig.Douaniers.Description,
                    GroupeNonCitConfig.Douaniers.CodeGroupe,
                    GroupeNonCitConfig.Douaniers.QuotaJournalier,
                    GroupeNonCitConfig.Douaniers.QuotaNuit,
                    GroupeNonCitConfig.Douaniers.RestrictionFormuleStandard
                ),
                GroupeNonCitEnum.ForcesOrdre => (
                    GroupeNonCitConfig.ForcesOrdre.Nom,
                    GroupeNonCitConfig.ForcesOrdre.Description,
                    GroupeNonCitConfig.ForcesOrdre.CodeGroupe,
                    GroupeNonCitConfig.ForcesOrdre.QuotaJournalier,
                    GroupeNonCitConfig.ForcesOrdre.QuotaNuit,
                    GroupeNonCitConfig.ForcesOrdre.RestrictionFormuleStandard
                ),
                GroupeNonCitEnum.Securite => (
                    GroupeNonCitConfig.Securite.Nom,
                    GroupeNonCitConfig.Securite.Description,
                    GroupeNonCitConfig.Securite.CodeGroupe,
                    GroupeNonCitConfig.Securite.QuotaJournalier,
                    GroupeNonCitConfig.Securite.QuotaNuit,
                    GroupeNonCitConfig.Securite.RestrictionFormuleStandard
                ),
                GroupeNonCitEnum.VisiteursOfficiels => (
                    GroupeNonCitConfig.VisiteursOfficiels.Nom,
                    GroupeNonCitConfig.VisiteursOfficiels.Description,
                    GroupeNonCitConfig.VisiteursOfficiels.CodeGroupe,
                    GroupeNonCitConfig.VisiteursOfficiels.QuotaJournalier,
                    GroupeNonCitConfig.VisiteursOfficiels.QuotaNuit,
                    GroupeNonCitConfig.VisiteursOfficiels.RestrictionFormuleStandard
                ),
                _ => throw new ArgumentException($"Groupe non supporté: {groupeEnum}")
            };
        }
    }
}
