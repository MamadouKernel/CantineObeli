using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Obeli_K.Data;
using Obeli_K.Services;
using Obeli_K.Services.Configuration;
using Obeli_K.Models.ViewModels;
using Obeli_K.Models;
using Microsoft.EntityFrameworkCore;

namespace Obeli_K.Controllers
{
    [Authorize(Roles = "Administrateur,RH")]
    public class DiagnosticFacturationController : Controller
    {
        private readonly ObeliDbContext _context;
        private readonly IFacturationService _facturationService;
        private readonly IConfigurationService _configService;
        private readonly ILogger<DiagnosticFacturationController> _logger;

        public DiagnosticFacturationController(
            ObeliDbContext context,
            IFacturationService facturationService,
            IConfigurationService configService,
            ILogger<DiagnosticFacturationController> logger)
        {
            _context = context;
            _facturationService = facturationService;
            _configService = configService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var currentUserId = User.FindFirst("UserId")?.Value;
                var userEmail = User.Identity?.Name;
                
                // Vérifier la configuration
                var facturationActive = await _configService.GetConfigurationAsync("FACTURATION_NON_CONSOMMEES_ACTIVE");
                var isFacturationActive = !string.IsNullOrEmpty(facturationActive) && facturationActive.ToLower() == "true";

                // Période par défaut (mois en cours)
                var today = DateTime.Today;
                var dateDebut = new DateTime(today.Year, today.Month, 1);
                var dateFin = dateDebut.AddMonths(1).AddDays(-1);

                ViewBag.FacturationActive = isFacturationActive;
                ViewBag.UserId = currentUserId;
                ViewBag.UserEmail = userEmail;
                ViewBag.IsEmploye = false; // Seuls Admin/RH peuvent accéder
                ViewBag.DateDebut = dateDebut;
                ViewBag.DateFin = dateFin;

                // Récupérer toutes les commandes pour diagnostic (Admin/RH voient toutes les commandes)
                var query = _context.Commandes
                    .Include(c => c.Utilisateur)
                    .Include(c => c.FormuleJour)
                    .Where(c => c.Supprimer == 0);

                var allCommandes = await query
                    .Where(c => c.DateConsommation.HasValue &&
                               c.DateConsommation.Value.Date >= dateDebut &&
                               c.DateConsommation.Value.Date <= dateFin)
                    .OrderByDescending(c => c.DateConsommation)
                    .ToListAsync();

                // Séparer par statut - Vérifier si les commandes "Consommées" ont vraiment été validées
                var commandesPrecommander = allCommandes.Where(c => c.StatusCommande == (int)Obeli_K.Enums.StatutCommande.Precommander).ToList();
                var commandesAnnulee = allCommandes.Where(c => c.StatusCommande == (int)Obeli_K.Enums.StatutCommande.Annulee).ToList();
                
                // Vérifier les commandes avec statut "Consommée" - elles doivent avoir un point de consommation
                var commandesAvecStatutConsommee = allCommandes.Where(c => c.StatusCommande == (int)Obeli_K.Enums.StatutCommande.Consommee).ToList();
                
                // Vérifier si ces commandes ont un point de consommation correspondant (validation par prestataire)
                var commandesConsommee = new List<Commande>();
                var commandesStatutConsommeeMaisPasValidee = new List<Commande>();
                
                foreach (var commande in commandesAvecStatutConsommee)
                {
                    // Vérifier s'il existe un point de consommation pour cette commande
                    var pointConsommation = await _context.PointsConsommation
                        .FirstOrDefaultAsync(pc => pc.CommandeId == commande.IdCommande && pc.Supprimer == 0);
                    
                    if (pointConsommation != null)
                    {
                        // Commande réellement consommée (validée par prestataire)
                        commandesConsommee.Add(commande);
                    }
                    else
                    {
                        // Commande avec statut "Consommée" mais pas de point de consommation = pas vraiment validée
                        commandesStatutConsommeeMaisPasValidee.Add(commande);
                        _logger.LogWarning("⚠️ Commande {CodeCommande} a le statut 'Consommée' mais aucun point de consommation - pas validée par prestataire", 
                            commande.CodeCommande);
                    }
                }

                // Identifier les commandes non consommées (Precommander avec date passée)
                var maintenant = DateTime.Now;
                var commandesNonConsommees = commandesPrecommander
                    .Where(c => c.DateConsommation.HasValue && c.DateConsommation.Value.Date < maintenant.Date)
                    .ToList();
                
                // Ajouter les commandes avec statut "Consommée" mais pas validées par prestataire
                commandesNonConsommees.AddRange(commandesStatutConsommeeMaisPasValidee);

                // Récupérer via le service de facturation
                var commandesNonConsommeesService = new List<CommandeNonConsommeeViewModel>();
                if (isFacturationActive)
                {
                    // Admin/RH voient toutes les commandes non consommées
                    commandesNonConsommeesService = await _facturationService.GetCommandesNonConsommeesAsync(dateDebut, dateFin);
                }

                // Créer une liste avec les statuts corrigés pour l'affichage
                var allCommandesWithCorrectedStatus = new List<object>();
                foreach (var cmd in allCommandes)
                {
                    var isReallyConsumed = commandesConsommee.Any(c => c.IdCommande == cmd.IdCommande);
                    var isStatusConsumedButNotValidated = commandesStatutConsommeeMaisPasValidee.Any(c => c.IdCommande == cmd.IdCommande);
                    
                    // Déterminer le statut d'affichage correct
                    string displayStatus;
                    string statusClass;
                    
                    if (isReallyConsumed)
                    {
                        displayStatus = "Consommée";
                        statusClass = "bg-success";
                    }
                    else if (isStatusConsumedButNotValidated)
                    {
                        displayStatus = "Précommandée";
                        statusClass = "bg-warning";
                    }
                    else
                    {
                        // Utiliser le statut original
                        switch (cmd.StatusCommande)
                        {
                            case 0:
                                displayStatus = "Précommandée";
                                statusClass = "bg-warning";
                                break;
                            case 1:
                                displayStatus = "Consommée";
                                statusClass = "bg-success";
                                break;
                            case 2:
                                displayStatus = "Annulée";
                                statusClass = "bg-danger";
                                break;
                            case 3:
                                displayStatus = "Facturée";
                                statusClass = "bg-primary";
                                break;
                            case 4:
                                displayStatus = "Exemptée";
                                statusClass = "bg-info";
                                break;
                            default:
                                displayStatus = "Inconnu";
                                statusClass = "bg-secondary";
                                break;
                        }
                    }
                    
                    allCommandesWithCorrectedStatus.Add(new {
                        Commande = cmd,
                        DisplayStatus = displayStatus,
                        StatusClass = statusClass
                    });
                }

                ViewBag.AllCommandes = allCommandes;
                ViewBag.AllCommandesWithCorrectedStatus = allCommandesWithCorrectedStatus;
                ViewBag.CommandesPrecommander = commandesPrecommander;
                ViewBag.CommandesAnnulee = commandesAnnulee;
                ViewBag.CommandesConsommee = commandesConsommee;
                ViewBag.CommandesStatutConsommeeMaisPasValidee = commandesStatutConsommeeMaisPasValidee;
                ViewBag.CommandesNonConsommees = commandesNonConsommees;
                ViewBag.CommandesNonConsommeesService = commandesNonConsommeesService;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors du diagnostic de facturation");
                TempData["ErrorMessage"] = "Erreur lors du diagnostic: " + ex.Message;
                return View();
            }
        }
    }
}
