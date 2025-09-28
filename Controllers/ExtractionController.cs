using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Obeli_K.Data;
using Obeli_K.Models;
using Obeli_K.Models.ViewModels;
using System.Security.Claims;

namespace Obeli_K.Controllers
{
    [Authorize(Roles = "Administrateur,RessourcesHumaines,PrestataireCantine")]
    public class ExtractionController : Controller
    {
        private readonly ObeliDbContext _context;
        private readonly ILogger<ExtractionController> _logger;

        public ExtractionController(ObeliDbContext context, ILogger<ExtractionController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Affiche le formulaire d'extraction des commandes
        /// </summary>
        [HttpGet]
        public IActionResult Index(bool? showModal, string? dateDebut, string? dateFin)
        {
            var model = new ExtractionViewModel
            {
                DateDebut = DateTime.Today.AddMonths(-1),
                DateFin = DateTime.Today
            };

            // Si on doit afficher la modal, pré-remplir les dates
            if (showModal == true && !string.IsNullOrEmpty(dateDebut) && !string.IsNullOrEmpty(dateFin))
            {
                if (DateTime.TryParse(dateDebut, out var debut) && DateTime.TryParse(dateFin, out var fin))
                {
                    model.DateDebut = debut;
                    model.DateFin = fin;
                }
                ViewBag.ShowModal = true;
            }

            return View(model);
        }

        /// <summary>
        /// Étape 1 : Affiche les menus pour définir les marges
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ExtractionViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Étape 1 : Récupérer les menus de la période pour définir les marges
                var menus = await _context.FormulesJour
                    .AsNoTracking()
                    .Where(f => f.Date.Date >= model.DateDebut.Date &&
                               f.Date.Date <= model.DateFin.Date &&
                               f.Supprimer == 0)
                    .OrderBy(f => f.Date)
                    .ToListAsync();

                if (!menus.Any())
                {
                    TempData["InfoMessage"] = "Aucun menu trouvé pour la période sélectionnée.";
                    return View(model);
                }

                var definirMargesViewModel = new DefinirMargesViewModel
                {
                    DateDebut = model.DateDebut,
                    DateFin = model.DateFin,
                    MenusAvecMarges = menus.Select(m => new MenuAvecMargeViewModel
                    {
                        IdFormule = m.IdFormule,
                        Date = m.Date,
                        NomFormule = GetNomPlatFromFormule(m), // Nom du plat
                        TypeFormule = m.NomFormule, // Type de formule (Standard 1, Standard 2, Amélioré)
                        MargeActuelle = m.Marge ?? 0,
                        NouvelleMarge = m.Marge ?? 0
                    }).ToList()
                };

                _logger.LogInformation("Marges trouvées pour {Count} menus: {Marges}", 
                    definirMargesViewModel.MenusAvecMarges.Count,
                    string.Join(", ", definirMargesViewModel.MenusAvecMarges.Select(m => $"{m.TypeFormule}: {m.MargeActuelle}%")));

                return View("DefinirMarges", definirMargesViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des menus");
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la récupération des menus.";
                return View(model);
            }
        }

        /// <summary>
        /// Étape 1 : Valide les marges et ouvre la modal d'extraction
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ValiderMarges(DefinirMargesViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("DefinirMarges", model);
            }

            try
            {
                // Mettre à jour les marges des formules
                foreach (var menu in model.MenusAvecMarges)
                {
                    var formule = await _context.FormulesJour
                        .FirstOrDefaultAsync(f => f.IdFormule == menu.IdFormule);

                    if (formule != null)
                    {
                        _logger.LogInformation("Mise à jour de la marge pour {TypeFormule} du {Date}: {AncienneMarge}% → {NouvelleMarge}%", 
                            menu.TypeFormule, menu.Date.ToString("dd/MM/yyyy"), formule.Marge ?? 0, menu.NouvelleMarge);
                        
                        formule.Marge = menu.NouvelleMarge;
                        formule.ModifiedOn = DateTime.UtcNow;
                        formule.ModifiedBy = User.Identity?.Name ?? "System";
                    }
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Marges mises à jour avec succès.";
                
                // Créer le ViewModel pour la modal d'extraction
                var modalViewModel = new ExtractionModalViewModel
                {
                    DateDebut = model.DateDebut,
                    DateFin = model.DateFin
                };

                return View("ExtractionModal", modalViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour des marges");
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la mise à jour des marges.";
                return View("DefinirMarges", model);
            }
        }

        /// <summary>
        /// Étape 2 : Extrait les commandes avec marges dans la modal
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExtraireCommandes(ExtractionModalViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("ModalExtraction", model);
            }

            try
            {
                _logger.LogInformation("Recherche des commandes pour la période {DateDebut} - {DateFin}", 
                    model.DateDebut, model.DateFin);

                // Récupérer les commandes PRÉCOMMANDÉES de la période
                var commandes = await _context.Commandes
                    .AsNoTracking()
                    .Include(c => c.FormuleJour)
                    .Where(c => c.DateConsommation.HasValue &&
                                c.DateConsommation.Value.Date >= model.DateDebut.Date &&
                                c.DateConsommation.Value.Date <= model.DateFin.Date &&
                                c.Supprimer == 0 &&
                                c.StatusCommande == (int)Enums.StatutCommande.Precommander)
                    .ToListAsync();

                _logger.LogInformation("Nombre de commandes précommandées trouvées: {Count}", commandes.Count);

                // Grouper par formule
                var commandesAvecMarges = commandes
                    .Where(c => c.FormuleJour != null)
                    .GroupBy(c => c.FormuleJour!.IdFormule)
                    .Select(g => new CommandeAvecMargeViewModel
                    {
                        IdFormule = g.First().FormuleJour!.IdFormule,
                        Date = g.First().FormuleJour!.Date,
                        TypeFormule = g.First().FormuleJour!.NomFormule, // Type de formule
                        NomFormule = GetNomPlatFromFormule(g.First().FormuleJour!), // Nom du plat
                        NombreCommandes = g.Count(),
                        Marge = g.First().FormuleJour!.Marge ?? 0
                    })
                    .OrderBy(c => c.Date)
                    .ToList();

                _logger.LogInformation("Nombre de formules avec commandes: {Count}", commandesAvecMarges.Count);

                return PartialView("ExtractionResults", commandesAvecMarges);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'extraction des commandes");
                TempData["ErrorMessage"] = "Une erreur est survenue lors de l'extraction des commandes.";
                return PartialView("ModalExtraction", model);
            }
        }

