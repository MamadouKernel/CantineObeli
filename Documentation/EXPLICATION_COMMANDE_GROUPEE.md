# 👥 Explication de la Commande Groupée

## 📋 Vue d'ensemble

La **commande groupée** est une fonctionnalité qui permet de créer des commandes de repas pour des **visiteurs de CIT** avec une **quantité variable** (1 à 1000 plats). Contrairement aux commandes normales qui sont individuelles, la commande groupée permet de commander plusieurs plats en une seule fois pour un groupe de visiteurs.

## 🎯 Objectif

Cette fonctionnalité est destinée aux **Administrateurs et RH** pour gérer les commandes de groupes de visiteurs qui nécessitent plusieurs plats pour une même date.

---

## 👥 Qui peut créer une commande groupée ?

### Rôles autorisés :
- ✅ **Administrateur** (Admin)
- ✅ **RH** (Ressources Humaines)

### Rôles non autorisés :
- ❌ **Employé** - ne peut pas créer de commandes groupées

---

## 🔑 Caractéristiques principales

### 1. **Type de client**
- **Uniquement pour les visiteurs de CIT**
- Pas de commandes groupées pour les employés individuels
- Pas de commandes groupées pour les groupes non-CIT (Douaniers, etc.)

### 2. **Quantité variable**
- Quantité : **1 à 1000 plats** par commande
- Permet de commander pour plusieurs visiteurs en une seule fois
- Calcul automatique du montant selon la quantité

### 3. **Date de consommation**
- Date sélectionnable librement (pas de restriction de 48h)
- Peut être pour aujourd'hui ou une date future
- Pas de limitation à la semaine N+1

