# 🔧 Guide de Dépannage - Système de Quotas

## 🚨 Problèmes Courants et Solutions

### ❌ **Problème 1 : Erreurs de chargement des quotas**

**Symptômes :**
- Messages d'erreur : "Erreur lors du chargement des quotas journaliers"
- Messages d'erreur : "Erreur lors du chargement des groupes non-CIT"
- Redirections vers des pages d'erreur

**Causes possibles :**
- Tables de base de données manquantes
- Migrations non appliquées
- Base de données corrompue

**Solutions :**

#### **Solution 1 : Script d'initialisation automatique**
```powershell
# Exécuter le script d'initialisation
.\Scripts\InitializeDatabase.ps1
```

#### **Solution 2 : Réinitialisation manuelle**
```bash
# 1. Arrêter l'application
# 2. Nettoyer et reconstruire
dotnet clean
dotnet build

# 3. Supprimer et recréer la base de données
dotnet ef database drop --force
dotnet ef database update

# 4. Redémarrer l'application
dotnet run
```

#### **Solution 3 : Vérification des migrations**
```bash
# Vérifier les migrations disponibles
dotnet ef migrations list

# Appliquer les migrations manquantes
dotnet ef database update
```

### ❌ **Problème 2 : Groupe Douaniers manquant**

**Symptômes :**
- "Le groupe Douaniers n'existe pas"
- Impossible de créer des commandes pour les Douaniers

**Solution :**
Le groupe Douaniers est créé automatiquement au premier accès. Si le problème persiste :

1. **Accédez à** : Paramètres → Gérer Quotas Permanents
2. **Le système créera automatiquement** le groupe Douaniers avec :
   - Quota Jour : 50 plats
   - Quota Nuit : 30 plats
   - Restriction : Plats standard uniquement

### ❌ **Problème 3 : Redirections incorrectes**

**Symptômes :**
- Clic sur "Nouveau Quota" → Redirection vers l'index
- Liens qui ne mènent pas au bon endroit

**Solution :**
Les liens ont été mis à jour pour pointer vers le bon système :
- **"Nouveau Quota"** → **"Gérer Quotas Permanents"**
- **Redirection automatique** vers Groupes Non-CIT

### ❌ **Problème 4 : Interface confuse**

**Symptômes :**
- Terminologie incohérente
- Messages d'erreur peu clairs

**Solution :**
La terminologie a été mise à jour :
- ✅ **"Quota Permanent"** au lieu de "Quota Journalier"
- ✅ **"Groupes Non-CIT"** pour la gestion des quotas
- ✅ **"Quotas Journaliers (Historique)"** pour référence

## 🔍 **Diagnostic des Problèmes**

### **Vérification 1 : Base de données**
```sql
-- Vérifier si les tables existent
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME IN ('GroupesNonCit', 'QuotasJournaliers')
```

### **Vérification 2 : Groupe Douaniers**
```sql
-- Vérifier si le groupe Douaniers existe
SELECT * FROM GroupesNonCit WHERE Nom = 'Douaniers' AND Supprimer = 0
```

### **Vérification 3 : Logs de l'application**
- Consultez les logs de l'application pour les erreurs détaillées
- Recherchez les messages commençant par "❌" ou "⚠️"

## 🚀 **Solutions Rapides**

### **Solution Express :**
1. **Arrêtez l'application** (Ctrl+C dans le terminal)
2. **Exécutez** : `.\Scripts\InitializeDatabase.ps1`
3. **Attendez** que le script se termine
4. **Ouvrez** : https://localhost:7021
5. **Connectez-vous** avec admin/admin123
6. **Allez dans** : Paramètres → Gérer Quotas Permanents

### **Solution Manuelle :**
1. **Terminal** : `dotnet ef database drop --force`
2. **Terminal** : `dotnet ef database update`
3. **Terminal** : `dotnet run`
4. **Navigateur** : https://localhost:7021

## 📞 **Support Technique**

### **Informations à fournir :**
- Version de .NET : `dotnet --version`
- Messages d'erreur exacts
- Étapes pour reproduire le problème
- Logs de l'application

### **Fichiers de log importants :**
- Logs de l'application (console)
- Logs de base de données
- Fichiers de migration

## ✅ **Vérification du Bon Fonctionnement**

### **Test 1 : Accès aux quotas**
1. **URL** : https://localhost:7021/GroupeNonCit
2. **Résultat attendu** : Page "Gestion des Groupes Non-CIT" avec le groupe "Douaniers"

### **Test 2 : Modification des quotas**
1. **Cliquez** sur "Modifier" pour le groupe Douaniers
2. **Résultat attendu** : Formulaire avec quotas permanents (Jour/Nuit)

### **Test 3 : Création de commandes**
1. **URL** : https://localhost:7021/Commande/CreerCommandeDouaniers
2. **Résultat attendu** : Interface de création de commandes Douaniers

## 🎯 **Navigation Correcte**

### **Pour Admin/RH :**
- **Paramètres** → **Gérer Quotas Permanents** → **Modifier** groupe Douaniers
- **Paramètres** → **Quotas Journaliers (Historique)** → **Pour référence uniquement**

### **Pour PrestataireCantine :**
- **Commandes** → **Commandes Douaniers** → **Créer commandes**
- **Commandes** → **Validation Douaniers** → **Valider commandes**

---

**📝 Note :** Si les problèmes persistent après avoir suivi ce guide, contactez l'équipe technique avec les informations de diagnostic.
