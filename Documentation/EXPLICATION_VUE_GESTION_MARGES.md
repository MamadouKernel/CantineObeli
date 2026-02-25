# 📊 Explication : Vue "Gestion des Marges Jour et Nuit"

## 📋 Vue d'ensemble

Cette vue permet aux **RH** et **Administrateurs** de paramétrer les **marges jour** et **marges nuit** pour chaque formule sur une période donnée. C'est une interface dédiée à la gestion des marges, distincte de la gestion des quotas.

**URL** : `https://localhost:7021/GestionMarges/ChargerFormules`

---

## 🎯 Objectif de la vue

Permettre aux RH et Administrateurs de :
1. **Visualiser** toutes les formules d'une période
2. **Voir** les quotas existants (à titre informatif)
3. **Définir/modifier** les marges jour et nuit pour chaque formule
4. **Sauvegarder** les modifications

---

## 🔄 Processus d'utilisation

### Étape 1 : Sélection de la période
1. **Date de début** : Sélectionner la date de début de la période (ex: 22/12/2025)
2. **Date de fin** : Sélectionner la date de fin de la période (ex: 28/12/2025)
3. **Bouton "Charger les formules"** : Cliquer pour charger toutes les formules de cette période

### Étape 2 : Visualisation du tableau
Le système affiche un tableau avec toutes les formules de la période sélectionnée.

### Étape 3 : Modification des marges
1. Modifier les valeurs dans les champs "Marge Jour" et/ou "Marge Nuit"
2. Les valeurs doivent être entre **0 et 1000**
3. Cliquer sur **"Sauvegarder les marges"** en bas du tableau

---

## 📊 Explication du tableau

### Structure du tableau

Le tableau contient **7 colonnes** :

#### 1. **Date** 📅
- **Type** : Lecture seule (affichage)
- **Contenu** : Date de la formule (format : dd/MM/yyyy)
- **Exemple** : "22/12/2025"
- **Utilité** : Identifier à quel jour correspond chaque formule

#### 2. **Type Formule** 🍽️
- **Type** : Lecture seule (badge)
- **Contenu** : Type de formule (Standard 1, Standard 2, Amélioré)
- **Affichage** : Badge orange avec le nom de la formule
- **Exemple** : "Formule Améliorée", "Formule Standard 1", "Formule Standard 2"
- **Utilité** : Identifier le type de formule

#### 3. **Plat** 🍗
- **Type** : Lecture seule (affichage)
- **Contenu** : Nom du plat principal de la formule
- **Exemple** : "Poulet rôti aux herbes", "Sauce graine", "Attiéké"
- **Utilité** : Identifier le plat proposé

