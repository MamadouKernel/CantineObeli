# Workflows de test par rôle - Application O'Beli

## Rôles identifiés
1. **Admin** (Administrateur)
2. **RH** (Ressources Humaines)
3. **Employé**
4. **PrestataireCantine**

---

## 1. Workflow ADMIN (Administrateur)

### Accès
- Accès complet à toutes les fonctionnalités
- Tous les rôles sont automatiquement attribués

### Tests à effectuer

#### A. Connexion et navigation
1. Se connecter avec un compte Admin
2. Vérifier l'accès à tous les menus
3. Confirmer l'affichage du tableau de bord

#### B. Gestion des utilisateurs
1. Aller dans Paramètres > Utilisateurs
2. Créer un utilisateur (tous rôles)
3. Modifier un utilisateur
4. Supprimer un utilisateur
5. Vérifier les permissions par rôle

#### C. Gestion des départements et fonctions
1. Paramètres > Départements
2. Paramètres > Fonctions
3. Créer/modifier/supprimer

#### D. Gestion des formules
1. Formules > Gérer les formules
2. Créer des formules pour la semaine
3. Modifier des formules existantes
4. Supprimer des formules

#### E. Gestion des commandes
1. Commandes > Mes commandes
2. Commandes > Commande instantanée
3. Commandes > Commande groupée
4. Commandes > Vérifier une commande
5. Commandes > Exporter Excel
6. **Tester le blocage vendredi 12h** (voir section "Règles de blocage")

#### F. Reporting et statistiques
1. Reporting > Dashboard
2. Tester les filtres (dates, sites, départements)
3. Vérifier les indicateurs
4. Exporter des rapports

#### G. Points de consommation
1. Points de consommation
2. Créer des points manquants
3. Vérifier les résumés par utilisateur

#### H. Extraction de données
1. Extraction
2. Tester les exports

#### I. Configuration des commandes
1. Paramètres > Configuration des commandes
2. Modifier le jour et l'heure de clôture
3. Tester le blocage des commandes
4. Vérifier la prochaine date de clôture

---

## 2. Workflow RH (Ressources Humaines)

### Accès
- Accès similaire à l'Admin, sauf gestion des utilisateurs
- Pas d'accès aux paramètres système

### Tests à effectuer

#### A. Connexion et navigation
1. Se connecter avec un compte RH
2. Vérifier l'accès aux menus autorisés
3. Confirmer l'absence des menus système

#### B. Gestion des commandes
1. Commandes > Mes commandes
2. Commandes > Commande groupée
3. Commandes > Exporter Excel
4. Vérifier l'absence de "Commande instantanée"

#### C. Gestion des formules
1. Formules > Gérer les formules
2. Créer/modifier des formules
3. Vérifier les permissions

#### D. Reporting
1. Reporting > Dashboard
2. Tester les filtres
3. Vérifier les indicateurs

#### E. Points de consommation
1. Points de consommation
2. Créer des points manquants
3. Vérifier les résumés

#### F. Extraction
1. Extraction
2. Tester les exports

#### G. Configuration des commandes
1. Paramètres > Configuration des commandes
2. Modifier le jour et l'heure de clôture
3. Tester le blocage des commandes
4. Vérifier la prochaine date de clôture

#### H. Restrictions
1. Tenter d'accéder à Paramètres > Utilisateurs (doit être refusé)
2. Tenter d'accéder à Paramètres > Départements (doit être refusé)
3. Tenter d'accéder à Paramètres > Fonctions (doit être refusé)

---

## 3. Workflow EMPLOYÉ

### Accès
- Accès limité aux fonctionnalités de base
- Pas d'accès aux paramètres ni au reporting

### Tests à effectuer

#### A. Connexion et navigation
1. Se connecter avec un compte Employé
2. Vérifier l'accès aux menus autorisés
3. Confirmer l'absence des menus système

#### B. Gestion des commandes
1. Commandes > Mes commandes
2. Créer une nouvelle commande
3. Modifier une commande existante
4. Annuler une commande
5. Vérifier l'absence de "Commande groupée"
6. **Tester le blocage vendredi 12h** (voir section "Règles de blocage")

#### C. Points de consommation
1. Points de consommation
2. Vérifier l'affichage des points personnels
3. Confirmer l'absence des résumés par utilisateur

#### D. Restrictions
1. Tenter d'accéder à Formules (doit être refusé)
2. Tenter d'accéder à Paramètres (doit être refusé)
3. Tenter d'accéder à Reporting (doit être refusé)
4. Tenter d'accéder à Extraction (doit être refusé)

#### E. Tableau de bord
1. Vérifier l'affichage des menus de la semaine
2. Vérifier l'affichage des commandes personnelles
3. Confirmer l'absence des statistiques globales

---

## 4. Workflow PRESTATAIRE CANTINE

### Accès
- Accès spécialisé pour la gestion des commandes
- Vue limitée aux commandes du jour

### Tests à effectuer

#### A. Connexion et navigation
1. Se connecter avec un compte PrestataireCantine
2. Vérifier l'accès aux menus autorisés
3. Confirmer l'absence des menus système

#### B. Gestion des commandes du jour
1. Tableau de bord : vérifier l'affichage des commandes du jour
2. Commandes > Vérifier une commande
3. Commandes > Commande instantanée
4. Vérifier l'absence de "Mes commandes"
5. **Tester le blocage vendredi 12h** (voir section "Règles de blocage")

#### C. Actions sur les commandes
1. Marquer une commande comme servie
2. Annuler une commande avec motif
3. Vérifier les statistiques par formule

