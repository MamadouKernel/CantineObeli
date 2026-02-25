# Migration vers le Nouveau Format d'Importation des Menus

## Vue d'Ensemble

Le système d'importation des menus a été simplifié pour réduire le nombre de lignes nécessaires par semaine.

### Ancien Format (Avant)
- **21 lignes** pour une semaine complète
- 1 ligne = 1 formule
- 3 formules par jour × 7 jours = 21 lignes
- Colonnes : Date, NomFormule, Entree, Plat, Garniture, Dessert, PlatStandard1, GarnitureStandard1, PlatStandard2, GarnitureStandard2, Feculent, Legumes, Marge, Statut

### Nouveau Format (Maintenant)
- **7 lignes** pour une semaine complète
- 1 ligne = 1 jour complet (toutes les formules)
- Le système crée automatiquement les 3 formules par jour
- Colonnes : Date, Entree, Dessert, Plat, Garniture, Feculent, Legumes, Plat standard 1, Garniture standard 1, Plat standard 2, Garniture standard 2

## Comparaison Visuelle

### Ancien Format (21 lignes)
```
Date       | NomFormule          | Entree              | Plat                | ...
02/02/2026 | Formule Améliorée   | Salade de Crudités  | Filet de Sosso      | ...
02/02/2026 | Formule Standard 1  |                     |                     | ...
02/02/2026 | Formule Standard 2  |                     |                     | ...
03/02/2026 | Formule Améliorée   | Salade Verdurette   | Gratin de Cabillaud | ...
03/02/2026 | Formule Standard 1  |                     |                     | ...
03/02/2026 | Formule Standard 2  |                     |                     | ...
... (15 lignes de plus)
```

### Nouveau Format (7 lignes)
```
Date       | Entree              | Dessert | Plat                | Garniture           | ... | Plat standard 1    | Garniture standard 1 | Plat standard 2         | Garniture standard 2
02/02/2026 | Salade de Crudités  | Yaourt  | Filet de Sosso      | Pois Chiches Sautés | ... | Lasagne Bolognaise | Salade Verte         | Soupe de Poulet         | Riz Blanc
03/02/2026 | Salade Verdurette   | Brownie | Gratin de Cabillaud | Pommes de Terre     | ... | APF                | Attiéké              | Bœuf Sauce Bawin        | Riz Blanc
... (5 lignes de plus)
```

## Changements Techniques

### Colonnes Supprimées
- ❌ **NomFormule** : Plus nécessaire, les noms sont automatiques
  - "Formule Améliorée"
  - "Formule Standard 1"
  - "Formule Standard 2"
