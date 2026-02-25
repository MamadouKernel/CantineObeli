# Guide du Nouveau Format d'Importation des Menus

## 🎯 Objectif

Simplifier l'importation des menus hebdomadaires en réduisant de **21 lignes à 7 lignes** par semaine.

## 📊 Structure du Fichier Excel

### En-têtes (Ligne 1)

| Colonne | Nom | Description | Obligatoire |
|---------|-----|-------------|-------------|
| A | Date | Date du menu (DD/MM/YYYY) | ✅ Oui |
| B | Entree | Entrée de la formule améliorée | ❌ Non |
| C | Dessert | Dessert de la formule améliorée | ❌ Non |
| D | Plat | Plat principal de la formule améliorée | ❌ Non |
| E | Garniture | Garniture de la formule améliorée | ❌ Non |
| F | Feculent | Féculent de la formule améliorée | ❌ Non |
| G | Legumes | Légumes de la formule améliorée | ❌ Non |
| H | Plat standard 1 | Plat de la formule standard 1 | ❌ Non |
| I | Garniture standard 1 | Garniture de la formule standard 1 | ❌ Non |
| J | Plat standard 2 | Plat de la formule standard 2 | ❌ Non |
| K | Garniture standard 2 | Garniture de la formule standard 2 | ❌ Non |

## 📝 Exemple Complet - Semaine du 02/02/2026

```
Date       | Entree                      | Dessert                  | Plat                           | Garniture                | Feculent | Legumes | Plat standard 1      | Garniture standard 1    | Plat standard 2                  | Garniture standard 2
02/02/2026 | Salade de Crudités          | Yaourt                   | Filet de Sosso au Four         | Pois Chiches Sautés      |          |         | Lasagne Bolognaise   | Salade Verte            | Soupe de Poulet                  | Riz Blanc
03/02/2026 | Salade Verdurette           | Brownie                  | Gratin de Cabillaud            | Pommes de Terre Vapeur   |          |         | APF                  | Attiéké                 | Bœuf Sauce Bawin                 | Riz Blanc
04/02/2026 | Friand au Fromage           | Beignet Nature           | Émincé de Bœuf à La Moutarde   | Riz Safrané              |          |         | Poulet au Four       | Pommes de Terre Sautées | Poisson Fumé Sauce Gouagouassou  | Riz Blanc
05/02/2026 | Salade Composée             | Gâteau Semoule Raisins   | Lapin aux Pruneaux             | Purée de Patates Douces  |          |         | Choukouya de Bœuf    | Attiéké                 | Akpéssi de Banane au Poulet      | Banane Plantain
06/02/2026 | Mini Quiche Légumes         | Salade de Fruits Maison  | Chili Con Carne Doux           | Riz Blanc                |          |         | Poisson Frit Abolo   | Abolo                   | Bœuf Sauce Pistache              | Riz
07/02/2026 | Cocktail de Crudités        | Pain Perdu               | Colombo de Poulet              | Couscous                 |          |         | Poulet à L'Ivoirienne| Attiéké                 | Poisson Frit Sauce Feuilles      | Riz Blanc
08/02/2026 | Œufs Brouillés aux Légumes  | Moka Café                | Saumon Grillé                  | Patates Douces Rôties    |          |         | Chicken Burger       | Pommes de Terre Sautées | Poulet Fumé Sauce Doumglé        | Riz Blanc
```

## 🔄 Comment ça Fonctionne

Pour **chaque ligne** (= 1 jour), le système crée automatiquement **jusqu'à 3 formules** :

### 1️⃣ Formule Améliorée
**Colonnes utilisées** : B, C, D, E, F, G  
**Créée si** : Au moins un de ces champs est rempli  
**Nom automatique** : "Formule Améliorée"

**Exemple** :
```
Entrée : Salade de Crudités
Dessert : Yaourt
Plat : Filet de Sosso au Four
Garniture : Pois Chiches Sautés
```

### 2️⃣ Formule Standard 1
**Colonnes utilisées** : H, I  
**Créée si** : Au moins un de ces champs est rempli  
**Nom automatique** : "Formule Standard 1"

**Exemple** :
```
Plat standard 1 : Lasagne Bolognaise
Garniture standard 1 : Salade Verte
```

### 3️⃣ Formule Standard 2
**Colonnes utilisées** : J, K  
**Créée si** : Au moins un de ces champs est rempli  
**Nom automatique** : "Formule Standard 2"

**Exemple** :
```
Plat standard 2 : Soupe de Poulet
Garniture standard 2 : Riz Blanc
```

## ✅ Règles de Validation

### Obligatoire
- ✅ **Date** (Colonne A) : Doit être remplie
- ✅ **Au moins un champ** : Au moins une colonne B-K doit être remplie

### Formats de Date Acceptés
- `DD/MM/YYYY` → 02/02/2026
- `YYYY-MM-DD` → 2026-02-02
- `DD-MM-YYYY` → 02-02-2026

