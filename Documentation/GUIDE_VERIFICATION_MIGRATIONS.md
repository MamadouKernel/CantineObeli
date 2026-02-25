# 🔍 Guide de Vérification des Migrations

## ✅ État Actuel

**Toutes les migrations sont déjà appliquées à votre base de données !**

La base de données `Kobeli_db` sur le serveur `10.88.179.112` contient :
- ✅ La table `__EFMigrationsHistory` (historique des migrations)
- ✅ Toutes les 10 migrations appliquées
- ✅ Toutes les tables créées par les migrations

---

## 📋 Comment Vérifier les Migrations

### Méthode 1 : Via Entity Framework CLI

Dans PowerShell, depuis le répertoire du projet :

```powershell
cd "C:\Users\kerne\Music\DIKO\restau\Obeli_K"
dotnet ef migrations list
```

**Résultat attendu :**
```
20250919104355_Init01
20250919121014_MakeFonctionIdNullable
20250919154137_Init001
20250919154740_FixRelationsAndMakeFonctionIdNullable
20250919155430_UpdateUserModelAndRelations
20250919163941_RemoveRequiredFromMotDePasseHash
20250921125010_AddPointsConsommation
20250921132421_UpdatePointsConsommationModel
20251012174805_AddGroupeNonCitQuotaColumns
20251222150413_update
```

### Méthode 2 : Via SQL Server Management Studio (SSMS)

1. **Connectez-vous au serveur** `10.88.179.112`
2. **Ouvrez la base de données** `Kobeli_db`
3. **Exécutez la requête suivante** :

```sql
USE Kobeli_db;
GO

-- Voir toutes les migrations appliquées
SELECT 
    [MigrationId] AS 'ID Migration',
    [ProductVersion] AS 'Version EF Core'
FROM [__EFMigrationsHistory]
ORDER BY [MigrationId];
```

### Méthode 3 : Via le Script SQL Fourni

Exécutez le script `Scripts/VerifierMigrations.sql` dans SSMS :

1. Ouvrez SSMS
2. Connectez-vous au serveur `10.88.179.112`
3. Ouvrez le fichier `Scripts/VerifierMigrations.sql`
4. Exécutez le script (F5)

Ce script affichera :
- ✅ Si la table `__EFMigrationsHistory` existe
- 📋 Liste de toutes les migrations appliquées
- 📊 Liste de toutes les tables créées
- 🔍 Vérification des tables principales

---

## 🚀 Appliquer les Migrations (si nécessaire)

Si pour une raison quelconque vous devez réappliquer les migrations :

### Option 1 : Via PowerShell

```powershell
cd "C:\Users\kerne\Music\DIKO\restau\Obeli_K"
dotnet ef database update
```

### Option 2 : Via le Script PowerShell Fourni

```powershell
.\Scripts\AppliquerMigrations.ps1
```

---

## 📊 Tables Créées par les Migrations

Les migrations ont créé les tables suivantes :

### Tables Principales
- ✅ `Utilisateurs` - Gestion des utilisateurs
- ✅ `Commandes` - Gestion des commandes
- ✅ `FormulesJour` - Gestion des menus/formules
- ✅ `PointsConsommation` - Points de consommation
- ✅ `GroupesNonCit` - Groupes non-CIT (Douaniers, etc.)

### Tables de Configuration
- ✅ `Departements` - Départements des employés
- ✅ `Fonctions` - Fonctions des employés
- ✅ `TypesFormule` - Types de formules
- ✅ `ConfigurationsCommande` - Configuration des commandes
- ✅ `QuotasJournaliers` - Quotas journaliers (historique)

### Tables Système
- ✅ `__EFMigrationsHistory` - Historique des migrations (Entity Framework)

---

## 🔍 Vérification Rapide

Pour vérifier rapidement que tout est en ordre, exécutez cette requête SQL :

```sql
USE Kobeli_db;
GO

-- Compter les migrations
SELECT COUNT(*) AS 'Nombre de migrations' FROM [__EFMigrationsHistory];

-- Compter les tables
SELECT COUNT(*) AS 'Nombre de tables' 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_NAME NOT LIKE '__%';

-- Voir les dernières migrations
SELECT TOP 5 
    [MigrationId],
    [ProductVersion]
FROM [__EFMigrationsHistory]
ORDER BY [MigrationId] DESC;
```

**Résultat attendu :**
- Nombre de migrations : **10**
- Nombre de tables : **Plus de 10 tables**
- Dernière migration : `20251222150413_update`

---

## ❓ Problèmes Courants

### Problème 1 : La table `__EFMigrationsHistory` n'apparaît pas dans SSMS

**Solution :**
- Dans SSMS, vérifiez que vous êtes dans la base de données `Kobeli_db`
- La table `__EFMigrationsHistory` commence par `__`, elle peut être masquée dans certains vues
- Utilisez la requête SQL ci-dessus pour la voir

### Problème 2 : Les migrations ne s'appliquent pas

**Vérifications :**
1. Vérifiez la chaîne de connexion dans `appsettings.json`
2. Vérifiez que SQL Server est accessible sur `10.88.179.112`
3. Vérifiez que la base de données `Kobeli_db` existe
4. Vérifiez les permissions de connexion

### Problème 3 : Erreur "Unable to create an object of type 'ObeliDbContext'"

**Solution :**
- Assurez-vous que `dotnet ef` est installé : `dotnet tool install --global dotnet-ef`
- Vérifiez que le projet compile correctement : `dotnet build`

---

## 📝 Notes Importantes

1. **Ne supprimez jamais la table `__EFMigrationsHistory`** - C'est elle qui permet à Entity Framework de savoir quelles migrations sont appliquées

2. **Les migrations sont appliquées automatiquement au démarrage** - Le code dans `Program.cs` exécute `db.Database.MigrateAsync()` au démarrage de l'application

3. **Les migrations sont irréversibles** - Une fois appliquées, elles ne peuvent pas être "annulées" facilement. Si vous devez revenir en arrière, créez une nouvelle migration.

---

## 🔗 Commandes Utiles

```powershell
# Lister les migrations
dotnet ef migrations list

# Appliquer les migrations
dotnet ef database update

# Créer une nouvelle migration
dotnet ef migrations add NomDeLaMigration

# Générer le script SQL des migrations
dotnet ef migrations script

# Voir les informations détaillées
dotnet ef database update --verbose
```

---

**Date de création** : 2025-01-XX  
**Base de données** : Kobeli_db  
**Serveur** : 10.88.179.112

