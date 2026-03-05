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
using Obeli_K.Services;
using Obeli_K.Services.Configuration;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Obeli_K.Controllers
{
    /// <summary>
    /// Contrôleur principal pour la gestion des commandes de restauration.
    /// Gère la création, modification, validation et suivi des commandes pour tous les types d'utilisateurs.
    /// </summary>
    [Authorize]
    public partial class CommandeController : Controller
    {
        private readonly ObeliDbContext _context;
        private readonly ILogger<CommandeController> _logger;
        private readonly IHubContext<NotificationsHub> _hubContext;
        private readonly IConfigurationService _configurationService;
        private readonly ExcelExportService _excelExportService;

        /// <summary>
        /// Initialise une nouvelle instance du contrôleur de commandes.
        /// </summary>
        /// <param name="context">Contexte de base de données Obeli</param>
        /// <param name="logger">Service de journalisation</param>
        /// <param name="hubContext">Contexte SignalR pour les notifications</param>
        /// <param name="configurationService">Service de configuration</param>
        /// <param name="excelExportService">Service d'export Excel</param>
        public CommandeController(ObeliDbContext context, ILogger<CommandeController> logger, IHubContext<NotificationsHub> hubContext, IConfigurationService configurationService, ExcelExportService excelExportService)
        {
            _context = context;
            _logger = logger;
            _hubContext = hubContext;
            _configurationService = configurationService;
            _excelExportService = excelExportService;
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
            var isEmploye = User.IsInRole("Employe") && !User.IsInRole("Administrateur") && !User.IsInRole("RH");
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
                new { Value = StatutCommande.Consommee.ToString(), Text = "Consommée" },
                new { Value = StatutCommande.Indisponible.ToString(), Text = "Indisponible" },
                new { Value = StatutCommande.NonRecuperer.ToString(), Text = "Non récupérée" }
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
        public async Task<IActionResult> Index(string? status = null, DateTime? dateDebut = null, DateTime? dateFin = null, string? matricule = null, int page = 1, int pageSize = 20)
        {
            try
            {
                // Déterminer si l'utilisateur est un employé (ne voit que ses commandes)
                var isEmploye = User.IsInRole("Employe") && !User.IsInRole("Administrateur") && !User.IsInRole("RH");
                var currentUserId = GetCurrentUserId();
                var isAdminOrRH = User.IsInRole("Administrateur") || User.IsInRole("RH");

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
                // Filtrer par matricule (pour Admin/RH)
                else if (isAdminOrRH && !string.IsNullOrWhiteSpace(matricule))
                {
                    query = query.Where(c => c.Utilisateur != null && c.Utilisateur.UserName == matricule.Trim());
                }

                // Filtrer par statut si spécifié
                if (!string.IsNullOrEmpty(status))
                {
                    // Gérer plusieurs statuts séparés par des virgules
                    var statuts = status.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    var statutsValides = new List<int>();
                    
                    foreach (var statutStr in statuts)
                    {
                        if (Enum.TryParse<StatutCommande>(statutStr.Trim(), out var statutCommande))
                        {
                            statutsValides.Add((int)statutCommande);
                        }
                    }
                    
                    if (statutsValides.Any())
                    {
                        query = query.Where(c => statutsValides.Contains(c.StatusCommande));
                    }
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

                var totalCount = await query.CountAsync();
                
                var commandesAvecFormules = await query
                    .OrderByDescending(c => c.DateConsommation ?? DateTime.MinValue)
                    .ThenByDescending(c => c.Date)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
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

                // Créer le modèle de pagination
                var pagination = new PaginationViewModel(HttpContext, "Index", "Commande")
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalItems = totalCount
                };

                var pagedModel = new PagedViewModel<CommandeListViewModel>
                {
                    Items = commandes,
                    Pagination = pagination
                };

                // Passer les paramètres de filtrage au ViewBag
                ViewBag.Status = status;
                ViewBag.DateDebut = dateDebut;
                ViewBag.DateFin = dateFin;
                ViewBag.Matricule = matricule;
                ViewBag.PageSize = pageSize;

                return View(pagedModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement de la liste des commandes");
                TempData["ErrorMessage"] = "Une erreur est survenue lors du chargement des commandes.";
                return View(new PagedViewModel<CommandeListViewModel>());
            }
        }

        /// <summary>
        /// Affiche le formulaire de création de commande avec interface par semaine
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Create(DateTime? dateDebut = null, DateTime? dateFin = null)
        {
            try
            {
                _logger.LogInformation("Chargement du formulaire de création de commande par semaine - Date début: {DateDebut}, Date fin: {DateFin}", dateDebut, dateFin);

                // Stocker les dates sélectionnées dans ViewBag
                ViewBag.DateDebut = dateDebut;
                ViewBag.DateFin = dateFin;

                // Vérifier si les commandes sont bloquées
                var isBlocked = await _configurationService.IsCommandeBlockedAsync();
                
                if (isBlocked)
                {
                    var prochaineCloture = await _configurationService.GetNextBlockingDateAsync();
                    ViewBag.IsBlocked = true;
                    ViewBag.ProchaineCloture = prochaineCloture;
                    ViewBag.MenusSemaineN1 = new List<object>();
                    _logger.LogInformation("Commandes bloquées - Aucun menu de la semaine N+1 affiché. Prochaine ouverture: {ProchaineCloture}", prochaineCloture);
                    
                    // Créer un modèle vide pour éviter la NullReferenceException
                    var emptyModel = new CommandeParSemaineViewModel
                    {
                        DateDebutSemaine = DateTime.Today,
                        DateFinSemaine = DateTime.Today.AddDays(6),
                        CommandesExistantes = new List<CommandeExistanteViewModel>(),
                        JoursSemaine = new List<JourSemaineViewModel>()
                    };
                    
                    return View(emptyModel);
                }

                // --- Déterminer la période à afficher ---
                DateTime debutSemaine, finSemaine;
                
                if (dateDebut.HasValue && dateFin.HasValue)
                {
                    // Utiliser les dates sélectionnées par l'utilisateur
                    debutSemaine = dateDebut.Value.Date;
                    finSemaine = dateFin.Value.Date;
                    _logger.LogInformation("Période sélectionnée par l'utilisateur: {DebutSemaine} à {FinSemaine}", debutSemaine, finSemaine);
                }
                else
                {
                    // Utiliser la semaine N+1 par défaut
                    (debutSemaine, finSemaine) = GetSemaineSuivanteComplete();
                    _logger.LogInformation("Semaine N+1 par défaut: {DebutSemaine} à {FinSemaine}", debutSemaine, finSemaine);
                }

                // ===== RÉCUPÉRATION DES MENUS DE LA SEMAINE N+1 =====
                // Normaliser les dates pour la comparaison (ignorer les heures)
                var debutSemaineDate = debutSemaine.Date;
                var finSemaineDate = finSemaine.Date;
                
                _logger.LogInformation("Recherche des formules entre {DebutDate} et {FinDate}", debutSemaineDate, finSemaineDate);
                
                var menusSemaineN1 = await _context.FormulesJour
                    .AsNoTracking()
                    .Include(fj => fj.NomFormuleNavigation)
                    .Where(fj => fj.Supprimer == 0
                              && fj.Date.Date >= debutSemaineDate
                              && fj.Date.Date <= finSemaineDate)
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
                
                // Logger les détails des formules trouvées pour déboguer
                if (menusSemaineN1.Count == 0)
                {
                    _logger.LogWarning("⚠️ Aucune formule trouvée ! Vérification dans toute la table FormulesJour...");
                    var toutesLesFormules = await _context.FormulesJour
                        .AsNoTracking()
                        .Where(fj => fj.Supprimer == 0)
                        .Select(fj => new { fj.Date, fj.NomFormule })
                        .ToListAsync();
                    _logger.LogWarning("Total de formules actives dans la base: {Count}", toutesLesFormules.Count);
                    foreach (var f in toutesLesFormules.Take(10))
                    {
                        _logger.LogInformation("Formule trouvée: Date={Date}, Nom={Nom}", f.Date, f.NomFormule);
                    }
                }
                else
                {
                    foreach (var menu in menusSemaineN1)
                    {
                        _logger.LogInformation("✅ Menu trouvé: Date={Date}, Type={Type}, Plat={Plat}", 
                            menu.Date, menu.TypeFormule, menu.Plat ?? menu.PlatStandard1 ?? "N/A");
                    }
                }

                var currentUserId = GetCurrentUserId();
                var commandesExistantes = new List<CommandeExistanteViewModel>();

                if (currentUserId.HasValue)
                {
                    var commandesAvecFormules = await _context.Commandes
                        .AsNoTracking()
                        .Include(c => c.FormuleJour)!.ThenInclude(f => f!.NomFormuleNavigation)
                        .Where(c => c.UtilisateurId == currentUserId
                                 && c.TypeClient == TypeClientCommande.CitUtilisateur
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
                        new { Type = "Améliorée", TypeFormule = "Formule Améliorée" },
                        new { Type = "Standard1", TypeFormule = "Formule Standard 1" },
                        new { Type = "Standard2", TypeFormule = "Formule Standard 2" }
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
                _logger.LogInformation("🚀 Création de commande - Formule: {IdFormule}, Date: {DateConsommation}, Période: {Periode}, Type: {TypeFormule}", 
                    idFormule, dateConsommation, periode, typeFormule);

                // Vérifier si les commandes sont bloquées
                var isBlocked = await _configurationService.IsCommandeBlockedAsync();
                _logger.LogInformation("🔒 Vérification blocage: {IsBlocked}", isBlocked);
                if (isBlocked)
                {
                    var prochaineCloture = await _configurationService.GetNextBlockingDateAsync();
                    _logger.LogWarning("❌ Commandes bloquées - Prochaine ouverture: {ProchaineCloture}", prochaineCloture);
                    return Json(new { 
                        success = false, 
                        message = $"Les commandes sont actuellement bloquées. Prochaine ouverture: {prochaineCloture:dd/MM/yyyy HH:mm}" 
                    });
                }

                var currentUserId = GetCurrentUserId();
                _logger.LogInformation("👤 Utilisateur: {UserId}", currentUserId);
                if (!currentUserId.HasValue)
                {
                    _logger.LogWarning("❌ Utilisateur non connecté");
                    return Json(new { success = false, message = "Utilisateur non connecté" });
                }

                // Note: La limite de 48h avant 12h est maintenant appliquée uniquement dans l'interface (affichage)
                // mais n'empêche plus la création de commande côté serveur
                var maintenant = DateTime.Now;
                var (debutSemaineCourante, finSemaineCourante) = GetSemaineCouranteComplete();
                
                // Log pour information (pas de blocage)
                if (dateConsommation.Date >= debutSemaineCourante && dateConsommation.Date <= finSemaineCourante)
                {
                    // Calcul du délai : 48h avant 12h00 de la date de consommation
                    var dateConsommationA12H = dateConsommation.Date.AddHours(12); // Date de consommation à 12h00
                    var limiteCommande = dateConsommationA12H.AddHours(-48); // 48h avant 12h00
                    
                    if (maintenant >= limiteCommande)
                    {
                        _logger.LogInformation("Commande créée après le délai de 48h avant 12h pour la semaine courante: {DateConsommation}", dateConsommation);
                    }
                }

                // Règle : une seule commande CitUtilisateur par jour / utilisateur
                _logger.LogInformation("🔍 Vérification commande existante pour le {Date}", dateConsommation);
                var dejaCommandeCeJour = await _context.Commandes
                    .AsNoTracking()
                    .AnyAsync(c => c.UtilisateurId == currentUserId
                                && c.TypeClient == TypeClientCommande.CitUtilisateur
                                && c.DateConsommation == dateConsommation
                                && c.Supprimer == 0
                                && !(c.StatusCommande == (int)StatutCommande.Annulee && !c.AnnuleeParPrestataire));

                _logger.LogInformation("📊 Commande existante trouvée: {DejaCommande}", dejaCommandeCeJour);
                if (dejaCommandeCeJour)
                {
                    _logger.LogWarning("❌ Commande déjà existante pour le {Date}", dateConsommation);
                    return Json(new { success = false, message = "Vous avez déjà une commande pour ce jour." });
                }

                // Récupérer la formule
                _logger.LogInformation("🔍 Recherche de la formule: {IdFormule}", idFormule);
                var formule = await _context.FormulesJour
                    .AsNoTracking()
                    .Include(f => f.NomFormuleNavigation)
                    .FirstOrDefaultAsync(f => f.IdFormule == idFormule && f.Supprimer == 0);

                _logger.LogInformation("📋 Formule trouvée: {Formule}", formule?.NomFormule);
                if (formule == null)
                {
                    _logger.LogWarning("❌ Formule non trouvée: {IdFormule}", idFormule);
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
                var isEmploye = User.IsInRole("Employe") && !User.IsInRole("Administrateur") && !User.IsInRole("RH");
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
                        // Seuls les RH, Administrateurs et PrestataireCantine peuvent créer des commandes pour les groupes non-CIT
                        if (!User.IsInRole("RH") && !User.IsInRole("Administrateur") && !User.IsInRole("PrestataireCantine"))
                        {
                            ModelState.AddModelError(nameof(model.TypeClient), "Seuls les RH, Administrateurs et PrestataireCantine peuvent créer des commandes pour les groupes non-CIT.");
                        }
                        else if (!model.GroupeNonCitId.HasValue || model.GroupeNonCitId == Guid.Empty)
                        {
                            ModelState.AddModelError(nameof(model.GroupeNonCitId), "Le groupe non-CIT est obligatoire pour ce type de client.");
                        }
                        break;
                    case TypeClientCommande.Visiteur:
                        // Seuls les RH et Administrateurs peuvent créer des commandes pour les visiteurs
                        if (!User.IsInRole("RH") && !User.IsInRole("Administrateur"))
                        {
                            ModelState.AddModelError(nameof(model.TypeClient), "Seuls les RH et Administrateurs peuvent créer des commandes pour les visiteurs.");
                        }
                        else if (string.IsNullOrWhiteSpace(model.VisiteurNom))
                        {
                            ModelState.AddModelError(nameof(model.VisiteurNom), "Le nom du visiteur est obligatoire pour ce type de client.");
                        }
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
        [Authorize(Roles = "Administrateur,RH,PrestataireCantine")]
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

                // Vérifier si la commande peut être modifiée selon les règles métier
                if (!CanModifyCommande(commande))
                {
                    _logger.LogWarning("Tentative d'accès à la modification d'une commande non modifiable: {Id}, Date: {Date}", id, commande.DateConsommation);
                    TempData["ErrorMessage"] = "Cette commande ne peut plus être modifiée. Les commandes consommées ne peuvent jamais être modifiées. Les commandes dont la date de consommation est passée ne peuvent être modifiées que par un Administrateur, et uniquement si elles ne sont pas consommées.";
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

                // Charger les listes communes (types de clients, sites, etc.)
                await PopulateViewBags();

                // Surcharger la liste des formules pour l'édition :
                // on affiche les formules du jour de consommation de la commande,
                // et on pré-sélectionne celle actuellement liée à la commande.
                if (model.DateConsommation != default)
                {
                    var dateConsommation = model.DateConsommation.Date;

                    var formulesPourJour = await _context.FormulesJour
                        .AsNoTracking()
                        .Include(f => f.NomFormuleNavigation)
                        .Where(f => f.Supprimer == 0 && f.Date.Date == dateConsommation)
                        .OrderBy(f => f.Date)
                        .ThenBy(f => f.NomFormuleNavigation!.Nom)
                        .ToListAsync();

                    var formulesSelectList = formulesPourJour
                        .Select(f =>
                        {
                            var platPrincipal = GetPlatPrincipal(f);
                            var displayText = string.IsNullOrWhiteSpace(platPrincipal)
                                ? $"{f.Date:dd/MM/yyyy} ({f.Date:dddd}) - {f.NomFormule}"
                                : $"{f.Date:dd/MM/yyyy} ({f.Date:dddd}) - {f.NomFormule} ({platPrincipal})";

                            return new
                            {
                                IdFormule = f.IdFormule,
                                DisplayText = displayText
                            };
                        })
                        .ToList();

                    ViewBag.Formules = new SelectList(formulesSelectList, "IdFormule", "DisplayText", model.IdFormule);
                }

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
        [Authorize(Roles = "Administrateur,RH,PrestataireCantine")]
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

                // Vérifier si la commande peut être modifiée selon les règles métier
                if (!CanModifyCommande(existingCommande))
                {
                    _logger.LogWarning("Tentative de modification d'une commande non modifiable: {Id}, Date: {Date}", id, existingCommande.DateConsommation);
                    TempData["ErrorMessage"] = "Cette commande ne peut plus être modifiée. Les commandes consommées ne peuvent jamais être modifiées. Les commandes dont la date de consommation est passée ne peuvent être modifiées que par un Administrateur, et uniquement si elles ne sont pas consommées.";
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
        [Authorize(Roles = "Administrateur,RH")]
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

                // Vérifier si la commande peut être supprimée selon les règles métier
                if (!CanModifyCommande(commande))
                {
                    _logger.LogWarning("Tentative de suppression d'une commande non modifiable: {Id}, Status: {Status}, Date: {Date}", 
                        id, commande.StatusCommande, commande.DateConsommation);
                    TempData["ErrorMessage"] = "Cette commande ne peut plus être supprimée. Les commandes consommées ne peuvent jamais être supprimées. Les commandes dont la date de consommation est passée ne peuvent être supprimées que par un Administrateur, et uniquement si elles ne sont pas consommées.";
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

        private (DateTime Lundi, DateTime Dimanche) GetSemaineCouranteComplete()
        {
            var today = DateTime.Today;
            int diffToMonday = ((int)today.DayOfWeek + 6) % 7; // Lundi=0
            var thisWeekMonday = today.AddDays(-diffToMonday).Date;
            var thisWeekSunday = thisWeekMonday.AddDays(6);
            return (thisWeekMonday, thisWeekSunday);
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
        /// Vérifie si les quotas sont disponibles pour créer une commande instantanée
        /// </summary>
        private async Task<(bool Disponible, string Message)> VerifierQuotasDisponiblesAsync(FormuleJour formule, Periode periode)
        {
            try
            {
                var maintenant = DateTime.Now;
                var heureActuelle = maintenant.Hour;
                var estPeriodeJour = heureActuelle < 18;

                // Initialiser les quotas si null
                formule.QuotaJourRestant ??= 0;
                formule.QuotaNuitRestant ??= 0;
                formule.MargeJourRestante ??= 0;
                formule.MargeNuitRestante ??= 0;

                if (estPeriodeJour && periode == Periode.Jour)
                {
                    // Période Jour : vérifier QuotaJourRestant puis MargeJourRestante
                    var totalDisponible = (formule.QuotaJourRestant ?? 0) + (formule.MargeJourRestante ?? 0);
                    if (totalDisponible <= 0)
                    {
                        return (false, "❌ Les quotas pour la période Jour sont épuisés. Impossible de créer une commande instantanée jusqu'à 18h.");
                    }
                }
                else if (!estPeriodeJour && periode == Periode.Nuit)
                {
                    // Période Nuit : vérifier QuotaNuitRestant puis MargeNuitRestante
                    var totalDisponible = (formule.QuotaNuitRestant ?? 0) + (formule.MargeNuitRestante ?? 0);
                    if (totalDisponible <= 0)
                    {
                        return (false, "❌ Les quotas pour la période Nuit sont épuisés. Impossible de créer une commande instantanée.");
                    }
                }
                else if (estPeriodeJour && periode == Periode.Nuit)
                {
                    // Tentative de créer une commande Nuit avant 18h
                    return (false, "❌ Les commandes instantanées pour la période Nuit ne sont disponibles qu'à partir de 18h.");
                }

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la vérification des quotas pour la formule {FormuleId}", formule.IdFormule);
                return (false, "Erreur lors de la vérification des quotas.");
            }
        }

        /// <summary>
        /// Décrémente les quotas d'une formule lors de la validation d'une commande
        /// </summary>
        private async Task DecrementerQuotasFormuleAsync(FormuleJour formule, Commande commande)
        {
            try
            {
                var maintenant = DateTime.Now;
                var heureActuelle = maintenant.Hour;
                var periodeCommande = commande.PeriodeService;
                var quantite = commande.Quantite;

                // Initialiser les quotas si null
                formule.QuotaJourRestant ??= 0;
                formule.QuotaNuitRestant ??= 0;
                formule.MargeJourRestante ??= 0;
                formule.MargeNuitRestante ??= 0;

                // Déterminer si on est en période Jour (< 18h) ou Nuit (>= 18h)
                bool estPeriodeJour = heureActuelle < 18;

                if (estPeriodeJour && periodeCommande == Periode.Jour)
                {
                    // Période Jour : décrémenter d'abord QuotaJourRestant, puis MargeJourRestante
                    if (formule.QuotaJourRestant > 0)
                    {
                        var decrement = Math.Min(quantite, formule.QuotaJourRestant.Value);
                        formule.QuotaJourRestant -= decrement;
                        quantite -= decrement;
                    }

                    // Si encore de la quantité à décrémenter, utiliser la marge jour
                    if (quantite > 0 && formule.MargeJourRestante > 0)
                    {
                        var decrement = Math.Min(quantite, formule.MargeJourRestante.Value);
                        formule.MargeJourRestante -= decrement;
                    }

                    _logger.LogInformation("Quotas décrémentés pour période Jour - Formule: {FormuleId}, QuotaJourRestant: {QuotaJour}, MargeJourRestante: {MargeJour}",
                        formule.IdFormule, formule.QuotaJourRestant, formule.MargeJourRestante);
                }
                else if (!estPeriodeJour && periodeCommande == Periode.Nuit)
                {
                    // Période Nuit : décrémenter d'abord QuotaNuitRestant, puis MargeNuitRestante
                    if (formule.QuotaNuitRestant > 0)
                    {
                        var decrement = Math.Min(quantite, formule.QuotaNuitRestant.Value);
                        formule.QuotaNuitRestant -= decrement;
                        quantite -= decrement;
                    }

                    // Si encore de la quantité à décrémenter, utiliser la marge nuit
                    if (quantite > 0 && formule.MargeNuitRestante > 0)
                    {
                        var decrement = Math.Min(quantite, formule.MargeNuitRestante.Value);
                        formule.MargeNuitRestante -= decrement;
                    }

                    _logger.LogInformation("Quotas décrémentés pour période Nuit - Formule: {FormuleId}, QuotaNuitRestant: {QuotaNuit}, MargeNuitRestante: {MargeNuit}",
                        formule.IdFormule, formule.QuotaNuitRestant, formule.MargeNuitRestante);
                }

                formule.ModifiedOn = DateTime.UtcNow;
                formule.ModifiedBy = User.Identity?.Name ?? "System";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la décrémentation des quotas pour la formule {FormuleId}", formule.IdFormule);
                // Ne pas bloquer la validation si la décrémentation échoue
            }
        }

        /// <summary>
        /// Calcule le prix d'une formule selon son type
        /// </summary>
        private decimal GetPrixFormule(string? nomFormule)
        {
            if (string.IsNullOrWhiteSpace(nomFormule))
            {
                return 550m; // Prix par défaut pour Standard
            }

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
        /// Affiche la page d'export Excel
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Administrateur,RH,Employe")]
        public IActionResult ExporterExcel()
        {
            return View();
        }

        /// <summary>
        /// Exporte les commandes en Excel
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExporterExcel(string? status = null, DateTime? dateDebut = null, DateTime? dateFin = null, string? matricule = null)
        {
            try
            {
                // Déterminer si l'utilisateur est un employé (ne voit que ses commandes)
                var isEmploye = User.IsInRole("Employe") && !User.IsInRole("Administrateur") && !User.IsInRole("RH");
                var currentUserId = GetCurrentUserId();
                var isAdminOrRH = User.IsInRole("Administrateur") || User.IsInRole("RH");

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
                // Filtrer par matricule (pour Admin/RH)
                else if (isAdminOrRH && !string.IsNullOrWhiteSpace(matricule))
                {
                    query = query.Where(c => c.Utilisateur != null && c.Utilisateur.UserName == matricule.Trim());
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
        [Authorize(Roles = "Administrateur,PrestataireCantine,RH")]
        public IActionResult VerifierCommande()
        {
            return View();
        }

        /// <summary>
        /// Traite la vérification d'une commande par matricule
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrateur,PrestataireCantine,RH")]
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
        [Authorize(Roles = "Administrateur,PrestataireCantine,RH")]
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

                // Récupérer la formule associée pour décrémenter les quotas
                var formule = await _context.FormulesJour
                    .FirstOrDefaultAsync(f => f.IdFormule == commande.IdFormule && f.Supprimer == 0);

                if (formule != null)
                {
                    // Décrémenter les quotas selon la période et l'heure
                    await DecrementerQuotasFormuleAsync(formule, commande);
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
        [Authorize(Roles = "Administrateur,PrestataireCantine,RH")]
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

                // Vérifier la limite de 24h pour annuler une commande (uniquement pour la semaine en cours)
                if (commande.DateConsommation.HasValue)
                {
                    var maintenant = DateTime.Now;
                    var (debutSemaineCourante, finSemaineCourante) = GetSemaineCouranteComplete();
                    
                    // Si la commande est pour la semaine en cours, appliquer la limite de 24h
                    if (commande.DateConsommation.Value.Date >= debutSemaineCourante && commande.DateConsommation.Value.Date <= finSemaineCourante)
                    {
                        var limiteAnnulation = commande.DateConsommation.Value.AddHours(-24);
                        
                        if (maintenant >= limiteAnnulation)
                        {
                            var tempsRestant = commande.DateConsommation.Value - maintenant;
                            var heuresRestantes = Math.Max(0, (int)tempsRestant.TotalHours);
                            var minutesRestantes = Math.Max(0, (int)(tempsRestant.TotalMinutes % 60));
                            
                            TempData["ErrorMessage"] = $"Impossible d'annuler cette commande de la semaine en cours. Délai de 24h dépassé. Il ne reste que {heuresRestantes}h {minutesRestantes}min avant la date de consommation ({commande.DateConsommation.Value:dd/MM/yyyy HH:mm})";
                            return RedirectToAction("VerifierCommande");
                        }
                    }
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
        /// Annule une commande côté utilisateur (avec limite de 24h)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AnnulerMaCommande(Guid idCommande)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (!currentUserId.HasValue)
                {
                    return Json(new { success = false, message = "Utilisateur non connecté" });
                }

                var commande = await _context.Commandes
                    .FirstOrDefaultAsync(c => c.IdCommande == idCommande 
                                           && c.UtilisateurId == currentUserId 
                                           && c.Supprimer == 0);

                if (commande == null)
                {
                    return Json(new { success = false, message = "Commande non trouvée ou vous n'êtes pas autorisé à l'annuler." });
                }

                // Vérifier que la commande n'est pas déjà annulée
                if (commande.StatusCommande == (int)StatutCommande.Annulee)
                {
                    return Json(new { success = false, message = "Cette commande est déjà annulée." });
                }

                // Vérifier la limite de 24h pour annuler une commande (uniquement pour la semaine en cours)
                if (commande.DateConsommation.HasValue)
                {
                    var maintenant = DateTime.Now;
                    var (debutSemaineCourante, finSemaineCourante) = GetSemaineCouranteComplete();
                    
                    // Si la commande est pour la semaine en cours, appliquer la limite de 24h
                    if (commande.DateConsommation.Value.Date >= debutSemaineCourante && commande.DateConsommation.Value.Date <= finSemaineCourante)
                    {
                        var limiteAnnulation = commande.DateConsommation.Value.AddHours(-24);
                        
                        if (maintenant >= limiteAnnulation)
                        {
                            var tempsRestant = commande.DateConsommation.Value - maintenant;
                            var heuresRestantes = Math.Max(0, (int)tempsRestant.TotalHours);
                            var minutesRestantes = Math.Max(0, (int)(tempsRestant.TotalMinutes % 60));
                            
                            return Json(new { 
                                success = false, 
                                message = $"Impossible d'annuler cette commande de la semaine en cours. Délai de 24h dépassé. Il ne reste que {heuresRestantes}h {minutesRestantes}min avant la date de consommation ({commande.DateConsommation.Value:dd/MM/yyyy HH:mm})" 
                            });
                        }
                    }
                }

                // Annuler la commande
                commande.StatusCommande = (int)StatutCommande.Annulee;
                commande.ModifiedOn = DateTime.UtcNow;
                commande.ModifiedBy = User.Identity?.Name ?? "Utilisateur";

                await _context.SaveChangesAsync();

                // Envoyer une notification aux prestataires
                await SendNotificationToPrestataires(
                    $"Commande annulée par l'utilisateur: {commande.CodeCommande} pour le {commande.DateConsommation:dd/MM/yyyy}",
                    Enums.TypeNotification.Warning
                );

                _logger.LogInformation("Commande {CodeCommande} annulée par l'utilisateur {UtilisateurId}", 
                    commande.CodeCommande, currentUserId);

                return Json(new { 
                    success = true, 
                    message = $"Commande {commande.CodeCommande} annulée avec succès" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'annulation de la commande {IdCommande} par l'utilisateur", idCommande);
                return Json(new { success = false, message = "Une erreur est survenue lors de l'annulation de la commande." });
            }
        }

        /// <summary>
        /// Affiche le formulaire spécialisé pour les commandes des Douaniers
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "PrestataireCantine,Administrateur,RH")]
        public async Task<IActionResult> CreerCommandeDouaniers()
        {
            try
            {
                var aujourdhui = DateTime.Today;
                ViewBag.DateAujourdhui = aujourdhui;

                // Vérifier si les commandes sont bloquées
                var isBlocked = await _configurationService.IsCommandeBlockedAsync();
                
                if (isBlocked)
                {
                    _logger.LogWarning("🚫 Commandes bloquées - Accès refusé à la création de commande Douaniers");
                    TempData["ErrorMessage"] = "Les commandes sont actuellement bloquées. Vous ne pouvez pas créer de commandes pour les Douaniers.";
                    await PopulateViewBagsForDouaniers();
                    return View();
                }

                // Récupérer les formules du jour (exclure les formules améliorées)
                var formulesAujourdhui = await _context.FormulesJour
                    .Where(f => f.Date.Date == aujourdhui && 
                               f.Supprimer == 0 &&
                               !(f.NomFormule != null && (
                                   f.NomFormule.ToUpper().Contains("AMÉLIORÉ") ||
                                   f.NomFormule.ToUpper().Contains("AMELIORE") ||
                                   f.NomFormule.ToUpper().Contains("AMELIOREE")
                               )))
                    .OrderBy(f => f.NomFormule)
                    .ToListAsync();

                _logger.LogInformation("🔍 Chargement des formules (sans améliorées) pour {Date}: {Count} formules trouvées", aujourdhui, formulesAujourdhui.Count);
                
                if (!formulesAujourdhui.Any())
                {
                    _logger.LogWarning("⚠️ Aucune formule trouvée pour la date {Date}", aujourdhui);
                    TempData["ErrorMessage"] = "Aucune formule n'est disponible pour aujourd'hui. Veuillez d'abord créer des formules pour cette date.";
                    await PopulateViewBagsForDouaniers();
                    return View();
                }
                
                // Log des formules trouvées
                foreach (var formule in formulesAujourdhui)
                {
                    _logger.LogInformation("📋 Formule trouvée: {NomFormule} (ID: {IdFormule})", formule.NomFormule, formule.IdFormule);
                }

                // Récupérer le groupe Douaniers
                var groupeDouaniers = await _context.GroupesNonCit
                    .FirstOrDefaultAsync(g => g.Nom == "Douaniers" && g.Supprimer == 0);

                if (groupeDouaniers == null)
                {
                    _logger.LogWarning("⚠️ Groupe Douaniers non trouvé");
                    TempData["ErrorMessage"] = "Le groupe Douaniers n'existe pas dans le système.";
                    await PopulateViewBagsForDouaniers();
                    return View();
                }

                // Créer la liste des formules pour le ViewBag
                var formulesList = formulesAujourdhui.Select(f => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = f.IdFormule.ToString(),
                    Text = GenerateMenuDisplayName(f)
                }).ToList();

                ViewBag.FormulesAujourdhui = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(formulesList, "Value", "Text");
                ViewBag.GroupeDouaniers = groupeDouaniers;

                // IMPORTANT: Appeler PopulateViewBagsForDouaniers pour récupérer les quotas
                await PopulateViewBagsForDouaniers();

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement du formulaire de commande Douaniers");
                TempData["ErrorMessage"] = "Une erreur est survenue lors du chargement du formulaire.";
                await PopulateViewBagsForDouaniers();
                return View();
            }
        }

        /// <summary>
        /// Affiche le formulaire de création de commande instantanée pour visiteurs et groupes non-CIT
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Administrateur,RH,PrestataireCantine")]
        public async Task<IActionResult> CreerCommandeInstantanee()
        {
            try
            {
                // Les PrestataireCantine peuvent maintenant accéder aux commandes instantanées générales
                // en plus des commandes spécialisées douaniers

                var aujourdhui = DateTime.Today;
                ViewBag.DateAujourdhui = aujourdhui;

                // Vérifier si les commandes sont bloquées
                var isBlocked = await _configurationService.IsCommandeBlockedAsync();
                
                if (isBlocked)
                {
                    _logger.LogWarning("🚫 Commandes bloquées - Accès refusé à la création de commande instantanée");
                    TempData["ErrorMessage"] = "Les commandes sont actuellement bloquées. Vous ne pouvez pas créer de commandes instantanées.";
                    await PopulateViewBagsForInstantOrder();
                    return View();
                }

                // Récupérer les formules du jour
                var formulesAujourdhui = await _context.FormulesJour
                    .Where(f => f.Date.Date == aujourdhui && f.Supprimer == 0)
                    .OrderBy(f => f.NomFormule)
                    .ToListAsync();

                _logger.LogInformation("🔍 Chargement des formules pour {Date}: {Count} formules trouvées", aujourdhui, formulesAujourdhui.Count);
                
                if (!formulesAujourdhui.Any())
                {
                    _logger.LogWarning("⚠️ Aucune formule trouvée pour la date {Date}", aujourdhui);
                    TempData["ErrorMessage"] = "Aucune formule n'est disponible pour aujourd'hui. Veuillez d'abord créer des formules pour cette date.";
                    await PopulateViewBagsForInstantOrder();
                    return View();
                }
                
                // Log des formules trouvées
                foreach (var formule in formulesAujourdhui)
                {
                    _logger.LogInformation("📋 Formule trouvée: {NomFormule} (ID: {IdFormule})", formule.NomFormule, formule.IdFormule);
                }

                // Les commandes instantanées sont uniquement pour les employés CIT
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
                // ViewBag.GroupesNonCit supprimé - Les commandes instantanées sont uniquement pour les employés CIT
                ViewBag.Utilisateurs = new SelectList(selectListItems, "Value", "Text");
                ViewBag.UtilisateursData = utilisateurs.ToDictionary(u => u.Value, u => u.UserName);

                // Les commandes instantanées sont uniquement pour les employés CIT
                var typesClient = new[]
                {
                    new { Value = TypeClientCommande.CitUtilisateur.ToString(), Text = "Employé CIT" }
                };
                ViewBag.TypeClients = new SelectList(typesClient, "Value", "Text");

                // Charger aussi les formules pour les Douaniers (sans améliorées) si la section Douaniers est présente
                await PopulateViewBagsForDouaniers();

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
        [Authorize(Roles = "Administrateur,RH,PrestataireCantine")]
        public async Task<IActionResult> CreerCommandeInstantanee(CreerCommandeInstantaneeViewModel model)
        {
            try
            {
                // Vérifier si les commandes sont bloquées
                var isBlocked = await _configurationService.IsCommandeBlockedAsync();
                
                if (isBlocked)
                {
                    _logger.LogWarning("🚫 Commandes bloquées - Création de commande instantanée refusée");
                    TempData["ErrorMessage"] = "Les commandes sont actuellement bloquées. Vous ne pouvez pas créer de commandes instantanées.";
                    await PopulateViewBagsForInstantOrder();
                    return View(model);
                }
                
                // Forcer le type de client à CitUtilisateur (employés uniquement)
                model.TypeClient = TypeClientCommande.CitUtilisateur;
                
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

                // Vérifier que l'utilisateur est fourni (obligatoire pour les employés CIT)
                if (!model.UtilisateurId.HasValue || model.UtilisateurId == Guid.Empty)
                {
                    ModelState.AddModelError(nameof(model.UtilisateurId), "L'employé est obligatoire. Veuillez saisir un matricule valide.");
                    await PopulateViewBagsForInstantOrder();
                    return View(model);
                }

                // Validation pour les employés CIT uniquement
                switch (model.TypeClient)
                {
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

                        // Pour les employés CIT, forcer la quantité à 1
                        model.Quantite = 1;
                        break;

                    default:
                        // Les autres types de clients ne sont pas autorisés pour les commandes instantanées
                        _logger.LogWarning("Type de client non autorisé pour commande instantanée: {TypeClient}", model.TypeClient);
                        ModelState.AddModelError(nameof(model.TypeClient), "Les commandes instantanées sont uniquement disponibles pour les employés CIT.");
                        await PopulateViewBagsForInstantOrder();
                        return View(model);
                }

                // Vérifier la limitation par période (une commande CitUtilisateur par période par jour) - SAUF pour les douaniers
                if (model.TypeClient == TypeClientCommande.CitUtilisateur && model.UtilisateurId.HasValue)
                {
                    var commandeExistante = await _context.Commandes
                        .Where(c => c.UtilisateurId == model.UtilisateurId 
                            && c.TypeClient == TypeClientCommande.CitUtilisateur
                            && c.Instantanee == true 
                            && c.DateConsommation.HasValue && c.DateConsommation.Value.Date == DateTime.Today
                            && c.PeriodeService == model.PeriodeService
                            && c.Supprimer == 0)
                        .FirstOrDefaultAsync();

                    if (commandeExistante != null)
                    {
                        var periodeText = model.PeriodeService == Periode.Jour ? "déjeuner (jour)" : "dîner (nuit)";
                        ModelState.AddModelError(nameof(model.PeriodeService), $"L'utilisateur a déjà une commande instantanée pour le {periodeText} aujourd'hui.");
                        await PopulateViewBagsForInstantOrder();
                        return View(model);
                    }
                }

                // Vérifier les quotas avant de créer la commande
                var verificationQuota = await VerifierQuotasDisponiblesAsync(formule, model.PeriodeService);
                if (!verificationQuota.Disponible)
                {
                    ModelState.AddModelError(nameof(model.IdFormule), verificationQuota.Message);
                    await PopulateViewBagsForInstantOrder();
                    return View(model);
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
                    // DirectionId supprimé - Table Direction non utilisée
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
        /// Recherche des utilisateurs par matricule (pour le filtre de la liste des commandes)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Administrateur,RH")]
        public async Task<IActionResult> SearchUsersByMatricule(string term)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
                {
                    return Json(new List<object>());
                }

                var users = await _context.Utilisateurs
                    .Where(u => u.Supprimer == 0 && 
                           (u.UserName.Contains(term) || 
                            u.Nom.Contains(term) || 
                            u.Prenoms.Contains(term)))
                    .OrderBy(u => u.UserName)
                    .Take(20)
                    .Select(u => new
                    {
                        id = u.UserName,
                        text = $"{u.Nom} {u.Prenoms} ({u.UserName})",
                        matricule = u.UserName
                    })
                    .ToListAsync();

                return Json(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la recherche d'utilisateurs par matricule");
                return Json(new List<object>());
            }
        }

        /// <summary>
        /// Récupère un utilisateur par matricule (pour les prestataires, administrateurs et RH)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Administrateur,PrestataireCantine,RH")]
        public async Task<IActionResult> GetUserByMatricule([FromBody] GetUserByMatriculeRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Matricule))
                {
                    return Json(new { success = false, message = "Matricule requis." });
                }

                var user = await _context.Utilisateurs
                    .FirstOrDefaultAsync(u => u.UserName == request.Matricule && u.Supprimer == 0);

                if (user == null)
                {
                    return Json(new { success = false, message = "Utilisateur non trouvé." });
                }

                return Json(new { 
                    success = true, 
                    userId = user.Id.ToString(),
                    userName = $"{user.Nom} {user.Prenoms}",
                    nom = user.Nom,
                    prenoms = user.Prenoms,
                    matricule = user.UserName
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la recherche utilisateur par matricule");
                return Json(new { success = false, message = "Erreur lors de la recherche." });
            }
        }

        /// <summary>
        /// Authentifie un utilisateur pour les prestataires
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "PrestataireCantine")]
        public async Task<IActionResult> AuthenticateUser([FromBody] AuthenticateUserRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Matricule) || string.IsNullOrWhiteSpace(request.Password))
                {
                    return Json(new { success = false, message = "Matricule et mot de passe requis." });
                }

                var user = await _context.Utilisateurs
                    .FirstOrDefaultAsync(u => u.UserName == request.Matricule && u.Supprimer == 0);

                if (user == null)
                {
                    return Json(new { success = false, message = "Utilisateur non trouvé." });
                }

                if (!BCrypt.Net.BCrypt.Verify(request.Password, user.MotDePasseHash))
                {
                    return Json(new { success = false, message = "Mot de passe incorrect." });
                }

                return Json(new { 
                    success = true, 
                    userId = user.Id.ToString(),
                    userName = $"{user.Nom} {user.Prenoms}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'authentification utilisateur");
                return Json(new { success = false, message = "Erreur lors de l'authentification." });
            }
        }


        /// <summary>
        /// API simple pour récupérer les menus du jour
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMenus()
        {
            try
            {
                _logger.LogInformation("🌐 Méthode GetMenus appelée via AJAX");
                
                // Vérifier si les commandes sont bloquées
                var isBlocked = await _configurationService.IsCommandeBlockedAsync();
                
                if (isBlocked)
                {
                    _logger.LogWarning("🚫 Commandes bloquées - Accès refusé à GetMenus");
                    return Json(new { error = "Les commandes sont actuellement bloquées." });
                }
                
                var aujourdhui = DateTime.Today;
                
                // Récupérer les formules du jour
                var formulesAujourdhui = await _context.FormulesJour
                    .Where(f => f.Date.Date == aujourdhui && f.Supprimer == 0)
                    .OrderBy(f => f.NomFormule)
                    .ToListAsync();

                _logger.LogInformation("📊 Formules du jour trouvées: {Count}", formulesAujourdhui.Count);

                // Si aucune formule pour aujourd'hui, charger les formules récentes
                if (!formulesAujourdhui.Any())
                {
                    _logger.LogWarning("⚠️ Aucune formule pour aujourd'hui, chargement des formules récentes");
                    formulesAujourdhui = await _context.FormulesJour
                        .Where(f => f.Supprimer == 0)
                        .OrderByDescending(f => f.Date)
                        .Take(10) // Prendre les 10 dernières formules
                        .ToListAsync();
                    _logger.LogInformation("📋 Formules récentes chargées: {Count}", formulesAujourdhui.Count);
                }

                var menus = formulesAujourdhui.Select(f => new
                {
                    id = f.IdFormule,
                    nom = f.NomFormule ?? "Menu"
                }).ToList();

                _logger.LogInformation("✅ Retour de {Count} menus via AJAX", menus.Count);
                return Json(menus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur dans GetMenus");
                return Json(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Récupère les détails des plats d'une formule
        /// </summary>
        private object GetPlatsDetails(FormuleJour formule)
        {
            var plats = new List<string>();
            
            // Entrée
            if (!string.IsNullOrWhiteSpace(formule.Entree))
                plats.Add($"Entrée: {formule.Entree}");
            
            // Plat principal
            if (!string.IsNullOrWhiteSpace(formule.Plat))
                plats.Add($"Plat: {formule.Plat}");
            
            // Garniture
            if (!string.IsNullOrWhiteSpace(formule.Garniture))
                plats.Add($"Garniture: {formule.Garniture}");
            
            // Dessert
            if (!string.IsNullOrWhiteSpace(formule.Dessert))
                plats.Add($"Dessert: {formule.Dessert}");
            
            // Légumes
            if (!string.IsNullOrWhiteSpace(formule.Legumes))
                plats.Add($"Légumes: {formule.Legumes}");

            return new
            {
                entree = formule.Entree ?? "",
                plat = formule.Plat ?? "",
                garniture = formule.Garniture ?? "",
                dessert = formule.Dessert ?? "",
                legumes = formule.Legumes ?? "",
                description = string.Join(" | ", plats)
            };
        }

        /// <summary>
        /// Crée une commande pour les Douaniers (plats standard uniquement)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "PrestataireCantine,Administrateur,RH")]
        public async Task<IActionResult> CreateDouanierOrder([FromBody] CreateDouanierOrderRequest request)
        {
            try
            {
                _logger.LogInformation("🛡️ Début de création de commande Douaniers avec: {Request}", 
                    System.Text.Json.JsonSerializer.Serialize(request));
                
                // Vérifier si les commandes sont bloquées
                var isBlocked = await _configurationService.IsCommandeBlockedAsync();
                
                if (isBlocked)
                {
                    _logger.LogWarning("🚫 Commandes bloquées - Création de commande Douaniers refusée");
                    return Json(new { success = false, message = "Les commandes sont actuellement bloquées. Vous ne pouvez pas créer de commandes pour les Douaniers." });
                }
                
                if (!Guid.TryParse(request.FormuleId, out var formuleId))
                {
                    _logger.LogWarning("❌ FormuleId invalide: {FormuleId}", request.FormuleId);
                    return Json(new { success = false, message = "Formule invalide." });
                }
                
                if (request.Quantite <= 0 || request.Quantite > 100)
                {
                    _logger.LogWarning("❌ Quantité invalide: {Quantite}", request.Quantite);
                    return Json(new { success = false, message = "La quantité doit être entre 1 et 100 plats." });
                }
                
                _logger.LogInformation("🔍 Recherche de la formule: {FormuleId}", formuleId);
                
                // Vérifier que la formule existe
                var formule = await _context.FormulesJour
                    .FirstOrDefaultAsync(f => f.IdFormule == formuleId && f.Supprimer == 0);
                
                if (formule == null)
                {
                    _logger.LogWarning("❌ Formule non trouvée: {FormuleId}", formuleId);
                    return Json(new { success = false, message = "La formule sélectionnée n'existe pas." });
                }
                
                _logger.LogInformation("✅ Formule trouvée: {NomFormule}", formule.NomFormule);
                
                // Vérifier que la formule n'est pas améliorée
                var isFormuleAmelioree = formule.NomFormule != null && (
                    formule.NomFormule.ToUpper().Contains("AMÉLIORÉ") ||
                    formule.NomFormule.ToUpper().Contains("AMELIORE") ||
                    formule.NomFormule.ToUpper().Contains("AMELIOREE")
                );
                
                if (isFormuleAmelioree)
                {
                    _logger.LogWarning("❌ Formule améliorée détectée: {NomFormule}", formule.NomFormule);
                    return Json(new { success = false, message = "Les formules améliorées ne sont pas autorisées pour les Douaniers. Veuillez sélectionner une formule standard." });
                }
                
                // Récupérer le groupe Douaniers
                var groupeDouaniers = await _context.GroupesNonCit
                    .FirstOrDefaultAsync(g => g.Nom == "Douaniers" && g.Supprimer == 0);
                
                if (groupeDouaniers == null)
                {
                    _logger.LogWarning("❌ Groupe Douaniers non trouvé");
                    return Json(new { success = false, message = "Le groupe Douaniers n'existe pas dans le système." });
                }
                
                _logger.LogInformation("✅ Groupe Douaniers trouvé: {GroupeId}", groupeDouaniers.Id);
                
                // Vérifier la restriction de formule standard pour les Douaniers (plus flexible)
                var hasStandardPlats = !string.IsNullOrEmpty(formule.PlatStandard1) || 
                                     !string.IsNullOrEmpty(formule.PlatStandard2) ||
                                     !string.IsNullOrEmpty(formule.Plat);
                
                if (!hasStandardPlats)
                {
                    _logger.LogWarning("❌ Aucun plat standard trouvé dans la formule: {NomFormule}", formule.NomFormule);
                    return Json(new { success = false, message = "Cette formule ne contient pas de plats standard. Les Douaniers ne peuvent commander que des plats standard." });
                }
                
                _logger.LogInformation("✅ Plats standard trouvés dans la formule");
                
                // Vérifier le quota permanent des Douaniers
                var periode = request.Periode == 1 ? Periode.Nuit : Periode.Jour;
                
                // Vérifier que le groupe a un quota défini
                if (!groupeDouaniers.QuotaJournalier.HasValue || !groupeDouaniers.QuotaNuit.HasValue)
                {
                    _logger.LogWarning("❌ Aucun quota défini pour le groupe Douaniers");
                    return Json(new { success = false, message = "Aucun quota n'a été défini pour les Douaniers. Veuillez contacter les RH/Administrateur pour configurer les quotas dans les paramètres." });
                }
                
                // Calculer les plats déjà consommés aujourd'hui pour cette période
                var aujourdhui = DateTime.Today;
                var platsConsommesAujourdhui = await _context.Commandes
                    .Where(c => c.GroupeNonCitId == groupeDouaniers.Id && 
                               c.DateConsommation.HasValue && c.DateConsommation.Value.Date == aujourdhui && 
                               c.PeriodeService == periode &&
                               c.Supprimer == 0)
                    .SumAsync(c => c.Quantite);
                
                // Déterminer le quota selon la période
                var quotaTotal = periode == Periode.Jour ? groupeDouaniers.QuotaJournalier.Value : groupeDouaniers.QuotaNuit.Value;
                var quotaRestant = quotaTotal - platsConsommesAujourdhui;
                
                _logger.LogInformation("📊 Quota Douaniers - {Periode}: {Consomme}/{Total} (Restant: {Restant})", 
                    periode == Periode.Jour ? "Jour" : "Nuit", platsConsommesAujourdhui, quotaTotal, quotaRestant);
                
                if (quotaRestant < request.Quantite)
                {
                    _logger.LogWarning("❌ Quota insuffisant pour les Douaniers: {Demande} > {Disponible}", request.Quantite, quotaRestant);
                    var periodeText = periode == Periode.Jour ? "jour" : "nuit";
                    return Json(new { 
                        success = false, 
                        message = $"Quota insuffisant pour les Douaniers. Demande: {request.Quantite} plats, Disponible: {quotaRestant} plats. Quota {periodeText}: {platsConsommesAujourdhui}/{quotaTotal}." 
                    });
                }
                
                // Créer la commande pour les Douaniers
                var commande = new Commande
                {
                    Date = DateTime.Now,
                    DateConsommation = DateTime.Today,
                    StatusCommande = (int)StatutCommande.Precommander,
                    CodeCommande = $"DOU-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}",
                    PeriodeService = request.Periode == 1 ? Periode.Nuit : Periode.Jour,
                    Montant = 0, // Les Douaniers ont généralement des plats gratuits
                    IdFormule = formuleId,
                    TypeClient = TypeClientCommande.GroupeNonCit,
                    GroupeNonCitId = groupeDouaniers.Id,
                    Site = request.Site == 1 ? SiteType.CIT_Billing : SiteType.CIT_Terminal,
                    CodeVerification = $"DOU-{request.Quantite}-{DateTime.Now:HHmm}",
                    Quantite = request.Quantite, // Ajouter la quantité
                    Instantanee = true // Marquer comme commande instantanée
                };
                
                _logger.LogInformation("🛡️ Création de la commande: {CodeCommande}", commande.CodeCommande);
                
                _context.Commandes.Add(commande);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("✅ Commande Douaniers sauvegardée avec succès: {CodeCommande}", commande.CodeCommande);
                
                // Note: Pas de PointConsommation pour les groupes non-CIT (Douaniers)
                // car ils ne sont pas des utilisateurs CIT avec des comptes individuels
                
                _logger.LogInformation("✅ Commande Douaniers créée avec succès: {CodeCommande} - {Quantite} plats - {Formule}", 
                    commande.CodeCommande, request.Quantite, formule.NomFormule);
                
                return Json(new { 
                    success = true, 
                    message = $"✅ Commande créée avec succès !\n\n🛡️ Code de commande : {commande.CodeCommande}\n📊 Quantité : {request.Quantite} plats\n🍽️ Formule : {formule.NomFormule}\n📍 Site : {(request.Site == 1 ? "CIT Billing" : "CIT Terminal")}\n\n💡 Utilisez le code de commande pour la validation.",
                    commandeId = commande.IdCommande,
                    codeCommande = commande.CodeCommande,
                    quantite = request.Quantite,
                    formule = formule.NomFormule,
                    site = request.Site == 1 ? "CIT Billing" : "CIT Terminal"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors de la création de la commande Douaniers");
                return Json(new { success = false, message = $"Erreur lors de la création de la commande: {ex.Message}" });
            }
        }
        
        /// <summary>
        /// Crée une commande instantanée pour un utilisateur (prestataire)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "PrestataireCantine")]
        public async Task<IActionResult> CreateInstantOrderForUser([FromBody] CreateInstantOrderRequest request)
        {
            try
            {
                // Vérifier si les commandes sont bloquées
                var isBlocked = await _configurationService.IsCommandeBlockedAsync();
                
                if (isBlocked)
                {
                    _logger.LogWarning("🚫 Commandes bloquées - Création de commande instantanée refusée");
                    return Json(new { success = false, message = "Les commandes sont actuellement bloquées. Vous ne pouvez pas créer de commandes instantanées." });
                }
                
                if (!Guid.TryParse(request.UserId, out var userId) || !Guid.TryParse(request.MenuId, out var menuId))
                {
                    return Json(new { success = false, message = "Données invalides." });
                }

                // Vérifier que l'utilisateur existe
                var user = await _context.Utilisateurs
                    .FirstOrDefaultAsync(u => u.Id == userId && u.Supprimer == 0);

                if (user == null)
                {
                    return Json(new { success = false, message = "Utilisateur non trouvé." });
                }

                // Vérifier que la formule existe
                var formule = await _context.FormulesJour
                    .FirstOrDefaultAsync(f => f.IdFormule == menuId && f.Supprimer == 0);

                if (formule == null)
                {
                    return Json(new { success = false, message = "Menu non trouvé." });
                }

                // Parser la période
                var periode = Periode.Jour; // Par défaut
                if (request.Periode != null && int.TryParse(request.Periode, out var periodeValue))
                {
                    periode = (Periode)periodeValue;
                }

                // Vérifier s'il existe déjà une commande CitUtilisateur pour cet utilisateur aujourd'hui pour cette période
                var commandeExistante = await _context.Commandes
                    .Where(c => c.UtilisateurId == userId 
                        && c.TypeClient == TypeClientCommande.CitUtilisateur
                        && c.Instantanee == true 
                        && c.DateConsommation.HasValue && c.DateConsommation.Value.Date == DateTime.Today
                        && c.PeriodeService == periode
                        && c.Supprimer == 0)
                    .FirstOrDefaultAsync();

                if (commandeExistante != null)
                {
                    var periodeText = periode == Periode.Jour ? "déjeuner (jour)" : "dîner (nuit)";
                    return Json(new { 
                        success = false, 
                        message = $"L'utilisateur a déjà une commande instantanée pour le {periodeText} aujourd'hui (Statut: {((StatutCommande)commandeExistante.StatusCommande).ToString()})" 
                    });
                }

                // Vérifier les quotas avant de créer la commande
                var verificationQuota = await VerifierQuotasDisponiblesAsync(formule, periode);
                if (!verificationQuota.Disponible)
                {
                    return Json(new { 
                        success = false, 
                        message = verificationQuota.Message 
                    });
                }

                // Créer la commande instantanée
                var commande = new Commande
                {
                    IdCommande = Guid.NewGuid(),
                    Date = DateTime.Now,
                    DateConsommation = DateTime.Today,
                    StatusCommande = (int)StatutCommande.Precommander,
                    CodeCommande = await GenerateCommandeCodeAsync(),
                    PeriodeService = periode,
                    Montant = CalculateMontant(formule, 1),
                    Quantite = 1,
                    IdFormule = menuId,
                    TypeClient = TypeClientCommande.CitUtilisateur,
                    UtilisateurId = userId,
                    Site = user.Site,
                    Instantanee = true,
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = User.Identity?.Name ?? "Prestataire"
                };

                _context.Commandes.Add(commande);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Commande instantanée créée par prestataire - ID: {IdCommande}, Utilisateur: {UserId}", 
                    commande.IdCommande, userId);

                return Json(new { 
                    success = true, 
                    commandeId = commande.IdCommande.ToString(),
                    message = "Commande instantanée créée avec succès !"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création de la commande instantanée par prestataire");
                return Json(new { success = false, message = "Erreur lors de la création de la commande." });
            }
        }

        /// <summary>
        /// Génère le texte d'affichage pour une formule avec les noms des plats
        /// </summary>
        private string GetFormuleDisplayText(FormuleJour formule)
        {
            // Utiliser la même logique que GenerateMenuDisplayName mais avec la date
            var dateStr = formule.Date.ToString("dd/MM/yyyy");
            var plats = new List<string>();
            
            // Ajouter les plats selon le type de formule
            if (!string.IsNullOrEmpty(formule.Plat))
            {
                plats.Add(formule.Plat);
                if (!string.IsNullOrEmpty(formule.Garniture))
                {
                    plats.Add(formule.Garniture);
                }
            }
            
            if (!string.IsNullOrEmpty(formule.PlatStandard1))
            {
                plats.Add(formule.PlatStandard1);
                if (!string.IsNullOrEmpty(formule.GarnitureStandard1))
                {
                    plats.Add(formule.GarnitureStandard1);
                }
            }
            
            if (!string.IsNullOrEmpty(formule.PlatStandard2))
            {
                plats.Add(formule.PlatStandard2);
                if (!string.IsNullOrEmpty(formule.GarnitureStandard2))
                {
                    plats.Add(formule.GarnitureStandard2);
                }
            }
            
            // Si aucun plat trouvé, utiliser le nom de formule
            if (!plats.Any())
            {
                return $"Menu du {dateStr} - {formule.NomFormule ?? "Menu sans description"}";
            }
            
            // Construire le nom d'affichage
            var nomFormule = formule.NomFormule ?? "Menu";
            var platsDescription = string.Join(" + ", plats);
            
            return $"Menu du {dateStr} - {nomFormule} - {platsDescription}";
        }

        /// <summary>
        /// Popule les ViewBags pour le formulaire de commande instantanée
        /// </summary>
        private async Task PopulateViewBagsForInstantOrder()
        {
            var aujourdhui = DateTime.Today;
            ViewBag.DateAujourdhui = aujourdhui;

            _logger.LogInformation("Recherche des formules pour la date: {Date}", aujourdhui);
            
            var formulesAujourdhui = await _context.FormulesJour
                .Where(f => f.Date.Date == aujourdhui && f.Supprimer == 0)
                .OrderBy(f => f.NomFormule)
                .ToListAsync();

            _logger.LogInformation("Formules trouvées pour aujourd'hui: {Count} formules", formulesAujourdhui.Count);

            // Si aucune formule pour aujourd'hui, charger les formules récentes
            if (!formulesAujourdhui.Any())
            {
                _logger.LogWarning("Aucune formule trouvée pour la date {Date}, chargement des formules récentes", aujourdhui);
                formulesAujourdhui = await _context.FormulesJour
                    .Where(f => f.Supprimer == 0)
                    .OrderByDescending(f => f.Date)
                    .Take(10) // Prendre les 10 dernières formules
                    .ToListAsync();
                _logger.LogInformation("Formules récentes chargées: {Count} formules", formulesAujourdhui.Count);
                
                // Si toujours aucune formule, créer des formules de test
                if (!formulesAujourdhui.Any())
                {
                    _logger.LogWarning("Aucune formule trouvée dans la base de données");
                    // Recharger les formules récentes
                    formulesAujourdhui = await _context.FormulesJour
                        .Where(f => f.Supprimer == 0)
                        .OrderByDescending(f => f.Date)
                        .Take(10)
                        .ToListAsync();
                    _logger.LogInformation("Formules récentes chargées: {Count} formules", formulesAujourdhui.Count);
                }
                
                // Log des détails des formules chargées
                foreach (var formule in formulesAujourdhui)
                {
                    _logger.LogInformation("Formule chargée: {NomFormule} - Date: {Date}", formule.NomFormule, formule.Date);
                }
            }
            else
            {
                _logger.LogInformation("Formules trouvées pour aujourd'hui: {Count} formules", formulesAujourdhui.Count);
                foreach (var formule in formulesAujourdhui)
                {
                    _logger.LogInformation("Formule d'aujourd'hui: {NomFormule} - Date: {Date}", formule.NomFormule, formule.Date);
                }
            }

            // Les commandes instantanées sont uniquement pour les employés CIT
            // Ne pas charger les groupes non-CIT
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
            // ViewBag.GroupesNonCit supprimé - Les commandes instantanées sont uniquement pour les employés CIT
            ViewBag.Utilisateurs = new SelectList(selectListItems, "Value", "Text");
            ViewBag.UtilisateursData = utilisateurs.ToDictionary(u => u.Value, u => u.UserName);
            // ViewBag.Directions supprimé - Table Direction non utilisée
            
            _logger.LogInformation("ViewBag.FormulesAujourdhui créé avec {Count} éléments", formulesSelectList.Count);
            foreach (var formule in formulesSelectList)
            {
                _logger.LogInformation("Formule dans SelectList: {DisplayText} (ID: {IdFormule})", formule.DisplayText, formule.IdFormule);
            }

            // Les commandes instantanées sont uniquement pour les employés CIT
            var typesClient = new[]
            {
                new { Value = TypeClientCommande.CitUtilisateur.ToString(), Text = "Employé CIT" }
            };
            ViewBag.TypeClients = new SelectList(typesClient, "Value", "Text");
        }

        /// <summary>
        /// Affiche le formulaire de création de commande groupée pour visiteurs
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Administrateur,RH")]
        public async Task<IActionResult> CreerCommandeGroupee()
        {
            try
            {
                // Récupérer les directions
                var directions = await _context.Directions
                    .Where(d => d.Supprimer == 0)
                    .OrderBy(d => d.Nom)
                    .ToListAsync();

                if (!directions.Any())
                {
                    TempData["ErrorMessage"] = "Aucune direction n'est configurée. Veuillez d'abord créer des directions.";
                    return RedirectToAction("Index");
                }

                ViewBag.Directions = new SelectList(directions, "Id", "Nom");

                var model = new CommandeGroupeeViewModel
                {
                    DateDebut = DateTime.Today.AddDays(2),
                    DateFin = DateTime.Today.AddDays(2)
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
        /// Traite la création de commande groupée pour visiteurs
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrateur,RH")]
        public async Task<IActionResult> CreerCommandeGroupee(CommandeGroupeeViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await PopulateViewBagsForGroupOrder();
                    return View(model);
                }

                // Vérifier que la direction existe
                var direction = await _context.Directions
                    .FirstOrDefaultAsync(d => d.Id == model.DirectionId && d.Supprimer == 0);

                if (direction == null)
                {
                    ModelState.AddModelError(nameof(model.DirectionId), "La direction sélectionnée n'existe pas.");
                    await PopulateViewBagsForGroupOrder();
                    return View(model);
                }

                // Vérifier que dateFin >= dateDebut
                if (model.DateFin < model.DateDebut)
                {
                    ModelState.AddModelError(nameof(model.DateFin), "La date de fin doit être supérieure ou égale à la date de début.");
                    await PopulateViewBagsForGroupOrder();
                    return View(model);
                }

                // Vérifier le délai de 48h minimum pour la date de début
                var maintenant = DateTime.Now;
                var dateDebutConsommation = model.DateDebut.Date.AddHours(12); // 12h00 de la date de début
                var delaiMinimum = maintenant.AddHours(48);

                if (dateDebutConsommation < delaiMinimum)
                {
                    ModelState.AddModelError(nameof(model.DateDebut), 
                        $"La commande doit être créée au moins 48h à l'avance. Date minimum : {delaiMinimum:dd/MM/yyyy HH:mm}");
                    await PopulateViewBagsForGroupOrder();
                    return View(model);
                }

                // Récupérer les formules sélectionnées depuis le JSON
                var formulesSelectionneesJson = Request.Form["FormulesSelectionneesJson"].ToString();
                if (string.IsNullOrEmpty(formulesSelectionneesJson))
                {
                    ModelState.AddModelError("", "Veuillez sélectionner au moins une formule améliorée.");
                    await PopulateViewBagsForGroupOrder();
                    return View(model);
                }

                // Désérialiser les formules sélectionnées
                List<(string formuleId, string date)> formulesSelectionnees;
                try
                {
                    using var doc = System.Text.Json.JsonDocument.Parse(formulesSelectionneesJson);
                    formulesSelectionnees = doc.RootElement.EnumerateArray()
                        .Select(e => (
                            formuleId: e.GetProperty("formuleId").GetString() ?? "", 
                            date: e.GetProperty("date").GetString() ?? ""
                        ))
                        .Where(f => !string.IsNullOrEmpty(f.formuleId) && !string.IsNullOrEmpty(f.date))
                        .ToList();
                }
                catch
                {
                    ModelState.AddModelError("", "Erreur lors de la lecture des formules sélectionnées.");
                    await PopulateViewBagsForGroupOrder();
                    return View(model);
                }

                if (formulesSelectionnees == null || formulesSelectionnees.Count == 0)
                {
                    ModelState.AddModelError("", "Veuillez sélectionner au moins une formule améliorée.");
                    await PopulateViewBagsForGroupOrder();
                    return View(model);
                }

                // Créer une commande pour chaque formule sélectionnée
                var commandesCreees = 0;
                var nomVisiteur = !string.IsNullOrEmpty(model.VisiteurNom) ? model.VisiteurNom : $"Groupe de {model.NombreVisiteurs} visiteur(s)";

                foreach (var (formuleIdStr, dateStr) in formulesSelectionnees)
                {

                    if (string.IsNullOrEmpty(formuleIdStr) || string.IsNullOrEmpty(dateStr))
                        continue;

                    if (!Guid.TryParse(formuleIdStr, out var formuleId))
                        continue;

                    if (!DateTime.TryParse(dateStr, out var dateConsommation))
                        continue;

                    // Vérifier que la formule existe et qu'elle est améliorée
                    var formule = await _context.FormulesJour
                        .FirstOrDefaultAsync(f => f.IdFormule == formuleId && f.Supprimer == 0);

                    if (formule == null)
                        continue;

                    var isFormuleAmelioree = formule.NomFormule?.ToUpper().Contains("AMÉLIORÉ") == true ||
                                            formule.NomFormule?.ToUpper().Contains("AMELIORE") == true ||
                                            formule.NomFormule?.ToUpper().Contains("AMELIOREE") == true;

                    if (!isFormuleAmelioree)
                        continue;

                    // Vérifier que la date est dans la période
                    if (dateConsommation.Date < model.DateDebut.Date || dateConsommation.Date > model.DateFin.Date)
                        continue;

                    // Créer la commande
                    var commande = new Commande
                    {
                        IdCommande = Guid.NewGuid(),
                        Date = DateTime.Now,
                        DateConsommation = dateConsommation,
                        StatusCommande = (int)StatutCommande.Precommander,
                        CodeCommande = await GenerateCommandeCodeAsync(),
                        PeriodeService = model.PeriodeService,
                        Montant = CalculateMontant(formule, model.NombreVisiteurs),
                        Quantite = model.NombreVisiteurs,
                        IdFormule = formuleId,
                        TypeClient = TypeClientCommande.Visiteur,
                        VisiteurNom = nomVisiteur,
                        VisiteurTelephone = null, // Supprimé
                        UtilisateurId = null,
                        Site = model.Site,
                        Instantanee = false,
                        CreatedOn = DateTime.UtcNow,
                        CreatedBy = User.Identity?.Name ?? "System"
                    };

                    _context.Commandes.Add(commande);
                    commandesCreees++;
                }

                if (commandesCreees == 0)
                {
                    ModelState.AddModelError("", "Aucune commande n'a pu être créée. Vérifiez que les formules sélectionnées sont valides et améliorées.");
                    await PopulateViewBagsForGroupOrder();
                    return View(model);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Commandes groupées créées avec succès - {Count} commande(s), Visiteur: {VisiteurNom}, Nombre de visiteurs: {NombreVisiteurs}", 
                    commandesCreees, nomVisiteur, model.NombreVisiteurs);

                TempData["SuccessMessage"] = $"{commandesCreees} commande(s) groupée(s) créée(s) avec succès pour {nomVisiteur} !";
                return RedirectToAction("Index");
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
        /// Retourne tous les menus disponibles pour une plage de dates
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMenusByDateRange(DateTime? dateDebut, DateTime? dateFin)
        {
            try
            {
                if (!dateDebut.HasValue || !dateFin.HasValue)
                {
                    return Json(new List<object>());
                }

                var menus = await _context.FormulesJour
                    .Where(f => f.Supprimer == 0 
                             && f.Date >= dateDebut.Value 
                             && f.Date <= dateFin.Value)
                    .OrderBy(f => f.Date)
                    .ThenBy(f => f.NomFormule)
                    .ToListAsync();

                var menusList = menus.Select(f => new
                {
                    value = $"{f.IdFormule}|{f.Date:yyyy-MM-dd}",
                    text = $"{f.NomFormule} - {GetFormuleDisplayText(f)}",
                    date = f.Date.ToString("yyyy-MM-dd"),
                    idFormule = f.IdFormule.ToString()
                }).ToList();

                return Json(menusList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement des menus par plage de dates");
                return Json(new List<object>());
            }
        }

        /// <summary>
        /// Popule les ViewBags pour le formulaire de commande groupée
        /// </summary>
        private async Task PopulateViewBagsForGroupOrder()
        {
            var directions = await _context.Directions
                .Where(d => d.Supprimer == 0)
                .OrderBy(d => d.Nom)
                .ToListAsync();

            ViewBag.Directions = new SelectList(directions, "Id", "Nom");
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

        /// <summary>
        /// Exporte la liste des commandes vers Excel
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ExportExcel(string? status = null, DateTime? dateDebut = null, DateTime? dateFin = null)
        {
            try
            {
                // Utiliser la même logique que l'action Index pour récupérer les données
                var query = _context.Commandes
                    .Include(c => c.Utilisateur)
                    .Include(c => c.FormuleJour)
                    .ThenInclude(f => f.NomFormuleNavigation)
                    .Where(c => c.Supprimer == 0);

                // Appliquer les filtres
                if (!string.IsNullOrEmpty(status))
                {
                    if (Enum.TryParse<StatutCommande>(status, out var statusEnum))
                    {
                        query = query.Where(c => c.StatusCommande == (int)statusEnum);
                    }
                }

                if (dateDebut.HasValue)
                {
                    query = query.Where(c => c.Date >= dateDebut.Value.Date);
                }

                if (dateFin.HasValue)
                {
                    query = query.Where(c => c.Date <= dateFin.Value.Date);
                }

                // Récupérer toutes les données (sans pagination pour l'export)
                var commandes = await query
                    .OrderByDescending(c => c.Date)
                    .Select(c => new CommandeListViewModel
                    {
                        IdCommande = c.IdCommande,
                        CodeCommande = c.CodeCommande,
                        Date = c.Date,
                        DateConsommation = c.DateConsommation,
                        Quantite = c.Quantite,
                        StatusCommande = (StatutCommande)c.StatusCommande,
                        PeriodeService = c.PeriodeService,
                        Site = c.Site,
                        UtilisateurNom = c.Utilisateur != null ? $"{c.Utilisateur.Nom} {c.Utilisateur.Prenoms}" : "N/A",
                        FormuleNom = c.FormuleJour != null ? c.FormuleJour.NomFormule : "N/A",
                        TypeFormuleNom = c.FormuleJour != null ? c.FormuleJour.NomFormule : "N/A",
                        CreatedOn = c.CreatedOn,
                        CreatedBy = c.CreatedBy
                    })
                    .ToListAsync();

                // Générer le nom du fichier
                var fileName = $"Commandes_{DateTime.Now:yyyyMMdd_HHmmss}";
                var title = "Liste des Commandes";

                // Exporter vers Excel
                var excelBytes = _excelExportService.ExportToExcel(commandes, fileName, "Commandes", title);

                _logger.LogInformation("Export Excel des commandes effectué - {Count} commandes exportées", commandes.Count);

                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{fileName}.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'export Excel des commandes");
                TempData["ErrorMessage"] = "Une erreur est survenue lors de l'export Excel.";
                return RedirectToAction(nameof(Index));
            }
        }





        /// <summary>
        /// Popule les ViewBags pour le formulaire de commande Douaniers
        /// </summary>
        private async Task PopulateViewBagsForDouaniers()
        {
            var aujourdhui = ViewBag.DateAujourdhui as DateTime? ?? DateTime.Today;
            
            // Récupérer les formules du jour seulement si pas déjà définies (exclure les formules améliorées)
            if (ViewBag.FormulesAujourdhui == null)
            {
                var dateAujourdhui = aujourdhui.Date;
                var formulesAujourdhui = await _context.FormulesJour
                    .Where(f => f.Date.Date == dateAujourdhui && 
                               f.Supprimer == 0 &&
                               !(f.NomFormule != null && (
                                   f.NomFormule.ToUpper().Contains("AMÉLIORÉ") ||
                                   f.NomFormule.ToUpper().Contains("AMELIORE") ||
                                   f.NomFormule.ToUpper().Contains("AMELIOREE")
                               )))
                    .OrderBy(f => f.NomFormule)
                    .ToListAsync();

                var formulesList = formulesAujourdhui.Select(f => new SelectListItem
                {
                    Value = f.IdFormule.ToString(),
                    Text = GenerateMenuDisplayName(f)
                }).ToList();

                ViewBag.FormulesAujourdhui = new SelectList(formulesList, "Value", "Text");
                ViewBag.DateAujourdhui = aujourdhui;
            }
            
            // Récupérer le groupe Douaniers avec ses quotas permanents
            var groupeDouaniers = await _context.GroupesNonCit
                .FirstOrDefaultAsync(g => g.Nom == "Douaniers" && g.Supprimer == 0);
            
            _logger.LogInformation("🔍 Recherche du groupe Douaniers: {GroupeExiste}", groupeDouaniers != null ? "Trouvé" : "Non trouvé");
            
            if (groupeDouaniers != null)
            {
                _logger.LogInformation("📊 Groupe Douaniers trouvé - QuotaJour: {QuotaJour}, QuotaNuit: {QuotaNuit}", 
                    groupeDouaniers.QuotaJournalier, groupeDouaniers.QuotaNuit);
                // Calculer les plats déjà consommés aujourd'hui
                var dateAujourdhui = aujourdhui.Date;
                var platsConsommesJour = await _context.Commandes
                    .Where(c => c.GroupeNonCitId == groupeDouaniers.Id && 
                               c.DateConsommation.HasValue && c.DateConsommation.Value.Date == dateAujourdhui && 
                               c.PeriodeService == Periode.Jour &&
                               c.Supprimer == 0)
                    .SumAsync(c => c.Quantite);
                
                var platsConsommesNuit = await _context.Commandes
                    .Where(c => c.GroupeNonCitId == groupeDouaniers.Id && 
                               c.DateConsommation.HasValue && c.DateConsommation.Value.Date == dateAujourdhui && 
                               c.PeriodeService == Periode.Nuit &&
                               c.Supprimer == 0)
                    .SumAsync(c => c.Quantite);
                
                // Créer un objet pour afficher les statistiques
                var quotaInfo = new
                {
                    QuotaJour = groupeDouaniers.QuotaJournalier ?? 0,
                    QuotaNuit = groupeDouaniers.QuotaNuit ?? 0,
                    PlatsConsommesJour = platsConsommesJour,
                    PlatsConsommesNuit = platsConsommesNuit,
                    PlatsRestantsJour = Math.Max(0, (groupeDouaniers.QuotaJournalier ?? 0) - platsConsommesJour),
                    PlatsRestantsNuit = Math.Max(0, (groupeDouaniers.QuotaNuit ?? 0) - platsConsommesNuit),
                    RestrictionFormuleStandard = groupeDouaniers.RestrictionFormuleStandard
                };
                
                // Ne définir ViewBag.QuotaDouaniers que si au moins un quota est défini et > 0
                var quotaJour = groupeDouaniers.QuotaJournalier ?? 0;
                var quotaNuit = groupeDouaniers.QuotaNuit ?? 0;
                
                _logger.LogInformation("🔍 Vérification des quotas - QuotaJour: {QuotaJour}, QuotaNuit: {QuotaNuit}", quotaJour, quotaNuit);
                
                if (quotaJour > 0 || quotaNuit > 0)
                {
                    _logger.LogInformation("✅ Quotas valides détectés - Définition de ViewBag.QuotaDouaniers");
                    ViewBag.QuotaDouaniers = quotaInfo;
                }
                else
                {
                    _logger.LogWarning("⚠️ Quotas à 0 détectés - ViewBag.QuotaDouaniers défini à null");
                    ViewBag.QuotaDouaniers = null; // Forcer à null si les quotas sont à 0
                }
                
                // Ne pas écraser ViewBag.GroupeDouaniers s'il est déjà défini
                if (ViewBag.GroupeDouaniers == null)
                {
                    ViewBag.GroupeDouaniers = groupeDouaniers;
                }
            }
            else
            {
                _logger.LogWarning("⚠️ Groupe Douaniers non trouvé dans la base de données");
            }
        }

        /// <summary>
        /// Vérifie si une commande peut être modifiée ou supprimée selon les règles métier.
        /// - Avant la date de consommation : modifiable pour les rôles autorisés (Edit/Delete) si non consommée.
        /// - Après la date de consommation : seule un Administrateur peut modifier/supprimer, et uniquement si la commande n'est pas consommée.
        /// - Une commande consommée n'est jamais modifiable/supprimable, même par un Administrateur.
        /// </summary>
        private bool CanModifyCommande(Commande commande)
        {
            if (commande.DateConsommation == null)
                return false;

            // Règle 0 : une commande consommée n'est jamais modifiable/supprimable
            if (commande.StatusCommande == (int)StatutCommande.Consommee)
            {
                _logger.LogInformation("❌ Commande non modifiable - Déjà consommée (Statut: {Status})", (StatutCommande)commande.StatusCommande);
                return false;
            }

            var dateConsommation = commande.DateConsommation.Value.Date;
            var aujourdHui = DateTime.Today;

            // Exception Administrateur : peut modifier/supprimer même après la date (tant que non consommée)
            if (User.IsInRole("Administrateur"))
            {
                _logger.LogInformation("✅ Commande modifiable - Administrateur (date de consommation: {Date})", dateConsommation);
                return true;
            }

            // Règle générale : tant que la date de consommation n'est pas encore arrivée, la commande est modifiable/supprimable
            if (dateConsommation >= aujourdHui)
            {
                _logger.LogInformation("✅ Commande modifiable - Date de consommation non encore arrivée: {Date}", dateConsommation);
                return true;
            }

            // Après la date de consommation, aucun autre rôle ne peut modifier/supprimer
            _logger.LogInformation("❌ Commande non modifiable - Date de consommation déjà passée: {Date}", dateConsommation);
            return false;
        }

        /// <summary>
        /// Génère un nom d'affichage descriptif pour un menu basé sur les plats
        /// </summary>
        private string GenerateMenuDisplayName(FormuleJour formule)
        {
            var plats = new List<string>();
            
            // Ajouter les plats selon le type de formule
            if (!string.IsNullOrEmpty(formule.Plat))
            {
                plats.Add(formule.Plat);
                if (!string.IsNullOrEmpty(formule.Garniture))
                {
                    plats.Add(formule.Garniture);
                }
            }
            
            if (!string.IsNullOrEmpty(formule.PlatStandard1))
            {
                plats.Add(formule.PlatStandard1);
                if (!string.IsNullOrEmpty(formule.GarnitureStandard1))
                {
                    plats.Add(formule.GarnitureStandard1);
                }
            }
            
            if (!string.IsNullOrEmpty(formule.PlatStandard2))
            {
                plats.Add(formule.PlatStandard2);
                if (!string.IsNullOrEmpty(formule.GarnitureStandard2))
                {
                    plats.Add(formule.GarnitureStandard2);
                }
            }
            
            // Si aucun plat trouvé, utiliser le nom de formule
            if (!plats.Any())
            {
                return formule.NomFormule ?? "Menu sans description";
            }
            
            // Construire le nom d'affichage
            var nomFormule = formule.NomFormule ?? "Menu";
            var platsDescription = string.Join(" + ", plats);
            
            return $"{nomFormule} - {platsDescription}";
        }

        /// <summary>
        /// Récupère le plat principal d'une formule pour le point de consommation
        /// </summary>
        private string GetPlatPrincipal(FormuleJour formule)
        {
            // Priorité : Plat principal > PlatStandard1 > PlatStandard2
            if (!string.IsNullOrEmpty(formule.Plat))
            {
                var platPrincipal = formule.Plat;
                if (!string.IsNullOrEmpty(formule.Garniture))
                {
                    platPrincipal += $" + {formule.Garniture}";
                }
                return platPrincipal;
            }
            
            if (!string.IsNullOrEmpty(formule.PlatStandard1))
            {
                var platStandard1 = formule.PlatStandard1;
                if (!string.IsNullOrEmpty(formule.GarnitureStandard1))
                {
                    platStandard1 += $" + {formule.GarnitureStandard1}";
                }
                return platStandard1;
            }
            
            if (!string.IsNullOrEmpty(formule.PlatStandard2))
            {
                var platStandard2 = formule.PlatStandard2;
                if (!string.IsNullOrEmpty(formule.GarnitureStandard2))
                {
                    platStandard2 += $" + {formule.GarnitureStandard2}";
                }
                return platStandard2;
            }
            
            // Fallback
            return formule.NomFormule ?? "Menu Douaniers";
        }

        /// <summary>
        /// Affiche la page de validation des commandes Douaniers
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "PrestataireCantine,Administrateur,RH")]
        public IActionResult ValiderCommandeDouaniers()
        {
            return View();
        }

        /// <summary>
        /// Valide une commande Douaniers par code de commande
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "PrestataireCantine,Administrateur,RH")]
        public async Task<IActionResult> ValiderCommandeDouaniers([FromBody] ValiderCommandeDouaniersRequest request)
        {
            try
            {
                _logger.LogInformation("🛡️ Validation de commande Douaniers avec code: {CodeCommande}", request.CodeCommande);
                
                if (string.IsNullOrWhiteSpace(request.CodeCommande))
                {
                    return Json(new { success = false, message = "Code de commande requis." });
                }
                
                // Rechercher la commande par code
                var commande = await _context.Commandes
                    .Include(c => c.FormuleJour)
                    .Include(c => c.GroupeNonCit)
                    .FirstOrDefaultAsync(c => c.CodeCommande == request.CodeCommande && c.Supprimer == 0);
                
                if (commande == null)
                {
                    _logger.LogWarning("❌ Commande non trouvée avec le code: {CodeCommande}", request.CodeCommande);
                    return Json(new { success = false, message = "Commande non trouvée avec ce code." });
                }
                
                // Vérifier que c'est bien une commande Douaniers
                if (commande.TypeClient != TypeClientCommande.GroupeNonCit || 
                    !commande.CodeCommande.StartsWith("DOU-"))
                {
                    _logger.LogWarning("❌ Ce n'est pas une commande Douaniers: {CodeCommande}", request.CodeCommande);
                    return Json(new { success = false, message = "Ce code ne correspond pas à une commande Douaniers." });
                }
                
                // Vérifier que la commande n'est pas déjà validée
                if (commande.StatusCommande == (int)StatutCommande.Consommee)
                {
                    _logger.LogWarning("⚠️ Commande déjà validée: {CodeCommande}", request.CodeCommande);
                    return Json(new { success = false, message = "Cette commande est déjà validée." });
                }
                
                // Valider la commande
                commande.StatusCommande = (int)StatutCommande.Consommee;
                commande.ModifiedOn = DateTime.UtcNow;
                commande.ModifiedBy = User.Identity?.Name ?? "PrestataireCantine";
                
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("✅ Commande Douaniers validée: {CodeCommande} - {Quantite} plats", 
                    commande.CodeCommande, commande.Quantite);
                
                return Json(new { 
                    success = true, 
                    message = $"✅ Commande validée avec succès !\n\n🛡️ Code: {commande.CodeCommande}\n📊 Quantité: {commande.Quantite} plats\n🍽️ Formule: {commande.FormuleJour?.NomFormule}\n📍 Site: {(commande.Site == SiteType.CIT_Billing ? "CIT Billing" : "CIT Terminal")}",
                    codeCommande = commande.CodeCommande,
                    quantite = commande.Quantite,
                    formule = commande.FormuleJour?.NomFormule,
                    site = commande.Site == SiteType.CIT_Billing ? "CIT Billing" : "CIT Terminal"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors de la validation de la commande Douaniers: {CodeCommande}", request.CodeCommande);
                return Json(new { success = false, message = $"Erreur lors de la validation: {ex.Message}" });
            }
        }
    }

    // Classes de requête pour les nouvelles méthodes
    public class AuthenticateUserRequest
    {
        public string Matricule { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class CreateInstantOrderRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string MenuId { get; set; } = string.Empty;
        public string? Periode { get; set; }
    }
    
    public class CreateDouanierOrderRequest
    {
        public string FormuleId { get; set; } = string.Empty;
        public int Quantite { get; set; }
        public int Periode { get; set; }
        public int Site { get; set; }
    }

    public class ValiderCommandeDouaniersRequest
    {
        public string CodeCommande { get; set; } = string.Empty;
    }

    public class GetUserByMatriculeRequest
    {
        public string Matricule { get; set; } = string.Empty;
    }
}

// Extension du CommandeController pour les points de consommation
namespace Obeli_K.Controllers
{
    public partial class CommandeController
    {
        /// <summary>
        /// Calcule et affiche les points de consommation de l'utilisateur connecté
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Administrateur,RH,Employe")]
        public async Task<IActionResult> MesPointsConsommation(DateTime? dateDebut, DateTime? dateFin)
        {
            try
            {
                // Récupérer l'ID de l'utilisateur connecté depuis les claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized();
                }

                // Récupérer l'utilisateur connecté par ID
                var utilisateur = await _context.Utilisateurs
                    .Include(u => u.Direction)
                    .Include(u => u.Fonction)
                    .FirstOrDefaultAsync(u => u.Id == userId && u.Supprimer == 0);

                if (utilisateur == null)
                {
                    TempData["ErrorMessage"] = "Utilisateur non trouvé.";
                    return RedirectToAction("Index", "Home");
                }

                // Définir les dates par défaut (30 derniers jours)
                var dateDebutValue = dateDebut ?? DateTime.Today.AddDays(-30);
                var dateFinValue = dateFin ?? DateTime.Today;

                _logger.LogInformation("Calcul des points de consommation pour {UserName} du {DateDebut} au {DateFin}",
                    utilisateur.UserName, dateDebutValue.ToString("yyyy-MM-dd"), dateFinValue.ToString("yyyy-MM-dd"));

                // Récupérer les points de consommation de l'utilisateur
                var pointsConsommation = await _context.PointsConsommation
                    .Include(pc => pc.Commande)
                        .ThenInclude(c => c.FormuleJour)
                    .Where(pc => pc.UtilisateurId == utilisateur.Id
                              && pc.Supprimer == 0
                              && pc.DateConsommation >= dateDebutValue.Date
                              && pc.DateConsommation <= dateFinValue.Date)
                    .OrderByDescending(pc => pc.DateConsommation)
                    .ToListAsync();

                // Calculer les statistiques
                var totalConsommations = pointsConsommation.Count;
                var totalMontant = pointsConsommation.Sum(pc => CalculerCoutPoint(pc));

                // Grouper par formule
                var parFormule = pointsConsommation
                    .GroupBy(pc => pc.TypeFormule ?? "Inconnu")
                    .Select(g => new
                    {
                        Formule = g.Key,
                        Nombre = g.Count(),
                        Montant = g.Sum(pc => CalculerCoutPoint(pc))
                    })
                    .OrderByDescending(x => x.Nombre)
                    .ToList();

                // Grouper par mois
                var parMois = pointsConsommation
                    .GroupBy(pc => new { pc.DateConsommation.Year, pc.DateConsommation.Month })
                    .Select(g => new
                    {
                        Annee = g.Key.Year,
                        Mois = g.Key.Month,
                        NomMois = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMMM yyyy"),
                        Nombre = g.Count(),
                        Montant = g.Sum(pc => CalculerCoutPoint(pc))
                    })
                    .OrderByDescending(x => x.Annee)
                    .ThenByDescending(x => x.Mois)
                    .ToList();

                // Préparer le ViewModel
                var viewModel = new
                {
                    Utilisateur = new
                    {
                        utilisateur.Nom,
                        utilisateur.Prenoms,
                        NomComplet = $"{utilisateur.Nom} {utilisateur.Prenoms}",
                        utilisateur.Email,
                        utilisateur.UserName
                    },
                    Periode = new
                    {
                        DateDebut = dateDebutValue,
                        DateFin = dateFinValue
                    },
                    Statistiques = new
                    {
                        TotalConsommations = totalConsommations,
                        TotalMontant = totalMontant,
                        MoyenneParJour = totalConsommations > 0 
                            ? Math.Round((double)totalConsommations / ((dateFinValue - dateDebutValue).Days + 1), 2) 
                            : 0,
                        MoyenneMontantParConsommation = totalConsommations > 0 
                            ? Math.Round(totalMontant / totalConsommations, 0) 
                            : 0
                    },
                    ParFormule = parFormule,
                    ParMois = parMois,
                    PointsConsommation = pointsConsommation.Select(pc => new
                    {
                        pc.IdPointConsommation,
                        pc.DateConsommation,
                        TypeFormule = pc.TypeFormule ?? "Inconnu",
                        NomPlat = pc.NomPlat ?? pc.Commande?.FormuleJour?.Plat ?? "Non spécifié",
                        pc.QuantiteConsommee,
                        Cout = CalculerCoutPoint(pc),
                        LieuConsommation = pc.LieuConsommation ?? "Restaurant CIT",
                        CodeCommande = pc.Commande?.CodeCommande,
                        pc.CreatedOn
                    }).ToList()
                };

                ViewBag.DateDebut = dateDebutValue.ToString("yyyy-MM-dd");
                ViewBag.DateFin = dateFinValue.ToString("yyyy-MM-dd");

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du calcul des points de consommation pour l'utilisateur connecté");
                TempData["ErrorMessage"] = "Erreur lors du calcul des points de consommation.";
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// Calcule le coût d'un point de consommation
        /// </summary>
        private decimal CalculerCoutPoint(PointConsommation pc)
        {
            // Si la commande est consommée ou facturée
            if (pc.Commande?.StatusCommande == 1 || pc.Commande?.StatusCommande == 3 ||
                (pc.Commande?.StatusCommande == 0 && pc.LieuConsommation?.Contains("FACTURATION") == true))
            {
                // Pour les commandes facturées, extraire le montant du lieu de consommation
                if (pc.LieuConsommation?.Contains("FACTURATION") == true)
                {
                    var match = System.Text.RegularExpressions.Regex.Match(pc.LieuConsommation, @"\((\d+(?:\.\d+)?)\s*F\s*CFA\)");
                    if (match.Success && decimal.TryParse(match.Groups[1].Value, out var montantFacture))
                    {
                        return montantFacture;
                    }
                }

                // Calcul standard basé sur le type de formule
                var typeFormule = pc.TypeFormule ?? "Standard";
                var prixUnitaire = GetPrixFormuleStandard(typeFormule);
                return pc.QuantiteConsommee * prixUnitaire;
            }

            return 0;
        }

        /// <summary>
        /// Retourne le prix standard d'une formule
        /// </summary>
        private decimal GetPrixFormuleStandard(string? nomFormule)
        {
            if (string.IsNullOrWhiteSpace(nomFormule))
            {
                return 550m;
            }

            return nomFormule.ToLower() switch
            {
                "amélioré" or "ameliore" or "améliorée" or "ameliorée" => 2800m,
                "standard" or "standard 1" or "standard 2" => 550m,
                _ => 550m
            };
        }

        /// <summary>
        /// Affiche le cumul de points de consommation de tous les utilisateurs
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Administrateur,RH")]
        public async Task<IActionResult> CumulPointsConsommation(DateTime? dateDebut, DateTime? dateFin)
        {
            try
            {
                // Définir les dates par défaut (du 17 du mois n-1 au 16 du mois en cours)
                var today = DateTime.Today;
                var moisPrecedent = today.Month == 1 ? 12 : today.Month - 1;
                var anneePrecedente = today.Month == 1 ? today.Year - 1 : today.Year;
                var dateDebutValue = dateDebut ?? new DateTime(anneePrecedente, moisPrecedent, 17);
                var dateFinValue = dateFin ?? new DateTime(today.Year, today.Month, 16);

                _logger.LogInformation("Calcul du cumul des points de consommation du {DateDebut} au {DateFin}",
                    dateDebutValue.ToString("yyyy-MM-dd"), dateFinValue.ToString("yyyy-MM-dd"));

                // Récupérer tous les points de consommation dans la période
                var pointsConsommation = await _context.PointsConsommation
                    .Include(pc => pc.Commande)
                        .ThenInclude(c => c.FormuleJour)
                    .Where(pc => pc.DateConsommation >= dateDebutValue && 
                                pc.DateConsommation <= dateFinValue &&
                                pc.Supprimer == 0)
                    .OrderBy(pc => pc.DateConsommation)
                    .ToListAsync();

                // Calculer les statistiques cumulatives
                var totalPoints = pointsConsommation.Count;
                var totalUtilisateurs = pointsConsommation.Select(pc => pc.UtilisateurId).Distinct().Count();
                
                // Calculer les repas consommés (statut Consommée)
                var totalRepasConsommes = pointsConsommation
                    .Count(pc => pc.Commande?.StatusCommande == (int)StatutCommande.Consommee);
                
                // Calculer les repas non récupérés (statut NonRecuperer ou Précommandée avec FACTURATION)
                var totalRepasNonRecuperes = pointsConsommation
                    .Count(pc => pc.Commande?.StatusCommande == (int)StatutCommande.NonRecuperer ||
                                (pc.Commande?.StatusCommande == (int)StatutCommande.Precommander && 
                                 pc.LieuConsommation?.Contains("FACTURATION") == true));
                
                // Calculer les repas indisponibles (statut Indisponible)
                var totalRepasIndisponibles = pointsConsommation
                    .Count(pc => pc.Commande?.StatusCommande == (int)StatutCommande.Indisponible);
                
                // Calculer le coût total : uniquement repas consommés + repas non récupérés
                var totalCout = pointsConsommation
                    .Where(pc => pc.Commande?.StatusCommande == (int)StatutCommande.Consommee ||
                                pc.Commande?.StatusCommande == (int)StatutCommande.NonRecuperer ||
                                (pc.Commande?.StatusCommande == (int)StatutCommande.Precommander && 
                                 pc.LieuConsommation?.Contains("FACTURATION") == true))
                    .Sum(pc => CalculerCoutPoint(pc));

                // Grouper par utilisateur pour les statistiques détaillées
                var utilisateursIds = pointsConsommation.Select(pc => pc.UtilisateurId).Distinct().ToList();
                var utilisateurs = await _context.Utilisateurs
                    .Where(u => utilisateursIds.Contains(u.Id))
                    .Select(u => new { u.Id, u.Nom, u.Prenoms })
                    .ToListAsync();

                var cumulParUtilisateur = pointsConsommation
                    .GroupBy(pc => pc.UtilisateurId)
                    .Select(g => new
                    {
                        UtilisateurId = g.Key,
                        NomComplet = utilisateurs.FirstOrDefault(u => u.Id == g.Key) != null 
                            ? $"{utilisateurs.First(u => u.Id == g.Key).Nom} {utilisateurs.First(u => u.Id == g.Key).Prenoms}"
                            : $"Utilisateur {g.Key.ToString().Substring(0, 8)}...",
                        NombrePoints = g.Count(),
                        CoutTotal = g.Sum(pc => CalculerCoutPoint(pc)),
                        DerniereConsommation = g.Max(pc => pc.DateConsommation)
                    })
                    .OrderByDescending(x => x.NombrePoints)
                    .ToList();

                // Grouper par formule pour les statistiques par formule
                var cumulParFormule = pointsConsommation
                    .Where(pc => pc.Commande?.FormuleJour != null)
                    .GroupBy(pc => pc.Commande.FormuleJour.NomFormule)
                    .Select(g => new
                    {
                        FormuleNom = g.Key,
                        NombreConsommations = g.Count(),
                        CoutTotal = g.Sum(pc => CalculerCoutPoint(pc)),
                        Pourcentage = Math.Round((double)g.Count() / totalPoints * 100, 1)
                    })
                    .OrderByDescending(x => x.NombreConsommations)
                    .ToList();

                // Grouper par jour pour le graphique
                var cumulParJour = pointsConsommation
                    .GroupBy(pc => pc.DateConsommation.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        NombrePoints = g.Count(),
                        CoutTotal = g.Sum(pc => CalculerCoutPoint(pc))
                    })
                    .OrderBy(x => x.Date)
                    .ToList();

                // Grouper par statut pour voir la répartition
                var cumulParStatut = pointsConsommation
                    .GroupBy(pc => pc.Commande?.StatusCommande ?? -1)
                    .Select(g => new
                    {
                        Statut = g.Key switch
                        {
                            (int)StatutCommande.Precommander => "Précommandée",
                            (int)StatutCommande.Consommee => "Consommée",
                            (int)StatutCommande.Annulee => "Annulée",
                            (int)StatutCommande.Facturee => "Facturée",
                            (int)StatutCommande.Exemptee => "Exemptée",
                            (int)StatutCommande.Indisponible => "Indisponible",
                            (int)StatutCommande.NonRecuperer => "Non Récupérée",
                            _ => "Inconnu"
                        },
                        NombrePoints = g.Count(),
                        CoutTotal = g.Sum(pc => CalculerCoutPoint(pc)),
                        Pourcentage = Math.Round((double)g.Count() / totalPoints * 100, 1)
                    })
                    .OrderByDescending(x => x.NombrePoints)
                    .ToList();

                var viewModel = new
                {
                    DateDebut = dateDebutValue,
                    DateFin = dateFinValue,
                    TotalPoints = totalPoints,
                    TotalCout = totalCout,
                    TotalUtilisateurs = totalUtilisateurs,
                    TotalRepasConsommes = totalRepasConsommes,
                    TotalRepasNonRecuperes = totalRepasNonRecuperes,
                    TotalRepasIndisponibles = totalRepasIndisponibles,
                    CumulParUtilisateur = cumulParUtilisateur,
                    CumulParFormule = cumulParFormule,
                    CumulParJour = cumulParJour,
                    CumulParStatut = cumulParStatut,
                    Periode = $"Du {dateDebutValue:dd/MM/yyyy} au {dateFinValue:dd/MM/yyyy}"
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du calcul du cumul des points de consommation");
                TempData["ErrorMessage"] = "Une erreur est survenue lors du calcul du cumul.";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        [Authorize(Roles = "Administrateur,RH")]
        public async Task<IActionResult> ExporterExcelCumul(DateTime? dateDebut, DateTime? dateFin)
        {
            try
            {
                // Définir les dates par défaut
                var today = DateTime.Today;
                var moisPrecedent = today.Month == 1 ? 12 : today.Month - 1;
                var anneePrecedente = today.Month == 1 ? today.Year - 1 : today.Year;
                var dateDebutValue = dateDebut ?? new DateTime(anneePrecedente, moisPrecedent, 17);
                var dateFinValue = dateFin ?? new DateTime(today.Year, today.Month, 16);

                // Récupérer tous les points de consommation dans la période
                var pointsConsommation = await _context.PointsConsommation
                    .Include(pc => pc.Commande)
                        .ThenInclude(c => c.FormuleJour)
                    .Where(pc => pc.DateConsommation >= dateDebutValue && 
                                pc.DateConsommation <= dateFinValue &&
                                pc.Supprimer == 0)
                    .OrderBy(pc => pc.DateConsommation)
                    .ToListAsync();

                if (!pointsConsommation.Any())
                {
                    TempData["InfoMessage"] = "Aucune donnée trouvée pour l'export.";
                    return RedirectToAction("CumulPointsConsommation", new { dateDebut, dateFin });
                }

                // Récupérer les utilisateurs avec Direction et Service
                var utilisateursIds = pointsConsommation.Select(pc => pc.UtilisateurId).Distinct().ToList();
                var utilisateurs = await _context.Utilisateurs
                    .Include(u => u.Service)
                        .ThenInclude(s => s.Direction)
                    .Where(u => utilisateursIds.Contains(u.Id))
                    .Select(u => new { 
                        u.Id, 
                        u.Nom, 
                        u.Prenoms,
                        u.Site,
                        DirectionNom = u.Service != null && u.Service.Direction != null ? u.Service.Direction.Nom : "N/A"
                    })
                    .ToListAsync();

                using var workbook = new ClosedXML.Excel.XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Cumul Consommations");

                // En-têtes
                var headers = new[] { "Utilisateur", "Direction", "Site", "Nombre de Points", "Coût Total (FCFA)", "Dernière Consommation" };
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cell(1, i + 1).Value = headers[i];
                }
                worksheet.Row(1).Style.Font.Bold = true;
                worksheet.Row(1).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightBlue;

                // Données
                var cumulParUtilisateur = pointsConsommation
                    .GroupBy(pc => pc.UtilisateurId)
                    .Select(g => new
                    {
                        UtilisateurId = g.Key,
                        Utilisateur = utilisateurs.FirstOrDefault(u => u.Id == g.Key),
                        NombrePoints = g.Count(),
                        CoutTotal = g.Sum(pc => CalculerCoutPoint(pc)),
                        DerniereConsommation = g.Max(pc => pc.DateConsommation)
                    })
                    .OrderByDescending(x => x.NombrePoints)
                    .ToList();

                int row = 2;
                foreach (var user in cumulParUtilisateur)
                {
                    var nomComplet = user.Utilisateur != null 
                        ? $"{user.Utilisateur.Nom} {user.Utilisateur.Prenoms}"
                        : $"Utilisateur {user.UtilisateurId.ToString().Substring(0, 8)}...";
                    var direction = user.Utilisateur?.DirectionNom ?? "N/A";
                    var site = user.Utilisateur?.Site?.ToString() ?? "N/A";

                    worksheet.Cell(row, 1).Value = nomComplet;
                    worksheet.Cell(row, 2).Value = direction;
                    worksheet.Cell(row, 3).Value = site;
                    worksheet.Cell(row, 4).Value = user.NombrePoints;
                    worksheet.Cell(row, 5).Value = user.CoutTotal;
                    worksheet.Cell(row, 6).Value = user.DerniereConsommation.ToString("dd/MM/yyyy HH:mm");
                    row++;
                }

                // Ajuster les colonnes
                worksheet.Columns().AdjustToContents();

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                var content = stream.ToArray();

                var fileName = $"Cumul_Consommations_{dateDebutValue:yyyyMMdd}_{dateFinValue:yyyyMMdd}.xlsx";
                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'export Excel du cumul");
                TempData["ErrorMessage"] = "Une erreur est survenue lors de l'export.";
                return RedirectToAction("CumulPointsConsommation", new { dateDebut, dateFin });
            }
        }
    }
}