#### D. Reporting limité
1. Reporting > Dashboard
2. Vérifier l'accès aux statistiques de base
3. Tester les filtres disponibles

#### E. Restrictions
1. Tenter d'accéder à Formules (doit être refusé)
2. Tenter d'accéder à Paramètres (doit être refusé)
3. Tenter d'accéder à Points de consommation (doit être refusé)
4. Tenter d'accéder à Extraction (doit être refusé)

#### F. Tableau de bord spécialisé
1. Vérifier l'affichage des menus du jour
2. Vérifier l'affichage des commandes du jour
3. Vérifier les statistiques par formule
4. Confirmer l'absence des commandes de la semaine

---

## Règles de blocage des commandes

### ⚠️ IMPORTANT : Blocage automatique le vendredi à 12h
- **Règle** : Les commandes pour la semaine N+1 sont automatiquement bloquées chaque **vendredi à 12h00**
- **Période de blocage** : Du vendredi 12h00 jusqu'au lundi suivant (samedi et dimanche inclus)
- **Configuration** : Paramètres configurables via `ConfigurationCommandeController` (Admin/RH uniquement)
- **Valeurs par défaut** : Vendredi 12:00
- **Impact** : Aucune nouvelle commande ne peut être passée pour la semaine suivante pendant cette période

### ✅ PROBLÈME RÉSOLU : Système de blocage activé
- **Statut** : Le système de blocage est maintenant **activé** et fonctionnel
- **Corrections apportées** : 
  - Réactivation du service dans `CommandeController.cs`
  - Correction de la logique dans `ConfigurationService.cs`
- **Nouvelle logique** : Blocage du vendredi 12h jusqu'au lundi suivant

### 🔧 CORRECTIONS APPORTÉES
1. **CommandeController.cs** : 
   - Réactivation du service de configuration
   - Ajout de la vérification de blocage dans `PopulateViewBags()`
   - Ajout de la vérification de blocage dans `Create()`
   - Ajout de la vérification de blocage dans `GetMenusByType()`
2. **ConfigurationService.cs** : Correction de la logique de blocage pour inclure samedi et dimanche
3. **Views/Commande/Create.cshtml** : Ajout d'un message de blocage avec la prochaine date d'ouverture
4. **Logique mise à jour** : 
   - Blocage du vendredi 12h jusqu'au lundi suivant
   - Les menus de la semaine N+1 ne s'affichent plus pendant le blocage
   - Message informatif affiché aux utilisateurs

### Tests spécifiques au blocage
1. **Test avant blocage** (vendredi avant 12h)
   - Vérifier que les commandes sont autorisées
   - Passer une commande pour la semaine N+1
   - Confirmer l'affichage des formules de la semaine suivante

2. **Test pendant blocage** (vendredi après 12h, samedi, dimanche)
   - Vérifier le message de blocage sur la page de commande
   - Confirmer l'absence des menus de la semaine N+1
   - Vérifier l'affichage de la prochaine date d'ouverture
   - Tenter de passer une commande (doit être refusée)
   - Vérifier que les appels AJAX retournent des listes vides

3. **Test de configuration** (Admin/RH uniquement)
   - Aller dans Paramètres > Configuration des commandes
   - Modifier le jour et l'heure de clôture
   - Tester le blocage avec "Test de blocage"
   - Vérifier la prochaine date de clôture

## Points de test communs

### Sécurité
1. Tester la déconnexion
2. Vérifier la redirection après expiration de session
3. Tester l'accès direct aux URLs non autorisées
4. Vérifier la validation des formulaires

### Interface utilisateur
1. Vérifier la responsivité sur mobile
2. Tester les notifications en temps réel
3. Vérifier l'affichage des messages d'erreur
4. Tester les modales et popups

### Performance
1. Tester le chargement des pages
2. Vérifier les temps de réponse des requêtes
3. Tester l'export de gros volumes de données

---

## Données de test recommandées

### Utilisateurs de test
- Admin : `admin001` / `password123`
- RH : `rh001` / `password123`
- Employé : `emp001` / `password123`
- Prestataire : `prest001` / `password123`

### Données de test
- Créer des formules pour la semaine en cours
- Créer des commandes de test
- Configurer des départements et fonctions
- Ajouter des points de consommation

---

## Matrice des permissions par rôle

| Fonctionnalité | Admin | RH | Employé | Prestataire |
|----------------|-------|----|---------| -----------| 
| Gestion utilisateurs | ✅ | ❌ | ❌ | ❌ |
| Gestion départements | ✅ | ❌ | ❌ | ❌ |
| Gestion fonctions | ✅ | ❌ | ❌ | ❌ |
| Gestion formules | ✅ | ✅ | ❌ | ❌ |
| Mes commandes | ✅ | ✅ | ✅ | ❌ |
| Commande instantanée | ✅ | ❌ | ❌ | ✅ |
| Commande groupée | ✅ | ✅ | ❌ | ❌ |
| Vérifier commande | ✅ | ❌ | ❌ | ✅ |
| Exporter Excel | ✅ | ✅ | ❌ | ✅ |
| Reporting | ✅ | ✅ | ❌ | ✅ |
| Points consommation | ✅ | ✅ | ✅ | ❌ |
| Extraction | ✅ | ✅ | ❌ | ❌ |
| **Configuration blocage** | ✅ | ✅ | ❌ | ❌ |

Ces workflows couvrent les fonctionnalités principales et les restrictions de chaque rôle.
