# Plan de Tests - Gestion des Menus

## 🎯 Objectif

Valider que toutes les fonctionnalités de gestion des menus fonctionnent correctement.

---

## ✅ Tests à Effectuer

### 1. Création de Menus

#### Test 1.1 : Création Unitaire - Menu Complet
**Objectif :** Créer un menu avec toutes les formules

**Étapes :**
1. Aller sur FormuleJour > Index
2. Cliquer sur "Nouvelle Formule"
3. Remplir tous les champs :
   - Date : Demain
   - Nom : "Menu Test Complet"
   - Formule Améliorée : Entrée, Plat, Garniture, Dessert
   - Formule Standard 1 : Plat, Garniture
   - Formule Standard 2 : Plat, Garniture
   - Féculent : "Riz"
   - Légumes : "Légumes de saison"
   - Marge : 15
   - Statut : Active
4. Cliquer sur "Créer la formule"

**Résultat attendu :**
- ✅ Message de succès "Formule créée avec succès"
- ✅ Redirection vers la liste
- ✅ Le menu apparaît dans la liste

---

#### Test 1.2 : Création Unitaire - Menu Minimal
**Objectif :** Créer un menu avec le minimum de champs

**Étapes :**
1. Cliquer sur "Nouvelle Formule"
2. Remplir uniquement :
   - Date : Après-demain
   - Nom : "Menu Test Minimal"
3. Cliquer sur "Créer la formule"

**Résultat attendu :**
- ✅ Message de succès
- ✅ Le menu est créé avec les champs vides

---

#### Test 1.3 : Création Unitaire - Date Existante
**Objectif :** Vérifier la validation des doublons

**Étapes :**
1. Cliquer sur "Nouvelle Formule"
2. Utiliser la même date qu'un menu existant
3. Cliquer sur "Créer la formule"

**Résultat attendu :**
- ❌ Message d'erreur "Une formule existe déjà pour cette date"
- ❌ Le formulaire reste affiché

---

#### Test 1.4 : Création en Lot - Semaine
**Objectif :** Créer les menus d'une semaine

**Étapes :**
1. Cliquer sur "Création en Lot"
2. Date de début : Lundi prochain
3. Date de fin : Vendredi prochain (5 jours)
4. Cocher "Exclure les weekends"
5. Remplir les informations communes
6. Cliquer sur "Créer les formules"

**Résultat attendu :**
- ✅ Message "5 formules créées avec succès"
- ✅ 5 menus apparaissent dans la liste (lundi à vendredi)

---

#### Test 1.5 : Création en Lot - Avec Remplacement
**Objectif :** Remplacer des menus existants

**Étapes :**
1. Créer un menu pour demain
2. Cliquer sur "Création en Lot"
3. Date de début : Demain
4. Date de fin : Demain + 2 jours
5. Cocher "Remplacer les formules existantes"
6. Cliquer sur "Créer les formules"

**Résultat attendu :**
- ✅ Message "X formules créées, Y formules modifiées"
- ✅ Le menu de demain est remplacé

---

#### Test 1.6 : Import Excel - Fichier Valide
**Objectif :** Importer des menus depuis Excel

**Étapes :**
1. Cliquer sur "Importer"
2. Télécharger le modèle Excel
3. Remplir 3 lignes avec des menus valides
4. Uploader le fichier
5. Cliquer sur "Importer"

**Résultat attendu :**
- ✅ Message "3 formules importées avec succès"
- ✅ Les 3 menus apparaissent dans la liste

---

#### Test 1.7 : Import Excel - Fichier avec Erreurs
**Objectif :** Gérer les erreurs d'import

**Étapes :**
1. Créer un fichier Excel avec :
   - Ligne 1 : Date manquante
   - Ligne 2 : Valide
   - Ligne 3 : Date existante
2. Uploader le fichier
3. Cocher "Ignorer les erreurs"
4. Cliquer sur "Importer"

**Résultat attendu :**
- ⚠️ Message "1 formule importée, 2 erreurs"
- ⚠️ Liste des erreurs affichée
- ✅ La ligne valide est importée

---

### 2. Modification de Menus

#### Test 2.1 : Modifier un Menu
**Objectif :** Modifier toutes les informations d'un menu

**Étapes :**
1. Cliquer sur "Modifier" d'un menu
2. Changer plusieurs champs
3. Cliquer sur "Enregistrer les modifications"

**Résultat attendu :**
- ✅ Message "Formule modifiée avec succès"
- ✅ Les modifications sont visibles dans la liste
- ✅ La date de modification est mise à jour

