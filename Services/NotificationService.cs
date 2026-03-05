using Microsoft.EntityFrameworkCore;
using Obeli_K.Data;
using Obeli_K.Models.Enums;
using Obeli_K.Models.ViewModels;
using System.Net;
using System.Net.Mail;

namespace Obeli_K.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;
        private readonly IConfiguration _configuration;
        private readonly ObeliDbContext _context;

        public NotificationService(
            ILogger<NotificationService> logger,
            IConfiguration configuration,
            ObeliDbContext context)
        {
            _logger = logger;
            _configuration = configuration;
            _context = context;
        }

        public async Task EnvoyerNotificationFacturationAsync(FacturationResult resultat, DateTime dateFacturation)
        {
            try
            {
                var destinataires = await ObtenirEmailsAdministrateurs();
                
                if (!destinataires.Any())
                {
                    _logger.LogWarning("Aucun destinataire trouvé");
                    return;
                }

                var sujet = $"Facturation Automatique - {dateFacturation:dd/MM/yyyy}";
                var corps = GenererCorpsEmailFacturation(resultat, dateFacturation);

                await EnvoyerEmailAsync(destinataires, sujet, corps);
                
                _logger.LogInformation("Notification envoyée à {Count} destinataires", destinataires.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'envoi de la notification");
            }
        }

        public async Task EnvoyerNotificationErreurAsync(string message, Exception exception)
        {
            try
            {
                var destinataires = await ObtenirEmailsAdministrateurs();
                
                if (!destinataires.Any())
                {
                    _logger.LogWarning("Aucun destinataire trouvé");
                    return;
                }

                var sujet = $"Erreur Facturation Automatique - {DateTime.Today:dd/MM/yyyy}";
                var corps = GenererCorpsEmailErreur(message, exception);

                await EnvoyerEmailAsync(destinataires, sujet, corps);
                
                _logger.LogInformation("Notification d'erreur envoyée");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'envoi de la notification d'erreur");
            }
        }

        public async Task EnvoyerRapportMensuelAsync(DateTime mois, List<FacturationResult> resultats)
        {
            try
            {
                var destinataires = await ObtenirEmailsAdministrateurs();
                
                if (!destinataires.Any())
                {
                    _logger.LogWarning("Aucun destinataire trouvé");
                    return;
                }

                var sujet = $"Rapport Mensuel Facturation - {mois:MMMM yyyy}";
                var corps = GenererCorpsEmailRapportMensuel(mois, resultats);

                await EnvoyerEmailAsync(destinataires, sujet, corps);
                
                _logger.LogInformation("Rapport mensuel envoyé");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'envoi du rapport mensuel");
            }
        }

        private async Task<List<string>> ObtenirEmailsAdministrateurs()
        {
            return await _context.Utilisateurs
                .Where(u => u.Supprimer == 0 && 
                           (u.Role == RoleType.Admin || u.Role == RoleType.RH) &&
                           !string.IsNullOrEmpty(u.Email))
                .Select(u => u.Email!)
                .ToListAsync();
        }

        private async Task EnvoyerEmailAsync(List<string> destinataires, string sujet, string corps)
        {
            var smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            var smtpUser = _configuration["Email:SmtpUser"];
            var smtpPassword = _configuration["Email:SmtpPassword"];
            var expediteur = _configuration["Email:From"] ?? "noreply@obeli.com";

            if (string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPassword))
            {
                _logger.LogWarning("Configuration email manquante");
                return;
            }

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPassword),
                EnableSsl = true
            };

            var message = new MailMessage
            {
                From = new MailAddress(expediteur, "Obeli - Facturation"),
                Subject = sujet,
                Body = corps,
                IsBodyHtml = true
            };

            foreach (var destinataire in destinataires)
            {
                message.To.Add(destinataire);
            }

            await client.SendMailAsync(message);
        }

        private string GenererCorpsEmailFacturation(FacturationResult resultat, DateTime dateFacturation)
        {
            return $@"
<html>
<body>
<h2>Facturation Automatique - {dateFacturation:dd/MM/yyyy}</h2>
<p>Commandes facturées: {resultat.NombreCommandesFacturables}</p>
<p>Commandes exemptées: {resultat.NombreCommandesNonFacturables}</p>
<p>Montant total: {resultat.MontantTotalAFacturer:C}</p>
</body>
</html>";
        }

        private string GenererCorpsEmailErreur(string message, Exception exception)
        {
            return $@"
<html>
<body>
<h2>Erreur Facturation Automatique</h2>
<p>Message: {message}</p>
<p>Exception: {exception.Message}</p>
<pre>{exception.StackTrace}</pre>
</body>
</html>";
        }

        private string GenererCorpsEmailRapportMensuel(DateTime mois, List<FacturationResult> resultats)
        {
            var totalFacture = resultats.Sum(r => r.NombreCommandesFacturables);
            var totalExempte = resultats.Sum(r => r.NombreCommandesNonFacturables);
            var montantTotal = resultats.Sum(r => r.MontantTotalAFacturer);

            return $@"
<html>
<body>
<h2>Rapport Mensuel - {mois:MMMM yyyy}</h2>
<p>Total commandes facturées: {totalFacture}</p>
<p>Total commandes exemptées: {totalExempte}</p>
<p>Montant total: {montantTotal:C}</p>
<p>Nombre de facturations: {resultats.Count}</p>
</body>
</html>";
        }
    }
}
