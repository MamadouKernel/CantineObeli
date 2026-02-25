namespace Obeli_K.Constants
{
    /// <summary>
    /// Constantes métier pour éviter les nombres magiques
    /// </summary>
    public static class BusinessConstants
    {
        // Facturation
        public const int DefaultFacturationPourcentage = 100;
        public const int DefaultAbsencesGratuites = 0;
        public const int DefaultDelaiAnnulationHeures = 24;
        
        // Quotas
        public const int DefaultQuotaDouaniersJournalier = 50;
        
        // Pagination
        public const int DefaultPageSize = 10;
        public const int MaxPageSize = 100;
        
        // Validation
        public const int MaxNomLength = 100;
        public const int MaxEmailLength = 256;
        public const int MaxPhoneLength = 32;
        public const int MaxCodeCommandeLength = 64;
        
        // Sécurité
        public const int MinPasswordLength = 8;
        public const int MaxPasswordLength = 100;
        public const int BcryptWorkFactor = 12;
        public const int ResetTokenExpirationHours = 24;
        
        // Soft Delete
        public const int NotDeleted = 0;
        public const int Deleted = 1;
    }
}
