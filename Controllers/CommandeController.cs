using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Obeli_K.Data;
using Obeli_K.Hubs;
using Obeli_K.Enums;
using Obeli_K.Models;
using Obeli_K.Models.Enums;
using Obeli_K.Models.ViewModels;
using Obeli_K.Services.Configuration;
using System.Security.Cryptography;

namespace Obeli_K.Controllers
{
    [Authorize]
    public class CommandeController : Controller
    {
        private readonly ObeliDbContext _context;
        private readonly ILogger<CommandeController> _logger;
        private readonly IHubContext<NotificationsHub> _hubContext;
        private readonly IConfigurationService _configurationService;

        public CommandeController(ObeliDbContext context, ILogger<CommandeController> logger, IHubContext<NotificationsHub> hubContext, IConfigurationService configurationService)
        {
            _context = context;
            _logger = logger;
            _hubContext = hubContext;
            _configurationService = configurationService;
        }

        // Populate the ViewBag with the list of formules, utilisateurs, etc.
        private async Task PopulateViewBags()
        {
            // Vérifier si les commandes sont bloquées
            var isBlocked = await _configurationService.IsCommandeBlockedAsync();
            
            if (isBlocked)
            {
                // Si bloqué, ne pas afficher les formules de la semaine N+1
                ViewBag.Formules = new SelectList(new List<object>(), "Value", "Text");
                ViewBag.IsBlocked = true;
                var prochaineCloture = await _configurationService.GetNextBlockingDateAsync();
                ViewBag.ProchaineCloture = prochaineCloture;
                _logger.LogInformation("Commandes bloquées - Aucune formule de la semaine N+1 affichée. Prochaine ouverture: {ProchaineCloture}", prochaineCloture);
                return;
            }

            // Formules disponibles (semaine N + 1 : du lundi au vendredi)
            var (lundiN1, vendrediN1) = GetSemaineSuivanteOuvree();

            var formules = await _context.FormulesJour
                .AsNoTracking()
                .Include(f => f.NomFormuleNavigation)
                .Where(f => f.Supprimer == 0 && f.Date >= lundiN1 && f.Date <= vendrediN1)
                .OrderBy(f => f.Date)
                .ThenBy(f => f.NomFormuleNavigation!.Nom)
                .Select(f => new
                {
                    Value = f.IdFormule.ToString(),
                    Text = $"{f.Date:dd/MM/yyyy} ({f.Date:dddd}) - {f.NomFormule} ({f.NomFormuleNavigation!.Nom})"
                })
                .ToListAsync();
            ViewBag.Formules = new SelectList(formules, "Value", "Text");
            ViewBag.IsBlocked = false;
            _logger.LogInformation("Chargement de {Count} formules (N+1 ouvrée {Debut} → {Fin})", formules.Count, lundiN1, vendrediN1);

            // Déterminer si l'utilisateur est un employé
            var isEmploye = User.IsInRole("Employe") && !User.IsInRole("Administrateur") && !User.IsInRole("RessourcesHumaines");
            var currentUserId = GetCurrentUserId();

            var utilisateursQuery = _context.Utilisateurs.AsNoTracking().Where(u => u.Supprimer == 0);

            // Si c'est un employé, il ne voit que son propre nom
            if (isEmploye && currentUserId.HasValue)
            {
                utilisateursQuery = utilisateursQuery.Where(u => u.Id == currentUserId);
            }

            var utilisateurs = await utilisateursQuery
                .OrderBy(u => u.Nom)
                .ThenBy(u => u.Prenoms)
                .Select(u => new
                {
                    Value = u.Id.ToString(),
                    Text = $"{u.Nom} {u.Prenoms}",
                    UserName = u.UserName
                })
                .ToListAsync();
            
            // Créer le SelectList avec les attributs data pour la recherche par matricule
            var selectListItems = utilisateurs.Select(u => new SelectListItem
            {
                Value = u.Value,
                Text = u.Text
            }).ToList();
            
            ViewBag.Utilisateurs = new SelectList(selectListItems, "Value", "Text");
            ViewBag.UtilisateursData = utilisateurs.ToDictionary(u => u.Value, u => u.UserName);
            _logger.LogInformation("Chargement de {Count} utilisateurs", utilisateurs.Count);

            // Groupes non-CIT disponibles
            var groupesNonCit = await _context.GroupesNonCit
                .AsNoTracking()
                .Where(g => g.Supprimer == 0)
                .OrderBy(g => g.Nom)
                .Select(g => new
                {
                    Value = g.Id.ToString(),
                    Text = g.Nom
                })
                .ToListAsync();
            ViewBag.GroupesNonCit = new SelectList(groupesNonCit, "Value", "Text");
            _logger.LogInformation("Chargement de {Count} groupes non-CIT", groupesNonCit.Count);

            // Types de client
            var typesClient = new List<object>
            {
                new { Value = TypeClientCommande.CitUtilisateur.ToString(), Text = "Utilisateur CIT" },
                new { Value = TypeClientCommande.GroupeNonCit.ToString(), Text = "Groupe non-CIT" },
                new { Value = TypeClientCommande.Visiteur.ToString(), Text = "Visiteur" }
            };
            ViewBag.TypeClients = new SelectList(typesClient, "Value", "Text");

            // Statuts de commande
            var statuts = new List<object>
            {
                new { Value = StatutCommande.Precommander.ToString(), Text = "Précommandée" },
                new { Value = StatutCommande.Annulee.ToString(), Text = "Annulée" },
                new { Value = StatutCommande.Consommee.ToString(), Text = "Consommée" }
            };
            ViewBag.Statuts = new SelectList(statuts, "Value", "Text");

            // Périodes de service
            var periodes = new List<object>
            {
                new { Value = Periode.Jour.ToString(), Text = "Jour" },
                new { Value = Periode.Nuit.ToString(), Text = "Nuit" }
            };
            ViewBag.Periodes = new SelectList(periodes, "Value", "Text");

            // Sites
            var sites = new List<object>
            {
                new { Value = SiteType.CIT_Terminal.ToString(), Text = "CIT Terminal" },
                new { Value = SiteType.CIT_Billing.ToString(), Text = "CIT Billing" }
            };
            ViewBag.Sites = new SelectList(sites, "Value", "Text");
        }

