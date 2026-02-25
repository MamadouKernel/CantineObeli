namespace Obeli_K.Constants
{
    /// <summary>
    /// Messages d'erreur centralisés pour une maintenance facile
    /// </summary>
    public static class ErrorMessages
    {
        // Erreurs génériques
        public const string GenericError = "Une erreur est survenue. Veuillez réessayer.";
        public const string UnauthorizedAccess = "Vous n'avez pas les droits nécessaires pour effectuer cette action.";
        public const string NotFound = "L'élément demandé n'a pas été trouvé.";
        
        // Erreurs d'authentification
        public const string InvalidCredentials = "Matricule ou mot de passe incorrect.";
        public const string AccountLocked = "Votre compte a été verrouillé. Contactez l'administrateur.";
        public const string PasswordResetRequired = "Vous devez réinitialiser votre mot de passe.";
        
        // Erreurs de validation
        public const string RequiredField = "Ce champ est obligatoire.";
        public const string InvalidEmail = "L'adresse email n'est pas valide.";
        public const string InvalidPhone = "Le numéro de téléphone n'est pas valide.";
        public const string PasswordTooShort = "Le mot de passe doit contenir au moins {0} caractères.";
        public const string PasswordMismatch = "Les mots de passe ne correspondent pas.";
        
        // Erreurs de commande
        public const string CommandeBloquee = "Les commandes sont actuellement bloquées.";
        public const string CommandeNotFound = "La commande n'a pas été trouvée.";
        public const string CommandeAlreadyExists = "Une commande existe déjà pour cette date.";
        public const string FormuleNotAvailable = "La formule sélectionnée n'est pas disponible.";
        public const string QuotaExceeded = "Le quota journalier a été dépassé.";
        
        // Erreurs d'utilisateur
        public const string UserNotFound = "L'utilisateur n'a pas été trouvé.";
        public const string UserAlreadyExists = "Un utilisateur avec ce matricule existe déjà.";
        public const string EmailAlreadyExists = "Cette adresse email est déjà utilisée.";
        
        // Erreurs d'import
        public const string ImportFileRequired = "Veuillez sélectionner un fichier.";
        public const string ImportInvalidFormat = "Le format du fichier n'est pas valide.";
        public const string ImportEmptyFile = "Le fichier est vide.";
        public const string ImportDateInvalid = "Le format de date n'est pas valide. Utilisez DD/MM/YYYY.";
    }
}
