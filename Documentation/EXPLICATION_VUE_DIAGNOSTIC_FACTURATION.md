# Explication de la Vue Diagnostic Facturation

## 📋 Vue d'ensemble

La vue `https://localhost:7021/DiagnosticFacturation` est une **outil de diagnostic et de débogage** pour analyser l'état des commandes et détecter les incohérences dans le système de facturation. Elle permet de comprendre pourquoi certaines commandes sont ou ne sont pas facturées.

---

## 🎯 Objectif principal

Cette vue permet de :

1. **Diagnostiquer** les problèmes de facturation
2. **Détecter** les incohérences entre le statut des commandes et leur état réel
3. **Vérifier** si les commandes "Consommées" ont vraiment été validées par le prestataire
4. **Comparer** les résultats directs avec ceux du service de facturation
5. **Comprendre** pourquoi certaines commandes ne sont pas facturées

---

## 🔐 Accès et autorisations

- **Rôles autorisés** : **Administrateur, RH** uniquement (`[Authorize(Roles = "Administrateur,RH")]`)
- **URL** : `/DiagnosticFacturation`
- **Filtrage** :
  - **Admin/RH** : Voient toutes les commandes (pas de filtre par utilisateur)

---

## 📊 Structure de la vue

### 1. **Informations Utilisateur**

Affiche les informations de l'utilisateur connecté :

- **Email** : Email de l'utilisateur connecté (Admin/RH)
- **User ID** : Identifiant unique de l'utilisateur
- **Rôle** : "Admin/RH" (toujours affiché comme Admin/RH car seuls ces rôles peuvent accéder)

---

### 2. **Configuration**

Affiche les paramètres de facturation :

- **Facturation Active** : Badge vert (OUI) ou rouge (NON)
- **Période** : Mois en cours (du 1er au dernier jour du mois)

---

### 3. **Résumé des Commandes**

Affiche 4 cartes avec des statistiques :

#### a) Total Commandes

- **Couleur** : Bleu (primary)
- **Nombre** : Total de toutes les commandes de la période

#### b) Précommandées

- **Couleur** : Jaune (warning)
- **Nombre** : Commandes avec statut "Précommandée"

#### c) Annulées

- **Couleur** : Rouge (danger)
- **Nombre** : Commandes avec statut "Annulée"

#### d) Consommées

- **Couleur** : Vert (success)
- **Nombre** : Commandes **réellement consommées** (avec point de consommation)

---

### 4. **Commandes Non Consommées (Direct)**

Affiche les commandes non consommées calculées **directement** depuis la base de données :

**Critères** :

- Commandes avec statut "Précommandée" ET date de consommation passée
- Commandes avec statut "Consommée" mais **sans point de consommation** (incohérence détectée)

**Colonnes** :

- **Code** : Code de la commande
- **Date** : Date de consommation
- **Plat** : Nom du plat

---

### 5. **Commandes Non Consommées (Service)**

Affiche les commandes non consommées calculées par le **service de facturation** (`IFacturationService`) :

**Critères** (selon le service) :

- Commandes avec statut "Précommandée" OU "Consommée"
- Date de consommation passée
- Type client = CIT Utilisateur
- **Pas de point de consommation** (non validée par prestataire)

**Colonnes** :

- **Code** : Code de la commande
- **Date** : Date de consommation
- **Plat** : Nom du plat
- **Montant** : Montant de la commande

**Note** : Cette liste n'apparaît que si la facturation est activée.

---

### 6. **Détail de Toutes les Commandes**

Tableau complet avec toutes les commandes et leur statut **corrigé** :

**Colonnes** :

- **Code** : Code de la commande
- **Date Commande** : Date de création de la commande
- **Date Consommation** : Date prévue de consommation
- **Statut** : Statut affiché avec correction automatique
- **Type Client** : CIT, Visiteur, ou Groupe
- **Plat** : Nom du plat
- **Montant** : Montant de la commande

**Correction automatique du statut** :

Le système détecte et corrige les incohérences :

