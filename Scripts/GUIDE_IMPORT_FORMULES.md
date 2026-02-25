# 📋 Guide d'Importation des Formules - O'Beli

## 🎯 Vue d'ensemble

Le système d'importation des formules permet de créer facilement plusieurs formules pour différentes dates en utilisant un fichier Excel. Chaque ligne du fichier représente **une formule distincte**.

## 📊 Structure du Fichier Excel

### Colonnes du fichier :

| Colonne | Nom | Description | Obligatoire |
|---------|-----|-------------|-------------|
| A | Date | Date au format YYYY-MM-DD | ✅ |
| B | NomFormule | Nom de la formule | ✅ |
| C | Entree | Entrée pour formule améliorée | ❌ |
| D | Plat | Plat principal pour formule améliorée | ❌ |
| E | Garniture | Garniture pour formule améliorée | ❌ |
| F | Dessert | Dessert pour formule améliorée | ❌ |
| G | PlatStandard1 | Plat principal pour formule standard 1 | ❌ |
| H | GarnitureStandard1 | Garniture pour formule standard 1 | ❌ |
| I | PlatStandard2 | Plat principal pour formule standard 2 | ❌ |
| J | GarnitureStandard2 | Garniture pour formule standard 2 | ❌ |
| K | Feculent | Féculent pour formule améliorée | ❌ |
| L | Legumes | Légumes pour formule améliorée | ❌ |
| M | Marge | Marge (laisser vide) | ❌ |
| N | Statut | 1 = actif, 0 = inactif | ❌ |

## 🍽️ Types de Formules

### 1. Formule Améliorée
- **Champs à remplir** : Au moins un parmi Entree, Plat, Garniture, Dessert, Feculent, Legumes
- **Exemple** : Entrée + Plat principal + Dessert
- **Nom recommandé** : "Formule Améliorée"

### 2. Formule Standard 1
- **Champs à remplir** : Au moins un parmi PlatStandard1, GarnitureStandard1
- **Exemple** : Sauce + Viande
- **Nom recommandé** : "Formule Standard 1"

### 3. Formule Standard 2
- **Champs à remplir** : Au moins un parmi PlatStandard2, GarnitureStandard2
- **Exemple** : Attiéké + Poisson
- **Nom recommandé** : "Formule Standard 2"

## 📝 Exemples d'Utilisation

### Exemple 1 : Menu complet (3 formules pour une date)

```
Date        | NomFormule         | Entree          | Plat           | Garniture      | Dessert    | PlatStandard1 | GarnitureStandard1 | PlatStandard2 | GarnitureStandard2 | Feculent | Legumes | Marge | Statut
2024-01-15  | Formule Améliorée  | Salade verte    | Poulet rôti    | Riz pilaf      | Fruit      |               |                   |               |                   | Riz blanc| Légumes |       | 1
2024-01-15  | Formule Standard 1 |                 |                |                |            | Sauce graine  | Viande bœuf       |               |                   |          |         |       | 1
2024-01-15  | Formule Standard 2 |                 |                |                |            |               |                   | Attiéké       | Poisson grillé    |          |         |       | 1
```

### Exemple 2 : Menu partiel (2 formules pour une date)

```
Date        | NomFormule         | Entree      | Plat           | Garniture | Dessert | PlatStandard1 | GarnitureStandard1 | PlatStandard2 | GarnitureStandard2 | Feculent | Legumes | Marge | Statut
2024-01-16  | Formule Améliorée  | Carottes    | Agouti sauce   | Riz parfumé| Banane  |               |                   |               |                   | Riz      | Gombo   |       | 1
2024-01-16  | Formule Standard 1 |             |                |            |         | Sauce arachide| Poulet            |               |                   |          |         |       | 1
```

### Exemple 3 : Menu simple (1 seule formule)

```
Date        | NomFormule         | Entree | Plat | Garniture | Dessert | PlatStandard1 | GarnitureStandard1 | PlatStandard2 | GarnitureStandard2 | Feculent | Legumes | Marge | Statut
2024-01-17  | Formule Améliorée  | Salade | Poisson | Riz    | Fruit   |               |                   |               |                   |          |         |       | 1
```

## ✅ Règles et Conseils

### Règles Obligatoires :
1. **Date et NomFormule** sont toujours obligatoires
2. Pour créer une formule, remplir **au moins un champ correspondant**
3. Chaque ligne = **une formule distincte**
4. Format de date : **YYYY-MM-DD** (ex: 2024-01-15)

### Conseils d'Organisation :
1. **Groupez vos formules par date** pour une meilleure organisation
2. **Utilisez des noms cohérents** pour les formules du même jour
3. **Vous pouvez créer 1, 2 ou 3 formules** selon vos besoins
4. **Les champs vides ne créent pas de formules vides**

### Noms de Formules Recommandés :
- `Formule Améliorée`
- `Formule Standard 1`
- `Formule Standard 2`

## 🚀 Comment Importer

1. **Téléchargez le modèle** via le bouton "Télécharger le modèle Excel"
2. **Remplissez le fichier** selon vos besoins
3. **Sauvegardez** le fichier Excel
4. **Uploadez** le fichier dans l'interface d'importation
5. **Vérifiez** les résultats et confirmez l'importation

## ⚠️ Points d'Attention

- Les formules existantes pour une date seront **ignorées** ou **remplacées** selon vos paramètres
- Le système vérifie automatiquement la cohérence des données
- Les erreurs seront affichées avant l'importation finale
- Sauvegardez toujours une copie de votre fichier avant l'importation

## 🎉 Résultat Attendu

Après importation, vous verrez :
- **1 formule** si vous n'avez rempli qu'un type de formulaire
- **2 formules** si vous avez rempli deux types de formules
- **3 formules** si vous avez rempli les trois types de formules

Chaque formule sera **indépendante** et pourra être **modifiée ou supprimée séparément**.
