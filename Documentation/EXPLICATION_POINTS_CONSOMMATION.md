# 📊 Explication des Points de Consommation

## 📋 Vue d'ensemble

Les **points de consommation** sont des enregistrements qui tracent chaque repas consommé par un employé CIT. Ils servent à :
- **Suivre les consommations réelles** : Chaque fois qu'un employé consomme un repas
- **Calculer les coûts** : Montant facturé selon le type de formule
- **Générer des statistiques** : Nombre de repas, montants, par période, par formule
- **Facturer les non-consommations** : Créer des points de facturation pour les commandes non récupérées

---

## 👥 Qui peut voir les points de consommation ?

### Pour ses propres points :
- ✅ **Tous les utilisateurs CIT** (Employés, Admin, RH, Prestataire) : Via "Mes Points de Consommation"
- ⚠️ **Visiteurs et Douaniers** : N'ont **pas** de points de consommation car ils n'ont pas de compte utilisateur individuel

### Pour tous les points (CIT) :
- ✅ **Administrateur** : Accès complet à tous les points de consommation des utilisateurs CIT
- ✅ **RH** : Accès complet à tous les points de consommation des utilisateurs CIT
- ❌ **Employé** - ne peut voir que ses propres points
- ❌ **PrestataireCantine** - ne peut voir que ses propres points

### ⚠️ Important : Limitation des points de consommation

Les **points de consommation** sont créés **uniquement pour les utilisateurs CIT** qui ont un compte dans la table `Utilisateurs` :
- ✅ **Employés CIT** : Ont des points de consommation
- ✅ **RH** : Ont des points de consommation
- ✅ **Administrateurs** : Ont des points de consommation
- ✅ **PrestataireCantine** : Ont des points de consommation

Les **visiteurs** et les **Douaniers** (groupes non-CIT) :
- ❌ **N'ont PAS** de points de consommation
- ❌ Leurs commandes ont `UtilisateurId = null`
- ❌ Les points de consommation nécessitent un `UtilisateurId` valide (obligatoire dans le modèle)
- ℹ️ Leurs consommations sont tracées via les **commandes** (table `Commandes`), pas via les points de consommation

**Conclusion** : Les RH et Admin voient uniquement les points de consommation des **utilisateurs CIT** (employés, RH, Admin, Prestataire), **pas** ceux des visiteurs et des Douaniers.

---

## 🔑 Caractéristiques principales

### 1. **Création automatique**
Les points de consommation sont créés automatiquement dans deux cas :

#### A. Validation d'une commande
- Quand une commande est **validée** (statut = Consommée)
- Création automatique d'un point de consommation
- Lieu : "Restaurant CIT" (par défaut)

#### B. Fermeture automatique (vendredi 12h)
- Le service `FermetureAutomatiqueService` s'exécute chaque vendredi à 12h
- Crée automatiquement des points de consommation pour toutes les commandes de la semaine N+1
- Marque les commandes comme "Consommées"

### 2. **Création manuelle**
- Les **Administrateurs et RH** peuvent créer manuellement des points de consommation
- Utile pour corriger des erreurs ou ajouter des consommations oubliées

### 3. **Facturation des non-consommations**
- Si la facturation est activée, les commandes **non récupérées** génèrent des points de facturation
- Lieu : `"FACTURATION - NON RÉCUPÉRÉE (Montant FCFA)"`
- Ces points apparaissent dans "Mes Points de Consommation" si la facturation est activée

---

## 📝 Structure d'un point de consommation

### Champs principaux :
```csharp
PointConsommation {
    IdPointConsommation : Guid (identifiant unique)
    UtilisateurId : Guid (employé qui a consommé)
    CommandeId : Guid (commande associée)
    DateConsommation : DateTime (date de consommation)
    TypeFormule : string (ex: "Standard 1", "Améliorée")
    NomPlat : string (nom du plat consommé)
    QuantiteConsommee : int (nombre de plats, généralement 1)
    LieuConsommation : string (ex: "Restaurant CIT", "FACTURATION - ...")
    CreatedOn : DateTime (date de création)
    CreatedBy : string (qui a créé le point)
}
```

### Relations :
- **PointConsommation → Utilisateur** (Many-to-One)
- **PointConsommation → Commande** (Many-to-One)

---

## 💰 Calcul du coût

### Formules de prix :
- **Formule Améliorée** : 2 800 FCFA par plat
- **Formule Standard 1** : 550 FCFA par plat
- **Formule Standard 2** : 550 FCFA par plat
- **Autres** : 550 FCFA par plat (par défaut)

### Calcul du montant :
```
Montant = QuantiteConsommee × PrixUnitaire
```

### Cas spéciaux :
- **Facturation** : Le montant est extrait du `LieuConsommation` si contient "FACTURATION"
- **Format** : `FACTURATION - NON RÉCUPÉRÉE (Montant FCFA)`
- **Exemple** : `FACTURATION - NON RÉCUPÉRÉE (550 FCFA)`

---

## 📊 Fonctionnalités

### 1. **Mes Points de Consommation** (Tous les utilisateurs)

#### Accès :
- **Menu** : Points de consommation → Mes Points de Consommation
- **URL** : `/PointsConsommation/MesPointsConsommation`

#### Fonctionnalités :
- **Filtrage par période** : Date de début et date de fin (par défaut : 30 derniers jours)
- **Affichage des points** :
  - Date de consommation
  - Type de formule
  - Nom du plat
  - Lieu de consommation
  - Montant
  - Commande associée
- **Statistiques** :
  - Total des consommations