- ❌ **Marge** : Définie séparément (pas à l'import)
- ❌ **Statut** : Toujours "Actif" par défaut

### Colonnes Réorganisées
L'ordre des colonnes a été optimisé pour correspondre au flux naturel d'un menu :

**Formule Améliorée (colonnes B-G)** :
1. Entrée
2. Dessert
3. Plat
4. Garniture
5. Féculent
6. Légumes

**Formule Standard 1 (colonnes H-I)** :
7. Plat standard 1
8. Garniture standard 1

**Formule Standard 2 (colonnes J-K)** :
9. Plat standard 2
10. Garniture standard 2

### Logique de Création Automatique

Le système crée automatiquement les formules selon les règles suivantes :

```csharp
// Formule Améliorée créée si au moins un champ est rempli
if (Entree OU Dessert OU Plat OU Garniture OU Feculent OU Legumes)
{
    Créer "Formule Améliorée" avec ces champs
}

// Formule Standard 1 créée si au moins un champ est rempli
if (PlatStandard1 OU GarnitureStandard1)
{
    Créer "Formule Standard 1" avec ces champs
}

// Formule Standard 2 créée si au moins un champ est rempli
if (PlatStandard2 OU GarnitureStandard2)
{
    Créer "Formule Standard 2" avec ces champs
}
```

## Migration de Vos Fichiers Existants

Si vous avez des fichiers Excel avec l'ancien format, voici comment les convertir :

### Option 1 : Conversion Manuelle (Recommandée)

1. **Téléchargez le nouveau modèle** depuis l'interface d'importation
2. **Groupez vos données par date** dans l'ancien fichier
3. **Copiez les données** ligne par ligne :
   - Pour chaque date, prenez les 3 lignes (3 formules)
   - Consolidez-les en 1 seule ligne dans le nouveau format
   - Colonnes B-G : Données de "Formule Améliorée"
   - Colonnes H-I : Données de "Formule Standard 1"
   - Colonnes J-K : Données de "Formule Standard 2"

### Option 2 : Script de Conversion (Pour les Développeurs)

Un script PowerShell peut être créé pour automatiser la conversion :

```powershell
# Exemple de logique de conversion
# Lire l'ancien fichier Excel
# Grouper par Date
# Pour chaque date :
#   - Extraire les 3 formules
#   - Créer 1 ligne consolidée
#   - Écrire dans le nouveau format
```

## Avantages du Nouveau Format

### 1. Simplicité
- ✅ 70% moins de lignes (7 au lieu de 21)
- ✅ Plus facile à lire et comprendre
- ✅ Moins de risques d'erreurs de saisie

### 2. Rapidité
- ✅ Import plus rapide (moins de lignes à traiter)
- ✅ Saisie plus rapide (1 ligne au lieu de 3)
- ✅ Validation plus rapide

### 3. Maintenance
- ✅ Fichiers Excel plus petits
- ✅ Moins de duplication de données
- ✅ Format plus intuitif

### 4. Flexibilité
- ✅ Pas besoin de créer des lignes vides
- ✅ Les formules vides ne sont pas créées
- ✅ Économie d'espace en base de données

## Compatibilité

### Rétrocompatibilité
- ❌ L'ancien format n'est **plus supporté**
- ⚠️ Les fichiers existants doivent être convertis
- ✅ Les données déjà importées restent inchangées

### Données Existantes
- ✅ Toutes les formules déjà en base de données sont conservées
- ✅ Aucune migration de données nécessaire
- ✅ Seul le format d'import change

## Guide de Démarrage Rapide

### Étape 1 : Télécharger le Modèle
1. Allez dans **Formules du Jour** > **Importer**
2. Cliquez sur **Télécharger le modèle**
3. Ouvrez le fichier `modele_import_menus.xlsx`

### Étape 2 : Remplir le Fichier
1. Gardez la ligne d'en-tête (ligne 1)
2. Remplissez une ligne par jour
3. Colonne A : Date obligatoire (format DD/MM/YYYY)
4. Colonnes B-K : Remplissez selon vos besoins

### Étape 3 : Importer
1. Retournez dans **Formules du Jour** > **Importer**
2. Sélectionnez votre fichier
3. Cochez "Remplacer les formules existantes" si nécessaire
4. Cliquez sur **Importer**

### Étape 4 : Vérifier
1. Allez dans **Formules du Jour** > **Liste**
2. Vérifiez que les 3 formules ont été créées pour chaque jour
3. Vérifiez les données importées

## Support et Aide

### Documentation
- 📄 `Scripts/Exemple_Import_Menu_Semaine_Nouveau_Format.md` : Exemples détaillés
- 📄 Feuille "Instructions" dans le modèle Excel : Guide complet

### En Cas de Problème
1. Vérifiez le format de la date (DD/MM/YYYY)
2. Assurez-vous qu'au moins un champ est rempli par ligne
3. Consultez les messages d'erreur détaillés après l'import
4. Téléchargez un nouveau modèle si nécessaire

### Messages d'Erreur Courants

**"Colonne A (Date) est vide"**
- Solution : Remplissez la date au format DD/MM/YYYY

**"Aucun champ de formule rempli"**
- Solution : Remplissez au moins un champ (B à K)

**"Format de date invalide"**
- Solution : Utilisez DD/MM/YYYY (ex: 02/02/2026)

**"Des formules existent déjà pour la date"**
- Solution : Cochez "Remplacer les formules existantes"

## Conclusion

Le nouveau format simplifie grandement l'importation des menus tout en conservant toutes les fonctionnalités. La transition est simple et les avantages sont immédiats.

**Résumé** :
- 7 lignes au lieu de 21 pour une semaine
- Format plus intuitif et plus rapide
- Création automatique des 3 formules par jour
- Téléchargez le nouveau modèle pour commencer
