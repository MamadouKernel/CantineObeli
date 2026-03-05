# 📋 Guide d'Utilisation: Quotas Permanents Groupes Non-CIT

## ✅ Statut: IMPLÉMENTÉ ET FONCTIONNEL

---

## 🎯 Objectif

Cette fonctionnalité permet de gérer les quotas permanents pour les groupes spéciaux non-CIT (comme les Douaniers, Forces de l'Ordre, Sécurité, etc.) qui ont des besoins alimentaires spécifiques et des quotas fixes.

---

## 🔐 Accès

**Rôles autorisés:**
- ✅ Administrateur
- ✅ RH (Ressources Humaines)

**URL d'accès:** `/GroupeNonCit/Index`

**Menu:** Paramètres → Groupes Non-CIT (ou navigation directe)

---

## 📊 Qu'est-ce qu'un Groupe Non-CIT ?

Un **Groupe Non-CIT** est un groupe d'utilisateurs qui:
- Ne sont pas des employés CIT réguliers
- Ont des besoins alimentaires spécifiques
- Bénéficient de quotas fixes de repas
- Peuvent avoir des restrictions sur les types de formules

**Exemples de groupes:**
- 🚔 Douaniers
- 👮 Forces de l'Ordre
- 🛡️ Sécurité
- 👔 Visiteurs Officiels

---

## 🎛️ Fonctionnalités Disponibles

### 1. 📋 Liste des Groupes

**Action:** Voir tous les groupes non-CIT  
**URL:** `/GroupeNonCit/Index`

**Informations affichées:**
- Nom du groupe
- Description
- Quota journalier (nombre de repas/jour)
- Quota nuit (nombre de repas/nuit)
- Code groupe
- Restriction formule standard (Oui/Non)

**Actions disponibles:**
- ✏️ Modifier
- 🗑️ Supprimer
- 👁️ Détails
- ➕ Créer nouveau groupe

---

### 2. ➕ Créer un Nouveau Groupe

**Action:** Créer un groupe non-CIT  
**URL:** `/GroupeNonCit/Create`

**Étapes:**

1. Cliquez sur **"Créer un nouveau groupe"**

2. Remplissez le formulaire:

   **Champs obligatoires:**
   - **Nom** (max 100 caractères)
     - Exemple: "Douaniers", "Forces de l'Ordre"
   
   **Champs optionnels:**
   - **Description** (max 500 caractères)
     - Exemple: "Personnel douanier en service"
   
   - **Code Groupe** (max 10 caractères)
     - Code unique pour identifier le groupe
     - Exemple: "DOU", "FO", "SEC"
   
   - **Quota Journalier** (nombre entier ≥ 0)
     - Nombre de repas autorisés par jour
     - Exemple: 50 (50 repas/jour)
   
   - **Quota Nuit** (nombre entier ≥ 0)
     - Nombre de repas autorisés par nuit
     - Exemple: 20 (20 repas/nuit)
   
   - **Restriction Formule Standard** (case à cocher)
     - ☑️ Coché = Limité aux formules standard uniquement
     - ☐ Décoché = Accès à toutes les formules

3. Cliquez sur **"Créer"**

**Validation:**
- ✅ Le nom doit être unique
- ✅ Les quotas doivent être positifs
- ✅ Le code groupe doit être unique (si spécifié)

---

### 3. ✏️ Modifier un Groupe

**Action:** Modifier les quotas et paramètres d'un groupe  
**URL:** `/GroupeNonCit/Edit/{id}`

**Étapes:**

1. Dans la liste, cliquez sur **"Modifier"** pour le groupe souhaité

2. Modifiez les champs nécessaires:
   - Nom
   - Description
   - Quota journalier
   - Quota nuit
   - Code groupe
   - Restriction formule standard

3. Cliquez sur **"Enregistrer"**

**Cas d'usage:**
- Augmenter/diminuer les quotas selon les besoins
- Ajouter/retirer la restriction formule standard
- Mettre à jour la description

---

### 4. 👁️ Voir les Détails

**Action:** Consulter les détails et statistiques d'un groupe  
**URL:** `/GroupeNonCit/Details/{id}`

**Informations affichées:**

**Informations générales:**
- Nom du groupe
- Description
- Code groupe
- Date de création
- Créé par
- Date de modification
- Modifié par

**Quotas configurés:**
- Quota journalier
- Quota nuit
- Restriction formule standard

**Statistiques du jour:**
- 📊 Plats consommés aujourd'hui (jour)
- 📊 Plats consommés aujourd'hui (nuit)
- 📊 Quota restant (jour)
- 📊 Quota restant (nuit)

---

### 5. 🗑️ Supprimer un Groupe

**Action:** Supprimer un groupe non-CIT  
**URL:** `/GroupeNonCit/Delete/{id}` (POST)

**Étapes:**

1. Dans la liste, cliquez sur **"Supprimer"** pour le groupe souhaité

2. Confirmez la suppression

**Règles de suppression:**
- ✅ Possible si le groupe n'a **aucune commande** associée
- ❌ Impossible si le groupe a des commandes (soft delete)
- ⚠️ Message d'erreur si des commandes existent

**Note:** La suppression est un "soft delete" (Supprimer = 1), les données restent en base.

---

## 💡 Exemples d'Utilisation

### Exemple 1: Créer un Groupe "Douaniers"

```
Nom: Douaniers
Description: Personnel douanier en service 24/7
Code Groupe: DOU
Quota Journalier: 50
Quota Nuit: 20
Restriction Formule Standard: ☑️ Oui
```

**Résultat:**
- Les douaniers peuvent commander jusqu'à 50 repas/jour
- Jusqu'à 20 repas/nuit
- Limités aux formules standard uniquement

---

### Exemple 2: Créer un Groupe "Visiteurs VIP"

```
Nom: Visiteurs VIP
Description: Visiteurs officiels et délégations
Code Groupe: VIP
Quota Journalier: 30
Quota Nuit: 10
Restriction Formule Standard: ☐ Non
```

**Résultat:**
- Les visiteurs VIP peuvent commander jusqu'à 30 repas/jour
- Jusqu'à 10 repas/nuit
- Accès à toutes les formules (standard et premium)

---

### Exemple 3: Modifier les Quotas

**Situation:** Les douaniers ont besoin de plus de repas

**Action:**
1. Aller dans `/GroupeNonCit/Index`
2. Cliquer sur "Modifier" pour "Douaniers"
3. Changer "Quota Journalier" de 50 à 75
4. Changer "Quota Nuit" de 20 à 30
5. Cliquer sur "Enregistrer"

**Résultat:** Les nouveaux quotas sont appliqués immédiatement

---

## 🔄 Workflow Complet

### Création d'un Nouveau Groupe

```
1. Administrateur/RH se connecte
   ↓
2. Accède à /GroupeNonCit/Index
   ↓
3. Clique sur "Créer un nouveau groupe"
   ↓
4. Remplit le formulaire
   ↓
5. Clique sur "Créer"
   ↓
6. Le groupe est créé et visible dans la liste
   ↓
7. Les utilisateurs peuvent maintenant être assignés à ce groupe
```

### Utilisation des Quotas

```
1. Un utilisateur du groupe "Douaniers" commande un repas
   ↓
2. Le système vérifie le quota journalier (ex: 50)
   ↓
3. Le système compte les commandes déjà passées aujourd'hui
   ↓
4. Si quota non atteint → Commande autorisée
   ↓
5. Si quota atteint → Commande refusée avec message
```

---

## 📊 Statistiques et Suivi

### Consulter la Consommation

1. Accédez à `/GroupeNonCit/Details/{id}`
2. Consultez les statistiques du jour:
   - Plats consommés (jour)
   - Plats consommés (nuit)
   - Quota restant

### Exemple de Statistiques

```
Groupe: Douaniers
Date: 05/03/2026

Quota Journalier: 50
Plats consommés (jour): 35
Quota restant (jour): 15

Quota Nuit: 20
Plats consommés (nuit): 12
Quota restant (nuit): 8
```

---

## ⚙️ Configuration Technique

### Structure de la Base de Données

**Table:** `GroupesNonCit`

**Colonnes principales:**
- `Id` (Guid) - Identifiant unique
- `Nom` (string) - Nom du groupe
- `Description` (string) - Description
- `QuotaJournalier` (int?) - Quota jour
- `QuotaNuit` (int?) - Quota nuit
- `RestrictionFormuleStandard` (bool) - Restriction
- `CodeGroupe` (string?) - Code unique
- `Supprimer` (int) - Soft delete (0=actif, 1=supprimé)

### Relations

- **GroupeNonCit** → **Commandes** (1-N)
  - Un groupe peut avoir plusieurs commandes
  - Une commande appartient à un groupe (optionnel)

---

## 🚨 Règles de Gestion

### Quotas

1. **Quota Journalier:**
   - S'applique aux commandes avec `PeriodeService = Jour`
   - Réinitialisé chaque jour à minuit
   - Valeur nullable (null = pas de limite)

2. **Quota Nuit:**
   - S'applique aux commandes avec `PeriodeService = Nuit`
   - Réinitialisé chaque jour à minuit
   - Valeur nullable (null = pas de limite)

3. **Restriction Formule Standard:**
   - Si `true`: Seules les formules standard sont autorisées
   - Si `false`: Toutes les formules sont autorisées

### Suppression

1. **Soft Delete:**
   - Les groupes ne sont jamais supprimés physiquement
   - `Supprimer = 1` pour marquer comme supprimé
   - Les données restent en base pour l'historique

2. **Protection:**
   - Impossible de supprimer un groupe avec des commandes
   - Message d'erreur explicite
   - Suggestion de désactivation plutôt que suppression

---

## 🔍 Cas d'Usage Avancés

### Cas 1: Groupe avec Quota Illimité

```
Nom: Visiteurs Occasionnels
Quota Journalier: null (ou laisser vide)
Quota Nuit: null (ou laisser vide)
```

**Résultat:** Aucune limite de commandes

---

### Cas 2: Groupe Jour Uniquement

```
Nom: Personnel Administratif
Quota Journalier: 100
Quota Nuit: 0
```

**Résultat:** Commandes autorisées le jour uniquement

---

### Cas 3: Groupe Nuit Uniquement

```
Nom: Équipe de Nuit
Quota Journalier: 0
Quota Nuit: 50
```

**Résultat:** Commandes autorisées la nuit uniquement

---

## 🛠️ Dépannage

### Problème: "Un groupe avec ce nom existe déjà"

**Cause:** Un groupe avec le même nom existe déjà  
**Solution:** Choisissez un nom différent ou modifiez le groupe existant

---

### Problème: "Impossible de supprimer le groupe"

**Cause:** Le groupe a des commandes associées  
**Solution:** 
1. Supprimez d'abord les commandes
2. Ou modifiez le groupe pour le désactiver

---

### Problème: Les quotas ne s'appliquent pas

**Cause:** Les quotas sont null (pas de limite)  
**Solution:** Définissez des valeurs numériques pour les quotas

---

## 📝 Bonnes Pratiques

### Nommage

✅ **Bon:**
- "Douaniers"
- "Forces de l'Ordre"
- "Sécurité CIT"

❌ **Mauvais:**
- "Groupe 1"
- "Test"
- "Autres"

### Quotas

✅ **Bon:**
- Définir des quotas réalistes basés sur l'historique
- Prévoir une marge de sécurité (+10-20%)
- Réviser les quotas mensuellement

❌ **Mauvais:**
- Quotas trop bas (frustration)
- Quotas trop élevés (gaspillage)
- Ne jamais réviser les quotas

### Codes Groupes

✅ **Bon:**
- "DOU" pour Douaniers
- "FO" pour Forces de l'Ordre
- "SEC" pour Sécurité

❌ **Mauvais:**
- "123"
- "GRP"
- Codes trop longs

---

## 📚 Références

### Fichiers Concernés

**Contrôleur:**
- `Controllers/GroupeNonCitController.cs`

**Modèle:**
- `Models/GroupeNonCit.cs`

**Vues:**
- `Views/GroupeNonCit/Index.cshtml`
- `Views/GroupeNonCit/Create.cshtml`
- `Views/GroupeNonCit/Edit.cshtml`
- `Views/GroupeNonCit/Details.cshtml`

**Services:**
- `Services/GroupeNonCitInitializationService.cs`

**Enums:**
- `Enums/GroupeNonCitEnum.cs`

---

## 🎓 Formation

### Pour les Administrateurs

1. Comprendre les besoins des groupes non-CIT
2. Définir les quotas appropriés
3. Créer et configurer les groupes
4. Surveiller la consommation
5. Ajuster les quotas selon les besoins

### Pour les RH

1. Identifier les groupes nécessaires
2. Collecter les besoins en quotas
3. Créer les groupes dans le système
4. Assigner les utilisateurs aux groupes
5. Suivre la consommation mensuelle

---

## ✅ Checklist de Configuration

- [ ] Se connecter avec un compte Administrateur ou RH
- [ ] Accéder à `/GroupeNonCit/Index`
- [ ] Créer les groupes nécessaires
- [ ] Définir les quotas pour chaque groupe
- [ ] Configurer les restrictions si nécessaire
- [ ] Tester la création de commandes
- [ ] Vérifier que les quotas s'appliquent
- [ ] Consulter les statistiques
- [ ] Former les utilisateurs

---

## 🎉 Résumé

La fonctionnalité **"Quotas Permanents Groupes Non-CIT"** est:

✅ **Implémentée** - Toutes les fonctionnalités sont disponibles  
✅ **Fonctionnelle** - Testée et opérationnelle  
✅ **Accessible** - Pour Administrateurs et RH  
✅ **Complète** - CRUD complet + statistiques  
✅ **Sécurisée** - Autorisations et validations  
✅ **Documentée** - Guide complet disponible

**Prête à l'emploi !** 🚀

---

**Date:** 05/03/2026  
**Version:** 1.0.0  
**Auteur:** Kiro AI Assistant  
**Statut:** ✅ PRODUCTION READY

