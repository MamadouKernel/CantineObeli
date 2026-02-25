using Microsoft.EntityFrameworkCore;
using Obeli_K.Data;
using Obeli_K.Enums;
using Obeli_K.Models;

namespace Obeli_K.Services
{
    /// <summary>
    /// Service pour le changement automatique de statut des commandes
    /// - À 23h59, passe les commandes précommandées non récupérées à "NonRecuperer"
    /// </summary>
    public class ChangementStatutAutomatiqueService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ChangementStatutAutomatiqueService> _logger;
        private DateTime? _derniereExecution = null;

        public ChangementStatutAutomatiqueService(
            IServiceProvider serviceProvider,
            ILogger<ChangementStatutAutomatiqueService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🔄 Service de changement automatique de statut démarré");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await VerifierEtChangerStatut();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Erreur lors de la vérification du changement de statut automatique");
                }

                // Vérifier toutes les minutes pour détecter 23h59
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private async Task VerifierEtChangerStatut()
        {
            var maintenant = DateTime.Now;
            
            // Vérifier si on est à 23h59 (ou entre 23h59 et 00h00)
            // Et s'assurer qu'on n'a pas déjà exécuté cette tâche aujourd'hui
            if (maintenant.Hour == 23 && maintenant.Minute >= 59)
            {
                // Vérifier si on a déjà exécuté cette tâche aujourd'hui
                if (_derniereExecution.HasValue && _derniereExecution.Value.Date == maintenant.Date)
                {
                    return; // Déjà exécuté aujourd'hui
                }

                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ObeliDbContext>();

                try
                {
                    var aujourdhui = DateTime.Today;
                    
                    // Récupérer toutes les commandes précommandées d'aujourd'hui qui n'ont pas été consommées
                    var commandesNonRecuperees = await context.Commandes
                        .Where(c => c.Supprimer == 0
                            && c.StatusCommande == (int)StatutCommande.Precommander
                            && c.DateConsommation.HasValue
                            && c.DateConsommation.Value.Date == aujourdhui
                            && c.StatusCommande != (int)StatutCommande.Consommee
                            && c.StatusCommande != (int)StatutCommande.Annulee
                            && c.StatusCommande != (int)StatutCommande.Indisponible
                            && c.StatusCommande != (int)StatutCommande.NonRecuperer)
                        .ToListAsync();

                    if (!commandesNonRecuperees.Any())
                    {
                        _logger.LogDebug("✅ Aucune commande précommandée non récupérée trouvée pour aujourd'hui");
                        return;
                    }

                    _logger.LogInformation("📋 Trouvé {Count} commande(s) précommandée(s) non récupérée(s) à passer en 'NonRecuperer'", 
                        commandesNonRecuperees.Count);

                    int countModifiees = 0;
                    foreach (var commande in commandesNonRecuperees)
                    {
                        commande.StatusCommande = (int)StatutCommande.NonRecuperer;
                        commande.ModifiedOn = DateTime.UtcNow;
                        commande.ModifiedBy = "ChangementStatutAutomatiqueService";
                        countModifiees++;
                    }

                    if (countModifiees > 0)
                    {
                        await context.SaveChangesAsync();
                        _logger.LogInformation("✅ {Count} commande(s) passée(s) automatiquement au statut 'NonRecuperer'", countModifiees);
                    }

                    // Marquer que la tâche a été exécutée aujourd'hui
                    _derniereExecution = maintenant;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Erreur lors du changement automatique de statut");
                }
            }
            else if (maintenant.Hour == 0 && maintenant.Minute < 5)
            {
                // Réinitialiser le flag à minuit pour permettre l'exécution le lendemain
                if (_derniereExecution.HasValue && _derniereExecution.Value.Date < maintenant.Date)
                {
                    _derniereExecution = null;
                }
            }
        }
    }
}

