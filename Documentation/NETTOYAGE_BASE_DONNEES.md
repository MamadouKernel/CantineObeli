# 🗑️ Nettoyage de la Base de Données Obeli_K

Ce guide explique comment nettoyer complètement la base de données pour repartir sur une base propre.

## 🎯 Objectif

Vider la base de données en gardant **uniquement** :
- ✅ Les comptes administrateurs
- ✅ Les données de référence (Directions, Départements, Fonctions, Types de formules)

## 🚀 Méthodes de Nettoyage

### 1. Via l'Interface Web (Recommandé)

1. **Connectez-vous** avec un compte administrateur
2. **Allez dans** : `Paramètres` → `Administration DB`
3. **Cliquez sur** : `Nettoyer Base de Données`
4. **Confirmez** l'action
5. **Attendez** la confirmation de succès

### 2. Via Script PowerShell

```powershell
# Exécuter le script de nettoyage
.\Scripts\NettoyerBaseDonnees.ps1

# Ou avec confirmation forcée
.\Scripts\NettoyerBaseDonnees.ps1 -Force
```

### 3. Via SQL Server Management Studio

Exécutez le script SQL suivant :

```sql
-- Nettoyage complet de la base de données
DELETE FROM PointsConsommation
DELETE FROM ExportCommandesPrestataire  
DELETE FROM Commandes
DELETE FROM QuotasJournaliers
DELETE FROM ConfigurationsCommande
DELETE FROM FormulesJour
DELETE FROM PrestataireCantines
DELETE FROM GroupesNonCit

-- Supprimer les utilisateurs non-admin (soft delete)
UPDATE Utilisateurs 
SET Supprimer = 1, ModifiedAt = GETUTCDATE(), ModifiedBy = 'Nettoyage manuel'
WHERE Role != 0 AND Supprimer = 0
```

## 📊 Données Conservées

| Type | Description | Raison |
|------|-------------|---------|
| **Comptes Admin** | Utilisateurs avec rôle Admin | Accès système |
| **Directions** | Direction Général, etc. | Référentiel obligatoire |
| **Départements** | Direction Général | Référentiel obligatoire |
| **Fonctions** | Fonction Général | Référentiel obligatoire |
| **Types Formule** | Standard, Amélioré, etc. | Référentiel obligatoire |

## ❌ Données Supprimées

| Type | Description |
|------|-------------|
| **Commandes** | Toutes les commandes passées |
| **Points Consommation** | Tous les points de consommation |
| **Formules Jour** | Tous les menus créés |
| **Utilisateurs** | Tous sauf les admins |
| **Prestataires** | Tous les prestataires cantine |
| **Groupes Non-CIT** | Douaniers, Visiteurs, etc. |
| **Configurations** | Toutes les configurations |
| **Quotas** | Tous les quotas journaliers |

## 🔄 Après Nettoyage

1. **Redémarrez** l'application
2. **Connectez-vous** avec `admin` / `admin123`
3. **Changez** le mot de passe admin
4. **Créez** les utilisateurs nécessaires (RH, Prestataire, etc.)
5. **Configurez** les paramètres de l'application
6. **Créez** les directions, départements, fonctions
7. **Créez** les groupes non-CIT si nécessaire
8. **Créez** les formules du jour

## ⚠️ Précautions

- **Sauvegardez** la base de données avant le nettoyage
- **Testez** d'abord sur un environnement de développement
- **Vérifiez** que vous avez bien accès au compte admin
- **Documentez** les configurations importantes

## 🆘 En Cas de Problème

Si vous perdez l'accès au système :

1. **Vérifiez** que le compte admin existe :
   ```sql
   SELECT * FROM Utilisateurs WHERE UserName = 'admin' AND Supprimer = 0
   ```

2. **Réinitialisez** le mot de passe admin :
   ```sql
   UPDATE Utilisateurs 
   SET MotDePasseHash = '$2a$12$...' -- Hash de 'admin123'
   WHERE UserName = 'admin'
   ```

3. **Contactez** l'administrateur système

## 📝 Notes Techniques

- Le nettoyage utilise le **soft delete** pour les utilisateurs (champ `Supprimer = 1`)
- Les **contraintes FK** sont temporairement désactivées pendant le nettoyage
- Le script est **transactionnel** (tout ou rien)
- Les **logs** détaillés sont disponibles dans la console

---

**🎯 Objectif atteint : Base de données propre avec seulement les données essentielles !**
