# 📊 Explication de la Vue "Point de Consommation CIT"

## 🎯 Vue d'ensemble

La vue **Point de Consommation CIT** (`https://localhost:7021/PointsConsommation/PointConsommationCIT`) est une interface administrative qui permet aux **Administrateurs** et aux **RH** de consulter et gérer les **nombres de repas consommés** par tous les utilisateurs CIT, classés par type de formule (Standard/Améliorée) et par statut (Consommée/Non Récupérée/Indisponible).

---

## 🔐 Accès et Autorisations

### Rôles autorisés
- ✅ **Administrateur**
- ✅ **RH** (Ressources Humaines)

### Accès refusé
- ❌ **Employé** (voit uniquement ses propres points via "Mes Points de Consommation")
- ❌ **PrestataireCantine**

### Action Index
- L'URL `/PointsConsommation` (sans action spécifique) redirige automatiquement :
  - **Admin/RH** → `PointConsommationCIT`
  - **Autres utilisateurs** → `MesPointsConsommation`

---

## 📋 Fonctionnalités principales

### 1. **Vue agrégée par utilisateur**

La vue regroupe tous les points de consommation par utilisateur et affiche :

#### Informations utilisateur
- **Matricule** : Identifiant unique de l'utilisateur
- **Nom & Prénoms** : Nom complet de l'utilisateur (trié par ordre alphabétique)

#### Nombres de repas par type et statut

**Formules Standard :**
- **Standard Consommée** : Nombre de repas standard effectivement consommés
- **Standard Non Récupérée** : Nombre de repas standard commandés mais non récupérés (ou facturés)
- **Standard Indisponible** : Nombre de repas standard commandés mais indisponibles (plats finis)

**Formules Améliorées :**
- **Améliorée Consommée** : Nombre de repas améliorés effectivement consommés
- **Améliorée Non Récupérée** : Nombre de repas améliorés commandés mais non récupérés (ou facturés)
- **Améliorée Indisponible** : Nombre de repas améliorés commandés mais indisponibles (plats finis)

#### Montant total
- **Montant Total** : Calculé selon la formule :
  ```
  MontantTotal = ((StandardNonRecuperee + StandardConsommee) × 550) + 
                 ((AmelioreeNonRecuperee + AmelioreeConsommee) × 2800)
  ```
  - **Standard** : 550 FCFA par unité
  - **Améliorée** : 2800 FCFA par unité

---

### 2. **Filtrage par période et utilisateur**

