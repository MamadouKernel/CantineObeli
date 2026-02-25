# Tests du Nouveau Format d'Importation des Menus

## 🧪 Plan de Tests

Ce document décrit les tests à effectuer pour valider le nouveau format d'importation.

## ✅ Tests Fonctionnels

### Test 1 : Import d'une Semaine Complète
**Objectif** : Vérifier l'import de 7 jours avec toutes les formules

**Données de test** :
```
Date       | Entree              | Dessert | Plat                | Garniture           | Feculent | Legumes | Plat std 1         | Garniture std 1 | Plat std 2              | Garniture std 2
02/02/2026 | Salade de Crudités  | Yaourt  | Filet de Sosso      | Pois Chiches Sautés |          |         | Lasagne Bolognaise | Salade Verte    | Soupe de Poulet         | Riz Blanc
03/02/2026 | Salade Verdurette   | Brownie | Gratin de Cabillaud | Pommes de Terre     |          |         | APF                | Attiéké         | Bœuf Sauce Bawin        | Riz Blanc
04/02/2026 | Friand au Fromage   | Beignet | Émincé de Bœuf      | Riz Safrané         |          |         | Poulet au Four     | Pommes Sautées  | Poisson Fumé Sauce      | Riz Blanc
05/02/2026 | Salade Composée     | Gâteau  | Lapin aux Pruneaux  | Purée Patates       |          |         | Choukouya de Bœuf  | Attiéké         | Akpéssi de Banane       | Banane Plantain
06/02/2026 | Mini Quiche         | Salade  | Chili Con Carne     | Riz Blanc           |          |         | Poisson Frit       | Abolo           | Bœuf Sauce Pistache     | Riz
07/02/2026 | Cocktail Crudités   | Pain    | Colombo de Poulet   | Couscous            |          |         | Poulet Ivoirienne  | Attiéké         | Poisson Frit Feuilles   | Riz Blanc
08/02/2026 | Œufs Brouillés      | Moka    | Saumon Grillé       | Patates Rôties      |          |         | Chicken Burger     | Pommes Sautées  | Poulet Fumé Doumglé     | Riz Blanc
```

**Résultat attendu** :
- ✅ 7 lignes importées
- ✅ 21 formules créées (3 par jour)
- ✅ Message : "7 lignes traitées, 21 formules créées avec succès"

**Vérifications** :
- [ ] Toutes les dates sont correctes
- [ ] Chaque jour a 3 formules (Améliorée, Standard 1, Standard 2)
- [ ] Les données sont correctement réparties
- [ ] Aucune erreur affichée

---

### Test 2 : Import avec Formules Partielles
**Objectif** : Vérifier que seules les formules remplies sont créées

**Données de test** :
```
Date       | Entree | Dessert | Plat    | Garniture | Feculent | Legumes | Plat std 1 | Garniture std 1 | Plat std 2 | Garniture std 2
02/02/2026 | Salade | Yaourt  | Poulet  | Riz       |          |         |            |                 |            |
03/02/2026 |        |         |         |           |          |         | Lasagne    | Salade          |            |
04/02/2026 | Salade | Brownie | Poisson | Riz       |          |         | APF        | Attiéké         |            |
```

**Résultat attendu** :
- ✅ 3 lignes importées
- ✅ 4 formules créées :
  - 02/02 : 1 formule (Améliorée)
  - 03/02 : 1 formule (Standard 1)
  - 04/02 : 2 formules (Améliorée + Standard 1)
- ✅ Message : "3 lignes traitées, 4 formules créées avec succès"

**Vérifications** :
- [ ] Ligne 1 : Seulement Formule Améliorée créée
- [ ] Ligne 2 : Seulement Formule Standard 1 créée
- [ ] Ligne 3 : Formule Améliorée + Standard 1 créées
- [ ] Aucune formule vide créée

---

### Test 3 : Formats de Date Multiples
**Objectif** : Vérifier que tous les formats de date sont acceptés

**Données de test** :
```
Date       | Entree | Dessert | Plat   | ...
02/02/2026 | Salade | Yaourt  | Poulet | ...  (Format DD/MM/YYYY)
2026-02-03 | Salade | Brownie | Poisson| ...  (Format YYYY-MM-DD)
04-02-2026 | Salade | Gâteau  | Bœuf   | ...  (Format DD-MM-YYYY)
```

