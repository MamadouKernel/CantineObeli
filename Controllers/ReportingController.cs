using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Obeli_K.Data;
using Obeli_K.Models;
using Obeli_K.Models.Enums;
using Obeli_K.Models.ViewModels;
using System.Security.Claims;

namespace Obeli_K.Controllers
{
    [Authorize(Roles = "Admin,RH,PrestataireCantine")]
    public class ReportingController : Controller
    {
        private readonly ObeliDbContext _context;
        private readonly ILogger<ReportingController> _logger;

        public ReportingController(ObeliDbContext context, ILogger<ReportingController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Tableau de bord principal avec indicateurs avancés
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Dashboard(DateTime? dateDebut, DateTime? dateFin, 
            SiteType? site, Guid? departementId, Guid? fonctionId)
        {
            try
            {
                // Période par défaut : mois en cours
                if (!dateDebut.HasValue) dateDebut = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                if (!dateFin.HasValue) dateFin = DateTime.Today;

                var model = new ReportingDashboardViewModel
                {
                    DateDebut = dateDebut.Value,
                    DateFin = dateFin.Value,
                    Site = site,
                    DepartementId = departementId,
                    FonctionId = fonctionId
                };

                // Récupérer les données avec filtres
                var commandesQuery = _context.Commandes
                    .Include(c => c.Utilisateur)
                        .ThenInclude(u => u!.Departement)
                    .Include(c => c.Utilisateur)
                        .ThenInclude(u => u!.Fonction)
                    .Include(c => c.FormuleJour)
                        .ThenInclude(f => f!.NomFormuleNavigation)
                    .Where(c => c.DateConsommation.HasValue &&
                                c.DateConsommation.Value.Date >= dateDebut.Value.Date &&
                                c.DateConsommation.Value.Date <= dateFin.Value.Date &&
                                c.Supprimer == 0);

                // Appliquer les filtres
                if (site.HasValue)
                    commandesQuery = commandesQuery.Where(c => c.Utilisateur!.Site == site.Value);

                if (departementId.HasValue)
                    commandesQuery = commandesQuery.Where(c => c.Utilisateur!.DepartementId == departementId.Value);

                if (fonctionId.HasValue)
                    commandesQuery = commandesQuery.Where(c => c.Utilisateur!.FonctionId == fonctionId.Value);

                var commandes = await commandesQuery.ToListAsync();

                // Calculer les indicateurs
                model.Indicateurs = await CalculerIndicateurs(commandes, dateDebut.Value, dateFin.Value);

                // Récupérer les listes pour les filtres
                await PopulateFilterLists(model);

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement du tableau de bord");
                TempData["ErrorMessage"] = "Une erreur est survenue lors du chargement du tableau de bord.";
                return View(new ReportingDashboardViewModel());
            }
        }

        /// <summary>
        /// Export CSV des données du tableau de bord
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExporterCsv(ReportingDashboardViewModel model)
        {
            try
            {
                var commandesQuery = _context.Commandes
                    .Include(c => c.Utilisateur)
                        .ThenInclude(u => u!.Departement)
                    .Include(c => c.Utilisateur)
                        .ThenInclude(u => u!.Fonction)
                    .Include(c => c.FormuleJour)
                        .ThenInclude(f => f!.NomFormuleNavigation)
                    .Where(c => c.DateConsommation.HasValue &&
                                c.DateConsommation.Value.Date >= model.DateDebut.Date &&
                                c.DateConsommation.Value.Date <= model.DateFin.Date &&
                                c.Supprimer == 0);

                // Appliquer les filtres
                if (model.Site.HasValue)
                    commandesQuery = commandesQuery.Where(c => c.Utilisateur!.Site == model.Site.Value);

                if (model.DepartementId.HasValue)
                    commandesQuery = commandesQuery.Where(c => c.Utilisateur!.DepartementId == model.DepartementId.Value);

                if (model.FonctionId.HasValue)
                    commandesQuery = commandesQuery.Where(c => c.Utilisateur!.FonctionId == model.FonctionId.Value);

                var commandes = await commandesQuery.ToListAsync();

                // Générer le CSV
                var csv = GenererCsv(commandes);

                var fileName = $"Rapport_Commandes_{model.DateDebut:yyyyMMdd}_{model.DateFin:yyyyMMdd}.csv";
                return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'export CSV");
                TempData["ErrorMessage"] = "Une erreur est survenue lors de l'export CSV.";
                return RedirectToAction("Dashboard");
            }
        }

        /// <summary>
        /// Export PDF des données du tableau de bord
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExporterPdf(ReportingDashboardViewModel model)
        {
            try
            {
                var commandesQuery = _context.Commandes
                    .Include(c => c.Utilisateur)
                        .ThenInclude(u => u!.Departement)
                    .Include(c => c.Utilisateur)
                        .ThenInclude(u => u!.Fonction)
                    .Include(c => c.FormuleJour)
                        .ThenInclude(f => f!.NomFormuleNavigation)
                    .Where(c => c.DateConsommation.HasValue &&
                                c.DateConsommation.Value.Date >= model.DateDebut.Date &&
                                c.DateConsommation.Value.Date <= model.DateFin.Date &&
                                c.Supprimer == 0);

                // Appliquer les filtres
                if (model.Site.HasValue)
                    commandesQuery = commandesQuery.Where(c => c.Utilisateur!.Site == model.Site.Value);

                if (model.DepartementId.HasValue)
                    commandesQuery = commandesQuery.Where(c => c.Utilisateur!.DepartementId == model.DepartementId.Value);

                if (model.FonctionId.HasValue)
                    commandesQuery = commandesQuery.Where(c => c.Utilisateur!.FonctionId == model.FonctionId.Value);

                var commandes = await commandesQuery.ToListAsync();
                var indicateurs = await CalculerIndicateurs(commandes, model.DateDebut, model.DateFin);

                // Générer le PDF (simulation - nécessiterait une librairie PDF)
                var pdfContent = GenererPdf(commandes, indicateurs, model);

                var fileName = $"Rapport_Commandes_{model.DateDebut:yyyyMMdd}_{model.DateFin:yyyyMMdd}.pdf";
                return File(pdfContent, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'export PDF");
                TempData["ErrorMessage"] = "Une erreur est survenue lors de l'export PDF.";
                return RedirectToAction("Dashboard");
            }
        }

        /// <summary>
        /// API pour les données du graphique de participation
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetParticipationData(DateTime? dateDebut, DateTime? dateFin, 
            SiteType? site, Guid? departementId)
        {
            try
            {
                if (!dateDebut.HasValue) dateDebut = DateTime.Today.AddDays(-30);
                if (!dateFin.HasValue) dateFin = DateTime.Today;

                var commandesQuery = _context.Commandes
                    .Include(c => c.Utilisateur)
                    .Where(c => c.DateConsommation.HasValue &&
                                c.DateConsommation.Value.Date >= dateDebut.Value.Date &&
                                c.DateConsommation.Value.Date <= dateFin.Value.Date &&
                                c.Supprimer == 0);

                if (site.HasValue)
                    commandesQuery = commandesQuery.Where(c => c.Utilisateur!.Site == site.Value);

                if (departementId.HasValue)
                    commandesQuery = commandesQuery.Where(c => c.Utilisateur!.DepartementId == departementId.Value);

                var commandes = await commandesQuery.ToListAsync();

                // Calculer la participation par jour
                var participationData = commandes
                    .GroupBy(c => c.DateConsommation!.Value.Date)
                    .Select(g => new
                    {
                        Date = g.Key.ToString("yyyy-MM-dd"),
                        NombreCommandes = g.Count(),
                        UtilisateursUniques = g.Select(c => c.UtilisateurId).Distinct().Count()
                    })
                    .OrderBy(x => x.Date)
                    .ToList();

                return Json(participationData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des données de participation");
                return Json(new { error = "Erreur lors de la récupération des données" });
            }
        }

        #region Méthodes privées

        private async Task<ReportingIndicateursViewModel> CalculerIndicateurs(List<Commande> commandes, DateTime dateDebut, DateTime dateFin)
        {
            var indicateurs = new ReportingIndicateursViewModel();

            // Nombre total de commandes
            indicateurs.NombreTotalCommandes = commandes.Count;

            // Commandes par site
            indicateurs.CommandesParSite = commandes
                .Where(c => c.Utilisateur?.Site.HasValue == true)
                .GroupBy(c => c.Utilisateur!.Site!.Value)
                .ToDictionary(g => g.Key.ToString(), g => g.Count());

            // Commandes par période (jour/nuit)
            indicateurs.CommandesParPeriode = commandes
                .GroupBy(c => c.PeriodeService)
                .ToDictionary(g => g.Key.ToString(), g => g.Count());

            // Répartition par formule
            indicateurs.RepartitionParFormule = commandes
                .Where(c => c.FormuleJour?.NomFormuleNavigation != null)
                .GroupBy(c => c.FormuleJour!.NomFormuleNavigation!.Nom)
                .ToDictionary(g => g.Key, g => g.Count());

            // Taux de participation
            var totalUtilisateurs = await _context.Utilisateurs
                .Where(u => u.Supprimer == 0)
                .CountAsync();

            var utilisateursAvecCommandes = commandes
                .Select(c => c.UtilisateurId)
                .Distinct()
                .Count();

            indicateurs.TauxParticipation = totalUtilisateurs > 0 
                ? Math.Round((double)utilisateursAvecCommandes / totalUtilisateurs * 100, 2)
                : 0;

            // Volume de surplus (commandes annulées)
            indicateurs.VolumeSurplus = commandes
                .Where(c => c.StatusCommande == (int)Enums.StatutCommande.Annulee)
                .Count();

            // Commandes par département
            indicateurs.CommandesParDepartement = commandes
                .Where(c => c.Utilisateur?.Departement != null)
                .GroupBy(c => c.Utilisateur!.Departement!.Nom)
                .ToDictionary(g => g.Key, g => g.Count());

            // Commandes par fonction
            indicateurs.CommandesParFonction = commandes
                .Where(c => c.Utilisateur?.Fonction != null)
                .GroupBy(c => c.Utilisateur!.Fonction!.Nom)
                .ToDictionary(g => g.Key, g => g.Count());

            return indicateurs;
        }

        private async Task PopulateFilterLists(ReportingDashboardViewModel model)
        {
            model.Sites = Enum.GetValues<SiteType>().ToList();
            model.Departements = await _context.Departements
                .Where(d => d.Supprimer == 0)
                .OrderBy(d => d.Nom)
                .ToListAsync();
            model.Fonctions = await _context.Fonctions
                .Where(f => f.Supprimer == 0)
                .OrderBy(f => f.Nom)
                .ToListAsync();
        }

        private string GenererCsv(List<Commande> commandes)
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

        private byte[] GenererPdf(List<Commande> commandes, ReportingIndicateursViewModel indicateurs, ReportingDashboardViewModel model)
        {
            // Simulation de génération PDF - nécessiterait une librairie comme iTextSharp
            var html = $@"
                <html>
                <head><title>Rapport Commandes</title></head>
                <body>
                    <h1>Rapport des Commandes</h1>
                    <p>Période: {model.DateDebut:dd/MM/yyyy} - {model.DateFin:dd/MM/yyyy}</p>
                    <h2>Indicateurs</h2>
                    <p>Total commandes: {indicateurs.NombreTotalCommandes}</p>
                    <p>Taux de participation: {indicateurs.TauxParticipation}%</p>
                    <p>Volume surplus: {indicateurs.VolumeSurplus}</p>
                </body>
                </html>";

            // Pour l'instant, retourner du HTML en tant que PDF
            return System.Text.Encoding.UTF8.GetBytes(html);
        }

        private string GetNomPlatFromFormule(FormuleJour? formule)
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

        #endregion
    }
}
