using Microsoft.EntityFrameworkCore;
using Obeli_K.Data;
using Obeli_K.Models;
using Obeli_K.Models.ViewModels;
using Obeli_K.Services.Configuration;

namespace Obeli_K.Services
{
    /// <summary>
    /// Service pour la facturation automatique des commandes non consommées
    /// S'exécute quotidiennement pour facturer les commandes non récupérées
    /// </summary>
    public class FacturationAutomatiqueService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<FacturationAutomatiqueService> _logger;

        public FacturationAutomatiqueService(
            IServiceProvider serviceProvider,
            ILogger<FacturationAutomatiqueService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("💰 Service de facturation automatique démarré");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await VerifierEtExecuterFacturation();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Erreur lors de la vérification de facturation automatique");
                }

                // Vérifier toutes les heures
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        private async Task VerifierEtExecuterFacturation()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ObeliDbContext>();
            var facturationService = scope.ServiceProvider.GetRequiredService<IFacturationService>();
            var configurationService = scope.ServiceProvider.GetRequiredService<IConfigurationService>();

            try
            {
                // Vérifier si la facturation est activée
                var facturationActive = await configurationService.GetConfigurationAsync("FACTURATION_NON_CONSOMMEES_ACTIVE");
                var isActive = !string.IsNullOrEmpty(facturationActive) && facturationActive.ToLower() == "true";

                if (!isActive)
                {
                    _logger.LogDebug("⚠️ Facturation automatique désactivée");
                    return;
                }

                // Vérifier si la facturation a déjà été effectuée aujourd'hui
                var aujourdhui = DateTime.Today;
                var facturationDejaEffectuee = await context.ConfigurationsCommande
                    .AnyAsync(c => c.Cle == $"FACTURATION_EFFECTUEE_{aujourdhui:yyyyMMdd}" && c.Supprimer == 0);

                if (facturationDejaEffectuee)
                {
                    _logger.LogDebug("✅ Facturation déjà effectuée aujourd'hui ({Date})", aujourdhui.ToString("dd/MM/yyyy"));
                    return;
                }

                // Vérifier s'il y a des commandes à facturer (commandes de la veille ou plus anciennes)
                var dateLimite = aujourdhui.AddDays(-1); // Commandes d'hier ou plus anciennes
                
                _logger.LogInformation("💰 Début de la facturation automatique pour les commandes non consommées");
                _logger.LogInformation("📅 Recherche des commandes non consommées depuis le {Date}", dateLimite.ToString("dd/MM/yyyy"));

                // Récupérer les commandes non consommées
                var commandesNonConsommees = await facturationService.GetCommandesNonConsommeesAsync(dateLimite, null);

                if (!commandesNonConsommees.Any())
                {
                    _logger.LogInformation("✅ Aucune commande non consommée trouvée");
                    await EnregistrerFacturationEffectuee(context, aujourdhui, 0, 0, 0);
                    return;
                }

                _logger.LogInformation("📊 Trouvé {Count} commandes non consommées à traiter", commandesNonConsommees.Count);

                // Calculer la facturation
                var resultatFacturation = await facturationService.CalculerFacturationAsync(commandesNonConsommees);

                if (!resultatFacturation.FacturationActive)
                {
                    _logger.LogInformation("⚠️ Facturation désactivée dans les paramètres");
                    await EnregistrerFacturationEffectuee(context, aujourdhui, 0, 0, 0);
                    return;
                }

                _logger.LogInformation("💳 Résultat du calcul de facturation:");
                _logger.LogInformation("   📊 Commandes facturables: {Facturables}", resultatFacturation.NombreCommandesFacturables);
                _logger.LogInformation("   🆓 Commandes non facturables: {NonFacturables}", resultatFacturation.NombreCommandesNonFacturables);
                _logger.LogInformation("   💰 Montant total à facturer: {Montant:C}", resultatFacturation.MontantTotalAFacturer);

                // Appliquer la facturation
                var facturationAppliquee = await facturationService.AppliquerFacturationAsync(commandesNonConsommees, resultatFacturation);

                if (facturationAppliquee)
                {
                    _logger.LogInformation("✅ Facturation automatique appliquée avec succès");
                    
                    // Enregistrer que la facturation a été effectuée
                    await EnregistrerFacturationEffectuee(
                        context, 
                        aujourdhui, 
                        resultatFacturation.NombreCommandesFacturables,
                        resultatFacturation.NombreCommandesNonFacturables,
                        resultatFacturation.MontantTotalAFacturer);

                    // Envoyer une notification
                    NotifierFacturationEffectuee(resultatFacturation);
                }
                else
                {
                    _logger.LogError("❌ Échec de l'application de la facturation");
                    await EnregistrerErreurFacturation(context, new Exception("Échec de l'application de la facturation"));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors de la facturation automatique");
                await EnregistrerErreurFacturation(context, ex);
            }
        }

        private async Task EnregistrerFacturationEffectuee(
            ObeliDbContext context, 
            DateTime date, 
            int commandesFacturables, 
            int commandesNonFacturables, 
            decimal montantTotal)
        {
            try
            {
                var config = new ConfigurationCommande
                {
                    Cle = $"FACTURATION_EFFECTUEE_{date:yyyyMMdd}",
                    Valeur = $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}|{commandesFacturables}|{commandesNonFacturables}|{montantTotal:F2}",
                    Description = $"Facturation automatique effectuée le {date:dd/MM/yyyy} - {commandesFacturables} facturées, {commandesNonFacturables} exemptées, {montantTotal:C}",
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = "FacturationAutomatiqueService",
                    Supprimer = 0
                };

                context.ConfigurationsCommande.Add(config);
                await context.SaveChangesAsync();

                _logger.LogInformation("📝 Facturation enregistrée: {Date} - {Facturables} facturées, {Exemptees} exemptées, {Montant:C}",
                    date.ToString("dd/MM/yyyy"), commandesFacturables, commandesNonFacturables, montantTotal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors de l'enregistrement de la facturation");
            }
        }

        private async Task EnregistrerErreurFacturation(ObeliDbContext context, Exception ex)
        {
            try
            {
                var config = new ConfigurationCommande
                {
                    Cle = $"FACTURATION_ERREUR_{DateTime.Today:yyyyMMdd}",
                    Valeur = $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - {ex.Message}",
                    Description = $"Erreur lors de la facturation automatique le {DateTime.Today:dd/MM/yyyy}",
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = "FacturationAutomatiqueService",
                    Supprimer = 0
                };

                context.ConfigurationsCommande.Add(config);
                await context.SaveChangesAsync();
            }
            catch (Exception innerEx)
            {
                _logger.LogError(innerEx, "❌ Erreur lors de l'enregistrement de l'erreur de facturation");
            }
        }

        private void NotifierFacturationEffectuee(FacturationResult resultat)
        {
            try
            {
                _logger.LogInformation("📧 Notification: Facturation automatique effectuée");
                _logger.LogInformation("   💰 Commandes facturées: {Facturables}", resultat.NombreCommandesFacturables);
                _logger.LogInformation("   🆓 Commandes exemptées: {Exemptees}", resultat.NombreCommandesNonFacturables);
                _logger.LogInformation("   💵 Montant total: {Montant:C}", resultat.MontantTotalAFacturer);
                
                // Ici on pourrait envoyer un email ou une notification SignalR
                // Pour l'instant, on se contente de logger
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors de l'envoi de notification de facturation");
            }
        }
    }
}
