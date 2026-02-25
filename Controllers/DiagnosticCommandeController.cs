using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Obeli_K.Data;
using Obeli_K.Enums;
using System.Security.Claims;

namespace Obeli_K.Controllers
{
    [Authorize]
    public class DiagnosticCommandeController : Controller
    {
        private readonly ObeliDbContext _context;

        public DiagnosticCommandeController(ObeliDbContext context)
        {
            _context = context;
        }

        private Guid? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdClaim, out Guid userId))
            {
                return userId;
            }
            return null;
        }

        [HttpGet]
        public async Task<IActionResult> VerifierCommandes()
        {
            var currentUserId = GetCurrentUserId();
            
            if (!currentUserId.HasValue)
            {
                return Content("Utilisateur non connecté");
            }

            // Récupérer TOUTES les commandes de l'utilisateur connecté
            var toutesCommandes = await _context.Commandes
                .AsNoTracking()
                .Include(c => c.FormuleJour)
                .Where(c => c.UtilisateurId == currentUserId && c.Supprimer == 0)
                .OrderByDescending(c => c.DateConsommation)
                .Select(c => new
                {
                    c.CodeCommande,
                    c.DateConsommation,
                    TypeClient = c.TypeClient.ToString(),
                    TypeClientValue = (int)c.TypeClient,
                    c.VisiteurNom,
                    c.UtilisateurId,
                    FormuleNom = c.FormuleJour != null ? c.FormuleJour.NomFormule : "N/A"
                })
                .ToListAsync();

            var html = $@"
<html>
<head>
    <title>Diagnostic Commandes</title>
    <style>
        body {{ font-family: Arial, sans-serif; padding: 20px; }}
        table {{ border-collapse: collapse; width: 100%; margin-top: 20px; }}
        th, td {{ border: 1px solid #ddd; padding: 12px; text-align: left; }}
        th {{ background-color: #4CAF50; color: white; }}
        tr:nth-child(even) {{ background-color: #f2f2f2; }}
        .visiteur {{ background-color: #ffebee !important; }}
        .cit {{ background-color: #e8f5e9 !important; }}
        .info {{ background-color: #e3f2fd; padding: 10px; border-radius: 5px; margin-bottom: 20px; }}
    </style>
</head>
<body>
    <h1>Diagnostic des commandes pour l'utilisateur {currentUserId}</h1>
    <div class='info'>
        <strong>Total de commandes trouvées:</strong> {toutesCommandes.Count}<br>
        <strong>Commandes CitUtilisateur:</strong> {toutesCommandes.Count(c => c.TypeClientValue == 0)}<br>
        <strong>Commandes GroupeNonCit:</strong> {toutesCommandes.Count(c => c.TypeClientValue == 1)}<br>
        <strong>Commandes Visiteur:</strong> {toutesCommandes.Count(c => c.TypeClientValue == 2)}
    </div>
    <table>
        <thead>
            <tr>
                <th>Code Commande</th>
                <th>Date Consommation</th>
                <th>Type Client</th>
                <th>Type Client (Value)</th>
                <th>Nom Visiteur</th>
                <th>Formule</th>
            </tr>
        </thead>
        <tbody>";

            foreach (var cmd in toutesCommandes)
            {
                var rowClass = cmd.TypeClientValue == 0 ? "cit" : (cmd.TypeClientValue == 2 ? "visiteur" : "");
                html += $@"
            <tr class='{rowClass}'>
                <td>{cmd.CodeCommande ?? "N/A"}</td>
                <td>{cmd.DateConsommation?.ToString("dd/MM/yyyy HH:mm") ?? "N/A"}</td>
                <td>{cmd.TypeClient}</td>
                <td>{cmd.TypeClientValue}</td>
                <td>{cmd.VisiteurNom ?? "-"}</td>
                <td>{cmd.FormuleNom}</td>
            </tr>";
            }

            html += @"
        </tbody>
    </table>
    <div style='margin-top: 20px;'>
        <strong>Légende:</strong><br>
        <span style='background-color: #e8f5e9; padding: 5px;'>Vert = CitUtilisateur (0)</span><br>
        <span style='background-color: #ffebee; padding: 5px;'>Rouge = Visiteur (2)</span><br>
        TypeClient doit être 2 pour les visiteurs et 0 pour les utilisateurs CIT
    </div>
</body>
</html>";

            return Content(html, "text/html");
        }

        [HttpGet]
        public async Task<IActionResult> VerifierCommandesSemaine()
        {
            var currentUserId = GetCurrentUserId();
            
            if (!currentUserId.HasValue)
            {
                return Content("Utilisateur non connecté");
            }

            // Reproduire exactement la même logique que dans CommandeController.Create()
            var (debutSemaine, finSemaine) = GetSemaineSuivanteComplete();
            
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
                .Select(c => new
                {
                    c.CodeCommande,
                    c.DateConsommation,
                    TypeClient = c.TypeClient.ToString(),
                    TypeClientValue = (int)c.TypeClient,
                    c.VisiteurNom,
                    c.UtilisateurId,
                    FormuleNom = c.FormuleJour != null ? c.FormuleJour.NomFormule : "N/A",
                    PeriodeService = c.PeriodeService.ToString()
                })
                .ToListAsync();

            var html = $@"
<html>
<head>
    <title>Diagnostic Commandes Semaine</title>
    <style>
        body {{ font-family: Arial, sans-serif; padding: 20px; }}
        table {{ border-collapse: collapse; width: 100%; margin-top: 20px; }}
        th, td {{ border: 1px solid #ddd; padding: 12px; text-align: left; }}
        th {{ background-color: #4CAF50; color: white; }}
        tr:nth-child(even) {{ background-color: #f2f2f2; }}
        .cit {{ background-color: #e8f5e9 !important; }}
        .info {{ background-color: #e3f2fd; padding: 10px; border-radius: 5px; margin-bottom: 20px; }}
        .warning {{ background-color: #fff3cd; padding: 10px; border-radius: 5px; margin-bottom: 20px; }}
    </style>
</head>
<body>
    <h1>Commandes qui apparaissent dans 'Mes Commandes' (Commande/Create)</h1>
    <div class='info'>
        <strong>Utilisateur:</strong> {currentUserId}<br>
        <strong>Période:</strong> {debutSemaine:dd/MM/yyyy} à {finSemaine:dd/MM/yyyy}<br>
        <strong>Commandes trouvées par la requête Create:</strong> {commandesAvecFormules.Count}
    </div>";

            if (commandesAvecFormules.Count > 0)
            {
                html += $@"
    <div class='warning'>
        <strong>⚠️ ATTENTION:</strong> Ces commandes apparaissent dans 'Mes Commandes' sur la page Create.
        Il ne devrait y avoir qu'UNE SEULE commande par jour !
    </div>
    
    <table>
        <thead>
            <tr>
                <th>Code Commande</th>
                <th>Date Consommation</th>
                <th>Type Client</th>
                <th>Période</th>
                <th>Nom Visiteur</th>
                <th>Formule</th>
            </tr>
        </thead>
        <tbody>";

                foreach (var cmd in commandesAvecFormules)
                {
                    html += $@"
            <tr class='cit'>
                <td>{cmd.CodeCommande ?? "N/A"}</td>
                <td>{cmd.DateConsommation?.ToString("dd/MM/yyyy HH:mm") ?? "N/A"}</td>
                <td>{cmd.TypeClient} ({cmd.TypeClientValue})</td>
                <td>{cmd.PeriodeService}</td>
                <td>{cmd.VisiteurNom ?? "-"}</td>
                <td>{cmd.FormuleNom}</td>
            </tr>";
                }

                html += @"
        </tbody>
    </table>";
            }
            else
            {
                html += @"
    <div style='background-color: #d4edda; padding: 15px; border-radius: 5px;'>
        ✅ Aucune commande trouvée avec le filtre CitUtilisateur pour cette semaine.
    </div>";
            }

            html += @"
</body>
</html>";

            return Content(html, "text/html");
        }

        // Copier la méthode GetSemaineSuivanteComplete du CommandeController
        private (DateTime debut, DateTime fin) GetSemaineSuivanteComplete()
        {
            var aujourdHui = DateTime.Today;
            var jourDeLaSemaine = (int)aujourdHui.DayOfWeek; // 0=Dimanche, 1=Lundi, ..., 6=Samedi
            
            // Calculer le lundi de la semaine courante
            var lundiCetteSemaine = aujourdHui.AddDays(-jourDeLaSemaine + (jourDeLaSemaine == 0 ? -6 : 1));
            
            // La semaine N+1 commence le lundi suivant
            var lundiSemaineN1 = lundiCetteSemaine.AddDays(7);
            var dimancheSemaineN1 = lundiSemaineN1.AddDays(6);
            
            return (lundiSemaineN1, dimancheSemaineN1);
        }
    }
}