- Si une commande a le statut "Consommée" **mais pas de point de consommation** → Statut affiché : **"Précommandée"** (badge jaune)
- Si une commande a le statut "Consommée" **avec point de consommation** → Statut affiché : **"Consommée"** (badge vert)
- Sinon, le statut original est conservé

---

## 🔍 Logique de diagnostic

### Détection des incohérences

Le contrôleur effectue plusieurs vérifications :

#### 1. Vérification des commandes "Consommées"

```csharp
// Pour chaque commande avec statut "Consommée"
var pointConsommation = await _context.PointsConsommation
    .FirstOrDefaultAsync(pc => pc.CommandeId == commande.IdCommande && pc.Supprimer == 0);

if (pointConsommation != null)
{
    // Commande réellement consommée (validée par prestataire)
    commandesConsommee.Add(commande);
}
else
{
    // Commande avec statut "Consommée" mais pas de point de consommation
    // = pas vraiment validée = incohérence détectée
    commandesStatutConsommeeMaisPasValidee.Add(commande);
}
```

#### 2. Identification des commandes non consommées

```csharp
// Commandes "Précommandées" avec date passée
var commandesNonConsommees = commandesPrecommander
    .Where(c => c.DateConsommation.HasValue && c.DateConsommation.Value.Date < maintenant.Date)
    .ToList();

// Ajouter les commandes avec statut "Consommée" mais pas validées
commandesNonConsommees.AddRange(commandesStatutConsommeeMaisPasValidee);
```

#### 3. Comparaison avec le service

Le service de facturation applique des règles supplémentaires :

- Filtre par type client (CIT uniquement)
- Vérifie l'absence de point de consommation
- Applique les règles de facturation

---

## 💡 Cas d'usage

### Cas 1 : Détecter les incohérences

**Problème** : Une commande a le statut "Consommée" mais n'a pas été validée par le prestataire.

**Solution** : La vue détecte cette incohérence et affiche la commande comme "Précommandée" dans le tableau de détail, avec un badge jaune.

### Cas 2 : Comprendre pourquoi une commande n'est pas facturée

**Problème** : Une commande non consommée n'apparaît pas dans la liste de facturation.

**Solution** : La vue compare les deux listes (Direct vs Service) pour identifier les différences et comprendre pourquoi le service exclut certaines commandes.

### Cas 3 : Vérifier le fonctionnement du service de facturation

**Problème** : Vérifier que le service de facturation fonctionne correctement.

**Solution** : Comparer les résultats du calcul direct avec ceux du service pour détecter d'éventuels bugs.

---

## 🔄 Flux de traitement

### Étape 1 : Chargement de la page

```csharp
1. Récupération de l'utilisateur connecté
2. Vérification du rôle (Employé vs Admin/RH)
3. Vérification de l'activation de la facturation
4. Définition de la période (mois en cours)
```

### Étape 2 : Récupération des commandes

```csharp
1. Récupération de toutes les commandes de la période
   - Filtrage par utilisateur si Employé
   - Inclusion des relations (Utilisateur, FormuleJour)
2. Séparation par statut :
   - Précommandées
   - Annulées
   - Consommées
```

### Étape 3 : Vérification des incohérences

```csharp
1. Pour chaque commande "Consommée" :
   - Vérifier l'existence d'un point de consommation
   - Si absent → Ajouter à la liste des incohérences
2. Identifier les commandes non consommées :
   - Précommandées avec date passée
   - Consommées sans point de consommation
```

### Étape 4 : Appel au service de facturation

```csharp
1. Si la facturation est activée :
   - Appeler FacturationService.GetCommandesNonConsommeesAsync()
   - Filtrer par utilisateur si Employé
2. Comparer avec les résultats directs
```

### Étape 5 : Correction des statuts pour l'affichage

```csharp
1. Pour chaque commande :
   - Vérifier si elle est réellement consommée
   - Vérifier si elle a un statut "Consommée" mais pas validée
   - Déterminer le statut d'affichage correct
2. Créer la liste avec les statuts corrigés
```

