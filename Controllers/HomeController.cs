using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Obeli_K.Data;
using Obeli_K.Enums;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Obeli_K.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ObeliDbContext _db;

        public HomeController(ILogger<HomeController> logger, ObeliDbContext db)
        {
            _logger = logger;
            _db = db;
        }


        public async Task<IActionResult> Index()
        {
            // --- Helper local : calcule le lundi (début) et le dimanche (fin) de la semaine en cours ---
            (DateTime debutSemaine, DateTime finSemaine) GetCurrentWeekRange()
            {
                // On force une semaine ISO façon Europe : lundi = début
                var today = DateTime.Today;
                int diff = (7 + (int)today.DayOfWeek - (int)DayOfWeek.Monday) % 7;
                var start = today.AddDays(-diff).Date;      // Lundi
                var end = start.AddDays(6).Date;          // Dimanche
                return (start, end);
            }

            // --- Récup des rôles utilisateur ---
            string? role = User?.FindFirst(ClaimTypes.Role)?.Value;

            // --- Semaine en cours ---
            var (debutSemaine, finSemaine) = GetCurrentWeekRange();

            if (role == "Administrateur" || role == "RessourcesHumaines" || role == "Employe")
            {
                // ===== RÉCUPÉRATION DES MENUS DE LA SEMAINE COURANTE =====
                // Équivalent de ton SQL :
                //   DECLARE @StartOfWeek, @EndOfWeek; SELECT ... WHERE [Date] BETWEEN @StartOfWeek AND @EndOfWeek
                //
                // NB : si ta colonne [Date] est de type DateTime, on compare sur .Date pour ignorer l'heure.
                var MenuSemaineEnCours = await _db.FormulesJour
                    .Include(fj => fj.NomFormuleNavigation)
                    .Where(fj => fj.Supprimer == 0
                              && fj.Date.Date >= debutSemaine
                              && fj.Date.Date <= finSemaine)
                    .Select(fj => new
                    {
                        fj.IdFormule,
                        fj.TypeFormuleId,
                        TypeFormule = fj.NomFormuleNavigation!.Nom,
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

                // ===== RÉCUPÉRATION DES COMMANDES DE L'UTILISATEUR CONNECTÉ =====
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var mesCommandesSemaine = new List<object>();

                if (Guid.TryParse(userIdClaim, out Guid userId))
                {
                    var commandesSemaine = await _db.Commandes
                        .Include(c => c.FormuleJour)
                        .Include(c => c.FormuleJour!.NomFormuleNavigation)
                        .Where(c => c.UtilisateurId == userId
                                 && c.DateConsommation.HasValue
                                 && c.DateConsommation.Value.Date >= debutSemaine
                                 && c.DateConsommation.Value.Date <= finSemaine
                                 && c.Supprimer == 0
                                 && !(c.StatusCommande == (int)StatutCommande.Annulee && !c.AnnuleeParPrestataire))
                        .Select(c => new
                        {
                            c.IdCommande,
                            c.Date,
                            c.DateConsommation,
                            c.Quantite,
                            c.StatusCommande,
                            c.MotifAnnulation,
                            c.Instantanee,
                            c.AnnuleeParPrestataire,
                            FormuleJour = new
                            {
                                c.FormuleJour!.IdFormule,
                                c.FormuleJour.NomFormule,
                                c.FormuleJour.Date,
                                c.FormuleJour.Plat,
                                c.FormuleJour.PlatStandard1,
                                c.FormuleJour.PlatStandard2,
                                c.FormuleJour.Entree,
                                c.FormuleJour.Dessert,
                                c.FormuleJour.Garniture,
                                c.FormuleJour.GarnitureStandard1,
                                c.FormuleJour.GarnitureStandard2,
                                c.FormuleJour.Feculent,
                                c.FormuleJour.Legumes,
                                TypeFormule = c.FormuleJour.NomFormuleNavigation!.Nom
                            }
                        })
                        .OrderBy(c => c.DateConsommation)
                        .ThenBy(c => c.Date)
                        .ToListAsync();

                    mesCommandesSemaine = commandesSemaine.Cast<object>().ToList();
                }

                // ===== PASSAGE DES DONNÉES À LA VUE =====
                ViewBag.MenuSemaineEnCours = MenuSemaineEnCours;
                ViewBag.MesCommandesSemaine = mesCommandesSemaine;
                ViewBag.DebutSemaine = debutSemaine;
                ViewBag.FinSemaine = finSemaine;
                ViewBag.NombreCommandes = mesCommandesSemaine.Count;

            }
            else if (role == "PrestataireCantine")
            {
                // ===== RÉCUPÉRATION DES MENUS DU JOUR POUR LES PRESTATAIRES =====
                var aujourdhui = DateTime.Today;
                
                var menusDuJour = await _db.FormulesJour
                    .Include(fj => fj.NomFormuleNavigation)
                    .Where(fj => fj.Supprimer == 0 && fj.Date.Date == aujourdhui)
                    .Select(fj => new
                    {
                        fj.IdFormule,
                        fj.TypeFormuleId,
                        TypeFormule = fj.NomFormuleNavigation!.Nom,
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
                        fj.Legumes,
                        fj.Marge
                    })
                    .OrderBy(fj => fj.TypeFormule)
                    .ToListAsync();

                // ===== RÉCUPÉRATION DES COMMANDES DU JOUR (TOUS STATUTS) =====
                var commandesDuJour = await _db.Commandes
                    .Include(c => c.Utilisateur)
                    .Include(c => c.FormuleJour)
                    .Include(c => c.FormuleJour!.NomFormuleNavigation)
                    .Where(c => c.DateConsommation.HasValue 
                             && c.DateConsommation.Value.Date == aujourdhui
                             && c.Supprimer == 0)
                    .Select(c => new
                    {
                        c.IdCommande,
                        c.CodeCommande,
                        c.Date,
                        c.DateConsommation,
                        c.Quantite,
                        c.StatusCommande,
                        c.PeriodeService,
                        c.Site,
                        UtilisateurNom = c.Utilisateur != null ? $"{c.Utilisateur.Nom} {c.Utilisateur.Prenoms}" : "N/A",
                        FormuleNom = c.FormuleJour!.NomFormule,
                        TypeFormule = c.FormuleJour.NomFormuleNavigation!.Nom
                    })
                    .OrderBy(c => c.DateConsommation)
                    .ToListAsync();

                // ===== COMPTER LES COMMANDES PAR FORMULE =====
                var commandesParFormule = commandesDuJour
                    .GroupBy(c => c.FormuleNom)
                    .ToDictionary(g => g.Key, g => g.Count());


                ViewBag.MenusDuJour = menusDuJour;
                ViewBag.CommandesDuJour = commandesDuJour;
                ViewBag.CommandesParFormule = commandesParFormule;
                ViewBag.DateAujourdhui = aujourdhui;
                ViewBag.NombreCommandes = commandesDuJour.Count;
            }

            return View();
        }

    }
}