#### 4. **Quota Jour** ☀️
- **Type** : **Lecture seule** (badge gris/bleu)
- **Contenu** : Nombre de plats disponibles dans le quota jour (pour le midi)
- **Affichage** : Badge circulaire avec la valeur
- **Exemple** : "0" (dans votre capture d'écran)
- **Utilité** : **Information uniquement** - Affiche le quota jour actuel
- **⚠️ Important** : Cette valeur n'est **pas modifiable** dans cette vue. Elle est gérée ailleurs (par le PrestataireCantine lors de la génération des commandes).

#### 5. **Marge Jour** ☀️ (fond jaune clair)
- **Type** : **Éditable** (champ de saisie)
- **Contenu** : Nombre de plats supplémentaires disponibles pour la période Jour (midi)
- **Affichage** : Champ numérique avec fond jaune clair
- **Valeurs** : Entre 0 et 1000
- **Exemple** : "5", "10" (dans votre capture d'écran)
- **Utilité** : **C'est ici que vous définissez la marge jour** pour chaque formule
- **Fond jaune** : Indique que c'est un champ modifiable pour la période Jour

#### 6. **Quota Nuit** 🌙
- **Type** : **Lecture seule** (badge sombre/noir)
- **Contenu** : Nombre de plats disponibles dans le quota nuit (pour le soir)
- **Affichage** : Badge circulaire sombre avec la valeur
- **Exemple** : "0" (dans votre capture d'écran)
- **Utilité** : **Information uniquement** - Affiche le quota nuit actuel
- **⚠️ Important** : Cette valeur n'est **pas modifiable** dans cette vue. Elle est gérée ailleurs.

#### 7. **Marge Nuit** 🌙 (fond bleu clair)
- **Type** : **Éditable** (champ de saisie)
- **Contenu** : Nombre de plats supplémentaires disponibles pour la période Nuit (soir)
- **Affichage** : Champ numérique avec fond bleu clair
- **Valeurs** : Entre 0 et 1000
- **Exemple** : "0", "5" (dans votre capture d'écran)
- **Utilité** : **C'est ici que vous définissez la marge nuit** pour chaque formule
- **Fond bleu** : Indique que c'est un champ modifiable pour la période Nuit

---

## 🔍 Analyse de votre capture d'écran

D'après votre capture d'écran, voici ce que l'on peut observer :

### Période sélectionnée
- **Date de début** : 22/12/2025
- **Date de fin** : 28/12/2025
- **Période** : Une semaine (du lundi au dimanche)

### Exemples de lignes du tableau

#### Ligne 1 : Formule Améliorée du 22/12/2025
- **Plat** : "Poulet rôti aux herbes"
- **Quota Jour** : 0 (lecture seule)
- **Marge Jour** : 5 (éditable) ✅
- **Quota Nuit** : 0 (lecture seule)
- **Marge Nuit** : 0 (éditable)

#### Ligne 2 : Formule Standard 1 du 22/12/2025
- **Plat** : "Sauce graine"
- **Quota Jour** : 0 (lecture seule)
- **Marge Jour** : 10 (éditable) ✅
- **Quota Nuit** : 0 (lecture seule)
- **Marge Nuit** : 5 (éditable) ✅

#### Ligne 3 : Formule Standard 2 du 22/12/2025
- **Plat** : "Attiéké"
- **Quota Jour** : 0 (lecture seule)
- **Marge Jour** : 10 (éditable) ✅
- **Quota Nuit** : 0 (lecture seule)
- **Marge Nuit** : 5 (éditable) ✅

---

## ❓ Pourquoi les quotas sont à 0 ?

Dans votre capture d'écran, tous les quotas (Jour et Nuit) sont à **0**. Cela peut signifier :

1. **Les quotas n'ont pas encore été définis** pour ces formules
2. **Les quotas ont été épuisés** (tous les plats ont été utilisés)
3. **Les quotas sont gérés ailleurs** (par le PrestataireCantine lors de la génération des commandes)

### ⚠️ Important
- Les quotas sont **gérés par le PrestataireCantine** lors de la génération des commandes
- Cette vue est **uniquement pour les marges**, pas pour les quotas
- Les quotas sont affichés **à titre informatif** pour voir l'état actuel

---

## ✅ Ce que vous pouvez faire dans cette vue

### ✅ Modifiable
- **Marge Jour** : Définir le nombre de plats supplémentaires pour le midi
- **Marge Nuit** : Définir le nombre de plats supplémentaires pour le soir

### ❌ Non modifiable (affichage uniquement)
- **Date** : Affichage uniquement
- **Type Formule** : Affichage uniquement
- **Plat** : Affichage uniquement
- **Quota Jour** : Affichage uniquement (géré ailleurs)
- **Quota Nuit** : Affichage uniquement (géré ailleurs)

---

## 🎯 Cas d'utilisation

### Scénario 1 : Définir les marges pour la semaine prochaine
1. Sélectionner la période (ex: du 22/12 au 28/12)
2. Cliquer sur "Charger les formules"
3. Pour chaque formule, définir :
   - **Marge Jour** : Exemple 5 pour les formules améliorées, 10 pour les standards
   - **Marge Nuit** : Exemple 0 pour certaines, 5 pour d'autres
4. Cliquer sur "Sauvegarder les marges"

### Scénario 2 : Ajuster les marges en cours de période
1. Sélectionner la période actuelle
2. Charger les formules
3. Modifier les marges si nécessaire (ex: augmenter la marge jour de 5 à 10)
4. Sauvegarder

---

## 💡 Exemple concret d'utilisation

### Situation
Vous voulez définir les marges pour la semaine du 22/12 au 28/12 :

1. **Formule Améliorée du 22/12** :
   - Marge Jour : 5 (5 plats supplémentaires pour le midi)
   - Marge Nuit : 0 (pas de marge pour le soir)

2. **Formule Standard 1 du 22/12** :
   - Marge Jour : 10 (10 plats supplémentaires pour le midi)
   - Marge Nuit : 5 (5 plats supplémentaires pour le soir)

3. **Formule Standard 2 du 22/12** :
   - Marge Jour : 10 (10 plats supplémentaires pour le midi)
   - Marge Nuit : 5 (5 plats supplémentaires pour le soir)

### Résultat
- Les marges sont sauvegardées dans la base de données
- Elles seront utilisées par le système de quotas lorsque les quotas seront épuisés
- Les commandes instantanées pourront utiliser ces marges si nécessaire

---

## 🔄 Différence avec la gestion des quotas

| Aspect | Gestion des Quotas | Gestion des Marges (cette vue) |
|--------|-------------------|--------------------------------|
| **Géré par** | PrestataireCantine | RH / Administrateur |
| **Quand** | Lors de la génération des commandes | Avant ou pendant la période |
| **Objectif** | Définir les plats principaux | Définir les plats supplémentaires |
| **Modifiable ici** | ❌ Non | ✅ Oui (marges uniquement) |
| **Affichage** | Badge (lecture seule) | Champ de saisie (éditable) |

---

## 📝 Points importants à retenir

1. **Cette vue est uniquement pour les marges**, pas pour les quotas
2. **Les quotas sont affichés à titre informatif** (lecture seule)
3. **Seules les marges jour et nuit sont modifiables** dans cette vue
4. **Les marges sont indépendantes** : vous pouvez avoir Marge Jour = 10 et Marge Nuit = 0
5. **Les valeurs sont entre 0 et 1000**
6. **Les modifications sont sauvegardées immédiatement** après validation

---

## 🎨 Codes couleur dans le tableau

- **Fond jaune clair** (Marge Jour) : Indique que c'est un champ modifiable pour la période Jour
- **Fond bleu clair** (Marge Nuit) : Indique que c'est un champ modifiable pour la période Nuit
- **Badge gris/bleu** (Quota Jour) : Indique que c'est une valeur en lecture seule
- **Badge sombre/noir** (Quota Nuit) : Indique que c'est une valeur en lecture seule

---

## ✅ Résumé

Cette vue permet aux RH et Administrateurs de :
- ✅ **Voir** toutes les formules d'une période
- ✅ **Voir** les quotas actuels (information)
- ✅ **Définir** les marges jour pour chaque formule
- ✅ **Définir** les marges nuit pour chaque formule
- ✅ **Sauvegarder** les modifications

Les quotas sont affichés à titre informatif mais ne sont **pas modifiables** dans cette vue. Ils sont gérés par le PrestataireCantine lors de la génération des commandes.