---

## ⚠️ Points importants

### 1. **Détection automatique des incohérences**

La vue détecte automatiquement les commandes qui ont le statut "Consommée" mais qui n'ont pas de point de consommation. Ces commandes sont considérées comme non consommées et peuvent être facturées.

### 2. **Différence entre "Direct" et "Service"**

- **Direct** : Calcul simple basé sur le statut et la date
- **Service** : Calcul complexe avec toutes les règles de facturation (type client, point de consommation, etc.)

### 3. **Filtrage par utilisateur**

- **Admin/RH** : Voient toutes les commandes (pas de filtre par utilisateur)

### 4. **Période par défaut**

La période est toujours le mois en cours (du 1er au dernier jour du mois). Il n'y a pas de filtre de période personnalisable dans cette vue.

### 5. **Logs de diagnostic**

Toutes les incohérences détectées sont loggées avec un niveau `Warning` pour faciliter le débogage.

---

## 🎨 Éléments visuels

### Couleurs des badges de statut

- **Vert (bg-success)** : Consommée (réellement validée)
- **Jaune (bg-warning)** : Précommandée ou incohérence détectée
- **Rouge (bg-danger)** : Annulée
- **Bleu (bg-primary)** : Facturée
- **Cyan (bg-info)** : Exemptée
- **Gris (bg-secondary)** : Inconnu

### Couleurs des cartes de résumé

- **Bleu (bg-primary)** : Total Commandes
- **Jaune (bg-warning)** : Précommandées
- **Rouge (bg-danger)** : Annulées
- **Vert (bg-success)** : Consommées

---

## 🔗 Différences avec la vue Facturation

| Aspect | Diagnostic Facturation | Facturation |
|--------|------------------------|-------------|
| **Objectif** | Diagnostic et débogage | Application de la facturation |
| **Accès** | Admin/RH uniquement | Admin/RH uniquement |
| **Période** | Mois en cours (fixe) | Personnalisable |
| **Affichage** | Détails techniques | Vue opérationnelle |
| **Actions** | Aucune | Application de la facturation |
| **Incohérences** | Détectées et affichées | Non affichées |

---

## 📝 Notes techniques

- **Service utilisé** : `IFacturationService` (optionnel, si facturation activée)
- **Modèles** : `Commande`, `CommandeNonConsommeeViewModel`
- **Base de données** : Tables `Commandes`, `PointsConsommation`
- **Logs** : Toutes les incohérences sont loggées avec `LogWarning`

---

## ✅ Checklist d'utilisation

Avant d'utiliser cette vue pour diagnostiquer un problème :

- [ ] Vérifier que la facturation est activée (si nécessaire)
- [ ] Vérifier la période (mois en cours)
- [ ] Examiner les incohérences détectées
- [ ] Comparer les listes "Direct" et "Service"
- [ ] Vérifier les logs pour plus de détails
- [ ] Utiliser la vue "Facturation" pour appliquer les corrections

---

## 🐛 Problèmes courants détectés

### 1. Commande "Consommée" sans point de consommation

**Symptôme** : Commande avec statut "Consommée" mais pas de point de consommation.

**Cause** : Le statut a été changé manuellement ou par erreur.

**Solution** : La commande sera considérée comme non consommée et pourra être facturée.

### 2. Différence entre "Direct" et "Service"

**Symptôme** : Une commande apparaît dans "Direct" mais pas dans "Service".

**Cause** : Le service applique des règles supplémentaires (type client, etc.).

**Solution** : Vérifier les règles du service de facturation.

### 3. Commandes non facturées alors qu'elles devraient l'être

**Symptôme** : Commandes non consommées qui n'apparaissent pas dans la facturation.

**Cause** : Règles de facturation (absences gratuites, week-end, etc.).

**Solution** : Vérifier les paramètres de facturation.

---

*Document créé le : 2025-01-XX*
*Dernière mise à jour : 2025-01-XX*