---

#### Test 2.2 : Modifier la Date vers une Date Existante
**Objectif :** Vérifier la validation lors de la modification

**Étapes :**
1. Cliquer sur "Modifier" d'un menu
2. Changer la date vers une date déjà utilisée
3. Cliquer sur "Enregistrer les modifications"

**Résultat attendu :**
- ❌ Message d'erreur "Une autre formule existe déjà pour cette date"
- ❌ Le formulaire reste affiché

---

#### Test 2.3 : Verrouiller un Menu
**Objectif :** Verrouiller un menu

**Étapes :**
1. Cliquer sur "Modifier" d'un menu
2. Cocher "Verrouillé"
3. Cliquer sur "Enregistrer les modifications"

**Résultat attendu :**
- ✅ Le menu affiche le badge "Verrouillé" dans la liste
- ✅ Le statut est visible dans les détails

---

### 3. Suppression de Menus

#### Test 3.1 : Supprimer un Menu sans Commandes
**Objectif :** Supprimer un menu non utilisé

**Étapes :**
1. Créer un menu de test
2. Cliquer sur "Supprimer"
3. Confirmer la suppression

**Résultat attendu :**
- ✅ Message "Formule supprimée avec succès"
- ✅ Le menu disparaît de la liste

---

#### Test 3.2 : Supprimer un Menu avec Commandes
**Objectif :** Vérifier la protection des menus utilisés

**Étapes :**
1. Créer une commande pour un menu
2. Essayer de supprimer ce menu
3. Confirmer la suppression

**Résultat attendu :**
- ❌ Message d'erreur "Impossible de supprimer cette formule car elle est liée à des commandes"
- ❌ Le menu reste dans la liste

---

### 4. Consultation des Menus

#### Test 4.1 : Afficher la Liste
**Objectif :** Afficher tous les menus

**Étapes :**
1. Aller sur FormuleJour > Index

**Résultat attendu :**
- ✅ Liste des menus affichée
- ✅ Colonnes : Date, Nom, Type, Formules, Statut, Actions
- ✅ Boutons d'action visibles

---

#### Test 4.2 : Filtrer par Date
**Objectif :** Filtrer les menus par période

**Étapes :**
1. Entrer une date de début
2. Entrer une date de fin
3. Cliquer sur "Filtrer"

**Résultat attendu :**
- ✅ Seuls les menus de la période sont affichés
- ✅ Les filtres sont conservés dans les champs

---

#### Test 4.3 : Consulter les Détails
**Objectif :** Voir tous les détails d'un menu

**Étapes :**
1. Cliquer sur "Détails" (œil) d'un menu

**Résultat attendu :**
- ✅ Page de détails affichée
- ✅ Toutes les sections visibles :
  - Informations générales
  - Formule Améliorée
  - Formules Standard 1 et 2
  - Éléments communs
  - Historique
  - Traçabilité
- ✅ Boutons "Modifier" et "Supprimer" visibles

---

#### Test 4.4 : Consulter la Semaine N+1
**Objectif :** Afficher les menus de la semaine suivante

**Étapes :**
1. Cliquer sur "Semaine N+1"

**Résultat attendu :**
- ✅ Seuls les menus de la semaine suivante sont affichés
- ✅ Badge "Semaine N+1" visible
- ✅ Dates correctes (lundi à dimanche de la semaine suivante)

---

#### Test 4.5 : Consulter l'Historique
**Objectif :** Voir tous les menus passés et futurs

**Étapes :**
1. Cliquer sur "Historique"

**Résultat attendu :**
- ✅ Page d'historique affichée
- ✅ Statistiques visibles (Total, Améliorées, Standard, Verrouillés)
- ✅ Timeline des menus affichée
- ✅ Menus triés par date décroissante

---

#### Test 4.6 : Filtrer l'Historique
**Objectif :** Filtrer l'historique par date et nom

**Étapes :**
1. Aller sur "Historique"
2. Entrer une date de début
3. Entrer un nom de formule
4. Cliquer sur "Filtrer"

**Résultat attendu :**
- ✅ Seuls les menus correspondants sont affichés
- ✅ Statistiques mises à jour

---

### 5. Tests de Validation

#### Test 5.1 : Champs Obligatoires
**Objectif :** Vérifier les validations

**Étapes :**
1. Cliquer sur "Nouvelle Formule"
2. Laisser la date vide
3. Cliquer sur "Créer la formule"