### 4. **Marqueur**
- `Instantanee = false` (ce n'est pas une commande instantanée)
- `TypeClient = Visiteur`
- `VisiteurNom` : Nom du groupe de visiteurs
- `VisiteurTelephone` : Téléphone (optionnel)

---

## 📝 Processus de création

### Étape 1 : Accès au formulaire
```
Menu → Commandes → Commande Groupée
```

### Étape 1 : Sélection du département
- Sélection du **département** d'origine des visiteurs (obligatoire)

### Étape 2 : Nombre de visiteurs
- Saisie du **nombre de visiteurs** (1 à 1000) (obligatoire)

### Étape 3 : Sélection de la période
- Sélection de la **période de service** : Jour ou Nuit (obligatoire)

### Étape 4 : Sélection de la formule
- Sélection d'une **formule améliorée** parmi celles disponibles pour la date choisie
- **Important** : Seules les formules améliorées sont disponibles pour les visiteurs (obligatoire)

### Informations optionnelles
- Saisie du **nom du groupe de visiteurs** (ex: "Groupe RH - 10 personnes") (optionnel)
- Saisie du **téléphone** (optionnel)

### Étape 3 : Configuration de la date
- Sélection de la **date de consommation**
- Délai minimum de **48h à l'avance** (obligatoire)
- Format : Date au format calendrier

### Étape 4 : Sélection de la formule améliorée
- Choix d'une **formule améliorée** parmi celles disponibles pour la date choisie
- **Important** : Seules les formules améliorées sont disponibles pour les visiteurs
- Les formules standard ne sont pas proposées
- Affichage des détails de chaque formule améliorée

### Configuration optionnelle
- **Nom du groupe de visiteurs** : Identification du groupe (optionnel)
- **Téléphone** : Contact (optionnel)
- **Site** : CIT Terminal ou CIT Billing (optionnel)
- **Commentaires** : Notes optionnelles (max 500 caractères)

### Validation et création
- Vérification que le département existe
- Vérification que la formule existe et est améliorée
- Vérification du délai de 48h minimum
- Création de la commande groupée

---

## ⚠️ Règles et validations

### 1. **Champs obligatoires**
- Le **département** est obligatoire
- Le **nombre de visiteurs** est obligatoire (1 à 1000)
- La **période de service** est obligatoire (Jour ou Nuit)
- La **date de consommation** est obligatoire (minimum 48h à l'avance)
- Une **formule améliorée** doit être sélectionnée

### 2. **Formule améliorée obligatoire**
- Seules les **formules améliorées** sont disponibles pour les visiteurs
- Les formules standard sont exclues
- La formule doit exister et être active
- Vérification que la formule est disponible pour la date choisie

### 3. **Nombre de visiteurs**
- Minimum : **1 visiteur**
- Maximum : **1000 visiteurs**
- Validation automatique de la plage
- Le nombre de visiteurs détermine le nombre de plats à commander

### 4. **Délai de commande pour visiteurs**
- Les commandes pour visiteurs doivent être créées **au moins 48h à l'avance**
- Vérification automatique du délai
- Si délai insuffisant → ❌ Refusé

---

## 💰 Calcul du montant

### Formule de calcul
```
Montant total = Quantité × Prix unitaire
```

### Prix unitaires
- **Formule Améliorée** : 2 800 FCFA par plat
- **Formule Standard** : 550 FCFA par plat

### Exemple
```
Groupe : Douaniers
Formule : Formule Standard 1
Quantité : 50 plats
Période : Jour

Calcul : 50 × 550 = 27 500 FCFA
```

---

## 📊 Différences avec les autres types de commandes

| Caractéristique | Commande Normale | Commande Instantanée | Commande Groupée |
|----------------|------------------|----------------------|------------------|
| **Type de client** | Employés CIT | Employés CIT | Visiteurs CIT |
| **Quantité** | 1 (fixe) | 1 (fixe) | 1-1000 (variable) |
| **Date consommation** | Semaine N+1 | Aujourd'hui | Date libre (48h min) |
| **Délai** | 48h avant 12h00 | Aucun délai | 48h minimum |
| **Création par** | Employés, Admin, RH | Admin, Prestataire | Admin, RH |
| **Marqueur** | `Instantanee = false` | `Instantanee = true` | `Instantanee = false` |
| **TypeClient** | CitUtilisateur | CitUtilisateur | Visiteur |
| **Identification** | UtilisateurId | UtilisateurId | VisiteurNom + Département |

---

## 🎯 Cas d'usage

### Cas 1 : Commande pour un groupe de visiteurs
```
Situation : Un groupe de 20 visiteurs arrive pour une formation
Processus :
  1. Sélection du département : "Ressources Humaines"
  2. Saisie du nombre de visiteurs : 20
  3. Sélection de la période : Jour (déjeuner)
  4. Sélection de la date : Date de la formation (au moins 48h à l'avance)
  5. Sélection formule : Formule Améliorée (seule option disponible)
  6. Vérification du délai de 48h
  7. Création de la commande groupée
Résultat : 20 plats (formule améliorée) commandés en une seule commande pour les visiteurs
Montant : 20 × 2 800 = 56 000 FCFA
```

### Cas 2 : Commande pour un événement avec visiteurs
```
Situation : Des visiteurs externes participent à un événement
Processus :
  1. Sélection du département : "Direction Générale"
  2. Saisie du nombre de visiteurs : 50
  3. Sélection de la période : Jour (déjeuner)
  4. Date de l'événement (au moins 48h à l'avance)
  5. Sélection de la formule améliorée (seule option disponible)
  6. Création de la commande groupée
Résultat : 50 plats (formule améliorée) pour les visiteurs de l'événement en une commande
Montant : 50 × 2 800 = 140 000 FCFA
```

### Cas 3 : Commande récurrente pour visiteurs
```
Situation : Des visiteurs réguliers ont besoin de repas chaque semaine
Processus :
  1. Sélection du département
  2. Saisie du nombre de visiteurs
  3. Sélection de la période
  4. Date de chaque semaine (au moins 48h à l'avance)
  5. Sélection de la formule améliorée
  6. Création de plusieurs commandes groupées
Résultat : Commandes groupées pour chaque semaine pour les visiteurs
```

---

## 🔍 Gestion des délais

### Délai minimum de 48h
Les commandes groupées pour visiteurs doivent être créées **au moins 48h à l'avance** :
- Date de consommation ≥ Date actuelle + 48h
- Vérification automatique avant création
- Si délai insuffisant → ❌ Refusé avec message d'erreur

### Calcul du délai
```
Date limite = Date de consommation - 48h
Si Date actuelle < Date limite → ✅ Autorisé
Si Date actuelle ≥ Date limite → ❌ Refusé
```

### Exemple
```
Date actuelle : 24/12/2025 à 10h00
Date de consommation souhaitée : 26/12/2025 à 12h00
Délai : 26/12 - 24/12 = 2 jours = 48h → ✅ Autorisé

Date actuelle : 24/12/2025 à 14h00
Date de consommation souhaitée : 26/12/2025 à 12h00
Délai : 26/12 12h - 24/12 14h = 46h → ❌ Refusé (moins de 48h)
```

---

## 📝 Exemple complet

### Scénario : Création d'une commande groupée pour des visiteurs

```
1. Administrateur accède à "Commande Groupée"
2. Étape 1 : Sélectionne le département : "Ressources Humaines"
3. Étape 2 : Saisit le nombre de visiteurs : 30
4. Étape 3 : Sélectionne la période : Jour (déjeuner)
5. Sélectionne la date : 28/12/2025 (au moins 48h à l'avance)
6. Étape 4 : Sélectionne la formule : Formule Améliorée (seule option disponible)
7. (Optionnel) Saisit le nom : "Groupe Formation - 30 personnes"
8. (Optionnel) Saisit le téléphone : "+225 07 12 34 56 78"
9. Vérifications automatiques :
   - ✅ Département sélectionné
   - ✅ Nombre de visiteurs dans la plage autorisée (1-1000)
   - ✅ Période sélectionnée
   - ✅ Formule existe, est disponible et est améliorée
   - ✅ Délai de 48h respecté (date >= aujourd'hui + 48h)
10. Clique sur "Créer la commande groupée"
11. Résultat :
   - ✅ Commande créée avec succès
   - Code : CMD-20251228-0001
   - Statut : Précommandée
   - Type : Visiteur
   - Nombre de visiteurs : 30
   - Formule : Améliorée
   - Montant : 30 × 2 800 = 84 000 FCFA
   - Visiteur : "Groupe Formation - 30 personnes" (si renseigné)
```

---

## 🛠️ Interface utilisateur

### Formulaire de création
- **Section principale** : Informations de base
  - Sélection du groupe
  - Date de consommation
  - Période de service
- **Section formules** : Sélection de la formule
  - Filtrage par type de formule
  - Affichage des détails
- **Section quantité** : Saisie de la quantité
  - Champ numérique avec validation
  - Calcul automatique du montant
- **Section options** : Configuration optionnelle
  - Site
  - Commentaires

### Affichage des commandes
- Badge spécial pour identifier les commandes groupées
- Affichage du nom du groupe
- Affichage de la quantité
- Filtrage possible par groupe
- Export Excel avec colonne "Groupe"

---

## 🔄 Cycle de vie d'une commande groupée

### États
```
1. Précommandée (0)
   - Commande créée
   - En attente de consommation
   - Peut être modifiée ou annulée

2. Consommée (1)
   - Repas récupérés et validés
   - Points de consommation créés
   - Ne peut plus être modifiée

3. Annulée (2)
   - Commande annulée
   - Motif enregistré
   - Peut être remplacée par une nouvelle commande
```

### Transitions
```
Précommandée → Consommée :
  - Validation manuelle (création de points de consommation)
  - Ou validation automatique lors de la fermeture

Précommandée → Annulée :
  - Annulation manuelle par Admin/RH

Consommée → (aucune transition possible)
Annulée → (aucune transition possible)
```

---

## 📊 Statistiques et suivi

### Suivi par groupe
- Nombre total de commandes par groupe
- Quantité totale de plats commandés
- Montant total facturé
- Taux de consommation

### Suivi par période
- Commandes groupées pour le déjeuner (Jour)
- Commandes groupées pour le dîner (Nuit)
- Répartition des quantités

### Export et reporting
- Export Excel des commandes groupées
- Filtrage par groupe, date, période
- Statistiques de consommation

---

## ⚙️ Configuration technique

### Modèle de données
```csharp
Commande {
    TypeClient = Visiteur
    VisiteurNom = string (nom du groupe de visiteurs)
    VisiteurTelephone = string (téléphone optionnel)
    Quantite = int (1-1000)
    Instantanee = false
    DateConsommation = DateTime (date libre, min 48h)
    UtilisateurId = null (pas d'utilisateur CIT)
}
```

### Relations
```
Commande → Visiteur (via TypeClient = Visiteur)
Pas de relation directe avec une table Visiteur
Les informations sont stockées dans Commande.VisiteurNom
```

---

## 🚨 Points d'attention

### 1. **Respect du délai de 48h**
- Vérifier toujours que la date est au moins 48h dans le futur
- Le système vérifie automatiquement le délai
- Message d'erreur clair si délai insuffisant

### 2. **Calcul du montant**
- Le montant est calculé automatiquement
- **Formule améliorée** : 2 800 FCFA par visiteur
- Le montant = Nombre de visiteurs × 2 800 FCFA

### 3. **Validation des commandes**
- Les commandes groupées nécessitent une validation manuelle
- Créer des points de consommation pour chaque plat
- Ou utiliser la validation automatique

### 4. **Informations sur les visiteurs**
- Le nom du groupe est obligatoire
- Le département d'origine est obligatoire
- Le téléphone est optionnel mais recommandé

---

## 📚 Documentation complémentaire

- **WORKFLOW_COMPLET.md** : Section "Commande groupée"
- **Controllers/CommandeController.cs** : Méthode `CreerCommandeGroupee`
- **Views/Commande/CreerCommandeGroupee.cshtml** : Interface utilisateur
- **Models/ViewModels/CommandeGroupeeViewModel.cs** : Modèle de vue

---

**Document créé le** : 2025-01-XX  
**Version** : 1.0  
**Application** : O'Beli K