- **Mode facturation** :
  - **Activée** : Affiche tous les points (consommations + facturations)
  - **Désactivée** : Affiche uniquement les consommations réelles (exclut les facturations)

### 2. **Points de Consommation CIT** (Admin/RH)

#### Accès :
- **Menu** : Points de consommation → Points de Consommation CIT
- **URL** : `/PointsConsommation/PointConsommationCIT`

#### Fonctionnalités :
- **Vue globale** : Tous les utilisateurs CIT
- **Groupement par utilisateur** :
  - Nom complet
  - Matricule
  - Email
  - Nombre de consommations
  - Montant total
- **Tri** : Par montant total décroissant
- **Filtrage par période** : Date de début et date de fin
- **Statistiques globales** :
  - Total utilisateurs
  - Total consommations
  - Montant global

### 3. **Création manuelle** (Admin/RH)

#### Accès :
- **Menu** : Points de consommation → Créer un point
- **URL** : `/PointsConsommation/Create`

#### Processus :
1. Sélectionner l'utilisateur
2. Sélectionner la commande associée
3. Renseigner :
   - Date de consommation
   - Type de formule
   - Nom du plat
   - Lieu de consommation
   - Quantité
4. Sauvegarder

---

## 🔄 Cycle de vie d'un point de consommation

### 1. **Création automatique lors de la validation**
```
Commande créée (Statut: Précommandée)
    ↓
Commande validée (Statut: Consommée)
    ↓
Point de consommation créé automatiquement
    ↓
Point visible dans "Mes Points de Consommation"
```

### 2. **Création automatique lors de la fermeture**
```
Vendredi 12h00
    ↓
FermetureAutomatiqueService s'exécute
    ↓
Pour chaque commande de la semaine N+1 :
    - Créer un point de consommation
    - Marquer la commande comme "Consommée"
    ↓
Points visibles dans "Mes Points de Consommation"
```

### 3. **Création pour facturation**
```
FacturationAutomatiqueService s'exécute
    ↓
Détecte les commandes non récupérées
    ↓
Pour chaque commande non récupérée :
    - Créer un point de facturation
    - LieuConsommation = "FACTURATION - NON RÉCUPÉRÉE (Montant)"
    ↓
Points visibles si facturation activée
```

---

## 📈 Statistiques et rapports

### Statistiques par utilisateur
- **Total consommations** : Nombre de points de consommation
- **Par formule** : Groupement par type de formule
- **Par mois** : Groupement par mois

---

### Statistiques globales (Admin/RH)
- **Total utilisateurs** : Nombre d'utilisateurs ayant consommé
- **Total consommations** : Nombre total de points
- **Montant global** : Somme de tous les montants

---

## 🔍 Différences entre consommations et facturations

### Consommations réelles
- **LieuConsommation** : "Restaurant CIT" ou autre lieu réel
- **Création** : Lors de la validation ou fermeture automatique
- **Signification** : Repas réellement consommé
- **Affichage** : Toujours visible

### Facturations
- **LieuConsommation** : Contient "FACTURATION"
- **Création** : Par le service de facturation automatique
- **Signification** : Commande non récupérée, facturée quand même
- **Affichage** : Visible uniquement si facturation activée

---

## 📊 Exemple complet

### Scénario : Consultation des points de consommation

```
1. Employé se connecte
2. Accède à "Points de consommation" → "Mes Points de Consommation"
3. Période par défaut : 30 derniers jours
4. Affichage :
   - Date : 15/12/2024
   - Formule : Standard 1
   - Plat : Riz au gras + Poulet
   - Lieu : Restaurant CIT
   - Montant : 550 FCFA
   - Commande : CMD-20241215-0001
   
   - Date : 16/12/2024
   - Formule : Améliorée
   - Plat : Riz au gras + Poisson
   - Lieu : Restaurant CIT
   - Montant : 2 800 FCFA
   - Commande : CMD-20241216-0002

5. Statistiques affichées :
   - Total des consommations : 2
```

---

## ⚙️ Configuration

### Facturation des non-consommations
- **Paramètre** : `FACTURATION_NON_CONSOMMEES_ACTIVE`
- **Valeurs** : `true` ou `false`
- **Effet** :
  - `true` : Les facturations apparaissent dans "Mes Points de Consommation"
  - `false` : Seules les consommations réelles sont affichées

---

## 🎯 Points clés à retenir

1. **Création automatique** : Les points sont créés lors de la validation ou fermeture automatique
2. **Un point = Un repas** : Chaque point représente un repas consommé
3. **Lien avec commande** : Chaque point est lié à une commande
4. **Calcul automatique** : Le montant est calculé selon le type de formule
5. **Facturation** : Les non-consommations peuvent générer des points de facturation
6. **Consultation** : Tous les utilisateurs peuvent voir leurs propres points
7. **Administration** : Admin/RH peuvent voir tous les points et créer manuellement

---

## 🆘 Dépannage

### Problème : "Aucun point de consommation affiché"
**Solutions** :
- Vérifier la période sélectionnée
- Vérifier que des commandes ont été validées
- Vérifier que la fermeture automatique s'est exécutée

### Problème : "Montant incorrect"
**Solutions** :
- Vérifier le type de formule dans le point
- Vérifier la quantité consommée
- Vérifier le calcul dans `CalculerCout`

### Problème : "Facturations non visibles"
**Solutions** :
- Vérifier que la facturation est activée (`FACTURATION_NON_CONSOMMEES_ACTIVE = true`)
- Vérifier que des commandes non récupérées existent
- Vérifier que le service de facturation s'est exécuté

---

**Dernière mise à jour** : Décembre 2024

