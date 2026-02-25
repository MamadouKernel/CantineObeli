using Microsoft.EntityFrameworkCore;
using Obeli_K.Data;
using Obeli_K.Models;
using Obeli_K.Enums;
using Obeli_K.Models.ViewModels;
using Obeli_K.Services.Configuration;

namespace Obeli_K.Services
{
    public interface IFacturationService
    {
        Task<List<CommandeNonConsommeeViewModel>> GetCommandesNonConsommeesAsync(DateTime? dateDebut = null, DateTime? dateFin = null);
        Task<FacturationResult> CalculerFacturationAsync(List<CommandeNonConsommeeViewModel> commandes);
        Task<bool> AppliquerFacturationAsync(List<CommandeNonConsommeeViewModel> commandes, FacturationResult resultat);
        Task<bool> EstWeekendAsync(DateTime date);
        Task<bool> EstJourFerieAsync(DateTime date);
    }

    public class FacturationService : IFacturationService
    {
        private readonly ObeliDbContext _context;
        private readonly IConfigurationService _configService;
        private readonly ILogger<FacturationService> _logger;

        public FacturationService(
            ObeliDbContext context,
            IConfigurationService configService,
            ILogger<FacturationService> logger)
        {
            _context = context;
            _configService = configService;
            _logger = logger;
        }

