namespace Obeli_K.Enums
{
    /// <summary>
    /// Enumération des groupes non-CIT prédéfinis avec leurs configurations par défaut
    /// </summary>
    public enum GroupeNonCitEnum
    {
        /// <summary>
        /// Groupe des agents des douanes
        /// </summary>
        Douaniers = 1,
        
        /// <summary>
        /// Groupe des forces de l'ordre
        /// </summary>
        ForcesOrdre = 2,
        
        /// <summary>
        /// Groupe des services de sécurité
        /// </summary>
        Securite = 3,
        
        /// <summary>
        /// Groupe des visiteurs officiels
        /// </summary>
        VisiteursOfficiels = 4
    }
    
    /// <summary>
    /// Configuration par défaut pour chaque groupe
    /// </summary>
    public static class GroupeNonCitConfig
    {
        public static class Douaniers
        {
            public const string Nom = "Douaniers";
            public const string Description = "Groupe des agents des douanes";
            public const string CodeGroupe = "DOU";
            public const int QuotaJournalier = 50;
            public const int QuotaNuit = 30;
            public const bool RestrictionFormuleStandard = true;
        }
        
        public static class ForcesOrdre
        {
            public const string Nom = "Forces de l'Ordre";
            public const string Description = "Groupe des forces de l'ordre";
            public const string CodeGroupe = "FDO";
            public const int QuotaJournalier = 40;
            public const int QuotaNuit = 25;
            public const bool RestrictionFormuleStandard = true;
        }
        
        public static class Securite
        {
            public const string Nom = "Sécurité";
            public const string Description = "Groupe des services de sécurité";
            public const string CodeGroupe = "SEC";
            public const int QuotaJournalier = 30;
            public const int QuotaNuit = 20;
            public const bool RestrictionFormuleStandard = true;
        }
        
        public static class VisiteursOfficiels
        {
            public const string Nom = "Visiteurs Officiels";
            public const string Description = "Groupe des visiteurs officiels";
            public const string CodeGroupe = "VOF";
            public const int QuotaJournalier = 20;
            public const int QuotaNuit = 15;
            public const bool RestrictionFormuleStandard = false;
        }
    }
}
