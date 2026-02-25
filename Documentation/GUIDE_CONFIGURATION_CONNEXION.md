# 🔧 Guide de Configuration de la Connexion SQL Server

## ❌ Problème Rencontré

L'erreur `Error Number:1326 - Le nom d'utilisateur ou le mot de passe est incorrect` indique que l'authentification Windows ne fonctionne pas pour se connecter au serveur distant `10.88.179.112`.

## ✅ Solutions

### Solution 1 : Authentification SQL Server (Recommandée pour serveurs distants)

Modifiez la chaîne de connexion pour utiliser l'authentification SQL Server avec un nom d'utilisateur et un mot de passe.

#### Format de la chaîne de connexion :

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=10.88.179.112;Database=Kobeli_db;User Id=VOTRE_UTILISATEUR;Password=VOTRE_MOT_DE_PASSE;Encrypt=False;TrustServerCertificate=True;MultipleActiveResultSets=true;Connection Timeout=30;"
}
```

#### Remplacez :
- `VOTRE_UTILISATEUR` : Votre nom d'utilisateur SQL Server (ex: `sa` ou un utilisateur spécifique)
- `VOTRE_MOT_DE_PASSE` : Le mot de passe de cet utilisateur

#### Exemple :

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=10.88.179.112;Database=Kobeli_db;User Id=sa;Password=MonMotDePasse123!;Encrypt=False;TrustServerCertificate=True;MultipleActiveResultSets=true;Connection Timeout=30;"
}
```

### Solution 2 : Authentification Windows (si disponible)

Si vous devez absolument utiliser l'authentification Windows, vous devez :

1. **Activer l'authentification Windows sur le serveur SQL Server**
2. **Configurer les permissions** pour que votre compte Windows puisse se connecter
3. **Utiliser un compte de domaine** (si le serveur est sur un domaine)

#### Format de la chaîne de connexion :

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=10.88.179.112;Database=Kobeli_db;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;MultipleActiveResultSets=true;Connection Timeout=30;"
}
```

## 🔐 Configuration Sécurisée (Recommandée)

Pour plus de sécurité, utilisez des **User Secrets** ou des **Variables d'Environnement** au lieu de mettre le mot de passe en clair dans les fichiers de configuration.

### Option A : User Secrets (Développement)

1. Activez User Secrets dans votre projet :
```powershell
dotnet user-secrets init
```

2. Ajoutez la chaîne de connexion :
```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=10.88.179.112;Database=Kobeli_db;User Id=sa;Password=VOTRE_MOT_DE_PASSE;Encrypt=False;TrustServerCertificate=True;MultipleActiveResultSets=true;Connection Timeout=30;"
```

3. Supprimez la chaîne de connexion de `appsettings.json` et `appsettings.Development.json`

### Option B : Variables d'Environnement (Production)

Définissez la variable d'environnement :
```powershell
$env:ConnectionStrings__DefaultConnection = "Server=10.88.179.112;Database=Kobeli_db;User Id=sa;Password=VOTRE_MOT_DE_PASSE;Encrypt=False;TrustServerCertificate=True;MultipleActiveResultSets=true;Connection Timeout=30;"
```

## 🧪 Tester la Connexion

### Test 1 : Via PowerShell

```powershell
# Testez la connexion avec Test-NetConnection
Test-NetConnection -ComputerName 10.88.179.112 -Port 1433
```

### Test 2 : Via SSMS

1. Ouvrez SQL Server Management Studio
2. Dans "Server name", entrez : `10.88.179.112`
3. Choisissez l'authentification :
   - **SQL Server Authentication** : Entrez User Id et Password
   - **Windows Authentication** : Utilisez vos identifiants Windows
4. Cliquez sur "Connect"

### Test 3 : Via l'Application

Une fois la configuration modifiée, testez avec :
```powershell
cd "C:\Users\kerne\Music\DIKO\restau\Obeli_K"
dotnet ef database update
```

## 📋 Vérifications à Faire

### Sur le Serveur SQL Server (10.88.179.112)

1. **Vérifiez que SQL Server est en cours d'exécution**
2. **Vérifiez que le port 1433 est ouvert** (ou le port personnalisé)
3. **Vérifiez que les connexions TCP/IP sont activées**
4. **Vérifiez que SQL Server écoute sur toutes les interfaces** (0.0.0.0)
5. **Vérifiez le pare-feu** : Le port SQL Server doit être autorisé

### Vérification des Paramètres SQL Server

1. Ouvrez **SQL Server Configuration Manager**
2. **SQL Server Network Configuration** → **Protocols for [Instance]**
3. Vérifiez que **TCP/IP** est **Enabled**
4. Cliquez droit sur **TCP/IP** → **Properties** → **IP Addresses**
5. Vérifiez que **TCP Port** est défini (généralement 1433)
6. Vérifiez que **IPAll** a un port configuré

### Vérification de l'Authentification

1. Dans SSMS, connectez-vous au serveur
2. Clic droit sur le serveur → **Properties**
3. Onglet **Security**
4. Vérifiez que **SQL Server and Windows Authentication mode** est sélectionné
5. Redémarrez le service SQL Server si vous avez changé ce paramètre

## 🔒 Sécurité

### Recommandations

1. **Ne commitez JAMAIS les fichiers `appsettings.json` avec des mots de passe en clair**
2. **Utilisez des User Secrets pour le développement**
3. **Utilisez Azure Key Vault ou des variables d'environnement en production**
4. **Créez un utilisateur SQL Server dédié** (ne pas utiliser `sa`)
5. **Limitez les permissions** de cet utilisateur aux seules bases de données nécessaires

### Créer un Utilisateur SQL Server Dédié

```sql
-- Se connecter en tant qu'administrateur
USE master;
GO

-- Créer un login
CREATE LOGIN [obeli_user] WITH PASSWORD = 'MotDePasseFort123!';
GO

-- Accorder l'accès à la base de données
USE Kobeli_db;
GO

-- Créer un utilisateur pour ce login
CREATE USER [obeli_user] FOR LOGIN [obeli_user];
GO

-- Accorder les permissions nécessaires
ALTER ROLE db_owner ADD MEMBER [obeli_user];
GO
```

## 📝 Exemple de Configuration Complète

### appsettings.json (Sans mot de passe)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=10.88.179.112;Database=Kobeli_db;User Id=obeli_user;Password=;Encrypt=False;TrustServerCertificate=True;MultipleActiveResultSets=true;Connection Timeout=30;"
  }
}
```

Le mot de passe sera récupéré depuis User Secrets ou Variables d'Environnement.

## ❓ Dépannage

### Erreur : "Cannot open server"

**Solution :** Vérifiez que SQL Server est démarré et accessible

### Erreur : "Login failed for user"

**Solution :** Vérifiez le nom d'utilisateur et le mot de passe

### Erreur : "A network-related error occurred"

**Solution :** Vérifiez le pare-feu et que le port SQL Server est ouvert

### Erreur : "Timeout expired"

**Solution :** Augmentez le `Connection Timeout` dans la chaîne de connexion

---

**Date de création** : 2025-01-XX  
**Serveur** : 10.88.179.112  
**Base de données** : Kobeli_db

