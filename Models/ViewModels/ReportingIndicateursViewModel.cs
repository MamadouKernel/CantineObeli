namespace Obeli_K.Models.ViewModels
{
    public class ReportingIndicateursViewModel
    {
        /// <summary>
        /// Nombre total de commandes sur la période
        /// </summary>
        public int NombreTotalCommandes { get; set; }

        /// <summary>
        /// Commandes par site (Billings, Terminal)
        /// </summary>
        public Dictionary<string, int> CommandesParSite { get; set; } = new();

        /// <summary>
        /// Commandes par période (Jour, Nuit)
        /// </summary>
        public Dictionary<string, int> CommandesParPeriode { get; set; } = new();

        /// <summary>
        /// Répartition des commandes entre Formule Standard et Formule Améliorée
        /// </summary>
        public Dictionary<string, int> RepartitionParFormule { get; set; } = new();

        /// <summary>
        /// Taux de participation des utilisateurs (%)
        /// </summary>
        public double TauxParticipation { get; set; }

        /// <summary>
        /// Volume de surplus consommé (commandes annulées)
        /// </summary>
        public int VolumeSurplus { get; set; }

        /// <summary>
        /// Commandes par département
        /// </summary>
        public Dictionary<string, int> CommandesParDepartement { get; set; } = new();

        /// <summary>
        /// Commandes par fonction
        /// </summary>
        public Dictionary<string, int> CommandesParFonction { get; set; } = new();

        /// <summary>
        /// Commandes par jour de la semaine
        /// </summary>
        public Dictionary<string, int> CommandesParJour { get; set; } = new();

        /// <summary>
        /// Évolution des commandes par semaine
        /// </summary>
        public Dictionary<string, int> EvolutionHebdomadaire { get; set; } = new();
    }
}