### Optionnel
- ❌ Tous les autres champs peuvent être vides
- ❌ Les formules avec tous les champs vides ne sont pas créées

## 🚀 Procédure d'Importation

### Étape 1 : Télécharger le Modèle
1. Connectez-vous à l'application
2. Allez dans **Formules du Jour** > **Importer**
3. Cliquez sur **Télécharger le modèle**
4. Ouvrez le fichier `modele_import_menus.xlsx`

### Étape 2 : Remplir le Fichier
1. **Ne modifiez pas** la ligne d'en-tête (ligne 1)
2. Remplissez **une ligne par jour** à partir de la ligne 2
3. Colonne A : Date au format DD/MM/YYYY
4. Colonnes B-K : Remplissez selon vos besoins
5. Laissez vides les colonnes non utilisées

### Étape 3 : Importer
1. Retournez dans **Formules du Jour** > **Importer**
2. Cliquez sur **Choisir un fichier**
3. Sélectionnez votre fichier Excel
4. Options :
   - ☑️ **Remplacer les formules existantes** : Cochez pour écraser les menus existants
   - ☑️ **Ignorer les erreurs** : Cochez pour continuer malgré les erreurs
5. Cliquez sur **Importer**

### Étape 4 : Vérifier
1. Consultez le message de confirmation
2. Allez dans **Formules du Jour** > **Liste**
3. Vérifiez que les formules ont été créées correctement
4. Pour chaque jour, vous devriez voir 3 formules (si tous les champs étaient remplis)

## ⚠️ Messages d'Erreur Courants

### "Colonne A (Date) est vide"
**Cause** : La date n'est pas renseignée  
**Solution** : Remplissez la colonne A avec une date au format DD/MM/YYYY

### "Aucun champ de formule rempli"
**Cause** : Toutes les colonnes B-K sont vides  
**Solution** : Remplissez au moins un champ pour créer une formule

### "Format de date invalide"
**Cause** : La date n'est pas au bon format  
**Solution** : Utilisez le format DD/MM/YYYY (ex: 02/02/2026)

### "Des formules existent déjà pour la date"
**Cause** : Des formules existent déjà pour cette date  
**Solution** : Cochez "Remplacer les formules existantes" pour les écraser

## 💡 Conseils et Astuces

### 1. Préparation du Fichier
- ✅ Utilisez le modèle fourni pour éviter les erreurs
- ✅ Copiez-collez depuis un autre fichier si nécessaire
- ✅ Vérifiez les dates avant l'import

### 2. Gestion des Formules
- ✅ Vous n'êtes pas obligé de remplir toutes les formules
- ✅ Si vous ne voulez que la formule améliorée, laissez H-K vides
- ✅ Les formules vides ne sont pas créées (économie d'espace)

### 3. Import en Masse
- ✅ Vous pouvez importer plusieurs semaines en une fois
- ✅ Ajoutez simplement plus de lignes dans le fichier
- ✅ Exemple : 4 semaines = 28 lignes (4 × 7 jours)

### 4. Mise à Jour
- ✅ Pour mettre à jour des menus existants, cochez "Remplacer les formules existantes"
- ✅ Les anciennes formules seront marquées comme supprimées
- ✅ Les nouvelles formules seront créées

## 📊 Comparaison Ancien vs Nouveau Format

| Critère | Ancien Format | Nouveau Format |
|---------|---------------|----------------|
| Lignes par semaine | 21 lignes | 7 lignes |
| Colonnes | 14 colonnes | 11 colonnes |
| Temps de saisie | ~15 minutes | ~5 minutes |
| Risque d'erreur | Élevé | Faible |
| Lisibilité | Moyenne | Excellente |
| Maintenance | Difficile | Facile |

## 🎓 Exemples Pratiques

### Exemple 1 : Menu Complet (3 formules)
```
02/02/2026 | Salade | Yaourt | Poulet | Riz | | | Lasagne | Salade | Soupe | Riz
```
**Résultat** : 3 formules créées pour le 02/02/2026

### Exemple 2 : Seulement Formule Améliorée
```
03/02/2026 | Salade | Brownie | Cabillaud | Pommes | | | | | |
```
**Résultat** : 1 formule créée (Formule Améliorée)

### Exemple 3 : Formule Améliorée + Standard 1
```
04/02/2026 | Friand | Beignet | Bœuf | Riz | | | Poulet | Pommes | |
```
**Résultat** : 2 formules créées (Améliorée + Standard 1)

## 📞 Support

En cas de problème :
1. Consultez ce guide
2. Téléchargez un nouveau modèle
3. Vérifiez les messages d'erreur détaillés
4. Contactez l'administrateur si le problème persiste

## 📚 Documentation Complémentaire

- `MIGRATION_FORMAT_IMPORT_MENUS.md` : Guide de migration complet
- `Scripts/Exemple_Import_Menu_Semaine_Nouveau_Format.md` : Exemples détaillés
- Modèle Excel : Téléchargeable depuis l'interface d'importation
