# 📊 Explication : Gestion des Marges Jour et Nuit

## 📋 Vue d'ensemble

Cette fonctionnalité permet aux **RH** et **Administrateurs** de paramétrer directement les **marges jour** et **marges nuit** pour chaque formule, de manière indépendante et séparée.

---

## 🎯 Objectif

Permettre aux RH et Administrateurs de définir et modifier les marges jour et nuit pour chaque formule, sans dépendre d'une marge totale. Les marges sont gérées de manière totalement indépendante.

---

## 🔑 Concepts clés

### Marges indépendantes
- **MargeJourRestante** : Marge disponible pour la période Jour (midi), définie indépendamment
- **MargeNuitRestante** : Marge disponible pour la période Nuit (soir), définie indépendamment

### Différence avec l'ancien système
- **Ancien système** : Les marges étaient calculées à partir d'une marge totale (`Marge`) qui était ensuite répartie (50/50) entre jour et nuit
- **Nouveau système** : Les marges jour et nuit sont définies directement et indépendamment, sans calcul intermédiaire

---

## 🔐 Accès et autorisations

### Rôles autorisés
- ✅ **Administrateur**
- ✅ **RH** (Ressources Humaines)
- ❌ **PrestataireCantine** (non autorisé)
- ❌ **Employé** (non autorisé)

### Accès dans le menu
- **Menu** : Point Financier → Gestion des Marges
- **URL** : `/GestionMarges/Index`

---

## 🔄 Processus d'utilisation

### Étape 1 : Sélection de la période
1. Accéder à "Gestion des Marges" depuis le menu
2. Sélectionner une **Date de début** et une **Date de fin**
3. Cliquer sur "Charger les formules"

### Étape 2 : Visualisation des formules
Le système affiche toutes les formules de la période sélectionnée avec :
- **Date** de la formule
- **Type de formule** (Standard 1, Standard 2, Amélioré)
- **Plat** (nom du plat)
- **Quota Jour** (affiché en lecture seule, à titre informatif)
- **Marge Jour** (modifiable)
- **Quota Nuit** (affiché en lecture seule, à titre informatif)
- **Marge Nuit** (modifiable)

### Étape 3 : Modification des marges
1. Modifier les valeurs dans les champs "Marge Jour" et/ou "Marge Nuit" pour chaque formule
2. Les valeurs doivent être entre **0 et 1000**
3. Cliquer sur "Sauvegarder les marges"

### Étape 4 : Confirmation
- Un message de succès confirme le nombre de formules modifiées
- Les valeurs sont immédiatement sauvegardées dans la base de données
- Les marges sont disponibles pour le système de quotas

---

## 📊 Interface utilisateur

### Tableau des formules
Le tableau affiche :
- **Colonnes** :
  - Date
  - Type Formule
  - Plat
  - Quota Jour (lecture seule, badge gris)
  - **Marge Jour** (modifiable, fond jaune clair)
  - Quota Nuit (lecture seule, badge sombre)
  - **Marge Nuit** (modifiable, fond bleu clair)

### Validation
- **Valeurs acceptées** : Entre 0 et 1000
- **Validation côté client** : Les champs numériques empêchent les valeurs négatives
- **Validation côté serveur** : Vérification des valeurs avant sauvegarde

---

## 💾 Sauvegarde des données

### Champs modifiés
Lors de la sauvegarde, seuls les champs suivants sont modifiés :
- `MargeJourRestante` : Valeur saisie pour la marge jour
- `MargeNuitRestante` : Valeur saisie pour la marge nuit
- `ModifiedOn` : Date et heure de modification (UTC)
- `ModifiedBy` : Nom de l'utilisateur ayant effectué la modification

### Champs non modifiés
Les champs suivants ne sont **pas** modifiés :
- `QuotaJourRestant` : Reste inchangé
- `QuotaNuitRestant` : Reste inchangé
- `Marge` : Reste inchangé (ancien champ, conservé pour compatibilité)

---

## 🔄 Intégration avec le système de quotas

### Utilisation des marges
Les marges définies via cette interface sont utilisées par le système de quotas pour :
1. **Vérifier la disponibilité** avant de créer une commande instantanée
2. **Décrémenter les marges** lorsque les quotas sont épuisés
3. **Bloquer les commandes** lorsque les marges sont également épuisées

### Période Jour (avant 18h)
```
Total disponible = QuotaJourRestant + MargeJourRestante
```

### Période Nuit (à partir de 18h)
```
Total disponible = QuotaNuitRestant + MargeNuitRestante
```