**Résultat attendu :**
- ❌ Message d'erreur "La date est obligatoire"
- ❌ Le formulaire reste affiché

---

#### Test 5.2 : Format de Fichier Excel
**Objectif :** Vérifier la validation du format

**Étapes :**
1. Cliquer sur "Importer"
2. Uploader un fichier .txt ou .pdf
3. Cliquer sur "Importer"

**Résultat attendu :**
- ❌ Message d'erreur "Le fichier doit être au format Excel"
- ❌ Le formulaire reste affiché

---

#### Test 5.3 : Marge Invalide
**Objectif :** Vérifier la validation de la marge

**Étapes :**
1. Cliquer sur "Nouvelle Formule"
2. Entrer une marge de 150 (> 100)
3. Cliquer sur "Créer la formule"

**Résultat attendu :**
- ❌ Message d'erreur "La marge doit être entre 0 et 100"
- ❌ Le formulaire reste affiché

---

### 6. Tests de Sécurité

#### Test 6.1 : Accès Non Autorisé
**Objectif :** Vérifier les autorisations

**Étapes :**
1. Se connecter avec un utilisateur sans rôle autorisé
2. Essayer d'accéder à FormuleJour

**Résultat attendu :**
- ❌ Redirection vers la page d'erreur "Non autorisé"
- ❌ Aucune action possible

---

#### Test 6.2 : Soft Delete
**Objectif :** Vérifier que la suppression est logique

**Étapes :**
1. Supprimer un menu
2. Vérifier en base de données

**Résultat attendu :**
- ✅ Le champ `Supprimer` = 1
- ✅ Le menu n'apparaît plus dans la liste
- ✅ Le menu existe toujours en base de données

---

### 7. Tests de Performance

#### Test 7.1 : Import de Gros Fichier
**Objectif :** Tester l'import de nombreux menus

**Étapes :**
1. Créer un fichier Excel avec 100 lignes
2. Importer le fichier

**Résultat attendu :**
- ✅ Import réussi en moins de 30 secondes
- ✅ Tous les menus valides sont importés

---

#### Test 7.2 : Affichage de Nombreux Menus
**Objectif :** Tester l'affichage d'une grande liste

**Étapes :**
1. Créer 50+ menus
2. Afficher la liste

**Résultat attendu :**
- ✅ Liste affichée en moins de 3 secondes
- ✅ Tous les menus sont visibles

---

## 📊 Résumé des Tests

| Catégorie | Nombre de Tests | Statut |
|-----------|-----------------|--------|
| Création | 7 | ⏳ À tester |
| Modification | 3 | ⏳ À tester |
| Suppression | 2 | ⏳ À tester |
| Consultation | 6 | ⏳ À tester |
| Validation | 3 | ⏳ À tester |
| Sécurité | 2 | ⏳ À tester |
| Performance | 2 | ⏳ À tester |
| **TOTAL** | **25** | **⏳ À tester** |

---

## 🐛 Rapport de Bugs

### Format de Rapport

```
**Bug #X : [Titre du bug]**

**Sévérité :** Critique / Majeure / Mineure

**Test :** [Numéro du test]

**Description :**
[Description détaillée du problème]

**Étapes pour reproduire :**
1. [Étape 1]
2. [Étape 2]
3. [Étape 3]

**Résultat attendu :**
[Ce qui devrait se passer]

**Résultat obtenu :**
[Ce qui se passe réellement]

**Capture d'écran :**
[Si applicable]

**Environnement :**
- Navigateur : [Chrome, Firefox, Edge, etc.]
- Version : [Version du navigateur]
- OS : [Windows, Mac, Linux]
```

---

## ✅ Checklist de Validation

Avant de considérer le module comme validé, vérifier que :

- [ ] Tous les tests de création passent
- [ ] Tous les tests de modification passent
- [ ] Tous les tests de suppression passent
- [ ] Tous les tests de consultation passent
- [ ] Tous les tests de validation passent
- [ ] Tous les tests de sécurité passent
- [ ] Tous les tests de performance passent
- [ ] Aucun bug critique n'est ouvert
- [ ] La documentation est à jour
- [ ] Le guide utilisateur est complet

---

## 📝 Notes

- Effectuer les tests dans l'ordre indiqué
- Noter tous les bugs rencontrés
- Prendre des captures d'écran si nécessaire
- Tester sur différents navigateurs (Chrome, Firefox, Edge)
- Tester avec différents rôles utilisateur

---

**Version :** 1.0  
**Date :** 5 février 2026  
**Statut :** ⏳ Tests à effectuer
