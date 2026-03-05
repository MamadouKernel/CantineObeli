using Obeli_K.Models.ViewModels;

namespace Obeli_K.Services
{
    /// <summary>
    /// Interface pour le service de notifications
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Envoie une notification de facturation automatique aux administrateurs
        /// </summary>
        Task EnvoyerNotificationFacturationAsync(FacturationResult resultat, DateTime dateFacturation);

        /// <summary>
        /// Envoie une notification d'erreur de facturation
        /// </summary>
        Task EnvoyerNotificationErreurAsync(string message, Exception exception);

        /// <summary>
        /// Envoie un rapport mensuel de facturation
        /// </summary>
        Task EnvoyerRapportMensuelAsync(DateTime mois, List<FacturationResult> resultats);
    }
}
