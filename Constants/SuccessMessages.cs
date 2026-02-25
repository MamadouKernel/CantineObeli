namespace Obeli_K.Constants
{
    /// <summary>
    /// Messages de succès centralisés
    /// </summary>
    public static class SuccessMessages
    {
        // Opérations génériques
        public const string CreateSuccess = "{0} créé(e) avec succès.";
        public const string UpdateSuccess = "{0} mis(e) à jour avec succès.";
        public const string DeleteSuccess = "{0} supprimé(e) avec succès.";
        
        // Commandes
        public const string CommandeCreated = "Commande créée avec succès.";
        public const string CommandeUpdated = "Commande mise à jour avec succès.";
        public const string CommandeCancelled = "Commande annulée avec succès.";
        public const string CommandeConfirmed = "Commande confirmée avec succès.";
        
        // Utilisateurs
        public const string UserCreated = "Utilisateur créé avec succès.";
        public const string UserUpdated = "Utilisateur mis à jour avec succès.";
        public const string PasswordChanged = "Mot de passe modifié avec succès.";
        public const string PasswordReset = "Mot de passe réinitialisé avec succès.";
        
        // Import
        public const string ImportSuccess = "{0} lignes importées avec succès.";
        public const string ImportPartialSuccess = "{0} lignes importées, {1} erreurs.";
        
        // Facturation
        public const string FacturationApplied = "Facturation appliquée avec succès.";
        public const string FacturationCalculated = "Facturation calculée : {0} commandes à facturer.";
    }
}