        /// <summary>
        /// Affiche la liste des commandes
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(string? status = null, DateTime? dateDebut = null, DateTime? dateFin = null)
        {
            try
            {
                // Déterminer si l'utilisateur est un employé (ne voit que ses commandes)
                var isEmploye = User.IsInRole("Employe") && !User.IsInRole("Administrateur") && !User.IsInRole("RessourcesHumaines");
                var currentUserId = GetCurrentUserId();

                var query = _context.Commandes
                    .AsNoTracking()
                    .Include(c => c.Utilisateur)
                    .Include(c => c.GroupeNonCit)
                    .Include(c => c.FormuleJour)!.ThenInclude(f => f!.NomFormuleNavigation)
                    .Where(c => c.Supprimer == 0);

                // Filtrer par utilisateur si c'est un employé (pas admin/RH)
                if (isEmploye && currentUserId.HasValue)
                {
                    query = query.Where(c => c.UtilisateurId == currentUserId);
                }

                // Filtrer par statut si spécifié
                if (!string.IsNullOrEmpty(status) && Enum.TryParse<StatutCommande>(status, out var statutCommande))
                {
                    query = query.Where(c => c.StatusCommande == (int)statutCommande);
                }

                // Filtrer par période si spécifiée
                if (dateDebut.HasValue)
                {
                    query = query.Where(c => c.DateConsommation.HasValue && c.DateConsommation.Value.Date >= dateDebut.Value.Date);
                }
                if (dateFin.HasValue)
                {
                    query = query.Where(c => c.DateConsommation.HasValue && c.DateConsommation.Value.Date <= dateFin.Value.Date);
                }

                _logger.LogInformation("Recherche des commandes - isEmploye: {isEmploye}, currentUserId: {currentUserId}, status: {status}, dateDebut: {dateDebut}, dateFin: {dateFin}", 
                    isEmploye, currentUserId, status, dateDebut, dateFin);

                var commandesAvecFormules = await query
                    .OrderByDescending(c => c.Date)
                    .ToListAsync();

                // Créer les view models avec le nom du plat
                var commandes = commandesAvecFormules.Select(c => new CommandeListViewModel
                    {
                        IdCommande = c.IdCommande,
                        Date = c.Date,
                        DateConsommation = c.DateConsommation,
                        StatusCommande = (StatutCommande)c.StatusCommande,
                        CodeCommande = c.CodeCommande,
                        PeriodeService = c.PeriodeService,
                        Montant = c.Montant,
                        Quantite = c.Quantite,
                        Instantanee = c.Instantanee,
                        AnnuleeParPrestataire = c.AnnuleeParPrestataire,
                        MotifAnnulation = c.MotifAnnulation,
                        TypeClient = c.TypeClient,
                        Site = c.Site,
                        DateLivraisonPrevueUtc = c.DateLivraisonPrevueUtc,
                        DateReceptionUtc = c.DateReceptionUtc,
                        UtilisateurNom = c.Utilisateur != null ? c.Utilisateur.Nom : null,
                        UtilisateurPrenoms = c.Utilisateur != null ? c.Utilisateur.Prenoms : null,
                        UtilisateurMatricule = c.Utilisateur != null ? c.Utilisateur.UserName : null,
                        UtilisateurNomComplet = c.Utilisateur != null ? $"{c.Utilisateur.Nom} {c.Utilisateur.Prenoms}" : null,
                        GroupeNonCitNom = c.GroupeNonCit != null ? c.GroupeNonCit.Nom : null,
                        VisiteurNom = c.VisiteurNom,
                        VisiteurTelephone = c.VisiteurTelephone,
                        FormuleNom = c.FormuleJour != null ? c.FormuleJour.NomFormule : null,
                        FormuleDate = c.FormuleJour != null ? c.FormuleJour.Date : null,
                        TypeFormuleNom = c.FormuleJour != null && c.FormuleJour.NomFormuleNavigation != null ? c.FormuleJour.NomFormuleNavigation.Nom : null,
                    NomPlat = GetNomPlatFromFormule(c.FormuleJour),
                        CreatedOn = c.CreatedOn,
                        CreatedBy = c.CreatedBy
                }).ToList();

                _logger.LogInformation("Nombre de commandes trouvées: {count}", commandes.Count);

                // Passer les paramètres de filtrage au ViewBag
                ViewBag.Status = status;
                ViewBag.DateDebut = dateDebut;
                ViewBag.DateFin = dateFin;

                return View(commandes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement de la liste des commandes");
                TempData["ErrorMessage"] = "Une erreur est survenue lors du chargement des commandes.";
                return View(new List<CommandeListViewModel>());
            }
        }

        /// <summary>
        /// Affiche le formulaire de création de commande avec interface par semaine
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                _logger.LogInformation("Chargement du formulaire de création de commande par semaine");

                // Vérifier si les commandes sont bloquées
                var isBlocked = await _configurationService.IsCommandeBlockedAsync();
                
                if (isBlocked)
                {
                    var prochaineCloture = await _configurationService.GetNextBlockingDateAsync();
                    ViewBag.IsBlocked = true;
                    ViewBag.ProchaineCloture = prochaineCloture;
                    ViewBag.MenusSemaineN1 = new List<object>();
                    _logger.LogInformation("Commandes bloquées - Aucun menu de la semaine N+1 affiché. Prochaine ouverture: {ProchaineCloture}", prochaineCloture);
                    return View();
                }

                // --- Semaine N+1 complète (lundi → dimanche). 
                // Si ta cantine ne sert que lun→ven, remplace par GetSemaineSuivanteOuvree().
                var (debutSemaine, finSemaine) = GetSemaineSuivanteComplete();
                _logger.LogInformation("Semaine N+1: {DebutSemaine} à {FinSemaine}", debutSemaine, finSemaine);

                // ===== RÉCUPÉRATION DES MENUS DE LA SEMAINE N+1 =====
                var menusSemaineN1 = await _context.FormulesJour
                    .AsNoTracking()
                    .Include(fj => fj.NomFormuleNavigation)
                    .Where(fj => fj.Supprimer == 0
                              && fj.Date >= debutSemaine
                              && fj.Date <= finSemaine)
                    .Select(fj => new
                    {
                        fj.IdFormule,
                        fj.TypeFormuleId,
                        TypeFormule = fj.NomFormule,
                        fj.Date,
                        fj.NomFormule,
                        fj.Plat,
                        fj.PlatStandard1,
                        fj.PlatStandard2,
                        fj.Entree,
                        fj.Dessert,
                        fj.Garniture,
                        fj.GarnitureStandard1,
                        fj.GarnitureStandard2,
                        fj.Feculent,
                        fj.Legumes
                    })
                    .OrderBy(fj => fj.Date)
                    .ThenBy(fj => fj.TypeFormule)
                    .ToListAsync();

                _logger.LogInformation("Nombre de formules trouvées pour la semaine N+1: {Count}", menusSemaineN1.Count);

                var currentUserId = GetCurrentUserId();
                var commandesExistantes = new List<CommandeExistanteViewModel>();

                if (currentUserId.HasValue)
                {
                    var commandesAvecFormules = await _context.Commandes
                        .AsNoTracking()
                        .Include(c => c.FormuleJour)!.ThenInclude(f => f!.NomFormuleNavigation)
                        .Where(c => c.UtilisateurId == currentUserId
                                 && c.DateConsommation.HasValue
                                 && c.DateConsommation.Value.Date >= debutSemaine
                                 && c.DateConsommation.Value.Date <= finSemaine
                                 && c.Supprimer == 0
                                 && !(c.StatusCommande == (int)StatutCommande.Annulee && !c.AnnuleeParPrestataire))
                        .ToListAsync();

                    // Créer les view models avec le nom du plat
                    var commandesSemaine = commandesAvecFormules.Select(c => new CommandeExistanteViewModel
                        {
                            IdCommande = c.IdCommande,
                            DateConsommation = c.DateConsommation!.Value,
                            NomJour = c.DateConsommation.Value.ToString("dddd", new System.Globalization.CultureInfo("fr-FR")),
                        TypeFormule = c.FormuleJour?.NomFormuleNavigation?.Nom ?? "Standard",
                            PeriodeService = c.PeriodeService,
                        CodeCommande = c.CodeCommande ?? "",
                        NomPlat = GetNomPlatFromFormule(c.FormuleJour)
                    }).ToList();

                    commandesExistantes = commandesSemaine;
                }

                // Organiser les données par jour
                var model = new CommandeParSemaineViewModel
                {
                    DateDebutSemaine = debutSemaine,
                    DateFinSemaine = finSemaine,
                    CommandesExistantes = commandesExistantes
                };

                // Créer les jours de la semaine (Lundi à Dimanche)
                for (int i = 0; i < 7; i++)
                {
                    var dateJour = debutSemaine.AddDays(i);
                    var nomJour = dateJour.ToString("dddd", new System.Globalization.CultureInfo("fr-FR"));

                    var jourViewModel = new JourSemaineViewModel
                    {
                        Date = dateJour,
                        NomJour = char.ToUpper(nomJour[0]) + nomJour.Substring(1) // Capitaliser 1ère lettre
                    };

                    // Récupérer TOUTES les formules pour ce jour
                    var formulesDuJour = menusSemaineN1.Where(f => f.Date.Date == dateJour.Date).ToList();
                    _logger.LogInformation("Jour {DateJour}: {Count} formules trouvées", 
                        dateJour, formulesDuJour.Count);
                    
                    // Log des formules trouvées pour ce jour
                    foreach (var formule in formulesDuJour)
                    {
                        _logger.LogInformation("Formule trouvée: {TypeFormule} - {NomFormule}", 
                            formule.TypeFormule, formule.NomFormule);
                    }

                    // Vignettes : Améliorée / Standard1 / Standard2
                    var typesFormules = new[] 
                    { 
                        new { Type = "Améliorée", TypeFormule = "Amélioré" },
                        new { Type = "Standard1", TypeFormule = "Standard 1" },
                        new { Type = "Standard2", TypeFormule = "Standard 2" }
                    };
                    
                    _logger.LogInformation("Types de formules configurés: {Types}", 
                        string.Join(", ", typesFormules.Select(t => $"{t.Type}->{t.TypeFormule}")));

                    foreach (var typeFormule in typesFormules)
                    {
                        // Trouver la formule correspondant au type
                        var formuleDuJour = formulesDuJour.FirstOrDefault(f => f.TypeFormule == typeFormule.TypeFormule);
                        _logger.LogInformation("Recherche {TypeFormule} -> {TypeFormuleRecherche}: {Trouvee}", 
                            typeFormule.Type, typeFormule.TypeFormule, formuleDuJour != null ? "OUI" : "NON");
                        
                        // Est-ce qu'une commande existe déjà pour ce type de formule ce jour
                        var commandeDuJour = commandesExistantes.FirstOrDefault(c => c.DateConsommation.Date == dateJour.Date && c.TypeFormule == typeFormule.Type);
                        bool aDejaCommande = commandeDuJour != null;

                        jourViewModel.Formules.Add(new FormuleJourSemaineViewModel
                        {
                            IdFormule = formuleDuJour?.IdFormule ?? Guid.NewGuid(), // si null, ID "temporaire"
                            TypeFormule = typeFormule.Type,
                            NomFormule = formuleDuJour?.NomFormule ?? "Formule du jour",
                            Date = formuleDuJour?.Date ?? dateJour,
                            // Contenu selon le type
                            Entree = typeFormule.Type == "Améliorée" ? formuleDuJour?.Entree : null,
                            Dessert = typeFormule.Type == "Améliorée" ? formuleDuJour?.Dessert : null,
                            Plat = typeFormule.Type == "Améliorée" ? formuleDuJour?.Plat : null,
                            Garniture = typeFormule.Type == "Améliorée" ? formuleDuJour?.Garniture : null,
                            PlatStandard1 = typeFormule.Type == "Standard1" ? formuleDuJour?.PlatStandard1 : null,
                            GarnitureStandard1 = typeFormule.Type == "Standard1" ? formuleDuJour?.GarnitureStandard1 : null,
                            PlatStandard2 = typeFormule.Type == "Standard2" ? formuleDuJour?.PlatStandard2 : null,
                            GarnitureStandard2 = typeFormule.Type == "Standard2" ? formuleDuJour?.GarnitureStandard2 : null,
                            Feculent = formuleDuJour?.Feculent,
                            Legumes = formuleDuJour?.Legumes,
                            EstCommande = aDejaCommande,
                            CommandeId = commandeDuJour?.IdCommande,
                            PeriodeCommande = commandeDuJour?.PeriodeService ?? Periode.Jour,
                            CodeCommande = commandeDuJour?.CodeCommande
                        });
                    }

                    model.JoursSemaine.Add(jourViewModel);
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement du formulaire de création de commande par semaine");
                TempData["ErrorMessage"] = "Une erreur est survenue lors du chargement du formulaire.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Traite la création d'une commande depuis l'interface par semaine
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCommandeSemaine(Guid idFormule, DateTime dateConsommation, Periode periode, string typeFormule, SiteType? site = null)
        {
            try
            {
                // Vérifier si les commandes sont bloquées
                var isBlocked = await _configurationService.IsCommandeBlockedAsync();
                if (isBlocked)
                {
                    var prochaineCloture = await _configurationService.GetNextBlockingDateAsync();
                    return Json(new { 
                        success = false, 
                        message = $"Les commandes sont actuellement bloquées. Prochaine ouverture: {prochaineCloture:dd/MM/yyyy HH:mm}" 
                    });
                }

                var currentUserId = GetCurrentUserId();
                if (!currentUserId.HasValue)
                {
                    return Json(new { success = false, message = "Utilisateur non connecté" });
                }

                // Règle : une seule commande par jour / utilisateur
                var dejaCommandeCeJour = await _context.Commandes
                    .AsNoTracking()
                    .AnyAsync(c => c.UtilisateurId == currentUserId
                                && c.DateConsommation == dateConsommation
                                && c.Supprimer == 0
                                && !(c.StatusCommande == (int)StatutCommande.Annulee && !c.AnnuleeParPrestataire));

                if (dejaCommandeCeJour)
                {
                    return Json(new { success = false, message = "Vous avez déjà une commande pour ce jour." });
                }

                // Récupérer la formule
                var formule = await _context.FormulesJour
                    .AsNoTracking()
                    .Include(f => f.NomFormuleNavigation)
                    .FirstOrDefaultAsync(f => f.IdFormule == idFormule && f.Supprimer == 0);

                if (formule == null)
                {
                    return Json(new { success = false, message = "Formule non trouvée" });
                }

                // Récupérer le site de l'utilisateur et la formule pour le prix
                var utilisateur = await _context.Utilisateurs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == currentUserId && u.Supprimer == 0);

                // Calculer le prix selon le type de formule
                decimal prixUnitaire = GetPrixFormule(formule.NomFormule);

                // Créer la commande
                var commande = new Commande
                {
                    IdCommande = Guid.NewGuid(),
                    Date = DateTime.UtcNow,
                    DateConsommation = dateConsommation,
                    StatusCommande = (int)StatutCommande.Precommander,
                    CodeCommande = GenerateCodeCommande(),
                    PeriodeService = periode,
                    Montant = prixUnitaire, // Prix calculé selon le type de formule
                    Quantite = 1,
                    TypeClient = TypeClientCommande.CitUtilisateur,
                    UtilisateurId = currentUserId,
                    IdFormule = idFormule,
                    Site = site ?? utilisateur?.Site, // Utiliser le site sélectionné ou celui de l'utilisateur
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = User.Identity?.Name ?? "System"
                };

                _context.Commandes.Add(commande);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Commande créée avec succès: {CodeCommande} pour {DateConsommation}",
                    commande.CodeCommande, dateConsommation);

                return Json(new
                {
                    success = true,
                    message = "Commande créée avec succès",
                    codeCommande = commande.CodeCommande,
                    commandeId = commande.IdCommande
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création de la commande");
                return Json(new { success = false, message = "Erreur lors de la création de la commande" });
            }
        }

        /// <summary>
        /// Traite la création d'une nouvelle commande (ancienne méthode)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCommandeViewModel model)
        {
            _logger.LogInformation("=== DÉBUT CRÉATION COMMANDE ===");
            _logger.LogInformation("Données reçues - DateConsommation: {DateConsommation}, IdFormule: {IdFormule}, TypeClient: {TypeClient}",
                model.DateConsommation, model.IdFormule, model.TypeClient);

            try
            {
                if (model == null)
                {
                    _logger.LogWarning("Les données de commande sont manquantes.");
                    ModelState.AddModelError("", "Les données de commande sont manquantes.");
                    await PopulateViewBags();
                    return View(model);
                }

                // Vérifier si les commandes sont bloquées
                var isBlocked = await _configurationService.IsCommandeBlockedAsync();
                if (isBlocked)
                {
                    var prochaineCloture = await _configurationService.GetNextBlockingDateAsync();
                    ModelState.AddModelError("", $"Les commandes sont actuellement bloquées. Prochaine ouverture: {prochaineCloture:dd/MM/yyyy HH:mm}");
                    await PopulateViewBags();
                    return View(model);
                }

                // Validation des champs obligatoires
                var (lundiSemaineSuivante, vendrediSemaineSuivante) = GetSemaineSuivanteOuvree();

                if (model.DateConsommation < lundiSemaineSuivante || model.DateConsommation > vendrediSemaineSuivante)
                    ModelState.AddModelError(nameof(model.DateConsommation), $"La date de consommation doit être entre le {lundiSemaineSuivante:dd/MM/yyyy} et le {vendrediSemaineSuivante:dd/MM/yyyy} (semaine suivante).");

                if (model.IdFormule == Guid.Empty)
                    ModelState.AddModelError(nameof(model.IdFormule), "La formule est obligatoire.");

                if (model.Quantite < 1)
                    ModelState.AddModelError(nameof(model.Quantite), "La quantité doit être au moins 1.");

                // Validation selon le type de client
                var isEmploye = User.IsInRole("Employe") && !User.IsInRole("Administrateur") && !User.IsInRole("RessourcesHumaines");
                var currentUserId = GetCurrentUserId();

                // Pré-remplir le site avec le site de l'utilisateur connecté si ce n'est pas déjà défini
                if (!model.Site.HasValue && currentUserId.HasValue)
                {
                    var utilisateur = await _context.Utilisateurs
                        .AsNoTracking()
                        .FirstOrDefaultAsync(u => u.Id == currentUserId && u.Supprimer == 0);
                    if (utilisateur?.Site.HasValue == true)
                    {
                        model.Site = utilisateur.Site;
                        _logger.LogInformation("Site pré-rempli avec le site de l'utilisateur: {Site}", utilisateur.Site);
                    }
                }

                switch (model.TypeClient)
                {
                    case TypeClientCommande.CitUtilisateur:
                        if (!model.UtilisateurId.HasValue || model.UtilisateurId == Guid.Empty)
                            ModelState.AddModelError(nameof(model.UtilisateurId), "L'utilisateur est obligatoire pour ce type de client.");

                        // Si c'est un employé, il ne peut commander que pour lui-même
                        if (isEmploye && currentUserId.HasValue && model.UtilisateurId != currentUserId)
                        {
                            ModelState.AddModelError(nameof(model.UtilisateurId), "Vous ne pouvez créer des commandes que pour vous-même.");
                        }
                        break;
                    case TypeClientCommande.GroupeNonCit:
                        if (!model.GroupeNonCitId.HasValue || model.GroupeNonCitId == Guid.Empty)
                            ModelState.AddModelError(nameof(model.GroupeNonCitId), "Le groupe non-CIT est obligatoire pour ce type de client.");
                        break;
                    case TypeClientCommande.Visiteur:
                        if (string.IsNullOrWhiteSpace(model.VisiteurNom))
                            ModelState.AddModelError(nameof(model.VisiteurNom), "Le nom du visiteur est obligatoire pour ce type de client.");
                        break;
                }

                // Vérifications en base
                if (model.IdFormule != Guid.Empty)
                {
                    bool formuleExists = await _context.FormulesJour
                        .AsNoTracking()
                        .AnyAsync(f => f.IdFormule == model.IdFormule && f.Supprimer == 0);
                    if (!formuleExists)
                        ModelState.AddModelError(nameof(model.IdFormule), "La formule sélectionnée n'existe pas.");
                }

                if (model.UtilisateurId.HasValue && model.UtilisateurId != Guid.Empty)
                {
                    bool userExists = await _context.Utilisateurs
                        .AsNoTracking()
                        .AnyAsync(u => u.Id == model.UtilisateurId && u.Supprimer == 0);
                    if (!userExists)
                        ModelState.AddModelError(nameof(model.UtilisateurId), "L'utilisateur sélectionné n'existe pas.");
                }

                if (model.GroupeNonCitId.HasValue && model.GroupeNonCitId != Guid.Empty)
                {
                    var groupe = await _context.GroupesNonCit
                        .AsNoTracking()
                        .FirstOrDefaultAsync(g => g.Id == model.GroupeNonCitId && g.Supprimer == 0);
                    
                    if (groupe == null)
                    {
                        ModelState.AddModelError(nameof(model.GroupeNonCitId), "Le groupe non-CIT sélectionné n'existe pas.");
                    }
                    else
                    {
                        // Vérifier les restrictions de formule pour les groupes spéciaux (temporairement commenté)
                        /*
                        if (groupe.RestrictionFormuleStandard)
                        {
                            var formuleGroupe = await _context.FormulesJour
                                .Include(f => f.NomFormuleNavigation)
                                .FirstOrDefaultAsync(f => f.IdFormule == model.IdFormule);
                            
                            if (formuleGroupe?.NomFormuleNavigation?.Nom?.ToUpper() != "STANDARD")
                            {
                                ModelState.AddModelError(nameof(model.IdFormule), $"Ce groupe est limité aux formules Standard uniquement.");
                            }
                        }
                        */

                        // Vérifier le quota journalier (temporairement commenté)
                        /*
                        if (groupe.QuotaJournalier.HasValue)
                        {
                            var commandesDuJour = await _context.Commandes
                                .Where(c => c.GroupeNonCitId == model.GroupeNonCitId 
                                         && c.DateConsommation.HasValue 
                                         && c.DateConsommation.Value.Date == model.DateConsommation.Date
                                         && c.Supprimer == 0)
                                .SumAsync(c => c.Quantite);

                            if (commandesDuJour + model.Quantite > groupe.QuotaJournalier.Value)
                            {
                                ModelState.AddModelError(nameof(model.Quantite), 
                                    $"Quota journalier dépassé. Maximum autorisé: {groupe.QuotaJournalier.Value} plats/jour. " +
                                    $"Déjà commandé: {commandesDuJour}, Tentative: {model.Quantite}");
                            }
                        }
                        */
                    }
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Échec de validation du formulaire de commande.");
                    foreach (var error in ModelState)
                    {
                        if (error.Value.Errors.Count > 0)
                        {
                            _logger.LogWarning("Erreur de validation - {Key}: {Errors}",
                                error.Key, string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage)));
                        }
                    }
                    await PopulateViewBags();
                    return View(model);
                }

                // Récupérer la formule pour vérifier son existence
                var formule = await _context.FormulesJour
                    .AsNoTracking()
                    .FirstOrDefaultAsync(f => f.IdFormule == model.IdFormule);

                if (formule == null)
                {
                    ModelState.AddModelError(nameof(model.IdFormule), "Impossible de récupérer les informations de la formule.");
                    await PopulateViewBags();
                    return View(model);
                }

                // Calculer le prix selon le type de formule
                decimal prixUnitaire = GetPrixFormule(formule.NomFormule);

                // Construction de l'entité
                var nouvelleCommande = new Commande
                {
                    Date = DateTime.UtcNow,
                    DateConsommation = model.DateConsommation,
                    StatusCommande = (int)StatutCommande.Precommander,
                    CodeCommande = GenerateCodeCommande(),
                    PeriodeService = model.PeriodeService,
                    Montant = prixUnitaire * model.Quantite,
                    IdFormule = model.IdFormule,
                    TypeClient = model.TypeClient,
                    UtilisateurId = model.UtilisateurId,
                    GroupeNonCitId = model.GroupeNonCitId,
                    VisiteurNom = string.IsNullOrWhiteSpace(model.VisiteurNom) ? null : model.VisiteurNom.Trim(),
                    VisiteurTelephone = string.IsNullOrWhiteSpace(model.VisiteurTelephone) ? null : model.VisiteurTelephone.Trim(),
                    Site = model.Site,
                    DateLivraisonPrevueUtc = model.DateLivraisonPrevueUtc,
                    Quantite = model.Quantite,
                    Instantanee = model.Instantanee,
                    AnnuleeParPrestataire = false,
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = User.Identity?.Name ?? "System",
                    ModifiedOn = DateTime.UtcNow,
                    ModifiedBy = User.Identity?.Name ?? "System",
                    Supprimer = 0
                };

                // Sauvegarde
                _logger.LogInformation("Sauvegarde de la commande en base de données...");
                _context.Commandes.Add(nouvelleCommande);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Commande sauvegardée avec succès !");

                // Envoyer une notification aux prestataires
                await SendNotificationToPrestataires(
                    $"Nouvelle commande créée: {nouvelleCommande.CodeCommande} pour le {nouvelleCommande.DateConsommation:dd/MM/yyyy}",
                    Enums.TypeNotification.Info
                );

                _logger.LogInformation("Commande créée avec succès : {IdCommande}", nouvelleCommande.IdCommande);
                TempData["SuccessMessage"] = $"La commande {nouvelleCommande.CodeCommande} a été créée avec succès.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création de la commande : {Message}", ex.Message);
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la création de la commande.";
                await PopulateViewBags();
                return View(model);
            }
        }