        public async Task<List<CommandeNonConsommeeViewModel>> GetCommandesNonConsommeesAsync(DateTime? dateDebut = null, DateTime? dateFin = null)
        {
            try
            {
                _logger.LogInformation("🔍 Recherche des commandes non consommées...");

                // Récupérer les commandes non réellement consommées (pas de point de consommation)
                // Inclut les commandes Précommandées ET les commandes avec statut "Consommée" mais pas validées par prestataire
                var query = _context.Commandes
                    .Include(c => c.FormuleJour)
                    .Include(c => c.Utilisateur)
                    .Where(c => c.Supprimer == 0 
                               && (c.StatusCommande == (int)StatutCommande.Precommander || c.StatusCommande == (int)StatutCommande.Consommee)
                               && c.DateConsommation.HasValue
                               && c.UtilisateurId.HasValue
                               && c.TypeClient == (int)TypeClientCommande.CitUtilisateur); // Seulement les commandes CIT

                if (dateDebut.HasValue)
                {
                    query = query.Where(c => c.DateConsommation.Value.Date >= dateDebut.Value.Date);
                }

                if (dateFin.HasValue)
                {
                    query = query.Where(c => c.DateConsommation.Value.Date <= dateFin.Value.Date);
                }

                var commandes = await query.ToListAsync();

                var result = new List<CommandeNonConsommeeViewModel>();

                foreach (var commande in commandes)
                {
                    // Vérifier si la commande a un point de consommation (réellement validée par prestataire)
                    var pointConsommation = await _context.PointsConsommation
                        .FirstOrDefaultAsync(pc => pc.CommandeId == commande.IdCommande && pc.Supprimer == 0);
                    
                    // Si la commande a un point de consommation, elle a été réellement validée - l'exclure
                    if (pointConsommation != null)
                    {
                        _logger.LogDebug("✅ Commande {CodeCommande} exclue - Déjà validée par prestataire (point de consommation existant)", 
                            commande.CodeCommande);
                        continue;
                    }
                    
                    // Vérifier que les relations nécessaires sont chargées
                    if (commande.Utilisateur == null || commande.FormuleJour == null)
                    {
                        _logger.LogWarning("⚠️ Commande {IdCommande} ignorée - Utilisateur ou FormuleJour null", commande.IdCommande);
                        continue;
                    }

                    var dateConsommation = commande.DateConsommation.Value;
                    var maintenant = DateTime.Now;

                    // Vérifier si la commande est passée (date de consommation dépassée)
                    if (dateConsommation.Date < maintenant.Date)
                    {
                        var commandeViewModel = new CommandeNonConsommeeViewModel
                        {
                            IdCommande = commande.IdCommande,
                            CodeCommande = commande.CodeCommande ?? "",
                            DateCommande = commande.Date,
                            DateConsommation = dateConsommation,
                            NomUtilisateur = $"{commande.Utilisateur.Nom} {commande.Utilisateur.Prenoms}",
                            EmailUtilisateur = commande.Utilisateur.Email ?? "",
                            NomFormule = commande.FormuleJour.NomFormule ?? "",
                            Plat = commande.FormuleJour.Plat ?? "",
                            Montant = commande.Montant,
                            StatusCommande = (StatutCommande)commande.StatusCommande,
                            TypeClient = (TypeClientCommande)commande.TypeClient,
                            EstWeekend = await EstWeekendAsync(dateConsommation),
                            EstJourFerie = await EstJourFerieAsync(dateConsommation),
                            NombreJoursRetard = (maintenant.Date - dateConsommation.Date).Days
                        };

                        result.Add(commandeViewModel);
                    }
                }

                _logger.LogInformation("✅ Trouvé {Count} commandes non consommées", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors de la recherche des commandes non consommées");
                return new List<CommandeNonConsommeeViewModel>();
            }
        }

        public async Task<FacturationResult> CalculerFacturationAsync(List<CommandeNonConsommeeViewModel> commandes)
        {
            try
            {
                _logger.LogInformation("💰 Calcul de la facturation pour {Count} commandes...", commandes.Count);

                // Récupérer les paramètres de facturation
                var facturationActive = await _configService.GetConfigurationAsync("FACTURATION_NON_CONSOMMEES_ACTIVE");
                var pourcentageFacturation = await _configService.GetConfigurationAsync("FACTURATION_POURCENTAGE");
                var nombreAbsencesGratuites = await _configService.GetConfigurationAsync("FACTURATION_ABSENCES_GRATUITES");
                var delaiAnnulationGratuite = await _configService.GetConfigurationAsync("FACTURATION_DELAI_ANNULATION_GRATUITE");
                var facturationWeekend = await _configService.GetConfigurationAsync("FACTURATION_WEEKEND");
                var facturationJoursFeries = await _configService.GetConfigurationAsync("FACTURATION_JOURS_FERIES");

                var isActive = !string.IsNullOrEmpty(facturationActive) && facturationActive.ToLower() == "true";
                var pourcentage = !string.IsNullOrEmpty(pourcentageFacturation) ? int.Parse(pourcentageFacturation) : 100;
                var absencesGratuites = !string.IsNullOrEmpty(nombreAbsencesGratuites) ? int.Parse(nombreAbsencesGratuites) : 0;
                var delaiGratuit = !string.IsNullOrEmpty(delaiAnnulationGratuite) ? int.Parse(delaiAnnulationGratuite) : 24;
                var facturerWeekend = !string.IsNullOrEmpty(facturationWeekend) && facturationWeekend.ToLower() == "true";
                var facturerJoursFeries = !string.IsNullOrEmpty(facturationJoursFeries) && facturationJoursFeries.ToLower() == "true";

                _logger.LogInformation("📊 Paramètres: Active={Active}, Pourcentage={Pourcentage}%, Absences gratuites={Absences}, Délai gratuit={Delai}h, Weekend={Weekend}, Jours fériés={Feries}",
                    isActive, pourcentage, absencesGratuites, delaiGratuit, facturerWeekend, facturerJoursFeries);

                var resultat = new FacturationResult
                {
                    FacturationActive = isActive,
                    PourcentageFacturation = pourcentage,
                    NombreAbsencesGratuites = absencesGratuites,
                    FacturationWeekend = facturerWeekend,
                    FacturationJoursFeries = facturerJoursFeries,
                    CommandesFacturables = new List<CommandeFacturable>(),
                    CommandesNonFacturables = new List<CommandeNonFacturable>()
                };

                if (!isActive)
                {
                    _logger.LogInformation("⚠️ Facturation désactivée - Toutes les commandes sont non facturables");
                    foreach (var commande in commandes)
                    {
                        resultat.CommandesNonFacturables.Add(new CommandeNonFacturable
                        {
                            Commande = commande,
                            Motif = "Facturation désactivée"
                        });
                    }
                    return resultat;
                }

                // Grouper les commandes par utilisateur pour gérer les absences gratuites
                var commandesParUtilisateur = commandes.GroupBy(c => c.EmailUtilisateur).ToList();

                foreach (var groupeUtilisateur in commandesParUtilisateur)
                {
                    var commandesUtilisateur = groupeUtilisateur.OrderBy(c => c.DateConsommation).ToList();
                    var absencesUtilisees = 0;

                    foreach (var commande in commandesUtilisateur)
                    {
                        // Vérifier si la commande doit être facturée selon les règles
                        bool doitEtreFacturee = true;
                        string motifNonFacturation = "";

                        // Règle 1: Week-end
                        if (commande.EstWeekend && !facturerWeekend)
                        {
                            doitEtreFacturee = false;
                            motifNonFacturation = "Week-end non facturé";
                        }
                        // Règle 2: Jours fériés
                        else if (commande.EstJourFerie && !facturerJoursFeries)
                        {
                            doitEtreFacturee = false;
                            motifNonFacturation = "Jour férié non facturé";
                        }
                        // Règle 3: Absences gratuites
                        else if (absencesUtilisees < absencesGratuites)
                        {
                            doitEtreFacturee = false;
                            motifNonFacturation = $"Absence gratuite ({absencesUtilisees + 1}/{absencesGratuites})";
                            absencesUtilisees++;
                        }

                        if (doitEtreFacturee)
                        {
                            var montantAFacturer = (commande.Montant * pourcentage) / 100;
                            resultat.CommandesFacturables.Add(new CommandeFacturable
                            {
                                Commande = commande,
                                MontantAFacturer = montantAFacturer,
                                MontantOriginal = commande.Montant,
                                PourcentageApplique = pourcentage
                            });
                        }
                        else
                        {
                            resultat.CommandesNonFacturables.Add(new CommandeNonFacturable
                            {
                                Commande = commande,
                                Motif = motifNonFacturation
                            });
                        }
                    }
                }

                resultat.MontantTotalAFacturer = resultat.CommandesFacturables.Sum(c => c.MontantAFacturer);
                resultat.NombreCommandesFacturables = resultat.CommandesFacturables.Count;
                resultat.NombreCommandesNonFacturables = resultat.CommandesNonFacturables.Count;

                _logger.LogInformation("✅ Calcul terminé: {Facturables} facturables, {NonFacturables} non facturables, Total: {Total:C}",
                    resultat.NombreCommandesFacturables, resultat.NombreCommandesNonFacturables, resultat.MontantTotalAFacturer);

                return resultat;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors du calcul de la facturation");
                throw;
            }
        }

        public async Task<bool> AppliquerFacturationAsync(List<CommandeNonConsommeeViewModel> commandes, FacturationResult resultat)
        {
            try
            {
                _logger.LogInformation("💾 Application de la facturation...");

                // Marquer les commandes comme facturées dans la base de données
                foreach (var commandeFacturable in resultat.CommandesFacturables)
                {
                    var commande = await _context.Commandes
                        .FirstOrDefaultAsync(c => c.IdCommande == commandeFacturable.Commande.IdCommande);

                    if (commande != null)
                    {
                        // Créer un point de consommation pour la facturation
                        var pointConsommation = new PointConsommation
                        {
                            IdPointConsommation = Guid.NewGuid(),
                            UtilisateurId = commande.UtilisateurId ?? Guid.Empty,
                            CommandeId = commande.IdCommande,
                            DateConsommation = commande.DateConsommation ?? DateTime.Today,
                            TypeFormule = commande.FormuleJour?.NomFormule ?? "NON RÉCUPÉRÉE",
                            NomPlat = commande.FormuleJour?.Plat ?? "Commande non récupérée",
                            QuantiteConsommee = commande.Quantite,
                            LieuConsommation = $"FACTURATION - NON RÉCUPÉRÉE ({commandeFacturable.MontantAFacturer:C})",
                            CreatedOn = DateTime.UtcNow,
                            CreatedBy = "Système de Facturation",
                            Supprimer = 0
                        };

                        _context.PointsConsommation.Add(pointConsommation);

                        // NE PAS changer le statut de la commande - elle reste "Précommandée" car pas physiquement récupérée
                        // Le statut "Précommandée" (0) est maintenu pour indiquer que la commande n'a pas été récupérée
                        commande.ModifiedOn = DateTime.UtcNow;
                        commande.ModifiedBy = "Système de Facturation";

                        _logger.LogInformation("💰 Facturation appliquée: {CodeCommande} - {Utilisateur} - {Montant:C}",
                            commandeFacturable.Commande.CodeCommande,
                            commandeFacturable.Commande.NomUtilisateur,
                            commandeFacturable.MontantAFacturer);
                    }
                }

                // Marquer les commandes non facturables comme exemptées
                foreach (var commandeNonFacturable in resultat.CommandesNonFacturables)
                {
                    var commande = await _context.Commandes
                        .FirstOrDefaultAsync(c => c.IdCommande == commandeNonFacturable.Commande.IdCommande);

                    if (commande != null)
                    {
                        // NE PAS changer le statut de la commande - elle reste "Précommandée" même si exemptée
                        // Le statut "Précommandée" (0) est maintenu pour indiquer que la commande n'a pas été récupérée
                        commande.ModifiedOn = DateTime.UtcNow;
                        commande.ModifiedBy = "Système de Facturation";

                        _logger.LogInformation("🆓 Exemption: {CodeCommande} - {Utilisateur} - Motif: {Motif}",
                            commandeNonFacturable.Commande.CodeCommande,
                            commandeNonFacturable.Commande.NomUtilisateur,
                            commandeNonFacturable.Motif);
                    }
                }

                // Sauvegarder toutes les modifications
                await _context.SaveChangesAsync();

                _logger.LogInformation("✅ Facturation appliquée avec succès: {Facturables} facturées, {Exemptees} exemptées",
                    resultat.CommandesFacturables.Count, resultat.CommandesNonFacturables.Count);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors de l'application de la facturation");
                return false;
            }
        }

        public async Task<bool> EstWeekendAsync(DateTime date)
        {
            return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
        }

        public async Task<bool> EstJourFerieAsync(DateTime date)
        {
            // Liste des jours fériés (à adapter selon votre pays/région)
            var joursFeries = new[]
            {
                new DateTime(date.Year, 1, 1),   // Jour de l'An
                new DateTime(date.Year, 4, 1),   // Pâques (à ajuster selon l'année)
                new DateTime(date.Year, 5, 1),   // Fête du Travail
                new DateTime(date.Year, 5, 8),   // Victoire 1945
                new DateTime(date.Year, 7, 14),  // Fête Nationale
                new DateTime(date.Year, 8, 15),  // Assomption
                new DateTime(date.Year, 11, 1),  // Toussaint
                new DateTime(date.Year, 11, 11), // Armistice
                new DateTime(date.Year, 12, 25)  // Noël
            };

            return joursFeries.Contains(date.Date);
        }
    }

}
