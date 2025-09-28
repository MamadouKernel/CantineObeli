using Microsoft.EntityFrameworkCore;
using Obeli_K.Data;
using Obeli_K.Models.ViewModels;

namespace Obeli_K.Services
{
    /// <summary>
    /// Service pour la programmation automatique des exports de reporting
    /// </summary>
    public class ReportingAutomatiqueService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ReportingAutomatiqueService> _logger;
        private readonly IConfiguration _configuration;

        public ReportingAutomatiqueService(
            IServiceProvider serviceProvider,
            ILogger<ReportingAutomatiqueService> logger,
            IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Service de reporting automatique démarré");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ExecuterExportsAutomatiques();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur lors de l'exécution des exports automatiques");
                }

                // Attendre jusqu'à la prochaine exécution (par défaut : tous les jours à 8h)
                var nextRun = GetNextRunTime();
                var delay = nextRun - DateTime.Now;
                
                if (delay > TimeSpan.Zero)
                {
                    _logger.LogInformation("Prochaine exécution programmée pour {NextRun}", nextRun);
                    await Task.Delay(delay, stoppingToken);
                }
                else
                {
                    // Si on a raté l'heure, attendre 1 heure
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
            }
        }

        private async Task ExecuterExportsAutomatiques()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ObeliDbContext>();

            try
            {
                // Vérifier si les exports automatiques sont activés
                var exportsActives = await GetConfigurationAsync(context, "ExportsAutomatiquesActives", "false");
                if (exportsActives != "true")
                {
                    _logger.LogInformation("Exports automatiques désactivés");
                    return;
                }

                // Récupérer la fréquence des exports
                var frequence = await GetConfigurationAsync(context, "FrequenceExports", "Quotidien");
                var heureExecution = await GetConfigurationAsync(context, "HeureExecutionExports", "08:00");

                // Vérifier si c'est le bon moment pour exécuter
                if (!DoitExecuterMaintenant(frequence, heureExecution))
                {
                    return;
                }

                _logger.LogInformation("Début des exports automatiques - Fréquence: {Frequence}, Heure: {Heure}", 
                    frequence, heureExecution);

                // Exécuter les exports selon la fréquence
                switch (frequence.ToLower())
                {
                    case "quotidien":
                        await ExecuterExportQuotidien(context);
                        break;
                    case "hebdomadaire":
                        await ExecuterExportHebdomadaire(context);
                        break;
                    case "mensuel":
                        await ExecuterExportMensuel(context);
                        break;
                    default:
                        _logger.LogWarning("Fréquence d'export non reconnue: {Frequence}", frequence);
                        break;
                }

                // Mettre à jour la dernière exécution
                await SetConfigurationAsync(context, "DerniereExecutionExport", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
                
                _logger.LogInformation("Exports automatiques terminés avec succès");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'exécution des exports automatiques");
                
                // Enregistrer l'erreur
                await SetConfigurationAsync(context, "DerniereErreurExport", 
                    $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - {ex.Message}");
            }
        }

        private async Task ExecuterExportQuotidien(ObeliDbContext context)
        {
            var hier = DateTime.Today.AddDays(-1);
            var aujourdhui = DateTime.Today;

            _logger.LogInformation("Exécution de l'export quotidien pour le {Date}", hier.ToString("dd/MM/yyyy"));

            // Générer l'export des commandes de la veille
            await GenererExportCommandes(context, hier, hier, "Quotidien");
        }

        private async Task ExecuterExportHebdomadaire(ObeliDbContext context)
        {
            // Semaine précédente (lundi à dimanche)
            var aujourdhui = DateTime.Today;
            var joursDepuisLundi = ((int)aujourdhui.DayOfWeek + 6) % 7; // Lundi = 0
            var lundiSemainePrecedente = aujourdhui.AddDays(-joursDepuisLundi - 7);
            var dimancheSemainePrecedente = lundiSemainePrecedente.AddDays(6);

            _logger.LogInformation("Exécution de l'export hebdomadaire pour la semaine du {Debut} au {Fin}", 
                lundiSemainePrecedente.ToString("dd/MM/yyyy"), dimancheSemainePrecedente.ToString("dd/MM/yyyy"));

            await GenererExportCommandes(context, lundiSemainePrecedente, dimancheSemainePrecedente, "Hebdomadaire");
        }

        private async Task ExecuterExportMensuel(ObeliDbContext context)
        {
            // Mois précédent
            var aujourdhui = DateTime.Today;
            var premierJourMoisPrecedent = new DateTime(aujourdhui.Year, aujourdhui.Month, 1).AddMonths(-1);
            var dernierJourMoisPrecedent = premierJourMoisPrecedent.AddMonths(1).AddDays(-1);

            _logger.LogInformation("Exécution de l'export mensuel pour le mois de {Mois}", 
                premierJourMoisPrecedent.ToString("MMMM yyyy"));

            await GenererExportCommandes(context, premierJourMoisPrecedent, dernierJourMoisPrecedent, "Mensuel");
        }

        private async Task GenererExportCommandes(ObeliDbContext context, DateTime dateDebut, DateTime dateFin, string typeExport)
        {
            try
            {
                // Récupérer les commandes de la période
                var commandes = await context.Commandes
                    .Include(c => c.Utilisateur)
                        .ThenInclude(u => u!.Departement)
                    .Include(c => c.Utilisateur)
                        .ThenInclude(u => u!.Fonction)
                    .Include(c => c.FormuleJour)
                        .ThenInclude(f => f!.NomFormuleNavigation)
                    .Where(c => c.DateConsommation.HasValue &&
                                c.DateConsommation.Value.Date >= dateDebut.Date &&
                                c.DateConsommation.Value.Date <= dateFin.Date &&
                                c.Supprimer == 0)
                    .ToListAsync();

                if (!commandes.Any())
                {
                    _logger.LogInformation("Aucune commande trouvée pour l'export {Type} du {Debut} au {Fin}", 
                        typeExport, dateDebut.ToString("dd/MM/yyyy"), dateFin.ToString("dd/MM/yyyy"));
                    return;
                }

                // Générer le fichier CSV
                var csvContent = GenererCsv(commandes);
                var fileName = $"Export_Automatique_{typeExport}_{dateDebut:yyyyMMdd}_{dateFin:yyyyMMdd}.csv";
                
                // Sauvegarder le fichier (dans un dossier d'exports automatiques)
                var exportPath = Path.Combine("wwwroot", "exports", "automatiques");
                Directory.CreateDirectory(exportPath);
                var filePath = Path.Combine(exportPath, fileName);
                
                await System.IO.File.WriteAllTextAsync(filePath, csvContent, System.Text.Encoding.UTF8);

                _logger.LogInformation("Export {Type} généré avec succès: {FileName} ({Count} commandes)", 
                    typeExport, fileName, commandes.Count);

                // Envoyer une notification (optionnel)
                await NotifierExportGenere(typeExport, fileName, commandes.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la génération de l'export {Type}", typeExport);
                throw;
            }
        }

        private string GenererCsv(List<Models.Commande> commandes)
        {
            var csv = new System.Text.StringBuilder();
            
            // En-têtes
            csv.AppendLine("Date Consommation,Code Commande,Utilisateur,Matricule,Département,Fonction,Site,Type Formule,Nom Plat,Quantité,Période,Statut");

            // Données
            foreach (var cmd in commandes)
            {
                csv.AppendLine($"{cmd.DateConsommation:dd/MM/yyyy HH:mm}," +
                              $"{cmd.CodeCommande}," +
                              $"\"{cmd.Utilisateur?.Nom} {cmd.Utilisateur?.Prenoms}\"," +
                              $"{cmd.Utilisateur?.UserName}," +
                              $"\"{cmd.Utilisateur?.Departement?.Nom}\"," +
                              $"\"{cmd.Utilisateur?.Fonction?.Nom}\"," +
                              $"{cmd.Utilisateur?.Site}," +
                              $"\"{cmd.FormuleJour?.NomFormuleNavigation?.Nom}\"," +
                              $"\"{GetNomPlatFromFormule(cmd.FormuleJour)}\"," +
                              $"{cmd.Quantite}," +
                              $"{cmd.PeriodeService}," +
                              $"{(Enums.StatutCommande)cmd.StatusCommande}");
            }

            return csv.ToString();
        }

        private string GetNomPlatFromFormule(Models.FormuleJour? formule)
        {
            if (formule == null) return "";

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

        private async Task NotifierExportGenere(string typeExport, string fileName, int nombreCommandes)
        {
            try
            {
                // Ici on pourrait envoyer une notification par email ou SignalR
                _logger.LogInformation("Notification: Export {Type} généré - {FileName} ({Count} commandes)", 
                    typeExport, fileName, nombreCommandes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'envoi de notification pour l'export {Type}", typeExport);
            }
        }

        private bool DoitExecuterMaintenant(string frequence, string heureExecution)
        {
            var maintenant = DateTime.Now;
            var heureCible = TimeSpan.Parse(heureExecution);

            switch (frequence.ToLower())
            {
                case "quotidien":
                    return maintenant.TimeOfDay >= heureCible && maintenant.TimeOfDay < heureCible.Add(TimeSpan.FromMinutes(5));
                
                case "hebdomadaire":
                    // Exécuter le lundi à l'heure spécifiée
                    return maintenant.DayOfWeek == DayOfWeek.Monday && 
                           maintenant.TimeOfDay >= heureCible && 
                           maintenant.TimeOfDay < heureCible.Add(TimeSpan.FromMinutes(5));
                
                case "mensuel":
                    // Exécuter le 1er du mois à l'heure spécifiée
                    return maintenant.Day == 1 && 
                           maintenant.TimeOfDay >= heureCible && 
                           maintenant.TimeOfDay < heureCible.Add(TimeSpan.FromMinutes(5));
                
                default:
                    return false;
            }
        }

        private DateTime GetNextRunTime()
        {
            var maintenant = DateTime.Now;
            var frequence = _configuration["Reporting:FrequenceExports"] ?? "Quotidien";
            var heureExecution = _configuration["Reporting:HeureExecutionExports"] ?? "08:00";
            var heureCible = TimeSpan.Parse(heureExecution);

            switch (frequence.ToLower())
            {
                case "quotidien":
                    var demain = maintenant.Date.AddDays(1).Add(heureCible);
                    return maintenant.TimeOfDay >= heureCible ? demain : maintenant.Date.Add(heureCible);
                
                case "hebdomadaire":
                    var prochainLundi = maintenant.Date.AddDays(((int)DayOfWeek.Monday - (int)maintenant.DayOfWeek + 7) % 7);
                    return prochainLundi.Add(heureCible);
                
                case "mensuel":
                    var prochainMois = new DateTime(maintenant.Year, maintenant.Month, 1).AddMonths(1);
                    return prochainMois.Add(heureCible);
                
                default:
                    return maintenant.AddHours(1);
            }
        }

        private async Task<string> GetConfigurationAsync(ObeliDbContext context, string cle, string valeurParDefaut)
        {
            var config = await context.ConfigurationsCommande
                .FirstOrDefaultAsync(c => c.Cle == cle && c.Supprimer == 0);
            
            return config?.Valeur ?? valeurParDefaut;
        }

        private async Task SetConfigurationAsync(ObeliDbContext context, string cle, string valeur)
        {
            var config = await context.ConfigurationsCommande
                .FirstOrDefaultAsync(c => c.Cle == cle && c.Supprimer == 0);

            if (config == null)
            {
                config = new Models.ConfigurationCommande
                {
                    Cle = cle,
                    Valeur = valeur,
                    Description = $"Configuration automatique pour {cle}",
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = "ReportingAutomatiqueService"
                };
                context.ConfigurationsCommande.Add(config);
            }
            else
            {
                config.Valeur = valeur;
                config.ModifiedOn = DateTime.UtcNow;
                config.ModifiedBy = "ReportingAutomatiqueService";
            }

            await context.SaveChangesAsync();
        }
    }
}