        /// <summary>
        /// Affiche les détails d'une commande
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var commande = await _context.Commandes
                    .AsNoTracking()
                    .Include(c => c.Utilisateur)
                    .Include(c => c.GroupeNonCit)
                    .Include(c => c.FormuleJour)!.ThenInclude(f => f!.NomFormuleNavigation)
                    .FirstOrDefaultAsync(c => c.IdCommande == id && c.Supprimer == 0);

                if (commande == null)
                {
                    TempData["ErrorMessage"] = "Commande introuvable.";
                    return RedirectToAction(nameof(Index));
                }

                // Convertir en CommandeListViewModel
                var commandeViewModel = new CommandeListViewModel
                {
                    IdCommande = commande.IdCommande,
                    Date = commande.Date,
                    DateConsommation = commande.DateConsommation,
                    StatusCommande = (StatutCommande)commande.StatusCommande,
                    CodeCommande = commande.CodeCommande,
                    PeriodeService = commande.PeriodeService,
                    Montant = commande.Montant,
                    Quantite = commande.Quantite,
                    Instantanee = commande.Instantanee,
                    AnnuleeParPrestataire = commande.AnnuleeParPrestataire,
                    MotifAnnulation = commande.MotifAnnulation,
                    TypeClient = commande.TypeClient,
                    Site = commande.Site,
                    DateLivraisonPrevueUtc = commande.DateLivraisonPrevueUtc,
                    DateReceptionUtc = commande.DateReceptionUtc,
                    UtilisateurNom = commande.Utilisateur != null ? commande.Utilisateur.Nom : null,
                    UtilisateurPrenoms = commande.Utilisateur != null ? commande.Utilisateur.Prenoms : null,
                    UtilisateurMatricule = commande.Utilisateur != null ? commande.Utilisateur.UserName : null,
                    UtilisateurNomComplet = commande.Utilisateur != null ? $"{commande.Utilisateur.Nom} {commande.Utilisateur.Prenoms}" : null,
                    GroupeNonCitNom = commande.GroupeNonCit != null ? commande.GroupeNonCit.Nom : null,
                    VisiteurNom = commande.VisiteurNom,
                    VisiteurTelephone = commande.VisiteurTelephone,
                    FormuleNom = commande.FormuleJour != null ? commande.FormuleJour.NomFormule : null,
                    FormuleDate = commande.FormuleJour != null ? commande.FormuleJour.Date : null,
                    TypeFormuleNom = commande.FormuleJour != null ? commande.FormuleJour.NomFormule : null,
                    NomPlat = GetNomPlatFromFormule(commande.FormuleJour),
                    CreatedOn = commande.CreatedOn,
                    CreatedBy = commande.CreatedBy
                };

