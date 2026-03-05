# 🎯 Plan d'Action pour Atteindre 100% des NOK

## Date: 05/03/2026

---

## 📊 Statut Actuel

- ✅ **13 NOK résolus** (37%)
- ⚠️ **14 NOK à vérifier/corriger** (40%)
- ❌ **8 NOK à développer** (23%)

**Objectif:** Atteindre 100% de résolution

---

## 🚀 Phase 1: Vérifications Immédiates (DÉJÀ FAIT)

### ✅ Vérifications Effectuées

1. ✅ **Quotas permanents groupes** → CONFIRMÉ: Déjà dans menu Paramètres (ligne 727)
2. ✅ **Configuration commandes** → CONFIRMÉ: Déjà dans menu Paramètres (ligne 714)
3. ✅ **Vérifier commande** → CONFIRMÉ: Déjà dans menu Commandes (lignes 397 et 501)
4. ✅ **Liste des commandes (Utilisateur)** → CONFIRMÉ: Accessible à tous (pas d'[Authorize])

### Résultat Phase 1
**+4 NOK résolus** → Total: 17/35 (49%)

---

## 🔧 Phase 2: Corrections d'Autorisations (À FAIRE)

### 2.1 Vérifier Autorisations CommandeController

**Actions à vérifier:**

1. ⚠️ **CommandeInstantanee** (GET/POST)
   - Vérifier: Admin, RH, Prestataire
   - Fichier: `Controllers/CommandeController.cs`

2. ⚠️ **Edit** (GET/POST) - Modifier commande
   - Vérifier règles métier (délai 24h)
   - Vérifier autorisations
   - Fichier: `Controllers/CommandeController.cs`

3. ⚠️ **Delete** (POST) - Supprimer commande
   - Vérifier: Admin, RH uniquement
   - Fichier: `Controllers/CommandeController.cs`

### 2.2 Vérifier Autorisations FormuleJourController

4. ⚠️ **Edit** (GET/POST) - Modifier menu
   - Vérifier: Admin, RH, Prestataire
   - Fichier: `Controllers/FormuleJourController.cs`

### 2.3 Vérifier Autorisations VisiteurController

5. ⚠️ **CreateCommand** (GET/POST) - Créer commande visiteur
   - Vérifier: Admin, RH
   - Fichier: `Controllers/VisiteurController.cs`

### 2.4 Vérifier Autorisations PointsConsommationController

6. ⚠️ **Create** (GET/POST) - Créer point consommation
   - Vérifier: Admin, RH
   - Fichier: `Controllers/PointsConsommationController.cs`

---

## 📝 Phase 3: Documentation Utilisateur (À FAIRE)

### 3.1 Créer Guides Utilisateur

7. ⚠️ **Guide: Différence Annuler vs Supprimer**
   - Fichier à créer: `Documentation/GUIDE_ANNULER_VS_SUPPRIMER.md`
   - Contenu:
     * Annuler: Soft delete, garde historique, motif requis
     * Supprimer: Hard delete, Admin/RH uniquement

8. ⚠️ **Guide: Modal Extraction**
   - Fichier à créer: `Documentation/GUIDE_MODAL_EXTRACTION.md`
   - Contenu: Comment accéder et utiliser la modal

9. ⚠️ **Guide: Créer Point Consommation**
   - Fichier à créer: `Documentation/GUIDE_POINT_CONSOMMATION.md`
   - Contenu: Étapes pour créer un point de consommation

10. ⚠️ **Guide: Workflow Commandes Douaniers**
    - Fichier à créer: `Documentation/GUIDE_COMMANDES_DOUANIERS.md`
    - Contenu: Processus complet de validation

---

## 🎨 Phase 4: Améliorations UX (À FAIRE)

### 4.1 Ajouter Boutons Manquants

11. ⚠️ **Bouton "Réinitialiser mot de passe"**
    - Fichier: `Views/Utilisateur/List.cshtml`
    - Action: Ajouter bouton dans la liste des utilisateurs
    - Lien: `/Utilisateur/ResetPassword/{id}`

12. ⚠️ **Améliorer visibilité "Liste formules visiteurs"**
    - Fichier: `Views/Visiteur/CreateCommand.cshtml`
    - Action: Rendre plus visible la sélection des formules

### 4.2 Clarifier Workflows

13. ⚠️ **Clarifier workflow "Valider commande douaniers"**
    - Fichier: `Views/Commande/ValiderCommandeDouaniers.cshtml`
    - Action: Ajouter instructions claires

---

## 💻 Phase 5: Développements API (À FAIRE - Si Nécessaire)

### 5.1 APIs Optionnelles (À discuter avec l'équipe)

14. ❌ **API Liste directions**
    - Fichier: `Controllers/DirectionController.cs`
    - Action: Ajouter endpoint `[HttpGet] GetDirections()`
    - Format: JSON

15. ❌ **API Formules visiteurs**
    - Fichier: `Controllers/VisiteurController.cs`
    - Action: Ajouter endpoint `[HttpGet] GetFormulesVisiteurs()`
    - Format: JSON

16. ❌ **API Créer commande visiteur**
    - Fichier: `Controllers/VisiteurController.cs`
    - Action: Ajouter endpoint `[HttpPost] CreateCommandApi()`
    - Format: JSON

17. ⚠️ **Interface pour API Formules disponibles**
    - Fichier: `Views/Commande/FormulesDisponibles.cshtml`
    - Action: Créer vue dédiée pour afficher les formules

### 5.2 Statistiques Système (À discuter)

18. ❌ **Page Statistiques système**
    - Fichier: `Controllers/StatistiquesController.cs` (à créer)
    - Action: Créer contrôleur et vues
    - Contenu: Stats serveur, DB, performances

---

## 🔐 Phase 6: Système Mot de Passe Oublié (À FAIRE)

### 6.1 Développement Complet

19. ❌ **Mot de passe oublié**
    - Fichiers à créer:
      * `Controllers/AuthController.cs` - Actions ForgotPassword, ResetPassword
      * `Views/Auth/ForgotPassword.cshtml`
      * `Views/Auth/ResetPassword.cshtml`
      * `Services/IEmailService.cs`
      * `Services/EmailService.cs`
    - Fonctionnalités:
      * Formulaire "Mot de passe oublié"
      * Génération token sécurisé
      * Envoi email avec lien
      * Réinitialisation avec token
      * Expiration token (24h)

20. ❌ **Réinitialisation mot de passe (utilisateur)**
    - Lié au point 19
    - Même développement

---

## 📋 Récapitulatif des Actions

### Actions Immédiates (Phase 1) ✅
- [x] Vérifier Quotas groupes → DÉJÀ PRÉSENT
- [x] Vérifier Configuration commandes → DÉJÀ PRÉSENT
- [x] Vérifier Vérifier commande → DÉJÀ PRÉSENT
- [x] Vérifier Liste commandes → ACCESSIBLE

**Résultat: +4 NOK résolus**

### Actions Prioritaires (Phases 2-4)
- [ ] Vérifier 6 autorisations (2-3 heures)
- [ ] Créer 4 guides utilisateur (2-3 heures)
- [ ] Ajouter 2 améliorations UX (1-2 heures)

**Estimation: 1 journée de travail**  
**Résultat attendu: +12 NOK résolus → Total: 29/35 (83%)**

### Actions Optionnelles (Phase 5)
- [ ] Créer 4 APIs (si nécessaire) (4-6 heures)
- [ ] Créer page Statistiques (si nécessaire) (2-3 heures)

**Estimation: 1 journée de travail**  
**Résultat attendu: +5 NOK résolus → Total: 34/35 (97%)**

### Actions Long Terme (Phase 6)
- [ ] Système Mot de passe oublié complet (1-2 jours)

**Estimation: 2 jours de travail**  
**Résultat attendu: +1 NOK résolu → Total: 35/35 (100%)**

---

## 🎯 Stratégie Recommandée

### Option 1: Résolution Rapide (83%)
**Durée:** 1 journée  
**Actions:** Phases 1-4 uniquement  
**Résultat:** 29/35 NOK résolus (83%)  
**Avantage:** Rapide, résout tous les problèmes critiques

### Option 2: Résolution Complète sans APIs (97%)
**Durée:** 2 jours  
**Actions:** Phases 1-4 + Phase 6  
**Résultat:** 34/35 NOK résolus (97%)  
**Avantage:** Presque complet, APIs optionnelles non faites

### Option 3: Résolution 100% ⭐
**Durée:** 3-4 jours  
**Actions:** Toutes les phases  
**Résultat:** 35/35 NOK résolus (100%)  
**Avantage:** Complet à 100%

---

## 📅 Planning Détaillé pour 100%

### Jour 1: Vérifications et Corrections
**Matin (4h):**
- Vérifier et corriger les 6 autorisations
- Tester avec différents rôles

**Après-midi (4h):**
- Créer les 4 guides utilisateur
- Ajouter les 2 améliorations UX

**Résultat Jour 1:** 29/35 (83%)

### Jour 2: APIs et Statistiques
**Matin (4h):**
- Créer les 4 endpoints API
- Tester les APIs

**Après-midi (4h):**
- Créer page Statistiques système
- Créer interface Formules disponibles

**Résultat Jour 2:** 34/35 (97%)

### Jour 3-4: Mot de Passe Oublié
**Jour 3 (8h):**
- Créer le système complet
- Formulaires et contrôleurs
- Service email

**Jour 4 (4h):**
- Tests complets
- Corrections bugs
- Documentation

**Résultat Final:** 35/35 (100%) ✅

---

## ✅ Checklist Complète

### Phase 1: Vérifications ✅
- [x] Quotas groupes
- [x] Configuration commandes
- [x] Vérifier commande
- [x] Liste commandes

### Phase 2: Autorisations
- [ ] CommandeInstantanee
- [ ] Edit commande
- [ ] Delete commande
- [ ] Edit menu
- [ ] CreateCommand visiteur
- [ ] Create point consommation

### Phase 3: Documentation
- [ ] Guide Annuler vs Supprimer
- [ ] Guide Modal Extraction
- [ ] Guide Point Consommation
- [ ] Guide Commandes Douaniers

### Phase 4: UX
- [ ] Bouton Réinitialiser mot de passe
- [ ] Visibilité formules visiteurs
- [ ] Clarifier workflow douaniers

### Phase 5: APIs
- [ ] API Liste directions
- [ ] API Formules visiteurs
- [ ] API Créer commande visiteur
- [ ] Interface Formules disponibles
- [ ] Page Statistiques système

### Phase 6: Mot de Passe
- [ ] Système Mot de passe oublié
- [ ] Réinitialisation utilisateur

---

## 🎉 Objectif Final

**35/35 NOK résolus (100%)** ✅

**Délai:** 3-4 jours de travail  
**Priorité:** HAUTE  
**Impact:** Majeur sur la satisfaction utilisateur

---

**Date:** 05/03/2026  
**Statut:** 📋 PLAN COMPLET ÉTABLI  
**Prochaine étape:** Commencer Phase 2 (Autorisations)

