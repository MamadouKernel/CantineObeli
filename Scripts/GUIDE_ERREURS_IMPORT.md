# 🚨 Guide des Erreurs d'Importation - O'Beli

## 🎯 Vue d'ensemble

Le système d'importation affiche maintenant des **erreurs détaillées** avec :
- ✅ **Numéro de ligne** exact
- ✅ **Nom de colonne** concernée  
- ✅ **Valeur problématique** (si applicable)
- ✅ **Message explicatif** avec solution

## 📋 Types d'Erreurs Détectées

### 1️⃣ **Erreurs de Champs Obligatoires**

#### **Date manquante**
```
Ligne 3: Colonne A (Date) est vide
```
**Solution :** Remplissez la colonne A avec une date au format YYYY-MM-DD

#### **NomFormule manquant**
```
Ligne 5: Colonne B (NomFormule) est vide
```
**Solution :** Remplissez la colonne B avec le nom de la formule (ex: "Formule Améliorée")

#### **Aucune formule définie**
```
Ligne 7: Aucun champ de formule rempli (Colonnes C-L). Remplissez au moins un champ pour créer une formule
```
**Solution :** Remplissez au moins un des champs : Entree, Plat, Garniture, Dessert, PlatStandard1, GarnitureStandard1, PlatStandard2, GarnitureStandard2, Feculent, Legumes

### 2️⃣ **Erreurs de Format**

#### **Date invalide**
```
Ligne 2, Colonne A (Date): Format de date invalide '15/1/2024'. Utilisez le format DD/MM/YYYY (ex: 15/01/2024) ou YYYY-MM-DD (ex: 2024-01-15)
```
**Solution :** Utilisez le format DD/MM/YYYY (ex: 15/01/2024) ou YYYY-MM-DD (ex: 2024-01-15)

#### **Statut invalide**
```
Ligne 4, Colonne N (Statut): Valeur '2' invalide. Utilisez 0 (inactif) ou 1 (actif)
```
**Solution :** Utilisez seulement 0 ou 1 dans la colonne Statut

### 3️⃣ **Erreurs Générales**

#### **Erreur de lecture**
```
Ligne 6: Erreur générale - Index was outside the bounds of the array. Valeurs: Col1: '2024-01-18', Col2: 'Formule Améliorée', Col3: 'Avocat', Col4: 'Poisson braisé', Col5: 'Plantain', Col6: 'Ananas', Col7: '', Col8: '', Col9: '', Col10: '', Col11: 'Plantain', Col12: 'Légumes verts', Col13: '', Col14: '1'
```
**Solution :** Vérifiez que toutes les colonnes sont présentes et correctement formatées

## 📊 Mapping des Colonnes

| Colonne | Nom | Description | Obligatoire |
|---------|-----|-------------|-------------|
| A | Date | Date au format DD/MM/YYYY ou YYYY-MM-DD | ✅ |
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
| N | Statut | Statut (1=actif, 0=inactif) | ❌ |

## 🎯 Exemples d'Erreurs et Solutions

### **Exemple 1 : Fichier avec erreurs multiples**
```
Ligne 2: Colonne A (Date) est vide
Ligne 3, Colonne N (Statut): Valeur '2' invalide. Utilisez 0 (inactif) ou 1 (actif)
Ligne 4: Aucun champ de formule rempli (Colonnes C-L). Remplissez au moins un champ pour créer une formule
```

**Corrections à apporter :**
- Ligne 2 : Ajouter une date dans la colonne A
- Ligne 3 : Changer le statut de '2' vers '1' ou '0'
- Ligne 4 : Remplir au moins un champ de formule (C, D, E, F, G, H, I, J, K, ou L)

### **Exemple 2 : Erreur de format de date**
```
Ligne 5, Colonne A (Date): Format de date invalide '15-01-2024'. Utilisez le format YYYY-MM-DD (ex: 2024-01-15)
```

**Correction :** Changer '15-01-2024' vers '15/01/2024' ou '2024-01-15'

## 💡 Conseils pour Éviter les Erreurs

### **Avant l'importation :**
1. ✅ **Téléchargez le modèle** Excel pour voir le format exact
2. ✅ **Vérifiez les formats** de date (DD/MM/YYYY ou YYYY-MM-DD)
3. ✅ **Testez avec un petit fichier** avant d'importer en masse
4. ✅ **Utilisez des noms cohérents** pour les formules

### **Pendant l'importation :**
1. ✅ **Ne cochez pas "Ignorer les erreurs"** pour la première importation
2. ✅ **Lisez attentivement** les messages d'erreur
3. ✅ **Corrigez toutes les erreurs** avant de relancer

### **Après l'importation :**
1. ✅ **Vérifiez les résultats** dans la liste des formules
2. ✅ **Sauvegardez votre fichier corrigé** pour référence future

## 🔧 Résolution Rapide

### **Erreurs les plus courantes :**

1. **"Colonne A (Date) est vide"**
   → Ajoutez une date dans la colonne A

2. **"Format de date invalide"**
   → Utilisez le format DD/MM/YYYY (ex: 15/01/2024) ou YYYY-MM-DD (ex: 2024-01-15)

3. **"Aucun champ de formule rempli"**
   → Remplissez au moins un champ de formule

4. **"Valeur 'X' invalide" dans Statut**
   → Utilisez seulement 0 ou 1

5. **"Erreur générale"**
   → Vérifiez que toutes les colonnes sont présentes

Avec ces informations détaillées, vous pouvez maintenant **identifier et corriger rapidement** toutes les erreurs dans votre fichier Excel ! 🎯
