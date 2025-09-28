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
                    existingConfig.Valeur = valeur;
                    existingConfig.Description = description ?? existingConfig.Description;
                    existingConfig.ModifiedOn = DateTime.UtcNow;
                    existingConfig.ModifiedBy = "System";
                }
                else
                {
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

                await _context.SaveChangesAsync();
                _logger.LogInformation("Configuration mise à jour: {Cle} = {Valeur}", cle, valeur);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour de la configuration {Cle}", cle);
                throw;
            }
        }

        public async Task<bool> IsCommandeBlockedAsync()
        {
            try
            {
                var jourCloture = await GetConfigurationAsync("COMMANDE_JOUR_CLOTURE");
                var heureCloture = await GetConfigurationAsync("COMMANDE_HEURE_CLOTURE");

                if (string.IsNullOrEmpty(jourCloture) || string.IsNullOrEmpty(heureCloture))
                {
                    // Valeurs par défaut si non configurées
                    jourCloture = "Friday";
                    heureCloture = "12:00";
                }

                var aujourdhui = DateTime.Now;
                var jourActuel = aujourdhui.DayOfWeek;
                var heureActuelle = aujourdhui.TimeOfDay;

                // Convertir le jour de clôture en enum
                var jourClotureEnum = Enum.Parse<DayOfWeek>(jourCloture);
                
                // Calculer le nombre de jours depuis le dernier jour de clôture
                var joursDepuisCloture = ((int)jourActuel - (int)jourClotureEnum + 7) % 7;
                
                // Si on est le jour de clôture, vérifier l'heure
                if (joursDepuisCloture == 0)
                {
                    if (TimeSpan.TryParse(heureCloture, out var heureClotureTime))
                    {
                        return heureActuelle >= heureClotureTime;
                    }
                }
                // Si on est après le jour de clôture (samedi, dimanche), bloquer
                else if (joursDepuisCloture > 0 && joursDepuisCloture <= 2) // Samedi=1, Dimanche=2
                {
                    return true;
                }

                return false;
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
    }
}