**Résultat attendu** :
- ✅ 3 lignes importées
- ✅ 9 formules créées (3 par jour)
- ✅ Dates correctement parsées

**Vérifications** :
- [ ] Date 1 : 02/02/2026 correctement importée
- [ ] Date 2 : 03/02/2026 correctement importée
- [ ] Date 3 : 04/02/2026 correctement importée

---

### Test 4 : Remplacement de Formules Existantes
**Objectif** : Vérifier l'option "Remplacer les formules existantes"

**Étapes** :
1. Importer des formules pour le 02/02/2026
2. Modifier les données
3. Réimporter avec "Remplacer les formules existantes" coché

**Résultat attendu** :
- ✅ Anciennes formules marquées comme supprimées (Supprimer = 1)
- ✅ Nouvelles formules créées
- ✅ Données mises à jour

**Vérifications** :
- [ ] Anciennes formules supprimées
- [ ] Nouvelles formules créées
- [ ] Données correctement mises à jour

---

### Test 5 : Gestion des Erreurs
**Objectif** : Vérifier la validation et les messages d'erreur

#### Test 5.1 : Date Manquante
**Données** :
```
Date | Entree | Dessert | Plat   | ...
     | Salade | Yaourt  | Poulet | ...
```

**Résultat attendu** :
- ❌ Erreur : "Ligne 2: Colonne A (Date) est vide"
- ❌ Import annulé

#### Test 5.2 : Aucun Champ Rempli
**Données** :
```
Date       | Entree | Dessert | Plat | ...
02/02/2026 |        |         |      | ...
```

**Résultat attendu** :
- ❌ Erreur : "Ligne 2: Aucun champ de formule rempli"
- ❌ Import annulé

#### Test 5.3 : Format de Date Invalide
**Données** :
```
Date       | Entree | Dessert | Plat   | ...
32/13/2026 | Salade | Yaourt  | Poulet | ...
```

**Résultat attendu** :
- ❌ Erreur : "Ligne 2, Colonne A (Date): Format de date invalide"
- ❌ Import annulé

#### Test 5.4 : Formules Existantes (sans option Remplacer)
**Données** :
```
Date       | Entree | Dessert | Plat   | ...
02/02/2026 | Salade | Yaourt  | Poulet | ...
```
(Avec des formules déjà existantes pour cette date)

**Résultat attendu** :
- ❌ Erreur : "Ligne 2: Des formules existent déjà pour la date 02/02/2026"
- ❌ Import annulé

---

## 🔍 Tests de Performance

### Test 6 : Import de Grande Quantité
**Objectif** : Vérifier les performances avec un grand volume

**Données de test** :
- 52 lignes (1 an de menus)
- Toutes les colonnes remplies

**Résultat attendu** :
- ✅ Import réussi en moins de 10 secondes
- ✅ 52 lignes importées
- ✅ 156 formules créées (3 × 52)

**Vérifications** :
- [ ] Temps d'import < 10 secondes
- [ ] Aucune erreur de mémoire
- [ ] Toutes les données correctement importées

---

### Test 7 : Import avec Caractères Spéciaux
**Objectif** : Vérifier la gestion des caractères spéciaux

**Données de test** :
```
Date       | Entree              | Dessert        | Plat                    | ...
02/02/2026 | Salade d'été        | Crème brûlée   | Poulet à l'ivoirienne   | ...
03/02/2026 | Œufs & légumes      | Gâteau "maison"| Bœuf sauce "spéciale"   | ...
```

**Résultat attendu** :
- ✅ Caractères accentués préservés
- ✅ Apostrophes et guillemets gérés
- ✅ Caractères spéciaux (&, œ, etc.) préservés

**Vérifications** :
- [ ] Accents corrects (é, è, à, ô, etc.)
- [ ] Apostrophes préservées
- [ ] Guillemets préservés
- [ ] Caractères spéciaux corrects

---

## 🎯 Tests d'Intégration

### Test 8 : Workflow Complet
**Objectif** : Tester le workflow complet de bout en bout

**Étapes** :
1. Télécharger le modèle
2. Remplir avec des données de test
3. Importer le fichier
4. Vérifier dans la liste des formules
5. Modifier une formule manuellement
6. Réimporter avec "Remplacer"
7. Vérifier la mise à jour

**Résultat attendu** :
- ✅ Toutes les étapes réussies
- ✅ Données cohérentes à chaque étape

