# Nouveau Format d'Importation des Menus

## Changement Important

Le format d'importation a été simplifié :
- **AVANT** : 21 lignes pour une semaine (3 formules × 7 jours)
- **MAINTENANT** : 7 lignes pour une semaine (1 ligne = 1 jour complet)

## Structure du Fichier Excel

### En-têtes (Ligne 1)
| A | B | C | D | E | F | G | H | I | J | K |
|---|---|---|---|---|---|---|---|---|---|---|
| Date | Entree | Dessert | Plat | Garniture | Feculent | Legumes | Plat standard 1 | Garniture standard 1 | Plat standard 2 | Garniture standard 2 |

### Exemple de Données (Semaine du 02/02/2026 au 08/02/2026)

**Ligne 2 - Lundi 02/02/2026**
```
02/02/2026 | Salade de Crudités | Yaourt | Filet de Sosso au Four | Pois Chiches Sautés | | | Lasagne Bolognaise | Salade Verte | Soupe de Poulet | Riz Blanc
```

**Ligne 3 - Mardi 03/02/2026**
```
03/02/2026 | Salade Verdurette | Brownie | Gratin de Cabillaud | Pommes de Terre Vapeur | | | APF | Attiéké | Bœuf Sauce Bawin | Riz Blanc
```

**Ligne 4 - Mercredi 04/02/2026**
```
04/02/2026 | Friand au Fromage | Beignet Nature | Émincé de Bœuf à La Moutarde | Riz Safrané | | | Poulet au Four | Pommes de Terre Sautées | Poisson Fumé Sauce Gouagouassou | Riz Blanc
```

**Ligne 5 - Jeudi 05/02/2026**
```
05/02/2026 | Salade Composée | Gâteau Semoule Raisins | Lapin aux Pruneaux | Purée de Patates Douces | | | Choukouya de Bœuf | Attiéké | Akpéssi de Banane au Poulet | Banane Plantain
```

**Ligne 6 - Vendredi 06/02/2026**
```
06/02/2026 | Mini Quiche Légumes | Salade de Fruits Maison | Chili Con Carne Doux | Riz Blanc | | | Poisson Frit Abolo | Abolo | Bœuf Sauce Pistache | Riz
```

**Ligne 7 - Samedi 07/02/2026**
```
07/02/2026 | Cocktail de Crudités | Pain Perdu | Colombo de Poulet | Couscous | | | Poulet à L'Ivoirienne | Attiéké | Poisson Frit Sauce Feuilles | Riz Blanc
```

**Ligne 8 - Dimanche 08/02/2026**
```
08/02/2026 | Œufs Brouillés aux Légumes | Moka Café | Saumon Grillé | Patates Douces Rôties | | | Chicken Burger | Pommes de Terre Sautées | Poulet Fumé Sauce Doumglé | Riz Blanc
```

## Comment ça Fonctionne

Pour chaque ligne (jour), le système crée automatiquement **jusqu'à 3 formules** :

1. **Formule Améliorée** (colonnes B-G) :
   - Entrée, Dessert, Plat, Garniture, Féculent, Légumes
   - Créée si au moins un de ces champs est rempli

2. **Formule Standard 1** (colonnes H-I) :
   - Plat standard 1, Garniture standard 1
   - Créée si au moins un de ces champs est rempli

3. **Formule Standard 2** (colonnes J-K) :
   - Plat standard 2, Garniture standard 2
   - Créée si au moins un de ces champs est rempli

## Règles d'Importation

✅ **Obligatoire** :
- Colonne A (Date) : Format DD/MM/YYYY (ex: 02/02/2026)
- Au moins un champ de formule doit être rempli par ligne

✅ **Optionnel** :
- Tous les autres champs peuvent être vides
- Les formules vides ne sont pas créées

✅ **Formats de date acceptés** :
- DD/MM/YYYY (ex: 02/02/2026)
- YYYY-MM-DD (ex: 2026-02-02)
- DD-MM-YYYY (ex: 02-02-2026)

## Avantages du Nouveau Format

1. **Plus Simple** : 7 lignes au lieu de 21 pour une semaine
2. **Plus Rapide** : Toutes les infos d'un jour sur une seule ligne
3. **Moins d'Erreurs** : Format plus compact et lisible
4. **Plus Flexible** : Pas besoin de créer des lignes vides

## Télécharger le Modèle

Pour télécharger un modèle Excel pré-rempli avec des exemples :
1. Allez dans **Formules du Jour** > **Importer**
2. Cliquez sur **Télécharger le modèle**
3. Le fichier contient des exemples et des instructions détaillées

## Notes Importantes

⚠️ **Attention** :
- Les marges ne sont PAS définies à l'import (à faire séparément)
- Toutes les formules sont créées avec le statut "Actif"
- Cochez "Remplacer les formules existantes" pour écraser les menus existants
- Les formules vides ne sont pas créées (économie d'espace en base de données)