                return View(commandeViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement des détails de la commande {CommandeId}", id);
                TempData["ErrorMessage"] = "Une erreur est survenue lors du chargement des détails.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Affiche le formulaire d'édition d'une commande
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Administrateur,RessourcesHumaines,Employe")]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var commande = await _context.Commandes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.IdCommande == id && c.Supprimer == 0);

                if (commande == null)
                {
                    TempData["ErrorMessage"] = "Commande introuvable.";
                    return RedirectToAction(nameof(Index));
                }

                // Mapper vers le ViewModel
                var model = new EditCommandeViewModel
                {
                    IdCommande = commande.IdCommande,
                    DateConsommation = commande.DateConsommation ?? DateTime.Today,
                    IdFormule = commande.IdFormule,
                    StatusCommande = (StatutCommande)commande.StatusCommande,
                    TypeClient = commande.TypeClient,
                    UtilisateurId = commande.UtilisateurId,
                    GroupeNonCitId = commande.GroupeNonCitId,
                    VisiteurNom = commande.VisiteurNom,
                    VisiteurTelephone = commande.VisiteurTelephone,
                    Site = commande.Site,
                    PeriodeService = commande.PeriodeService,
                    Quantite = commande.Quantite,
                    Instantanee = commande.Instantanee,
                    AnnuleeParPrestataire = commande.AnnuleeParPrestataire,
                    MotifAnnulation = commande.MotifAnnulation,
                    DateLivraisonPrevueUtc = commande.DateLivraisonPrevueUtc,
                    DateReceptionUtc = commande.DateReceptionUtc,
                    ReceptionConfirmeeParNom = commande.ReceptionConfirmeeParNom
                };

                await PopulateViewBags();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement de la commande pour édition {CommandeId}", id);
                TempData["ErrorMessage"] = "Une erreur est survenue lors du chargement de la commande.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Traite la modification d'une commande
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrateur,RessourcesHumaines,Employe")]
        public async Task<IActionResult> Edit(Guid id, EditCommandeViewModel model)
        {
            _logger.LogInformation("=== DÉBUT MODIFICATION COMMANDE ===");
            _logger.LogInformation("Modification commande ID: {Id}, Status: {Status}", id, model?.StatusCommande);

            try
            {
                if (model == null)
                {
                    _logger.LogWarning("Les données de commande sont manquantes.");
                    TempData["ErrorMessage"] = "Les données de commande sont manquantes.";
                    return RedirectToAction(nameof(Index));
                }

                if (id != model.IdCommande)
                {
                    _logger.LogWarning("Identifiant commande invalide. ID attendu: {ExpectedId}, ID reçu: {ReceivedId}", id, model.IdCommande);
                    TempData["ErrorMessage"] = "Identifiant commande invalide.";
                    return RedirectToAction(nameof(Index));
                }

                var existingCommande = await _context.Commandes
                    .FirstOrDefaultAsync(c => c.IdCommande == id && c.Supprimer == 0);

                if (existingCommande == null)
                {
                    _logger.LogWarning("Commande introuvable avec l'ID: {Id}", id);
                    TempData["ErrorMessage"] = "Commande introuvable.";
                    return RedirectToAction(nameof(Index));
                }

                // Validation des champs obligatoires
                if (model.DateConsommation < DateTime.Today)
                    ModelState.AddModelError(nameof(model.DateConsommation), "La date de consommation ne peut pas être dans le passé.");

                if (model.IdFormule == Guid.Empty)
                    ModelState.AddModelError(nameof(model.IdFormule), "La formule est obligatoire.");

                if (model.Quantite < 1)
                    ModelState.AddModelError(nameof(model.Quantite), "La quantité doit être au moins 1.");

                // Validation selon le type de client
                switch (model.TypeClient)
                {
                    case TypeClientCommande.CitUtilisateur:
                        if (!model.UtilisateurId.HasValue || model.UtilisateurId == Guid.Empty)
                            ModelState.AddModelError(nameof(model.UtilisateurId), "L'utilisateur est obligatoire pour ce type de client.");
                        break;
                    case TypeClientCommande.GroupeNonCit:
                        if (!model.GroupeNonCitId.HasValue || model.GroupeNonCitId == Guid.Empty)
                            ModelState.AddModelError(nameof(model.GroupeNonCitId), "Le groupe non-CIT est obligatoire pour ce type de client.");
                        break;
                    case TypeClientCommande.Visiteur:
                        if (string.IsNullOrWhiteSpace(model.VisiteurNom))
                            ModelState.AddModelError(nameof(model.VisiteurNom), "Le nom du visiteur est obligatoire pour ce type de client.");
                        break;
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Échec de validation du formulaire de modification de commande.");
                    foreach (var error in ModelState)
                    {
                        if (error.Value.Errors.Count > 0)
                        {
                            _logger.LogWarning("Erreur de validation - {Key}: {Errors}",
                                error.Key, string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage)));
                        }
                    }
                    await PopulateViewBags();
                    return View(model);
                }

                // Mettre à jour les propriétés
                existingCommande.DateConsommation = model.DateConsommation;
                existingCommande.IdFormule = model.IdFormule;
                existingCommande.StatusCommande = (int)model.StatusCommande;
                existingCommande.TypeClient = model.TypeClient;
                existingCommande.UtilisateurId = model.UtilisateurId;
                existingCommande.GroupeNonCitId = model.GroupeNonCitId;
                existingCommande.VisiteurNom = string.IsNullOrWhiteSpace(model.VisiteurNom) ? null : model.VisiteurNom.Trim();
                existingCommande.VisiteurTelephone = string.IsNullOrWhiteSpace(model.VisiteurTelephone) ? null : model.VisiteurTelephone.Trim();
                existingCommande.Site = model.Site;
                existingCommande.PeriodeService = model.PeriodeService;
                existingCommande.Quantite = model.Quantite;
                existingCommande.Instantanee = model.Instantanee;
                existingCommande.AnnuleeParPrestataire = model.AnnuleeParPrestataire;
                existingCommande.MotifAnnulation = string.IsNullOrWhiteSpace(model.MotifAnnulation) ? null : model.MotifAnnulation.Trim();
                existingCommande.DateLivraisonPrevueUtc = model.DateLivraisonPrevueUtc;
                existingCommande.DateReceptionUtc = model.DateReceptionUtc;
                existingCommande.ReceptionConfirmeeParNom = string.IsNullOrWhiteSpace(model.ReceptionConfirmeeParNom) ? null : model.ReceptionConfirmeeParNom.Trim();
                existingCommande.ModifiedOn = DateTime.UtcNow;
                existingCommande.ModifiedBy = User.Identity?.Name ?? "System";

                await _context.SaveChangesAsync();

                // Créer un point de consommation si la commande est marquée comme consommée
                if (model.StatusCommande == StatutCommande.Consommee)
                {
                    await CreerPointConsommationAsync(existingCommande);
                }

                // Envoyer une notification aux prestataires pour la modification
                await SendNotificationToPrestataires(
                    $"Commande modifiée: {existingCommande.CodeCommande} pour le {existingCommande.DateConsommation:dd/MM/yyyy} par {User.Identity?.Name}",
                    Enums.TypeNotification.Warning
                );

                _logger.LogInformation("Commande modifiée avec succès: {IdCommande} par {ModifiedBy}",
                    existingCommande.IdCommande, User.Identity?.Name);
                TempData["SuccessMessage"] = $"La commande {existingCommande.CodeCommande} a été modifiée avec succès.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la modification de la commande {CommandeId}: {Message}", id, ex.Message);
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la modification de la commande.";
                await PopulateViewBags();
                return View(model);
            }
        }

        /// <summary>
        /// Supprime une commande (soft delete)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrateur,RessourcesHumaines,Employe")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var commande = await _context.Commandes
                    .FirstOrDefaultAsync(c => c.IdCommande == id && c.Supprimer == 0);

                if (commande == null)
                {
                    TempData["ErrorMessage"] = "Commande introuvable.";
                    return RedirectToAction(nameof(Index));
                }

                // Soft delete
                commande.Supprimer = 1;
                commande.ModifiedOn = DateTime.UtcNow;
                commande.ModifiedBy = User.Identity?.Name ?? "System";

                await _context.SaveChangesAsync();

                _logger.LogInformation("Commande supprimée: {CodeCommande} par {DeletedBy}",
                    commande.CodeCommande, User.Identity?.Name);
                TempData["SuccessMessage"] = $"La commande {commande.CodeCommande} a été supprimée avec succès.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression de la commande {CommandeId}", id);
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la suppression de la commande.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Génère un code de commande unique
        /// </summary>
        private string GenerateCodeCommande()
        {
            var date = DateTime.UtcNow;
            var prefix = $"CMD{date:yyyyMMdd}";
            // suffix aléatoire cryptographiquement fort (4 chiffres)
            var bytes = new byte[2];
            RandomNumberGenerator.Fill(bytes);
            var rnd = BitConverter.ToUInt16(bytes, 0) % 9000 + 1000;
            return $"{prefix}{rnd}";
        }

        /// <summary>
        /// Retourne le lundi et le vendredi de la semaine N+1 (prochaine semaine ouvrée)
        /// </summary>
        private (DateTime Lundi, DateTime Vendredi) GetSemaineSuivanteOuvree()
        {
            var today = DateTime.Today;
            int diffToMonday = ((int)today.DayOfWeek + 6) % 7; // Lundi=0
            var thisWeekMonday = today.AddDays(-diffToMonday).Date;

            var nextWeekMonday = thisWeekMonday.AddDays(7);
            var nextWeekFriday = nextWeekMonday.AddDays(4);
            return (nextWeekMonday, nextWeekFriday);
        }

        /// <summary>
        /// Retourne le lundi et le dimanche de la semaine N+1 (semaine complète)
        /// </summary>
        private (DateTime Lundi, DateTime Dimanche) GetSemaineSuivanteComplete()
        {
            var today = DateTime.Today;
            int diffToMonday = ((int)today.DayOfWeek + 6) % 7; // Lundi=0
            var thisWeekMonday = today.AddDays(-diffToMonday).Date;

            var nextWeekMonday = thisWeekMonday.AddDays(7);
            var nextWeekSunday = nextWeekMonday.AddDays(6);
            return (nextWeekMonday, nextWeekSunday);
        }

        /// <summary>
        /// Extrait le nom du plat selon le type de formule
        /// </summary>
        private string GetNomPlatFromFormule(FormuleJour? formule)
        {
            if (formule == null) return "Plat non spécifié";

            // Déterminer le type de formule basé sur le nom
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
                    // Fallback : essayer de déterminer le type basé sur les champs remplis
                    if (!string.IsNullOrEmpty(formule.Plat)) return formule.Plat;
                    if (!string.IsNullOrEmpty(formule.PlatStandard1)) return formule.PlatStandard1;
                    if (!string.IsNullOrEmpty(formule.PlatStandard2)) return formule.PlatStandard2;
                    return "Plat du jour";
            }
        }

        /// <summary>
        /// Crée un point de consommation quand une commande est validée par le prestataire
        /// </summary>
        private async Task CreerPointConsommationAsync(Commande commande)
        {
            try
            {
                // Vérifier si un point de consommation existe déjà pour cette commande
                var pointExistant = await _context.PointsConsommation
                    .AsNoTracking()
                    .AnyAsync(pc => pc.CommandeId == commande.IdCommande && pc.Supprimer == 0);

                if (pointExistant)
                {
                    _logger.LogInformation("Point de consommation déjà existant pour la commande {CommandeId}", commande.IdCommande);
                    return;
                }

                // Récupérer les informations de la formule
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
                var typeFormule = formule.NomFormule ?? "Standard";

                var pointConsommation = new PointConsommation
                {
                    IdPointConsommation = Guid.NewGuid(),
                    UtilisateurId = commande.UtilisateurId ?? Guid.Empty,
                    CommandeId = commande.IdCommande,
                    DateConsommation = commande.DateConsommation ?? DateTime.Today,
                    TypeFormule = typeFormule,
                    NomPlat = nomPlat,
                    QuantiteConsommee = commande.Quantite,
                    LieuConsommation = "Restaurant CIT", // Par défaut
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = User.Identity?.Name ?? "System",
                    Supprimer = 0
                };

                _context.PointsConsommation.Add(pointConsommation);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Point de consommation créé pour la commande {CommandeId}: {NomPlat}", 
                    commande.IdCommande, nomPlat);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création du point de consommation pour la commande {CommandeId}", 
                    commande.IdCommande);
            }
        }

        /// <summary>
        /// Calcule le prix d'une formule selon son type
        /// </summary>
        private decimal GetPrixFormule(string nomFormule)
        {
            if (string.IsNullOrEmpty(nomFormule))
                return 550m; // Prix par défaut pour Standard

            var nomFormuleLower = nomFormule.ToLower().Trim();
            
            return nomFormuleLower switch
            {
                "amélioré" or "ameliore" => 2800m,
                "standard 1" or "standard1" => 550m,
                "standard 2" or "standard2" => 550m,
                _ => 550m // Prix par défaut pour les autres types
            };
        }

        /// <summary>
        /// Récupère l'ID de l'utilisateur actuellement connecté
        /// </summary>
        private Guid? GetCurrentUserId()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                // Essayer d'abord avec l'ID directement depuis les claims
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var userId))
                {
                    return userId;
                }

                // Fallback : chercher par UserName depuis les claims
                var userNameClaim = User.FindFirst("UserName")?.Value;
                if (!string.IsNullOrEmpty(userNameClaim))
            {
                var utilisateur = _context.Utilisateurs
                    .AsNoTracking()
                        .FirstOrDefault(u => u.UserName == userNameClaim && u.Supprimer == 0);
                return utilisateur?.Id;
                }
            }
            return null;
        }

        /// <summary>
        /// Exporte les commandes en Excel
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExporterExcel(string? status = null, DateTime? dateDebut = null, DateTime? dateFin = null)
        {
            try
            {
                // Déterminer si l'utilisateur est un employé (ne voit que ses commandes)
                var isEmploye = User.IsInRole("Employe") && !User.IsInRole("Administrateur") && !User.IsInRole("RessourcesHumaines");
                var currentUserId = GetCurrentUserId();

                var query = _context.Commandes
                    .AsNoTracking()
                    .Include(c => c.Utilisateur)
                    .Include(c => c.GroupeNonCit)
                    .Include(c => c.FormuleJour)!.ThenInclude(f => f!.NomFormuleNavigation)
                    .Where(c => c.Supprimer == 0);

                // Filtrer par utilisateur si c'est un employé (pas admin/RH)
                if (isEmploye && currentUserId.HasValue)
                {
                    query = query.Where(c => c.UtilisateurId == currentUserId);
                }

                // Filtrer par statut si spécifié
                if (!string.IsNullOrEmpty(status) && Enum.TryParse<StatutCommande>(status, out var statutCommande))
                {
                    query = query.Where(c => c.StatusCommande == (int)statutCommande);
                }

                // Filtrer par période si spécifiée
                if (dateDebut.HasValue)
                {
                    query = query.Where(c => c.DateConsommation.HasValue && c.DateConsommation.Value.Date >= dateDebut.Value.Date);
                }
                if (dateFin.HasValue)
                {
                    query = query.Where(c => c.DateConsommation.HasValue && c.DateConsommation.Value.Date <= dateFin.Value.Date);
                }

                var commandes = await query
                    .OrderByDescending(c => c.Date)
                    .ToListAsync();

                if (!commandes.Any())
                {
                    TempData["InfoMessage"] = "Aucune commande trouvée pour l'export.";
                    return RedirectToAction("Index");
                }

                using var workbook = new ClosedXML.Excel.XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Commandes");

                // En-têtes
                var headers = new[]
                {
                    "Code Commande", "Date", "Date Consommation", "Client", "Type Client", "Site",
                    "Formule", "Nom Plat", "Statut", "Période", "Quantité", "Montant", "Instantanée"
                };
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cell(1, i + 1).Value = headers[i];
                }
                worksheet.Row(1).Style.Font.Bold = true;
                worksheet.Row(1).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightBlue;

                // Données
                int row = 2;
                foreach (var cmd in commandes)
                {
                    var nomPlat = GetNomPlatFromFormule(cmd.FormuleJour);
                    var clientNom = "";
                    var typeClient = cmd.TypeClient.ToString();
                    
                    switch (cmd.TypeClient)
                    {
                        case TypeClientCommande.CitUtilisateur:
                            clientNom = $"{cmd.Utilisateur?.Nom} {cmd.Utilisateur?.Prenoms}";
                            break;
                        case TypeClientCommande.GroupeNonCit:
                            clientNom = cmd.GroupeNonCit?.Nom ?? "N/A";
                            break;
                        case TypeClientCommande.Visiteur:
                            clientNom = cmd.VisiteurNom ?? "N/A";
                            break;
                    }

                    worksheet.Cell(row, 1).Value = cmd.CodeCommande;
                    worksheet.Cell(row, 2).Value = cmd.Date.ToString("dd/MM/yyyy HH:mm");
                    worksheet.Cell(row, 3).Value = cmd.DateConsommation?.ToString("dd/MM/yyyy HH:mm") ?? "N/A";
                    worksheet.Cell(row, 4).Value = clientNom;
                    worksheet.Cell(row, 5).Value = typeClient;
                    worksheet.Cell(row, 6).Value = cmd.Site?.ToString() ?? "N/A";
                    worksheet.Cell(row, 7).Value = cmd.FormuleJour?.NomFormule ?? "N/A";
                    worksheet.Cell(row, 8).Value = nomPlat;
                    worksheet.Cell(row, 9).Value = ((StatutCommande)cmd.StatusCommande).ToString();
                    worksheet.Cell(row, 10).Value = cmd.PeriodeService.ToString();
                    worksheet.Cell(row, 11).Value = cmd.Quantite;
                    worksheet.Cell(row, 12).Value = cmd.Montant;
                    worksheet.Cell(row, 13).Value = cmd.Instantanee ? "Oui" : "Non";
                    row++;
                }

                worksheet.Columns().AdjustToContents();

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                var content = stream.ToArray();

                var fileName = $"Commandes_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                if (dateDebut.HasValue && dateFin.HasValue)
                {
                    fileName = $"Commandes_{dateDebut.Value:yyyyMMdd}_{dateFin.Value:yyyyMMdd}.xlsx";
                }

                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'export Excel des commandes");
                TempData["ErrorMessage"] = "Une erreur est survenue lors de l'export Excel.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Affiche le formulaire pour vérifier une commande par matricule
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Administrateur,PrestataireCantine")]
        public IActionResult VerifierCommande()
        {
            return View();
        }

        /// <summary>
        /// Traite la vérification d'une commande par matricule
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrateur,PrestataireCantine")]
        public async Task<IActionResult> VerifierCommande(string matricule)
        {
            if (string.IsNullOrWhiteSpace(matricule))
            {
                TempData["ErrorMessage"] = "Veuillez saisir un matricule.";
                return View();
            }

            try
            {
                var aujourdhui = DateTime.Today;
                
                var commande = await _context.Commandes
                    .AsNoTracking()
                    .Include(c => c.Utilisateur)
                    .Include(c => c.FormuleJour)
                    .Include(c => c.FormuleJour!.NomFormuleNavigation)
                    .Where(c => c.Supprimer == 0 
                             && c.Utilisateur != null 
                             && c.Utilisateur.UserName == matricule.Trim()
                             && c.DateConsommation.HasValue 
                             && c.DateConsommation.Value.Date == aujourdhui)
                    .FirstOrDefaultAsync();

                if (commande == null)
                {
                    TempData["ErrorMessage"] = "Aucune commande trouvée pour ce matricule aujourd'hui.";
                    return View();
                }

                // Convertir en ViewModel pour l'affichage
                var commandeViewModel = new CommandeListViewModel
                {
                    IdCommande = commande.IdCommande,
                    Date = commande.Date,
                    DateConsommation = commande.DateConsommation,
                    StatusCommande = (StatutCommande)commande.StatusCommande,
                    CodeCommande = commande.CodeCommande,
                    PeriodeService = commande.PeriodeService,
                    Montant = commande.Montant,
                    Quantite = commande.Quantite,
                    Instantanee = commande.Instantanee,
                    AnnuleeParPrestataire = commande.AnnuleeParPrestataire,
                    MotifAnnulation = commande.MotifAnnulation,
                    TypeClient = commande.TypeClient,
                    Site = commande.Site,
                    DateLivraisonPrevueUtc = commande.DateLivraisonPrevueUtc,
                    DateReceptionUtc = commande.DateReceptionUtc,
                    UtilisateurNomComplet = commande.Utilisateur != null ? $"{commande.Utilisateur.Nom} {commande.Utilisateur.Prenoms}" : null,
                    GroupeNonCitNom = null,
                    VisiteurNom = commande.VisiteurNom,
                    VisiteurTelephone = commande.VisiteurTelephone,
                    FormuleNom = commande.FormuleJour != null ? commande.FormuleJour.NomFormule : null,
                    FormuleDate = commande.FormuleJour != null ? commande.FormuleJour.Date : null,
                    TypeFormuleNom = commande.FormuleJour != null ? commande.FormuleJour.NomFormule : null,
                    NomPlat = GetNomPlatFromFormule(commande.FormuleJour),
                    CreatedOn = commande.CreatedOn,
                    CreatedBy = commande.CreatedBy
                };

                return View("Details_Test", commandeViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la vérification de la commande pour le matricule {Matricule}", matricule);
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la vérification de la commande.";
                return View();
            }
        }

        /// <summary>
        /// Valide une commande (change le statut à Consommée)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrateur,PrestataireCantine")]
        public async Task<IActionResult> ValiderCommande(Guid id)
        {
            try
            {
                var commande = await _context.Commandes
                    .FirstOrDefaultAsync(c => c.IdCommande == id && c.Supprimer == 0);

                if (commande == null)
                {
                    TempData["ErrorMessage"] = "Commande non trouvée.";
                    return RedirectToAction("VerifierCommande");
                }

                // Vérifier que la commande n'est pas déjà validée
                if (commande.StatusCommande == (int)StatutCommande.Consommee)
                {
                    TempData["ErrorMessage"] = "Cette commande est déjà validée.";
                    return RedirectToAction("VerifierCommande");
                }

                // Changer le statut à Consommée
                commande.StatusCommande = (int)StatutCommande.Consommee;
                commande.ModifiedOn = DateTime.UtcNow;
                commande.ModifiedBy = User.Identity?.Name ?? "Prestataire";

                await _context.SaveChangesAsync();

                // Créer un point de consommation pour cette commande validée
                await CreerPointConsommationAsync(commande);

                TempData["SuccessMessage"] = "Commande validée avec succès.";
                return RedirectToAction("VerifierCommande");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la validation de la commande {IdCommande}", id);
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la validation de la commande.";
                return RedirectToAction("VerifierCommande");
            }
        }

        /// <summary>
        /// Annule une commande avec motif
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrateur,PrestataireCantine")]
        public async Task<IActionResult> AnnulerCommande(Guid id, string motif)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(motif))
                {
                    TempData["ErrorMessage"] = "Veuillez spécifier un motif d'annulation.";
                    return RedirectToAction("VerifierCommande");
                }

                var commande = await _context.Commandes
                    .FirstOrDefaultAsync(c => c.IdCommande == id && c.Supprimer == 0);

                if (commande == null)
                {
                    TempData["ErrorMessage"] = "Commande non trouvée.";
                    return RedirectToAction("VerifierCommande");
                }

                // Vérifier que la commande n'est pas déjà annulée
                if (commande.StatusCommande == (int)StatutCommande.Annulee)
                {
                    TempData["ErrorMessage"] = "Cette commande est déjà annulée.";
                    return RedirectToAction("VerifierCommande");
                }

                // Changer le statut à Annulée
                commande.StatusCommande = (int)StatutCommande.Annulee;
                commande.ModifiedOn = DateTime.UtcNow;
                commande.ModifiedBy = User.Identity?.Name ?? "Prestataire";

                // Le motif d'annulation est stocké dans TempData pour affichage
                // Dans une version future, on pourrait ajouter un champ dédié pour le motif

                await _context.SaveChangesAsync();

                // Envoyer une notification aux prestataires pour l'annulation
                await SendNotificationToPrestataires(
                    $"Commande annulée: {commande.CodeCommande} pour le {commande.DateConsommation:dd/MM/yyyy}. Motif: {motif}",
                    Enums.TypeNotification.Error
                );

                TempData["SuccessMessage"] = $"Commande annulée avec succès. Motif: {motif}";
                return RedirectToAction("VerifierCommande");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'annulation de la commande {IdCommande}", id);
                TempData["ErrorMessage"] = "Une erreur est survenue lors de l'annulation de la commande.";
                return RedirectToAction("VerifierCommande");
            }
        }

        /// <summary>
        /// Affiche le formulaire de création de commande instantanée pour visiteurs et groupes non-CIT
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Administrateur,RessourcesHumaines,PrestataireCantine")]
        public async Task<IActionResult> CreerCommandeInstantanee()
        {
            try
            {
                var aujourdhui = DateTime.Today;
                ViewBag.DateAujourdhui = aujourdhui;

                // Récupérer les formules du jour
                var formulesAujourdhui = await _context.FormulesJour
                    .Where(f => f.Date.Date == aujourdhui && f.Supprimer == 0)
                    .OrderBy(f => f.NomFormule)
                    .ToListAsync();

                if (!formulesAujourdhui.Any())
                {
                    TempData["ErrorMessage"] = "Aucune formule n'est disponible pour aujourd'hui. Veuillez d'abord créer des formules pour cette date.";
                    await PopulateViewBagsForInstantOrder();
                    return View();
                }

                // Récupérer les groupes non-CIT
                var groupesNonCit = await _context.GroupesNonCit
                    .Where(g => g.Supprimer == 0)
                    .OrderBy(g => g.Nom)
                    .ToListAsync();

                // Récupérer les utilisateurs CIT
                var utilisateurs = await _context.Utilisateurs
                    .Where(u => u.Supprimer == 0)
                    .OrderBy(u => u.Nom)
                    .ThenBy(u => u.Prenoms)
                    .Select(u => new
                    {
                        Value = u.Id.ToString(),
                        Text = $"{u.Nom} {u.Prenoms}",
                        UserName = u.UserName
                    })
                    .ToListAsync();

                // Créer le SelectList avec les attributs data pour la recherche par matricule
                var selectListItems = utilisateurs.Select(u => new SelectListItem
                {
                    Value = u.Value,
                    Text = u.Text
                }).ToList();

                // Créer un SelectList personnalisé avec le nom du menu et les plats
                var formulesSelectList = formulesAujourdhui.Select(f => new
                {
                    IdFormule = f.IdFormule,
                    DisplayText = GetFormuleDisplayText(f)
                }).ToList();

                ViewBag.FormulesAujourdhui = new SelectList(formulesSelectList, "IdFormule", "DisplayText");
                ViewBag.GroupesNonCit = new SelectList(groupesNonCit, "Id", "Nom");
                ViewBag.Utilisateurs = new SelectList(selectListItems, "Value", "Text");
                ViewBag.UtilisateursData = utilisateurs.ToDictionary(u => u.Value, u => u.UserName);

                // Types de clients
                var typesClient = new[]
                {
                    new { Value = TypeClientCommande.GroupeNonCit.ToString(), Text = "Groupe non-CIT" },
                    new { Value = TypeClientCommande.CitUtilisateur.ToString(), Text = "Groupe CIT" }
                };
                ViewBag.TypeClients = new SelectList(typesClient, "Value", "Text");

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement du formulaire de commande instantanée");
                TempData["ErrorMessage"] = "Une erreur est survenue lors du chargement du formulaire.";
                await PopulateViewBagsForInstantOrder();
                return View();
            }
        }

        /// <summary>
        /// Traite la création de commande instantanée
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrateur,RessourcesHumaines,PrestataireCantine")]
        public async Task<IActionResult> CreerCommandeInstantanee(CreerCommandeInstantaneeViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await PopulateViewBagsForInstantOrder();
                    return View(model);
                }

                // Vérifier que la formule existe
                var formule = await _context.FormulesJour
                    .FirstOrDefaultAsync(f => f.IdFormule == model.IdFormule && f.Supprimer == 0);

                if (formule == null)
                {
                    ModelState.AddModelError(nameof(model.IdFormule), "La formule sélectionnée n'existe pas.");
                    await PopulateViewBagsForInstantOrder();
                    return View(model);
                }

                // Validation selon le type de client
                switch (model.TypeClient)
                {
                    case TypeClientCommande.GroupeNonCit:
                        if (!model.GroupeNonCitId.HasValue || model.GroupeNonCitId == Guid.Empty)
                        {
                            ModelState.AddModelError(nameof(model.GroupeNonCitId), "Le groupe non-CIT est obligatoire pour ce type de client.");
                            await PopulateViewBagsForInstantOrder();
                            return View(model);
                        }

                        // Vérifier que le groupe existe
                        var groupe = await _context.GroupesNonCit
                            .FirstOrDefaultAsync(g => g.Id == model.GroupeNonCitId && g.Supprimer == 0);

                        if (groupe == null)
                        {
                            ModelState.AddModelError(nameof(model.GroupeNonCitId), "Le groupe non-CIT sélectionné n'existe pas.");
                            await PopulateViewBagsForInstantOrder();
                            return View(model);
                        }

                        // Les groupes non-CIT peuvent avoir N quantités (pas de restriction)
                        break;

                    case TypeClientCommande.CitUtilisateur:
                        if (!model.UtilisateurId.HasValue || model.UtilisateurId == Guid.Empty)
                        {
                            ModelState.AddModelError(nameof(model.UtilisateurId), "L'utilisateur CIT est obligatoire pour ce type de client.");
                            await PopulateViewBagsForInstantOrder();
                            return View(model);
                        }

                        // Vérifier l'état des commandes existantes pour cet utilisateur CIT aujourd'hui
                        var commandesExistantes = await _context.Commandes
                            .Include(c => c.Utilisateur)
                            .Include(c => c.FormuleJour)
                            .Where(c => c.UtilisateurId == model.UtilisateurId 
                                && c.Instantanee == true 
                                && c.DateConsommation.HasValue && c.DateConsommation.Value.Date == DateTime.Today
                                && c.Supprimer == 0)
                            .OrderByDescending(c => c.Date)
                            .ToListAsync();

                        if (commandesExistantes.Any())
                        {
                            var commandeRecente = commandesExistantes.First();
                            var utilisateur = commandeRecente.Utilisateur;
                            var nomUtilisateur = $"{utilisateur?.Nom} {utilisateur?.Prenoms}".Trim();
                            var matricule = utilisateur?.UserName ?? "N/A";
                            
                            string messageErreur;
                            string icone;
                            
                            switch ((StatutCommande)commandeRecente.StatusCommande)
                            {
                                case StatutCommande.Precommander:
                                    icone = "⏳";
                                    messageErreur = $"{icone} <strong>{nomUtilisateur} ({matricule})</strong> a déjà une commande instantanée <strong>en attente de validation</strong> pour aujourd'hui.<br/>" +
                                                  $"📅 <strong>Date de consommation :</strong> {commandeRecente.DateConsommation?.ToString("dd/MM/yyyy")}<br/>" +
                                                  $"🍽️ <strong>Formule :</strong> {commandeRecente.FormuleJour?.NomFormule ?? "N/A"}<br/>" +
                                                  $"⏰ <strong>Créée le :</strong> {commandeRecente.Date.ToString("dd/MM/yyyy à HH:mm")}<br/>" +
                                                  $"<em>Veuillez attendre que le prestataire valide ou annule cette commande avant d'en créer une nouvelle.</em>";
                                    break;
                                    
                                case StatutCommande.Consommee:
                                    icone = "✅";
                                    messageErreur = $"{icone} <strong>{nomUtilisateur} ({matricule})</strong> a déjà une commande instantanée <strong>consommée</strong> pour aujourd'hui.<br/>" +
                                                  $"📅 <strong>Date de consommation :</strong> {commandeRecente.DateConsommation?.ToString("dd/MM/yyyy")}<br/>" +
                                                  $"🍽️ <strong>Formule :</strong> {commandeRecente.FormuleJour?.NomFormule ?? "N/A"}<br/>" +
                                                  $"⏰ <strong>Créée le :</strong> {commandeRecente.Date.ToString("dd/MM/yyyy à HH:mm")}<br/>" +
                                                  $"<em>Cette commande a déjà été consommée et ne peut plus être modifiée.</em>";
                                    break;
                                    
                                case StatutCommande.Annulee:
                                    icone = "❌";
                                    messageErreur = $"{icone} <strong>{nomUtilisateur} ({matricule})</strong> a une commande instantanée <strong>annulée par le prestataire</strong> pour aujourd'hui.<br/>" +
                                                  $"📅 <strong>Date de consommation :</strong> {commandeRecente.DateConsommation?.ToString("dd/MM/yyyy")}<br/>" +
                                                  $"🍽️ <strong>Formule :</strong> {commandeRecente.FormuleJour?.NomFormule ?? "N/A"}<br/>" +
                                                  $"⏰ <strong>Créée le :</strong> {commandeRecente.Date.ToString("dd/MM/yyyy à HH:mm")}<br/>" +
                                                  $"<em>Vous pouvez créer une nouvelle commande pour remplacer celle qui a été annulée.</em>";
                                    break;
                                    
                                default:
                                    icone = "⚠️";
                                    messageErreur = $"{icone} <strong>{nomUtilisateur} ({matricule})</strong> a déjà une commande instantanée pour aujourd'hui avec un statut inconnu.";
                                    break;
                            }
                            
                            ModelState.AddModelError(nameof(model.UtilisateurId), messageErreur);
                            await PopulateViewBagsForInstantOrder();
                            return View(model);
                        }

                        // Pour les groupes CIT, forcer la quantité à 1
                        model.Quantite = 1;
                        break;
                }

                // Créer la commande instantanée
                var commande = new Commande
                {
                    IdCommande = Guid.NewGuid(),
                    Date = DateTime.Now,
                    DateConsommation = DateTime.Today, // Commande pour aujourd'hui
                    StatusCommande = (int)StatutCommande.Precommander,
                    CodeCommande = await GenerateCommandeCodeAsync(),
                    PeriodeService = model.PeriodeService,
                    Montant = CalculateMontant(formule, model.Quantite),
                    Quantite = model.Quantite,
                    IdFormule = model.IdFormule,
                    TypeClient = model.TypeClient,
                    UtilisateurId = model.UtilisateurId,
                    GroupeNonCitId = model.GroupeNonCitId,
                    VisiteurNom = string.IsNullOrWhiteSpace(model.VisiteurNom) ? null : model.VisiteurNom.Trim(),
                    VisiteurTelephone = string.IsNullOrWhiteSpace(model.VisiteurTelephone) ? null : model.VisiteurTelephone.Trim(),
                    DirectionId = model.DirectionId,
                    CodeVerification = string.IsNullOrWhiteSpace(model.CodeVerification) ? null : model.CodeVerification.Trim(),
                    Site = model.Site,
                    Instantanee = true, // Marquer comme commande instantanée
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = User.Identity?.Name ?? "System"
                };

                _context.Commandes.Add(commande);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Commande instantanée créée avec succès - ID: {IdCommande}, Type: {TypeClient}", 
                    commande.IdCommande, commande.TypeClient);

                TempData["SuccessMessage"] = "Commande instantanée créée avec succès !";
                return RedirectToAction("Details", new { id = commande.IdCommande });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création de la commande instantanée");
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la création de la commande instantanée.";
                await PopulateViewBagsForInstantOrder();
                return View(model);
            }
        }

        /// <summary>
        /// Génère le texte d'affichage pour une formule avec les noms des plats
        /// </summary>
        private string GetFormuleDisplayText(FormuleJour formule)
        {
            var dateStr = formule.Date.ToString("dd/MM/yyyy");
            var nomFormule = formule.NomFormule ?? "Formule";
            
            var plats = new List<string>();
            
            // Vérifier si c'est une formule améliorée (a des plats améliorés)
            if (!string.IsNullOrWhiteSpace(formule.Plat) || !string.IsNullOrWhiteSpace(formule.Entree))
            {
                if (!string.IsNullOrWhiteSpace(formule.Entree))
                    plats.Add($"Entrée: {formule.Entree}");
                if (!string.IsNullOrWhiteSpace(formule.Plat))
                    plats.Add($"Plat: {formule.Plat}");
                if (!string.IsNullOrWhiteSpace(formule.Garniture))
                    plats.Add($"Garniture: {formule.Garniture}");
                if (!string.IsNullOrWhiteSpace(formule.Dessert))
                    plats.Add($"Dessert: {formule.Dessert}");
            }
            // Vérifier si c'est une formule standard (a des plats standard)
            else if (!string.IsNullOrWhiteSpace(formule.PlatStandard1) || !string.IsNullOrWhiteSpace(formule.PlatStandard2))
            {
                if (!string.IsNullOrWhiteSpace(formule.PlatStandard1))
                    plats.Add($"Plat 1: {formule.PlatStandard1}");
                if (!string.IsNullOrWhiteSpace(formule.GarnitureStandard1))
                    plats.Add($"Garniture 1: {formule.GarnitureStandard1}");
                if (!string.IsNullOrWhiteSpace(formule.PlatStandard2))
                    plats.Add($"Plat 2: {formule.PlatStandard2}");
                if (!string.IsNullOrWhiteSpace(formule.GarnitureStandard2))
                    plats.Add($"Garniture 2: {formule.GarnitureStandard2}");
            }
            
            // Ajouter les éléments communs
            if (!string.IsNullOrWhiteSpace(formule.Feculent))
                plats.Add($"Féculent: {formule.Feculent}");
            if (!string.IsNullOrWhiteSpace(formule.Legumes))
                plats.Add($"Légumes: {formule.Legumes}");
            
            var platsStr = plats.Any() ? $" - {string.Join(", ", plats)}" : "";
            
            return $"Menu du {dateStr} - {nomFormule}{platsStr}";
        }

        /// <summary>
        /// Popule les ViewBags pour le formulaire de commande instantanée
        /// </summary>
        private async Task PopulateViewBagsForInstantOrder()
        {
            var aujourdhui = DateTime.Today;
            ViewBag.DateAujourdhui = aujourdhui;

            var formulesAujourdhui = await _context.FormulesJour
                .Where(f => f.Date.Date == aujourdhui && f.Supprimer == 0)
                .OrderBy(f => f.NomFormule)
                .ToListAsync();

            var groupesNonCit = await _context.GroupesNonCit
                .Where(g => g.Supprimer == 0)
                .OrderBy(g => g.Nom)
                .ToListAsync();

            var utilisateurs = await _context.Utilisateurs
                .Where(u => u.Supprimer == 0)
                .OrderBy(u => u.Nom)
                .ThenBy(u => u.Prenoms)
                .Select(u => new
                {
                    Value = u.Id.ToString(),
                    Text = $"{u.Nom} {u.Prenoms}",
                    UserName = u.UserName
                })
                .ToListAsync();

            // Créer le SelectList avec les attributs data pour la recherche par matricule
            var selectListItems = utilisateurs.Select(u => new SelectListItem
            {
                Value = u.Value,
                Text = u.Text
            }).ToList();

            var directions = await _context.Directions
                .Where(d => d.Supprimer == 0)
                .OrderBy(d => d.Nom)
                .ToListAsync();

            // Créer un SelectList personnalisé avec le nom du menu et les plats
            var formulesSelectList = formulesAujourdhui.Select(f => new
            {
                IdFormule = f.IdFormule,
                DisplayText = GetFormuleDisplayText(f)
            }).ToList();

            ViewBag.FormulesAujourdhui = new SelectList(formulesSelectList, "IdFormule", "DisplayText");
            ViewBag.GroupesNonCit = new SelectList(groupesNonCit, "Id", "Nom");
            ViewBag.Utilisateurs = new SelectList(selectListItems, "Value", "Text");
            ViewBag.UtilisateursData = utilisateurs.ToDictionary(u => u.Value, u => u.UserName);
            ViewBag.Directions = new SelectList(directions, "Id", "Nom");

            var typesClient = new[]
            {
                new { Value = TypeClientCommande.GroupeNonCit.ToString(), Text = "Groupe non-CIT" },
                new { Value = TypeClientCommande.CitUtilisateur.ToString(), Text = "Groupe CIT" }
            };
            ViewBag.TypeClients = new SelectList(typesClient, "Value", "Text");
        }

        /// <summary>
        /// Affiche le formulaire de création de commande groupée
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Administrateur,RessourcesHumaines")]
        public async Task<IActionResult> CreerCommandeGroupee()
        {
            try
            {
                // Récupérer les groupes non-CIT
                var groupesNonCit = await _context.GroupesNonCit
                    .Where(g => g.Supprimer == 0)
                    .OrderBy(g => g.Nom)
                    .ToListAsync();

                if (!groupesNonCit.Any())
                {
                    TempData["ErrorMessage"] = "Aucun groupe non-CIT n'est configuré. Veuillez d'abord créer des groupes non-CIT.";
                    return RedirectToAction("Index");
                }

                ViewBag.GroupesNonCit = new SelectList(groupesNonCit, "Id", "Nom");

                var model = new CommandeGroupeeViewModel
                {
                    Date = DateTime.Today
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement du formulaire de commande groupée");
                TempData["ErrorMessage"] = "Une erreur est survenue lors du chargement du formulaire.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Traite la création de commande groupée
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrateur,RessourcesHumaines")]
        public async Task<IActionResult> CreerCommandeGroupee(CommandeGroupeeViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await PopulateViewBagsForGroupOrder();
                    return View(model);
                }

                // Vérifier que le groupe existe
                var groupe = await _context.GroupesNonCit
                    .FirstOrDefaultAsync(g => g.Id == model.GroupeNonCitId && g.Supprimer == 0);

                if (groupe == null)
                {
                    ModelState.AddModelError(nameof(model.GroupeNonCitId), "Le groupe non-CIT sélectionné n'existe pas.");
                    await PopulateViewBagsForGroupOrder();
                    return View(model);
                }

                // Vérifier que la formule existe
                var formule = await _context.FormulesJour
                    .FirstOrDefaultAsync(f => f.IdFormule == model.IdFormule && f.Supprimer == 0);

                if (formule == null)
                {
                    ModelState.AddModelError(nameof(model.IdFormule), "La formule sélectionnée n'existe pas.");
                    await PopulateViewBagsForGroupOrder();
                    return View(model);
                }

                // Vérifier les quotas si applicable (temporairement commenté)
                /*
                var quota = await _context.QuotasJournaliers
                    .FirstOrDefaultAsync(q => q.GroupeNonCitId == model.GroupeNonCitId && 
                                            q.Date.Date == model.Date.Date && 
                                            q.Supprimer == 0);

                if (quota != null)
                {
                    var quotaDisponible = model.PeriodeService == Periode.Jour ? 
                        quota.PlatsRestantsJour : quota.PlatsRestantsNuit;

                    if (model.Quantite > quotaDisponible)
                    {
                        ModelState.AddModelError(nameof(model.Quantite), 
                            $"Quota insuffisant. Disponible : {quotaDisponible}, Demandé : {model.Quantite}");
                        await PopulateViewBagsForGroupOrder();
                        return View(model);
                    }

                    // Vérifier la restriction de formule standard pour les Douaniers
                    if (quota.RestrictionFormuleStandard && 
                        !formule.NomFormule?.ToUpper().Contains("STANDARD") == true)
                    {
                        ModelState.AddModelError(nameof(model.IdFormule), 
                            "Ce groupe est restreint aux formules standard uniquement.");
                        await PopulateViewBagsForGroupOrder();
                        return View(model);
                    }
                }
                */

                // Créer la commande groupée
                var commande = new Commande
                {
                    IdCommande = Guid.NewGuid(),
                    Date = DateTime.Now,
                    DateConsommation = model.Date,
                    StatusCommande = (int)StatutCommande.Precommander,
                    CodeCommande = await GenerateCommandeCodeAsync(),
                    PeriodeService = model.PeriodeService,
                    Montant = CalculateMontant(formule, model.Quantite),
                    Quantite = model.Quantite,
                    IdFormule = model.IdFormule,
                    TypeClient = TypeClientCommande.GroupeNonCit,
                    GroupeNonCitId = model.GroupeNonCitId,
                    Site = model.Site,
                    Instantanee = false,
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = User.Identity?.Name ?? "System"
                };

                _context.Commandes.Add(commande);

                // Mettre à jour les quotas si applicable (temporairement commenté)
                /*
                if (quota != null)
                {
                    if (model.PeriodeService == Periode.Jour)
                        quota.PlatsConsommesJour += model.Quantite;
                    else
                        quota.PlatsConsommesNuit += model.Quantite;

                    quota.ModifiedOn = DateTime.UtcNow;
                    quota.ModifiedBy = User.Identity?.Name ?? "System";
                }
                */

                await _context.SaveChangesAsync();

                _logger.LogInformation("Commande groupée créée avec succès - ID: {IdCommande}, Groupe: {GroupeNom}, Quantité: {Quantite}", 
                    commande.IdCommande, groupe.Nom, commande.Quantite);

                TempData["SuccessMessage"] = $"Commande groupée créée avec succès pour {groupe.Nom} !";
                return RedirectToAction("Details", new { id = commande.IdCommande });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création de la commande groupée");
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la création de la commande groupée.";
                await PopulateViewBagsForGroupOrder();
                return View(model);
            }
        }

        /// <summary>
        /// Retourne les menus par type de formule pour la semaine N+1
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMenusByType(string typeFormule, DateTime? date)
        {
            try
            {
                if (string.IsNullOrEmpty(typeFormule))
                {
                    return Json(new List<object>());
                }

                // Vérifier si les commandes sont bloquées
                var isBlocked = await _configurationService.IsCommandeBlockedAsync();
                if (isBlocked)
                {
                    return Json(new List<object>());
                }

                // Calculer la semaine N+1
                var (lundiN1, vendrediN1) = GetSemaineSuivanteOuvree();
                
                var menus = await _context.FormulesJour
                    .Where(f => f.Supprimer == 0 
                             && f.Date >= lundiN1 
                             && f.Date <= vendrediN1
                             && f.NomFormule == typeFormule)
                    .OrderBy(f => f.Date)
                    .ToListAsync();

                var menusList = menus.Select(f => new
                {
                    value = f.IdFormule.ToString(),
                    text = GetFormuleDisplayText(f)
                }).ToList();

                return Json(menusList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement des menus par type");
                return Json(new List<object>());
            }
        }

        /// <summary>
        /// Popule les ViewBags pour le formulaire de commande groupée
        /// </summary>
        private async Task PopulateViewBagsForGroupOrder()
        {
            var groupesNonCit = await _context.GroupesNonCit
                .Where(g => g.Supprimer == 0)
                .OrderBy(g => g.Nom)
                .ToListAsync();

            ViewBag.GroupesNonCit = new SelectList(groupesNonCit, "Id", "Nom");
        }

        /// <summary>
        /// Génère un code de commande unique
        /// </summary>
        private async Task<string> GenerateCommandeCodeAsync()
        {
            var today = DateTime.Today;
            var count = await _context.Commandes
                .Where(c => c.Date.Date == today)
                .CountAsync();
            
            return $"CMD{today:yyyyMMdd}{(count + 1):D4}";
        }

        /// <summary>
        /// Calcule le montant d'une commande
        /// </summary>
        private decimal CalculateMontant(FormuleJour formule, int quantite)
        {
            // Prix par défaut selon le type de formule
            decimal prixUnitaire = 0;
            
            if (formule.NomFormule?.ToUpper().Contains("AMÉLIORÉ") == true || 
                formule.NomFormule?.ToUpper().Contains("AMELIORE") == true)
            {
                prixUnitaire = 2800; // Prix formule améliorée
            }
            else
            {
                prixUnitaire = 550; // Prix formule standard
            }
            
            return quantite * prixUnitaire;
        }

        /// <summary>
        /// Envoie une notification aux prestataires cantine
        /// </summary>
        private async Task SendNotificationToPrestataires(string message, Enums.TypeNotification type)
        {
            try
            {
                await _hubContext.Clients.Group("PrestataireCantine").SendAsync("ReceiveNotification", new
                {
                    Message = message,
                    Type = type.ToString(),
                    Timestamp = DateTime.UtcNow
                });

                _logger.LogInformation("Notification envoyée aux prestataires: {Message}", message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'envoi de notification aux prestataires: {Message}", message);
            }
        }
    }
}
