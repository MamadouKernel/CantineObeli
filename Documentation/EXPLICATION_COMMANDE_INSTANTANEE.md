# 🚀 Explication de la Commande Instantanée

## 📋 Vue d'ensemble

La **commande instantanée** est une fonctionnalité qui permet de créer des commandes de repas **pour le jour même** (aujourd'hui), contrairement aux commandes normales qui doivent être passées **48h avant 12h00** de la date de consommation.

### Processus simplifié :
1. **Saisie du matricule** de l'employé
2. **Choix d'une formule** parmi celles disponibles pour le jour J
3. **Sélection de la période** : Midi ou Soir
4. **Création** de la commande instantanée

## 🎯 Objectif

Cette fonctionnalité est principalement destinée aux **Prestataires de Cantine** et aux **Administrateurs** pour gérer des commandes de dernière minute pour les employés qui ont oublié de commander ou qui ont besoin d'un repas le jour même.

---

## 👥 Qui peut créer une commande instantanée ?

### Rôles autorisés :
- ✅ **Administrateur** (Admin)
- ✅ **PrestataireCantine**

### Rôles non autorisés :
- ❌ **RH** (Ressources Humaines) - ne peut pas créer de commandes instantanées
- ❌ **Employé** - ne peut pas créer de commandes instantanées

---

## 🔑 Caractéristiques principales

### 1. **Date de consommation**
- **Commande normale** : Date dans la semaine N+1 (48h avant 12h00)
- **Commande instantanée** : **Date = Aujourd'hui** (jour même)

### 2. **Marqueur spécial**
- Toutes les commandes instantanées ont le champ `Instantanee = true`
- Permet de les distinguer des commandes normales dans les listes et exports

### 3. **Pas de délai de précommande**
- Contrairement aux commandes normales, il n'y a **pas de délai de 48h**
- La commande peut être créée et consommée le même jour

---

## 📝 Type de client supporté

La commande instantanée peut être créée **uniquement pour les employés CIT** :

### **Employé CIT** (CitUtilisateur)

#### Caractéristiques :
- ✅ **Saisie du matricule** de l'employé CIT (recherche par matricule)
- ✅ **Choix parmi les formules du jour J** (formules disponibles pour aujourd'hui)
- ✅ **Sélection de la période** : Midi (Jour) ou Soir (Nuit)
- ✅ Quantité fixée à **1** (un seul plat par commande)
- ✅ **Une seule commande par période** (Midi ou Soir) par employé par jour
- ✅ Vérification des commandes existantes pour éviter les doublons

#### Restrictions :
- Si l'utilisateur a déjà une commande instantanée **en attente** (Précommandée) → ❌ Impossible de créer une nouvelle
- Si l'utilisateur a déjà une commande instantanée **consommée** → ❌ Impossible de créer une nouvelle
- Si l'utilisateur a une commande instantanée **annulée** → ✅ Possible de créer une nouvelle

#### Exemple :
```
1. Saisie du matricule : "JD001"
2. Système trouve : Jean Dupont
3. Affichage des formules du jour (26/12/2025) :
   - Formule Améliorée
   - Formule Standard 1
   - Formule Standard 2
4. Sélection : Formule Améliorée
5. Sélection période : Midi
6. → Création possible si aucune commande instantanée pour le midi aujourd'hui
```

> **Note importante** : Les commandes instantanées ne sont **pas disponibles** pour les groupes non-CIT ni pour les visiteurs. Ces types de clients doivent utiliser les autres fonctionnalités de commande (commande normale ou commande pour visiteurs).

---

## 🔄 Processus de création

### Étape 1 : Accès au formulaire
```
Menu → Commandes → Commande Instantanée
```

### Étape 2 : Saisie du matricule de l'employé
- Saisie du **matricule** de l'employé CIT
- Recherche automatique de l'employé dans la base de données
- Affichage des informations de l'employé trouvé (nom, prénom, département)

### Étape 3 : Sélection de la formule du jour
- Affichage des **formules disponibles pour aujourd'hui** (jour J)
- Choix d'une formule parmi celles proposées :
  - Formule Améliorée
  - Formule Standard 1
  - Formule Standard 2
- Affichage des détails de chaque formule (entrée, plat, dessert, garniture, etc.)

### Étape 4 : Sélection de la période
- Choix de la période de service :
  - **Midi** (Jour) - pour le déjeuner
  - **Soir** (Nuit) - pour le dîner

### Étape 5 : Validation
- Vérification que l'employé existe et est actif
- Vérification des doublons (une seule commande par période par jour)
- Vérification que la formule existe pour aujourd'hui

### Étape 6 : Création
- Génération d'un code de commande unique
- Statut initial : **Précommandée**
- Date de consommation : **Aujourd'hui** (jour J)
- Quantité : **1** (fixe, un seul plat par commande)

---

## ⚠️ Règles et validations

### 1. **Blocage des commandes**
- Si les commandes sont bloquées (vendredi 12h, samedi, dimanche) → ❌ Impossible de créer une commande instantanée

### 2. **Employé CIT - Limitation par période**
```
Un employé CIT ne peut avoir qu'UNE SEULE commande instantanée par période par jour :
- Soit une commande pour le midi (Jour)
- Soit une commande pour le soir (Nuit)
- Soit les deux (une pour chaque période)
```

### 3. **Employé CIT - Vérification des statuts**
```
Si commande existante :
- Statut "Précommandée" → ❌ Bloqué (en attente de validation)
- Statut "Consommée" → ❌ Bloqué (déjà consommée)
- Statut "Annulée" → ✅ Autorisé (peut créer une nouvelle)
```

### 4. **Formule obligatoire**
- La formule doit exister et être active
- La formule doit être disponible pour aujourd'hui

---

## 📊 Statuts d'une commande instantanée

### 1. **Précommandée** (0)
- Commande créée, en attente de validation
- Peut être modifiée ou annulée par le prestataire
- L'utilisateur ne peut pas créer une nouvelle commande si une précommandée existe

### 2. **Consommée** (1)
- Repas récupéré et validé
- Point de consommation créé
- Commande finalisée, ne peut plus être modifiée

### 3. **Annulée** (2)
- Commande annulée par le prestataire
- L'utilisateur peut créer une nouvelle commande pour remplacer

---

## 🎯 Cas d'usage

### Cas 1 : Employé oublié de commander
```
Situation : Un employé a oublié de commander pour aujourd'hui
Processus :
  1. Prestataire saisit le matricule de l'employé (ex: "JD001")
  2. Système trouve l'employé : Jean Dupont
  3. Affichage des formules disponibles pour aujourd'hui
  4. Sélection d'une formule (ex: Formule Améliorée)
  5. Sélection de la période : Midi ou Soir
  6. Création de la commande instantanée
Résultat : L'employé peut récupérer son repas le jour même
```

### Cas 2 : Remplacement de commande annulée
```
Situation : Une commande instantanée a été annulée
Processus :
  1. Prestataire saisit le matricule de l'employé
  2. Sélection d'une formule du jour parmi celles disponibles
  3. Sélection de la période (Midi ou Soir)
  4. Création d'une nouvelle commande instantanée pour remplacer
Résultat : L'employé peut quand même récupérer son repas
```

---

## 🔍 Différences avec les commandes normales

| Caractéristique | Commande Normale | Commande Instantanée |
|----------------|------------------|----------------------|
| **Délai** | 48h avant 12h00 | Aucun délai (jour même) |
| **Date consommation** | Semaine N+1 | Aujourd'hui (jour J) |
| **Création par** | Employés, Admin, RH | Admin, Prestataire uniquement |
| **Identification employé** | Sélection dans liste | Saisie du matricule |
| **Formules disponibles** | Formules semaine N+1 | Formules du jour J uniquement |
| **Période** | Jour ou Nuit | Midi (Jour) ou Soir (Nuit) |
| **Quantité employé** | 1 | 1 (fixe) |
| **Type de client** | Employés, Groupes, Visiteurs | Employés uniquement |
| **Marqueur** | `Instantanee = false` | `Instantanee = true` |
| **Limite par période** | Non | Oui (une par période/jour) |

---

## 🛠️ Interface utilisateur

### Formulaire de création
- **Design moderne** avec dégradés dorés
- **Champ de saisie du matricule** avec recherche automatique
- **Affichage des formules du jour J** (formules disponibles pour aujourd'hui)
- **Sélection de la période** : Midi ou Soir
- **Validation en temps réel** des champs
- **Messages d'erreur détaillés** en cas de problème (employé non trouvé, commande existante, etc.)

### Affichage des commandes
- Badge spécial pour identifier les commandes instantanées
- Filtrage possible par type de commande
- Export Excel avec colonne "Instantanée"

---

## 📝 Exemple complet

### Scénario : Création d'une commande instantanée pour un employé

```
1. Prestataire accède à "Commande Instantanée"
2. Saisit le matricule : "JD001"
3. Système recherche et trouve : Jean Dupont (Département: RH)
4. Affichage des formules disponibles pour aujourd'hui (26/12/2025) :
   - Formule Améliorée : Salade, Poulet rôti, Riz, Fruit
   - Formule Standard 1 : Poisson grillé, Riz
   - Formule Standard 2 : Viande sauce, Attiéké
5. Sélectionne : Formule Améliorée
6. Sélectionne la période : Midi (Jour)
7. Clique sur "Créer la commande"
8. Vérifications automatiques :
   - ✅ Employé existe et est actif
   - ✅ Aucune commande instantanée pour le midi aujourd'hui
   - ✅ Formule disponible pour aujourd'hui
9. Résultat :
   - ✅ Commande créée avec succès
   - Code : CMD-20251226-ABC123
   - Statut : Précommandée
   - Date consommation : 26/12/2025
   - Période : Midi (Jour)
   - Formule : Formule Améliorée
   - Instantanée : Oui
```

---

## ⚙️ Configuration technique

### Champ dans la base de données
```sql
Instantanee BIT NOT NULL DEFAULT 0
```

### Dans le modèle Commande
```csharp
public bool Instantanee { get; set; }
```

### Vérification dans les requêtes
```csharp
.Where(c => c.Instantanee == true 
    && c.DateConsommation.HasValue 
    && c.DateConsommation.Value.Date == DateTime.Today)
```

---

## 🚨 Points d'attention

### 1. **Pas de délai de 48h**
- Les commandes instantanées contournent la règle des 48h
- Attention à ne pas abuser de cette fonctionnalité

### 2. **Limitation par période**
- Un employé ne peut avoir qu'une seule commande instantanée par période (Jour ou Nuit) par jour
- Vérification automatique avant création

### 3. **Doublons employés**
- Un employé ne peut avoir qu'une commande instantanée par période par jour
- Vérification automatique avant création

### 4. **Blocage des commandes**
- Même les commandes instantanées sont bloquées pendant les périodes de fermeture
- Respecter les règles de blocage (vendredi 12h, etc.)

---

## 📚 Documentation complémentaire

- **WORKFLOW_COMPLET.md** : Section "Commande instantanée"
- **Controllers/CommandeController.cs** : Méthode `CreerCommandeInstantanee`
- **Views/Commande/CreerCommandeInstantanee.cshtml** : Interface utilisateur

---

**Document créé le** : 2025-01-XX  
**Version** : 1.0  
**Application** : O'Beli K

