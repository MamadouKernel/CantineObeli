namespace Obeli_K.Constants
{
    /// <summary>
    /// Clés de configuration centralisées pour éviter les strings magiques
    /// </summary>
    public static class ConfigurationKeys
    {
        // Configuration des commandes
        public const string CommandeJourCloture = "COMMANDE_JOUR_CLOTURE";
        public const string CommandeHeureCloture = "COMMANDE_HEURE_CLOTURE";
        public const string CommandeDelaiCloture = "COMMANDE_DELAI_CLOTURE_HEURES";
        public const string CommandeBloquee = "COMMANDE_BLOQUEE";
        
        // Configuration de la facturation
        public const string FacturationActive = "FACTURATION_NON_CONSOMMEES_ACTIVE";
        public const string FacturationPourcentage = "FACTURATION_POURCENTAGE";
        public const string FacturationAbsencesGratuites = "FACTURATION_ABSENCES_GRATUITES_MOIS";
        public const string FacturationDelaiAnnulation = "FACTURATION_DELAI_ANNULATION_GRATUITE_HEURES";
        
        // Configuration des quotas
        public const string QuotaDouaniersActif = "QUOTA_DOUANIERS_ACTIF";
        public const string QuotaDouaniersJournalier = "QUOTA_DOUANIERS_JOURNALIER";
        
        // Configuration des notifications
        public const string NotificationEmailActive = "NOTIFICATION_EMAIL_ACTIVE";
        public const string NotificationSmsActive = "NOTIFICATION_SMS_ACTIVE";
    }
}