        /// <summary>
        /// Exporte les commandes en Excel
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExporterExcel(ExtractionModalViewModel model)
        {
            try
            {
                // Récupérer les commandes précommandées de la période
                var commandes = await _context.Commandes
                    .AsNoTracking()
                    .Include(c => c.Utilisateur)
                    .Include(c => c.GroupeNonCit)
                    .Include(c => c.FormuleJour)
                        .ThenInclude(f => f!.NomFormuleNavigation)
                    .Where(c => c.DateConsommation.HasValue &&
                                c.DateConsommation.Value.Date >= model.DateDebut.Date &&
                                c.DateConsommation.Value.Date <= model.DateFin.Date &&
                                c.Supprimer == 0 &&
                                c.StatusCommande == (int)Enums.StatutCommande.Precommander)
                    .OrderBy(c => c.DateConsommation)
                    .ToListAsync();

                if (!commandes.Any())
                {
                    TempData["InfoMessage"] = "Aucune commande trouvée pour l'export.";
                    return RedirectToAction("Index");
                }

                // Générer le fichier Excel
                using var workbook = new ClosedXML.Excel.XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Extraction Commandes");

                // En-têtes
                var headers = new[]
                {
                    "Date Consommation", "Code Commande", "Type Client", "Client", "Matricule/Code Groupe", "Site",
                    "Type Formule", "Nom Plat", "Quantité", "Période", "Marge (%)"
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
                    var typeFormuleNom = cmd.FormuleJour?.NomFormuleNavigation?.Nom ?? "N/A";
                    var margeFormule = cmd.FormuleJour?.Marge ?? 0;

                    // Déterminer le type de client et les informations
                    string typeClient = cmd.TypeClient.ToString();
                    string clientNom = "";
                    string matriculeCode = "";

                    switch (cmd.TypeClient)
                    {
                        case Enums.TypeClientCommande.CitUtilisateur:
                            clientNom = $"{cmd.Utilisateur?.Nom} {cmd.Utilisateur?.Prenoms}";
                            matriculeCode = cmd.Utilisateur?.UserName ?? "";
                            break;
                        case Enums.TypeClientCommande.GroupeNonCit:
                            clientNom = cmd.GroupeNonCit?.Nom ?? "N/A";
                            matriculeCode = ""; // cmd.GroupeNonCit?.CodeGroupe ?? ""; // Temporairement commenté
                            break;
                        case Enums.TypeClientCommande.Visiteur:
                            clientNom = cmd.VisiteurNom ?? "N/A";
                            matriculeCode = cmd.VisiteurTelephone ?? "";
                            break;
                    }

                    worksheet.Cell(row, 1).Value = cmd.DateConsommation?.ToString("dd/MM/yyyy HH:mm");
                    worksheet.Cell(row, 2).Value = cmd.CodeCommande;
                    worksheet.Cell(row, 3).Value = typeClient;
                    worksheet.Cell(row, 4).Value = clientNom;
                    worksheet.Cell(row, 5).Value = matriculeCode;
                    worksheet.Cell(row, 6).Value = cmd.Utilisateur?.Site.ToString() ?? "";
                    worksheet.Cell(row, 7).Value = typeFormuleNom;
                    worksheet.Cell(row, 8).Value = nomPlat;
                    worksheet.Cell(row, 9).Value = cmd.Quantite;
                    worksheet.Cell(row, 10).Value = cmd.PeriodeService.ToString();
                    worksheet.Cell(row, 11).Value = margeFormule;
                    row++;
                }

                worksheet.Columns().AdjustToContents();

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                var content = stream.ToArray();

                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                    $"Extraction_Commandes_{model.DateDebut:yyyyMMdd}_{model.DateFin:yyyyMMdd}.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'export Excel");
                TempData["ErrorMessage"] = "Une erreur est survenue lors de l'export Excel.";
                return RedirectToAction("Index");
            }
        }



        /// <summary>
        /// Obtient le nom du plat à partir de la formule
        /// </summary>
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
    }
}