---

### Test 9 : Compatibilité avec les Commandes
**Objectif** : Vérifier que les formules importées fonctionnent avec les commandes

**Étapes** :
1. Importer des formules pour la semaine prochaine
2. Créer une commande avec ces formules
3. Vérifier que les formules sont disponibles
4. Valider la commande

**Résultat attendu** :
- ✅ Formules disponibles dans la liste
- ✅ Commande créée avec succès
- ✅ Données correctes dans la commande

---

## 📊 Tests de Régression

### Test 10 : Fonctionnalités Existantes
**Objectif** : Vérifier que les fonctionnalités existantes fonctionnent toujours

**Vérifications** :
- [ ] Création manuelle de formules
- [ ] Modification de formules
- [ ] Suppression de formules
- [ ] Export Excel
- [ ] Gestion des marges
- [ ] Filtrage par date
- [ ] Pagination

---

## 🐛 Tests de Cas Limites

### Test 11 : Fichier Vide
**Données** : Fichier avec seulement les en-têtes

**Résultat attendu** :
- ⚠️ Message : "Aucune ligne à importer"

### Test 12 : Fichier avec Lignes Vides
**Données** : Fichier avec des lignes vides entre les données

**Résultat attendu** :
- ✅ Lignes vides ignorées
- ✅ Données valides importées

### Test 13 : Colonnes Supplémentaires
**Données** : Fichier avec des colonnes supplémentaires après la colonne K

**Résultat attendu** :
- ✅ Colonnes supplémentaires ignorées
- ✅ Import réussi

### Test 14 : Ordre des Colonnes Modifié
**Données** : Fichier avec les colonnes dans un ordre différent

**Résultat attendu** :
- ❌ Erreur ou données incorrectes
- ⚠️ Recommandation : Utiliser le modèle fourni

---

## 📝 Checklist de Tests

### Tests Obligatoires (Avant Mise en Production)
- [ ] Test 1 : Import semaine complète
- [ ] Test 2 : Formules partielles
- [ ] Test 3 : Formats de date
- [ ] Test 4 : Remplacement
- [ ] Test 5 : Gestion des erreurs (tous les sous-tests)
- [ ] Test 8 : Workflow complet
- [ ] Test 9 : Compatibilité commandes
- [ ] Test 10 : Régression

### Tests Recommandés
- [ ] Test 6 : Performance
- [ ] Test 7 : Caractères spéciaux
- [ ] Tests 11-14 : Cas limites

### Tests Optionnels
- [ ] Tests de charge (100+ lignes)
- [ ] Tests de concurrence (imports simultanés)
- [ ] Tests de compatibilité navigateurs

---

## 🔧 Environnement de Test

### Prérequis
- Application déployée et fonctionnelle
- Base de données de test
- Compte utilisateur avec droits d'import
- Fichiers Excel de test préparés

### Données de Test
- Utiliser le modèle fourni
- Créer des variations pour chaque test
- Sauvegarder les fichiers de test pour réutilisation

### Outils
- Excel ou LibreOffice Calc
- Navigateur web (Chrome, Firefox, Edge)
- Outil de capture d'écran pour documentation

---

## 📊 Rapport de Tests

### Modèle de Rapport

```
Test N° : [Numéro]
Nom : [Nom du test]
Date : [Date d'exécution]
Testeur : [Nom]

Résultat : ✅ Réussi / ❌ Échoué / ⚠️ Partiel

Détails :
- Données utilisées : [Description]
- Résultat obtenu : [Description]
- Résultat attendu : [Description]
- Écarts : [Si applicable]

Captures d'écran : [Liens]

Commentaires : [Observations]
```

---

## 🎯 Critères de Validation

### Critères de Succès
- ✅ Tous les tests obligatoires réussis
- ✅ Aucune régression détectée
- ✅ Performance acceptable (< 10s pour 52 lignes)
- ✅ Messages d'erreur clairs et utiles
- ✅ Documentation complète et à jour

### Critères d'Échec
- ❌ Un test obligatoire échoué
- ❌ Régression sur fonctionnalité existante
- ❌ Performance inacceptable (> 30s pour 52 lignes)
- ❌ Perte de données
- ❌ Erreurs non gérées (crashes)

---

**Bonne chance pour les tests ! 🧪**

*Dernière mise à jour : 10 février 2026*