---

## 📝 Exemple d'utilisation

### Scénario : Définir les marges pour la semaine prochaine

1. **Accès** : Menu → Point Financier → Gestion des Marges
2. **Période** : Sélectionner du lundi au dimanche de la semaine prochaine
3. **Chargement** : Cliquer sur "Charger les formules"
4. **Modification** :
   - Formule Standard 1 du lundi : Marge Jour = 5, Marge Nuit = 3
   - Formule Amélioré du mardi : Marge Jour = 2, Marge Nuit = 2
   - ... (pour toutes les formules de la semaine)
5. **Sauvegarde** : Cliquer sur "Sauvegarder les marges"
6. **Confirmation** : Message "Marges sauvegardées avec succès ! X formule(s) modifiée(s)."

### Résultat
- Les marges sont maintenant disponibles pour le système de quotas
- Les commandes instantanées peuvent utiliser ces marges lorsque les quotas sont épuisés
- Chaque formule a ses propres marges jour et nuit, indépendantes

---

## ⚠️ Notes importantes

1. **Indépendance des marges** :
   - Les marges jour et nuit sont totalement indépendantes
   - Une marge peut être à 0 tandis que l'autre est à 10
   - Aucune relation entre les deux marges

2. **Période par défaut** :
   - La période par défaut est la **semaine suivante** (du lundi au dimanche)
   - L'utilisateur peut sélectionner n'importe quelle période

3. **Modifications** :
   - Seules les formules modifiées sont sauvegardées
   - Les valeurs non modifiées restent inchangées
   - Un message indique le nombre de formules modifiées

4. **Historique** :
   - Chaque modification est enregistrée avec :
     - Date et heure de modification (`ModifiedOn`)
     - Utilisateur ayant effectué la modification (`ModifiedBy`)

5. **Compatibilité** :
   - L'ancien champ `Marge` (totale) est conservé pour compatibilité
   - Il n'est plus utilisé pour le calcul des marges jour/nuit
   - Les marges jour et nuit sont prioritaires

---

## 🔧 Aspects techniques

### Contrôleur
- **Fichier** : `Controllers/GestionMargesController.cs`
- **Actions** :
  - `Index()` (GET) : Affiche le formulaire de sélection de période
  - `ChargerFormules(GestionMargesViewModel)` (POST) : Charge les formules pour la période
  - `SauvegarderMarges(GestionMargesViewModel)` (POST) : Sauvegarde les marges modifiées

### ViewModel
- **Fichier** : `Models/ViewModels/GestionMargesViewModel.cs`
- **Classes** :
  - `GestionMargesViewModel` : Contient la période et la liste des formules
  - `FormuleMargeViewModel` : Contient les informations d'une formule avec ses marges

### Vue
- **Fichier** : `Views/GestionMarges/Index.cshtml`
- **Fonctionnalités** :
  - Formulaire de sélection de période
  - Tableau des formules avec champs modifiables
  - Validation côté client et serveur

---

## 🎯 Cas d'utilisation

### Cas 1 : Initialisation des marges pour une nouvelle période
- Définir les marges jour et nuit pour toutes les formules de la semaine prochaine
- Permet de préparer les marges avant le début de la période

### Cas 2 : Ajustement des marges en cours de période
- Modifier les marges si nécessaire (ex: augmentation de la demande)
- Les modifications sont immédiatement prises en compte

### Cas 3 : Correction d'erreur
- Corriger une marge mal définie
- Les modifications sont tracées dans l'historique

---

## 🔍 Différences avec l'ancien système

| Aspect | Ancien système | Nouveau système |
|--------|---------------|-----------------|
| **Gestion** | Marge totale répartie 50/50 | Marges jour et nuit indépendantes |
| **Interface** | PrestataireCantine | RH et Administrateur uniquement |
| **Flexibilité** | Limitée (répartition fixe) | Totale (valeurs indépendantes) |
| **Champs DB** | `Marge` (total) | `MargeJourRestante` et `MargeNuitRestante` |
| **Calcul** | Automatique (Marge / 2) | Manuel (saisie directe) |

---

## ✅ Avantages

1. **Flexibilité** : Marges jour et nuit totalement indépendantes
2. **Précision** : Valeurs définies directement, sans calcul intermédiaire
3. **Contrôle** : Gestion centralisée par les RH et Administrateurs
4. **Traçabilité** : Historique des modifications avec utilisateur et date
5. **Simplicité** : Interface claire et intuitive

