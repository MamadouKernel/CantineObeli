using Microsoft.EntityFrameworkCore;
using Obeli_K.Data;
using Obeli_K.Enums;
using Obeli_K.Models;

namespace Obeli_K.Services
{
    /// <summary>
    /// Service pour la fermeture automatique des commandes de la semaine N+1
    /// S'exécute le vendredi à 12h pour fermer automatiquement les commandes
    /// </summary>
    public class FermetureAutomatiqueService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<FermetureAutomatiqueService> _logger;

        public FermetureAutomatiqueService(
            IServiceProvider serviceProvider,
            ILogger<FermetureAutomatiqueService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🚀 Service de fermeture automatique démarré");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await VerifierEtExecuterFermeture();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Erreur lors de la vérification de fermeture automatique");
                }

                // Vérifier toutes les 5 minutes
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        private async Task VerifierEtExecuterFermeture()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ObeliDbContext>();
            var configurationService = scope.ServiceProvider.GetRequiredService<Obeli_K.Services.Configuration.IConfigurationService>();

            try
            {
                // Vérifier si c'est le moment de fermer les commandes
                var isBlockingTime = await configurationService.IsCommandeBlockedAsync();
                
                if (!isBlockingTime)
                {
                    return; // Pas encore l'heure
                }

                // Vérifier si la fermeture a déjà été effectuée aujourd'hui
                var aujourdhui = DateTime.Today;
                var fermetureDejaEffectuee = await context.ConfigurationsCommande
                    .AnyAsync(c => c.Cle == $"FERMETURE_EFFECTUEE_{aujourdhui:yyyyMMdd}" && c.Supprimer == 0);

                if (fermetureDejaEffectuee)
                {
                    _logger.LogInformation("✅ Fermeture déjà effectuée aujourd'hui ({Date})", aujourdhui.ToString("dd/MM/yyyy"));
                    return;
                }

                _logger.LogInformation("🔒 Début de la fermeture automatique des commandes pour la semaine N+1");

                // Calculer la semaine N+1
                var (lundiN1, vendrediN1) = GetSemaineSuivanteOuvree();

                // 1. Marquer les commandes précommandées comme confirmées (prêtes à être consommées)
                // Note: On ne les passe PAS en statut Consommee, on les laisse en Precommander
                // Elles seront marquées Consommee quand l'utilisateur scanne au point de consommation
                // Si elles ne sont pas scannées, elles restent en Precommander et seront facturées
                var commandesAConfirmer = await context.Commandes
                    .Where(c => c.DateConsommation.HasValue &&
                               c.DateConsommation.Value.Date >= lundiN1 &&
                               c.DateConsommation.Value.Date <= vendrediN1 &&
                               c.StatusCommande == (int)StatutCommande.Precommander &&
                               c.Supprimer == 0)
                    .ToListAsync();

                var commandesConfirmees = commandesAConfirmer.Count;
                
                _logger.LogInformation("✅ {Count} commandes de la semaine N+1 confirmées (restent en Precommander jusqu'à consommation)", commandesConfirmees);

                // Les commandes restent en statut Precommander
                // Elles seront marquées Consommee au point de consommation
                // Sinon, elles seront facturées comme non consommées

                await context.SaveChangesAsync();

                // 3. Enregistrer que la fermeture a été effectuée
                await EnregistrerFermetureEffectuee(context, aujourdhui);

                _logger.LogInformation("✅ Fermeture automatique terminée:");
                _logger.LogInformation("   📊 Commandes confirmées: {Confirmees} (restent en Precommander)", commandesConfirmees);
                _logger.LogInformation("   📅 Semaine N+1: {Lundi} au {Vendredi}", 
                    lundiN1.ToString("dd/MM/yyyy"), vendrediN1.ToString("dd/MM/yyyy"));
                _logger.LogInformation("   ℹ️ Les commandes seront marquées Consommee au point de consommation");
                _logger.LogInformation("   💰 Les commandes non consommées seront facturées si la facturation est activée");

                // 4. Envoyer une notification (optionnel)
                NotifierFermetureEffectuee(commandesConfirmees, 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors de la fermeture automatique");
                
                // Enregistrer l'erreur
                await EnregistrerErreurFermeture(context, ex);
            }
        }

        private async Task CreerPointConsommationAsync(ObeliDbContext context, Commande commande)
        {
            try
            {
                // Vérifier si un point de consommation existe déjà
                var pointExistant = await context.PointsConsommation
                    .AsNoTracking()
                    .AnyAsync(pc => pc.CommandeId == commande.IdCommande && pc.Supprimer == 0);

                if (pointExistant)
                {
                    return;
                }

                // Récupérer la formule
                var formule = await context.FormulesJour
                    .AsNoTracking()
                    .Include(f => f.NomFormuleNavigation)
                    .FirstOrDefaultAsync(f => f.IdFormule == commande.IdFormule);

                if (formule == null)
                {
                    _logger.LogWarning("⚠️ Formule non trouvée pour la commande {CommandeId}", commande.IdCommande);
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
                    CreatedBy = "FermetureAutomatiqueService",
                    Supprimer = 0
                };

                context.PointsConsommation.Add(pointConsommation);
                _logger.LogInformation("🍽️ Point de consommation créé pour la commande {CommandeId}: {NomPlat}", 
                    commande.IdCommande, nomPlat);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors de la création du point de consommation pour la commande {CommandeId}", 
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

        private (DateTime Lundi, DateTime Vendredi) GetSemaineSuivanteOuvree()
        {
            var today = DateTime.Today;
            int diffToMonday = ((int)today.DayOfWeek + 6) % 7; // Lundi=0
            var thisWeekMonday = today.AddDays(-diffToMonday).Date;

            var nextWeekMonday = thisWeekMonday.AddDays(7);
            var nextWeekFriday = nextWeekMonday.AddDays(4);
            return (nextWeekMonday, nextWeekFriday);
        }

        private async Task EnregistrerFermetureEffectuee(ObeliDbContext context, DateTime date)
        {
            var config = new ConfigurationCommande
            {
                Cle = $"FERMETURE_EFFECTUEE_{date:yyyyMMdd}",
                Valeur = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                Description = $"Fermeture automatique effectuée le {date:dd/MM/yyyy}",
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "FermetureAutomatiqueService",
                Supprimer = 0
            };

            context.ConfigurationsCommande.Add(config);
            await context.SaveChangesAsync();
        }

        private async Task EnregistrerErreurFermeture(ObeliDbContext context, Exception ex)
        {
            var config = new ConfigurationCommande
            {
                Cle = $"FERMETURE_ERREUR_{DateTime.Today:yyyyMMdd}",
                Valeur = $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - {ex.Message}",
                Description = $"Erreur lors de la fermeture automatique le {DateTime.Today:dd/MM/yyyy}",
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "FermetureAutomatiqueService",
                Supprimer = 0
            };

            context.ConfigurationsCommande.Add(config);
            await context.SaveChangesAsync();
        }

        private void NotifierFermetureEffectuee(int commandesConfirmees, int commandesAnnulees)
        {
            try
            {
                _logger.LogInformation("📧 Notification: Fermeture automatique effectuée");
                _logger.LogInformation("   ✅ Commandes confirmées: {Confirmees}", commandesConfirmees);
                _logger.LogInformation("   ❌ Commandes annulées: {Annulees}", commandesAnnulees);
                
                // Ici on pourrait envoyer un email ou une notification SignalR
                // Pour l'instant, on se contente de logger
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors de l'envoi de notification");
            }
            
        }
    }
}
