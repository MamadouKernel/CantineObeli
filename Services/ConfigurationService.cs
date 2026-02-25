using Microsoft.EntityFrameworkCore;
using Obeli_K.Data;
using Obeli_K.Models;

namespace Obeli_K.Services.Configuration
{
    public interface IConfigurationService
    {
        Task<string?> GetConfigurationAsync(string cle);
        Task SetConfigurationAsync(string cle, string valeur, string? description = null);
        Task<bool> IsCommandeBlockedAsync();
        Task<DateTime> GetNextBlockingDateAsync();
        Task<DateTime> GetNextConfirmationDateAsync();
        Task<bool> ShouldAutoConfirmCommandsAsync();
        Task InitializeBillingConfigurationsAsync();
    }

    public class ConfigurationService : IConfigurationService
    {
        private readonly ObeliDbContext _context;
        private readonly ILogger<ConfigurationService> _logger;

        public ConfigurationService(ObeliDbContext context, ILogger<ConfigurationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<string?> GetConfigurationAsync(string cle)
        {
            try
            {
                var config = await _context.ConfigurationsCommande
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Cle == cle && c.Supprimer == 0);
                
                return config?.Valeur;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de la configuration {Cle}", cle);
                return null;
            }
        }

        public async Task SetConfigurationAsync(string cle, string valeur, string? description = null)
        {
            try
            {
                var existingConfig = await _context.ConfigurationsCommande
                    .FirstOrDefaultAsync(c => c.Cle == cle && c.Supprimer == 0);

                if (existingConfig != null)
                {
                    _logger.LogInformation("🔧 Configuration existante trouvée: {Cle} = {AncienneValeur}", cle, existingConfig.Valeur);
                    
                    // Marquer comme modifié AVANT de changer les propriétés
                    _context.Entry(existingConfig).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    
                    existingConfig.Valeur = valeur;
                    existingConfig.Description = description ?? existingConfig.Description;
                    existingConfig.ModifiedOn = DateTime.UtcNow;
                    existingConfig.ModifiedBy = "System";
                    
                    _logger.LogInformation("🔄 Configuration modifiée: {Cle} = {NouvelleValeur}", cle, valeur);
                }
                else
                {
                    _logger.LogInformation("🆕 Nouvelle configuration créée: {Cle} = {Valeur}", cle, valeur);
                    var newConfig = new ConfigurationCommande
                    {
                        Id = Guid.NewGuid(),
                        Cle = cle,
                        Valeur = valeur,
                        Description = description,
                        CreatedOn = DateTime.UtcNow,
                        CreatedBy = "System",
                        Supprimer = 0
                    };
                    _context.ConfigurationsCommande.Add(newConfig);
                }

                var result = await _context.SaveChangesAsync();
                _logger.LogInformation("✅ Configuration sauvegardée avec succès: {Cle} = {Valeur} (Changements: {Changements})", cle, valeur, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors de la mise à jour de la configuration {Cle}", cle);
                throw;
            }
        }

        public async Task<bool> IsCommandeBlockedAsync()
        {
            try
            {
                // TEMPORAIRE : Désactiver complètement le blocage pour permettre les tests
                // TODO: Réactiver la logique de blocage une fois le système stabilisé
                _logger.LogInformation("Blocage des commandes temporairement désactivé pour les tests");
                return false; // Toujours permettre les commandes
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la vérification du blocage des commandes");
                return false; // En cas d'erreur, ne pas bloquer
            }
        }

        public async Task<DateTime> GetNextBlockingDateAsync()
        {
            try
            {
                var jourCloture = await GetConfigurationAsync("COMMANDE_JOUR_CLOTURE");
                var heureCloture = await GetConfigurationAsync("COMMANDE_HEURE_CLOTURE");

                if (string.IsNullOrEmpty(jourCloture) || string.IsNullOrEmpty(heureCloture))
                {
                    jourCloture = "Friday";
                    heureCloture = "12:00";
                }

                var aujourdhui = DateTime.Now;
                var joursSemaine = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
                var indexJourCloture = Array.IndexOf(joursSemaine, jourCloture);
                var indexAujourdhui = (int)aujourdhui.DayOfWeek;

                // Calculer le nombre de jours jusqu'au prochain jour de clôture
                var joursJusquCloture = (indexJourCloture - indexAujourdhui + 7) % 7;
                if (joursJusquCloture == 0 && TimeSpan.TryParse(heureCloture, out var heureClotureTime))
                {
                    // Si c'est le jour de clôture, vérifier l'heure
                    if (aujourdhui.TimeOfDay < heureClotureTime)
                    {
                        joursJusquCloture = 0; // Aujourd'hui
                    }
                    else
                    {
                        joursJusquCloture = 7; // Semaine prochaine
                    }
                }

                var prochaineCloture = aujourdhui.AddDays(joursJusquCloture).Date;
                if (TimeSpan.TryParse(heureCloture, out var heure))
                {
                    prochaineCloture = prochaineCloture.Add(heure);
                }

                return prochaineCloture;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du calcul de la prochaine date de clôture");
                // Valeur par défaut : vendredi prochain à 12h
                var aujourdhui = DateTime.Now;
                var joursJusquVendredi = ((int)DayOfWeek.Friday - (int)aujourdhui.DayOfWeek + 7) % 7;
                if (joursJusquVendredi == 0 && aujourdhui.TimeOfDay >= new TimeSpan(12, 0, 0))
                {
                    joursJusquVendredi = 7;
                }
                return aujourdhui.AddDays(joursJusquVendredi).Date.AddHours(12);
            }
        }

        public async Task<DateTime> GetNextConfirmationDateAsync()
        {
            // La confirmation se fait le même jour que le blocage
            return await GetNextBlockingDateAsync();
        }

        public async Task<bool> ShouldAutoConfirmCommandsAsync()
        {
            try
            {
                var autoConfirm = await GetConfigurationAsync("COMMANDE_AUTO_CONFIRMATION");
                return !string.IsNullOrEmpty(autoConfirm) && autoConfirm.ToLower() == "true";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la vérification de l'auto-confirmation");
                return false; // Par défaut, pas d'auto-confirmation
            }
        }

        public async Task InitializeBillingConfigurationsAsync()
        {
            try
            {
                _logger.LogInformation("🚀 Initialisation des configurations de facturation...");

                // Liste des configurations de facturation avec leurs valeurs par défaut
                var billingConfigs = new[]
                {
                    new { Cle = "FACTURATION_NON_CONSOMMEES_ACTIVE", Valeur = "false", Description = "Active ou désactive la facturation des commandes non consommées" },
                    new { Cle = "FACTURATION_POURCENTAGE", Valeur = "100", Description = "Pourcentage du prix de la commande à facturer (0-100%)" },
                    new { Cle = "FACTURATION_ABSENCES_GRATUITES", Valeur = "0", Description = "Nombre d'absences non consommées gratuites par mois" },
                    new { Cle = "FACTURATION_DELAI_ANNULATION_GRATUITE", Valeur = "24", Description = "Délai en heures avant la consommation pour annuler gratuitement" },
                    new { Cle = "FACTURATION_WEEKEND", Valeur = "false", Description = "Facturer les commandes non consommées le weekend" },
                    new { Cle = "FACTURATION_JOURS_FERIES", Valeur = "false", Description = "Facturer les commandes non consommées les jours fériés" }
                };

                foreach (var config in billingConfigs)
                {
                    var existingConfig = await _context.ConfigurationsCommande
                        .FirstOrDefaultAsync(c => c.Cle == config.Cle && c.Supprimer == 0);

                    if (existingConfig == null)
                    {
                        _logger.LogInformation("➕ Création de la configuration: {Cle} = {Valeur}", config.Cle, config.Valeur);
                        var newConfig = new ConfigurationCommande
                        {
                            Id = Guid.NewGuid(),
                            Cle = config.Cle,
                            Valeur = config.Valeur,
                            Description = config.Description,
                            CreatedOn = DateTime.UtcNow,
                            CreatedBy = "System",
                            Supprimer = 0
                        };
                        _context.ConfigurationsCommande.Add(newConfig);
                    }
                    else
                    {
                        _logger.LogInformation("✅ Configuration déjà existante: {Cle} = {Valeur}", config.Cle, existingConfig.Valeur);
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("✅ Initialisation des configurations de facturation terminée");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors de l'initialisation des configurations de facturation");
                throw;
            }
        }
    }
}
