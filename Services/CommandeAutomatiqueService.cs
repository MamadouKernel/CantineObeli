using Microsoft.EntityFrameworkCore;
using Obeli_K.Data;
using Obeli_K.Enums;
using Obeli_K.Models;
using Obeli_K.Models.Enums;

namespace Obeli_K.Services
{
    public interface ICommandeAutomatiqueService
    {
        Task<bool> VerifierBlocageCommandesAsync();
        Task<bool> ConfirmerCommandesAutomatiquementAsync();
        Task InitialiserConfigurationsParDefautAsync();
    }

    public class CommandeAutomatiqueService : ICommandeAutomatiqueService
    {
        private readonly ObeliDbContext _context;
        private readonly Obeli_K.Services.Configuration.IConfigurationService _configurationService;
        private readonly ILogger<CommandeAutomatiqueService> _logger;

        public CommandeAutomatiqueService(
            ObeliDbContext context, 
            Obeli_K.Services.Configuration.IConfigurationService configurationService,
            ILogger<CommandeAutomatiqueService> logger)
        {
            _context = context;
            _configurationService = configurationService;
            _logger = logger;
        }

        public async Task<bool> VerifierBlocageCommandesAsync()
        {
            try
            {
                var isBlocked = await _configurationService.IsCommandeBlockedAsync();
                
                if (isBlocked)
                {
                    _logger.LogInformation("Blocage des commandes activé - Vérification des commandes en attente");
                    
                    // Ici on pourrait ajouter une logique pour marquer les commandes comme bloquées
                    // ou empêcher la création de nouvelles commandes
                    // Pour l'instant, on se contente de logger l'information
                    
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la vérification du blocage des commandes");
                return false;
            }
        }

        public async Task<bool> ConfirmerCommandesAutomatiquementAsync()
        {
            try
            {
                var shouldAutoConfirm = await _configurationService.ShouldAutoConfirmCommandsAsync();
                
                if (!shouldAutoConfirm)
                {
                    _logger.LogInformation("Auto-confirmation des commandes désactivée");
                    return false;
                }

                var isBlockingTime = await _configurationService.IsCommandeBlockedAsync();
                
                if (!isBlockingTime)
                {
                    _logger.LogInformation("Ce n'est pas encore l'heure de l'auto-confirmation");
                    return false;
                }

                // Récupérer les commandes précommandées de la semaine N+1
                var (lundiN1, vendrediN1) = GetSemaineSuivanteOuvree();
                
                var commandesAConfirmer = await _context.Commandes
                    .Where(c => c.DateConsommation.HasValue &&
                               c.DateConsommation.Value.Date >= lundiN1 &&
                               c.DateConsommation.Value.Date <= vendrediN1 &&
                               c.StatusCommande == (int)Enums.StatutCommande.Precommander &&
                               c.Supprimer == 0)
                    .ToListAsync();

                if (!commandesAConfirmer.Any())
                {
                    _logger.LogInformation("Aucune commande à confirmer automatiquement");
                    return true;
                }

                var commandesConfirmees = 0;
                foreach (var commande in commandesAConfirmer)
                {
                    commande.StatusCommande = (int)Enums.StatutCommande.Consommee;
                    commande.ModifiedOn = DateTime.UtcNow;
                    commande.ModifiedBy = "System_AutoConfirmation";
                    commandesConfirmees++;

                    // Créer le point de consommation
                    await CreerPointConsommationAsync(commande);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Auto-confirmation terminée: {Count} commandes confirmées", commandesConfirmees);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'auto-confirmation des commandes");
                return false;
            }
        }

        public async Task InitialiserConfigurationsParDefautAsync()
        {
            try
            {
                // Vérifier si les configurations existent déjà
                var jourCloture = await _configurationService.GetConfigurationAsync("COMMANDE_JOUR_CLOTURE");
                if (string.IsNullOrEmpty(jourCloture))
                {
                    await _configurationService.SetConfigurationAsync(
                        "COMMANDE_JOUR_CLOTURE", 
                        "Friday", 
                        "Jour de la semaine pour la clôture des commandes (Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday)");
                }

                var heureCloture = await _configurationService.GetConfigurationAsync("COMMANDE_HEURE_CLOTURE");
                if (string.IsNullOrEmpty(heureCloture))
                {
                    await _configurationService.SetConfigurationAsync(
                        "COMMANDE_HEURE_CLOTURE", 
                        "12:00", 
                        "Heure de clôture des commandes (format HH:mm)");
                }

                var autoConfirm = await _configurationService.GetConfigurationAsync("COMMANDE_AUTO_CONFIRMATION");
                if (string.IsNullOrEmpty(autoConfirm))
                {
                    await _configurationService.SetConfigurationAsync(
                        "COMMANDE_AUTO_CONFIRMATION", 
                        "true", 
                        "Activer la confirmation automatique des commandes (true/false)");
                }

                _logger.LogInformation("Configurations par défaut initialisées");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'initialisation des configurations par défaut");
            }
        }

        private (DateTime Lundi, DateTime Vendredi) GetSemaineSuivanteOuvree()
        {
            var today = DateTime.Today;
            int diffToMonday = ((int)today.DayOfWeek + 6) % 7; // Lundi=0
            var thisWeekMonday = today.AddDays(-diffToMonday).Date;

            var nextWeekMonday = thisWeekMonday.AddDays(7);
            var nextWeekFriday = nextWeekMonday.AddDays(4);
            return (nextWeekMonday, nextWeekFriday);
        }

        private async Task CreerPointConsommationAsync(Commande commande)
        {
            try
            {
                // Vérifier si un point de consommation existe déjà
                var pointExistant = await _context.PointsConsommation
                    .AsNoTracking()
                    .AnyAsync(pc => pc.CommandeId == commande.IdCommande && pc.Supprimer == 0);

                if (pointExistant)
                {
                    return;
                }

                // Récupérer la formule
                var formule = await _context.FormulesJour
                    .AsNoTracking()
                    .Include(f => f.NomFormuleNavigation)
                    .FirstOrDefaultAsync(f => f.IdFormule == commande.IdFormule);

                if (formule == null)
                {
                    _logger.LogWarning("Formule non trouvée pour la commande {CommandeId}", commande.IdCommande);
                    return;
                }

                var nomPlat = GetNomPlatFromFormule(formule);
                var typeFormule = formule.NomFormuleNavigation?.Nom ?? "Standard";

                var pointConsommation = new PointConsommation
                {
                    IdPointConsommation = Guid.NewGuid(),
                    UtilisateurId = commande.UtilisateurId ?? Guid.Empty,
                    CommandeId = commande.IdCommande,
                    DateConsommation = commande.DateConsommation ?? DateTime.Today,
                    TypeFormule = typeFormule,
                    NomPlat = nomPlat,
                    QuantiteConsommee = commande.Quantite,
                    LieuConsommation = "Restaurant CIT",
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = "System_AutoConfirmation",
                    Supprimer = 0
                };

                _context.PointsConsommation.Add(pointConsommation);
                _logger.LogInformation("Point de consommation créé pour la commande {CommandeId}: {NomPlat}", 
                    commande.IdCommande, nomPlat);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création du point de consommation pour la commande {CommandeId}", 
                    commande.IdCommande);
            }
        }

        private string GetNomPlatFromFormule(FormuleJour? formule)
        {
            if (formule == null) return "Plat non spécifié";

            var nomFormule = formule.NomFormule?.ToLower();
            
            switch (nomFormule)
            {
                case "amélioré":
                case "ameliore":
                    return !string.IsNullOrEmpty(formule.Plat) ? formule.Plat : "Plat amélioré";
                
                case "standard 1":
                case "standard1":
                    return !string.IsNullOrEmpty(formule.PlatStandard1) ? formule.PlatStandard1 : "Plat Standard 1";
                
                case "standard 2":
                case "standard2":
                    return !string.IsNullOrEmpty(formule.PlatStandard2) ? formule.PlatStandard2 : "Plat Standard 2";
                
                default:
                    if (!string.IsNullOrEmpty(formule.Plat)) return formule.Plat;
                    if (!string.IsNullOrEmpty(formule.PlatStandard1)) return formule.PlatStandard1;
                    if (!string.IsNullOrEmpty(formule.PlatStandard2)) return formule.PlatStandard2;
                    return "Plat du jour";
            }
        }
    }
}