#### Filtres disponibles
- **Date de début** : Date de début de la période (par défaut : 30 jours avant aujourd'hui)
- **Date de fin** : Date de fin de la période (par défaut : aujourd'hui)
- **Matricule** : Recherche par matricule, nom ou prénom (avec autocomplete Select2)

#### Comportement
- Les points de consommation sont filtrés selon leur `DateConsommation`
- Seuls les points dans la période sélectionnée sont inclus dans les calculs
- Le filtre par utilisateur permet de rechercher un utilisateur spécifique

---

### 3. **Tri et organisation**

#### Ordre d'affichage
- **Par nom alphabétique** : Les utilisateurs sont triés par ordre alphabétique sur la colonne "Nom & Prénoms"
- Un indicateur visuel (icône `fa-sort-alpha-down`) indique le tri alphabétique

#### Pagination
- Tous les résultats sont affichés sur une seule page (pas de pagination pour l'instant)

---

### 4. **Actions disponibles**

#### Bouton "Synchroniser"
- **Action** : `SynchroniserFacturation`
- **Fonction** : Synchronise toutes les données de facturation pour la période sélectionnée
- **Confirmation** : Demande confirmation avant exécution

#### Bouton "Facturer"
- **Action** : `DeclencherFacturation`
- **Fonction** : Déclenche la facturation pour les commandes non récupérées de la période
- **Confirmation** : Demande confirmation avant exécution

#### Bouton "Export Excel"
- **Action** : `ExportExcelCIT`
- **Fonction** : Exporte les données au format Excel
- **Paramètres** : Inclut les dates de début et de fin sélectionnées

#### Bouton "Retour"
- **Action** : Retour vers la liste des points de consommation individuels

---

## 💰 Calcul des montants et quantités

### Méthode de calcul des quantités

Pour chaque point de consommation, la quantité est comptabilisée selon :

#### 1. **Points facturables**
Un point est facturable si :
- La commande associée a le statut **Consommée** (1)
- La commande associée a le statut **Non Récupérée** (6)
- La commande associée a le statut **Indisponible** (5)
- La commande associée a le statut **Précommandée** (0) mais le lieu de consommation contient "FACTURATION"

#### 2. **Classification par statut**

**Consommée** :
- Statut de la commande = `Consommee` (1)
- Le repas a été effectivement consommé

**Non Récupérée** :
- Statut de la commande = `NonRecuperer` (6)
- OU statut = `Precommander` (0) avec lieu de consommation contenant "FACTURATION"
- Le repas a été commandé mais non récupéré avant 23h59

**Indisponible** :
- Statut de la commande = `Indisponible` (5)
- Le repas a été commandé mais les plats étaient finis

#### 3. **Classification par type de formule**

**Standard** :
- Type de formule contient "standard" (Standard 1, Standard 2)
- Prix unitaire : **550 FCFA**

**Améliorée** :
- Type de formule contient "amélioré", "ameliore" ou "ameliorée"
- Prix unitaire : **2800 FCFA**

#### 4. **Calcul du Montant Total**

```
MontantTotal = ((StandardNonRecuperee + StandardConsommee) × 550) + 
               ((AmelioreeNonRecuperee + AmelioreeConsommee) × 2800)
```

**Note** : Les repas "Indisponible" ne sont **pas** inclus dans le calcul du montant total (ils ne sont pas facturés).

---

## 📊 Structure des données affichées

### Tableau principal

| Colonne | Description | Format |
|---------|-------------|--------|
| **Matricule** | Identifiant unique de l'utilisateur | Texte (badge bleu) |
| **Nom & Prénoms** | Nom complet de l'utilisateur (trié alphabétiquement) | Texte avec icône de tri |
| **Standard Consommée** | Nombre de repas standard consommés | Badge vert (nombre) |
| **Standard Non Récupérée** | Nombre de repas standard non récupérés | Badge jaune (nombre) |
| **Standard Indisponible** | Nombre de repas standard indisponibles | Badge bleu clair (nombre) |
| **Améliorée Consommée** | Nombre de repas améliorés consommés | Badge vert (nombre) |
| **Améliorée Non Récupérée** | Nombre de repas améliorés non récupérés | Badge jaune (nombre) |
| **Améliorée Indisponible** | Nombre de repas améliorés indisponibles | Badge bleu clair (nombre) |
| **Montant Total** | Montant total facturable | Badge sombre (N0 FCFA) |

### Structure des en-têtes

Les en-têtes sont organisés en deux niveaux :
- **Niveau 1** : Groupement par type (Standard / Améliorée)
- **Niveau 2** : Détail par statut (Consommée / Non Récupérée / Indisponible)

---

## 🔄 Flux de données

### 1. **Récupération des utilisateurs**
```
Tous les utilisateurs actifs (Supprimer = 0)
    ↓
Filtrage par matricule (si fourni)
    ↓
Sélection des informations : Id, Nom, Prénoms, Email, UserName
```

### 2. **Récupération des points de consommation**
```
Points de consommation pour la période
    ↓
Filtrage : Supprimer = 0, DateConsommation dans la période
    ↓
Inclusion des relations : Utilisateur, Commande, FormuleJour
```

### 3. **Groupement et calcul**
```
Pour chaque utilisateur :
    ↓
Points de consommation de l'utilisateur
    ↓
Classification par type (Standard/Améliorée) et statut (Consommée/Non Récupérée/Indisponible)
    ↓
Calcul des quantités et du montant total
    ↓
Création du ViewModel avec les données agrégées
```

### 4. **Tri et affichage**
```
Tri par nom alphabétique (UtilisateurNomComplet)
    ↓
Affichage dans le tableau
```

---

## 🎨 Interface utilisateur

### Design
- **Couleurs** :
  - Standard/Améliorée Consommée : Badge vert (`bg-success`)
  - Standard/Améliorée Non Récupérée : Badge jaune (`bg-warning`)
  - Standard/Améliorée Indisponible : Badge bleu clair (`bg-info`)
  - Montant Total : Badge sombre (`bg-dark`)

### Responsive
- Tableau responsive avec scroll horizontal si nécessaire
- Filtres adaptatifs selon la taille d'écran

### Interactions
- Hover sur les lignes du tableau
- Boutons avec effets visuels au survol
- Confirmation avant actions critiques (Synchroniser, Facturer)
- **Recherche par utilisateur** : Select2 avec autocomplete (minimum 2 caractères)

---

## ⚠️ Points importants

### 1. **Période par défaut**
- Si aucune période n'est spécifiée, la vue affiche les **30 derniers jours** par défaut

### 2. **Utilisateurs sans consommation**
- Tous les utilisateurs actifs sont affichés, même ceux sans point de consommation
- Les utilisateurs sans consommation auront toutes les quantités à **0** et un montant total de **0 FCFA**

### 3. **Facturation automatique**
- Les points de facturation sont créés automatiquement par le `FacturationAutomatiqueService`
- Le lieu de consommation contient "FACTURATION" pour ces points
- Ces points sont comptabilisés comme "Non Récupérée"

### 4. **Calcul des montants**
- Seuls les repas **Consommés** et **Non Récupérés** sont facturés
- Les repas **Indisponibles** ne sont **pas** facturés (montant = 0)
- Les prix sont fixes :
  - **Standard** : 550 FCFA
  - **Améliorée** : 2800 FCFA

### 5. **Tri alphabétique**
- Le tableau est trié par ordre alphabétique sur la colonne "Nom & Prénoms"
- Un indicateur visuel (icône) indique le tri actif

### 6. **Filtre par utilisateur**
- Recherche par matricule, nom ou prénom
- Utilise Select2 pour une recherche avec autocomplete
- Minimum 2 caractères requis pour lancer la recherche
- Les résultats sont limités à 20 utilisateurs

---

## 🔍 Cas d'utilisation

### 1. **Consultation des consommations par utilisateur**
Un administrateur veut voir combien de repas chaque employé a consommé sur le mois dernier, classés par type et statut.

### 2. **Vérification des non-récupérations**
L'administrateur consulte les repas non récupérés pour identifier les utilisateurs qui doivent être facturés.

### 3. **Préparation de la facturation**
Avant de facturer, l'administrateur consulte les montants totaux pour vérifier les données et calculer les montants dus.

### 4. **Export pour comptabilité**
L'administrateur exporte les données en Excel pour les transmettre au service comptable avec les détails par utilisateur.

### 5. **Recherche d'un utilisateur spécifique**
L'administrateur utilise le filtre par matricule pour trouver rapidement les consommations d'un utilisateur particulier.

---

## 📝 Notes techniques

### Contrôleur
- **Méthode Index** : `Index()` - Redirige vers la vue appropriée selon le rôle
- **Méthode principale** : `PointConsommationCIT(DateTime? dateDebut, DateTime? dateFin, string? matricule = null)`
- **Autorisation** : `[Authorize(Roles = "Administrateur,RH")]`
- **Méthode de recherche** : `SearchUsersByMatricule(string term)` - Pour l'autocomplete Select2
- **Logique** : Groupement par utilisateur, classification par type et statut, calcul des quantités et montants, tri alphabétique

### ViewModel
- **Type** : `PagedViewModel<PointConsommationCITViewModel>`
- **Propriétés principales** :
  - `UtilisateurId`, `UtilisateurNom`, `UtilisateurPrenoms`, `UtilisateurNomComplet`
  - `Matricule`, `Email`
  - `StandardConsommee`, `StandardNonRecuperee`, `StandardIndisponible` (int)
  - `AmelioreeConsommee`, `AmelioreeNonRecuperee`, `AmelioreeIndisponible` (int)
  - `Total` (int) - Total des quantités
  - `MontantTotal` (decimal) - Montant total facturable

### Performance
- Les requêtes utilisent `Include` pour charger les relations nécessaires
- Le groupement et le calcul sont effectués en mémoire après récupération des données
- Le filtre par utilisateur est appliqué au niveau de la base de données pour optimiser les performances

### JavaScript
- **Select2** : Utilisé pour la recherche d'utilisateurs avec autocomplete
- **AJAX** : Appels asynchrones pour charger les utilisateurs au fur et à mesure de la saisie
- **Minimum 2 caractères** : Requis pour lancer la recherche

---

## 🚀 Actions futures possibles

- Pagination pour gérer un grand nombre d'utilisateurs
- Export PDF en plus de l'export Excel
- Graphiques de visualisation des consommations
- Détails par utilisateur (clic pour voir les points individuels)
- Historique des facturations
- Filtres supplémentaires (par département, par type de formule, etc.)

---

## 📚 Voir aussi

- [Explication des Points de Consommation](./EXPLICATION_POINTS_CONSOMMATION.md)
- [Système de Facturation](./Services/FacturationService.cs)
- [Service de Facturation Automatique](./Services/FacturationAutomatiqueService.cs)
- [Service de Changement de Statut Automatique](./Services/ChangementStatutAutomatiqueService.cs)
