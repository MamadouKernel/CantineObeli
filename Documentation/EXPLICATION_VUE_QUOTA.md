# Explication de la Vue Quota (Historique)

## 📋 Vue d'ensemble

La vue `https://localhost:7021/Quota` est une interface de **consultation et gestion de l'historique des quotas journaliers** pour les groupes non-CIT (principalement les Douaniers). Cette vue permet de consulter, créer, modifier et supprimer des quotas historiques pour référence.

---

## 🎯 Objectif principal

Cette vue permet de :

1. **Consulter** l'historique des quotas journaliers des groupes non-CIT
2. **Créer** des quotas historiques pour référence
3. **Modifier** des quotas existants
4. **Supprimer** des quotas (soft delete)
5. **Visualiser** les statistiques de consommation par période

---

## 🔐 Accès et autorisations

- **Rôles autorisés** : **Administrateur, RH** uniquement (`[Authorize(Roles = "Administrateur,RH")]`)
- **URL** : `/Quota`
- **Note importante** : Les quotas permanents sont maintenant gérés dans **"Groupes Non-CIT"**. Cette vue sert uniquement à l'historique.

---

## 📊 Structure de la vue

### 1. **En-tête**

- **Titre** : "Gestion des Quotas Journaliers (Historique)"
- **Bouton** : "Gérer Quotas Permanents" (lien vers `/GroupeNonCit`)

---

### 2. **Tableau des quotas historiques**

Affiche tous les quotas journaliers historiques avec les colonnes suivantes :

#### a) Groupe

- **Badge** : Badge bleu (info) avec icône utilisateurs
- **Contenu** : Nom du groupe non-CIT (principalement "Douaniers")

#### b) Date

- **Format** : `dd/MM/yyyy`
- **Icône** : Calendrier
- **Tri** : Par défaut, tri décroissant (plus récent en premier)

#### c) Quota Jour

- **Badge** : Badge jaune (warning)
- **Contenu** : Nombre de plats alloués pour la période jour (midi)

#### d) Quota Nuit

- **Badge** : Badge bleu (info)
- **Contenu** : Nombre de plats alloués pour la période nuit (soir)

#### e) Consommé Jour

- **Badge** : Vert si > 0, gris si = 0
- **Contenu** : Nombre de plats consommés pendant la période jour

#### f) Consommé Nuit

- **Badge** : Vert si > 0, gris si = 0
- **Contenu** : Nombre de plats consommés pendant la période nuit

#### g) Restant Jour

- **Badge** : Vert si > 0, rouge si = 0
- **Contenu** : Nombre de plats restants pour la période jour (calculé automatiquement : `QuotaJour - PlatsConsommesJour`)

#### h) Restant Nuit

- **Badge** : Vert si > 0, rouge si = 0
- **Contenu** : Nombre de plats restants pour la période nuit (calculé automatiquement : `QuotaNuit - PlatsConsommesNuit`)

#### i) Actions

- **Voir les détails** : Bouton bleu avec icône œil
- **Modifier** : Bouton jaune avec icône crayon
- **Supprimer** : Bouton rouge avec icône poubelle

---

### 3. **État vide**

Si aucun quota historique n'est trouvé :

- **Icône** : Graphique en camembert
- **Message** : "Aucun quota journalier historique trouvé"
- **Information** : "Les quotas permanents sont maintenant gérés dans **Groupes Non-CIT**."
- **Boutons** :
  - "Gérer les Quotas Permanents" (lien vers `/GroupeNonCit`)
  - "Créer un Quota Historique" (lien vers `/Quota/Create`)

---

## 🔄 Fonctionnalités

### 1. **Consultation (Index)**

- Affiche tous les quotas journaliers historiques
- Tri par date décroissante, puis par nom de groupe
- Filtrage automatique des quotas supprimés (soft delete)

### 2. **Création (Create)**

- Permet de créer un quota historique pour référence
- **Restriction** : Uniquement pour le groupe "Douaniers"
- **Validation** : Un quota ne peut pas exister deux fois pour le même groupe et la même date
- **Redirection** : Si la table n'existe pas, redirige vers "Groupes Non-CIT"

### 3. **Modification (Edit)**

- Permet de modifier un quota existant
- **Validation** : Vérifie qu'aucun autre quota n'existe pour le même groupe et la même date (excluant le quota actuel)

### 4. **Suppression (Delete)**

- **Soft delete** : Marque le quota comme supprimé (`Supprimer = 1`)
- **Confirmation** : Demande confirmation avant suppression
- **Logs** : Enregistre qui a supprimé et quand

### 5. **Détails (Details)**

- Affiche les détails complets d'un quota
- Inclut toutes les informations (commentaires, dates de création/modification, etc.)

---

## 📐 Modèle de données

### QuotaJournalier

```csharp
- Id : Guid (clé primaire)
- GroupeNonCitId : Guid (référence au groupe)
- Date : DateTime (date du quota)
- QuotaJour : int (quota pour la période jour)
- QuotaNuit : int (quota pour la période nuit)
- PlatsConsommesJour : int (plats consommés jour)
- PlatsConsommesNuit : int (plats consommés nuit)
- RestrictionFormuleStandard : bool (restriction aux formules standard)
- Commentaires : string? (commentaires optionnels)
- CreatedOn, ModifiedOn : DateTime (dates de création/modification)
- CreatedBy, ModifiedBy : string (utilisateurs)
- Supprimer : int (0 = actif, 1 = supprimé)
```

### Propriétés calculées

- `PlatsRestantsJour` : `Math.Max(0, QuotaJour - PlatsConsommesJour)`
- `PlatsRestantsNuit` : `Math.Max(0, QuotaNuit - PlatsConsommesNuit)`
- `TotalQuota` : `QuotaJour + QuotaNuit`
- `TotalConsomme` : `PlatsConsommesJour + PlatsConsommesNuit`
- `TotalRestant` : `PlatsRestantsJour + PlatsRestantsNuit`

---

## 🔍 Différences avec "Groupes Non-CIT"

| Aspect | Quota (Historique) | Groupes Non-CIT |
|--------|-------------------|-----------------|
| **Objectif** | Historique et référence | Quotas permanents actuels |
| **Type** | Quotas journaliers (par date) | Quotas permanents (toujours actifs) |
| **Gestion** | Consultation et archivage | Gestion active |
| **Utilisation** | Référence historique | Utilisation quotidienne |
| **Création** | Manuelle pour référence | Gestion des groupes actifs |

---

## 💡 Cas d'usage

### Cas 1 : Consulter l'historique

**Scénario** : Un administrateur veut voir les quotas alloués aux Douaniers pour le mois de décembre.

**Solution** : Consulter la vue Quota, filtrer par date ou groupe, voir les statistiques de consommation.

### Cas 2 : Créer un quota historique

**Scénario** : Archiver les quotas d'une période spécifique pour référence future.

**Solution** : Utiliser "Créer un Quota Historique" pour enregistrer les quotas d'une date passée.

### Cas 3 : Modifier un quota historique

**Scénario** : Corriger une erreur dans un quota historique.

**Solution** : Utiliser le bouton "Modifier" pour ajuster les valeurs.

---

## ⚠️ Points importants

### 1. **Vue historique uniquement**

Cette vue sert uniquement à consulter et gérer l'historique. Les quotas permanents sont gérés dans "Groupes Non-CIT".

### 2. **Restriction aux Douaniers**

La création de quotas historiques est principalement destinée au groupe "Douaniers". Si le groupe n'existe pas, il est créé automatiquement.

### 3. **Soft delete**

Les quotas ne sont jamais supprimés définitivement. Ils sont marqués comme supprimés (`Supprimer = 1`) pour conserver l'historique.

### 4. **Calculs automatiques**

Les plats restants sont calculés automatiquement par les propriétés calculées du modèle. Pas besoin de les mettre à jour manuellement.

### 5. **Validation des doublons**

Un quota ne peut pas exister deux fois pour le même groupe et la même date. Le système vérifie cela lors de la création et de la modification.

### 6. **Migration vers Groupes Non-CIT**

Si la table `QuotasJournaliers` n'existe pas (migration), le système redirige automatiquement vers "Groupes Non-CIT".

---

## 🔄 Flux de traitement

### Étape 1 : Chargement de la page (GET `/Quota`)

```csharp
1. Récupération de tous les quotas journaliers
   - Inclusion de la relation GroupeNonCit
   - Filtrage des quotas supprimés (Supprimer == 0)
   - Tri par date décroissante, puis par nom de groupe
2. Affichage dans le tableau
3. Si aucun quota : Affichage de l'état vide avec liens vers Groupes Non-CIT
```

### Étape 2 : Création d'un quota (POST `/Quota/Create`)

```csharp
1. Vérification de l'existence du groupe "Douaniers"
   - Si absent : Création automatique
2. Validation du modèle
3. Vérification des doublons (même groupe + même date)
4. Ajout des métadonnées (CreatedOn, CreatedBy, etc.)
5. Sauvegarde dans la base de données
6. Redirection vers l'index avec message de succès
```

### Étape 3 : Modification d'un quota (POST `/Quota/Edit`)

```csharp
1. Récupération du quota existant
2. Validation du modèle
3. Vérification des doublons (excluant le quota actuel)
4. Mise à jour des propriétés
5. Mise à jour des métadonnées (ModifiedOn, ModifiedBy)
6. Sauvegarde dans la base de données
7. Redirection vers l'index avec message de succès
```

### Étape 4 : Suppression d'un quota (POST `/Quota/Delete`)

```csharp
1. Récupération du quota
2. Soft delete : Supprimer = 1
3. Mise à jour des métadonnées (ModifiedOn, ModifiedBy)
4. Sauvegarde dans la base de données
5. Redirection vers l'index avec message de succès
```

---

## 🎨 Éléments visuels

### Couleurs des badges

- **Bleu (bg-info)** : Groupe, Quota Nuit
- **Jaune (bg-warning)** : Quota Jour
- **Vert (bg-success)** : Consommé (si > 0), Restant (si > 0)
- **Gris (bg-secondary)** : Consommé (si = 0)
- **Rouge (bg-danger)** : Restant (si = 0)

### Icônes

- `fa-users` : Groupe
- `fa-calendar-alt` : Date
- `fa-sun` : Quota Jour
- `fa-moon` : Quota Nuit
- `fa-utensils` : Consommé
- `fa-chart-bar` : Restant
- `fa-cogs` : Actions
- `fa-eye` : Voir détails
- `fa-edit` : Modifier
- `fa-trash` : Supprimer

---

## 📝 Notes techniques

- **Service utilisé** : Aucun service spécifique, accès direct à la base de données
- **Modèle** : `QuotaJournalier`
- **Base de données** : Table `QuotasJournaliers`
- **Relations** : `GroupeNonCit` (clé étrangère)
- **Logs** : Toutes les opérations sont loggées

---

## 🔗 Liens connexes

- **Groupes Non-CIT** : `/GroupeNonCit` (gestion des quotas permanents)
- **Créer un quota historique** : `/Quota/Create`
- **Détails d'un quota** : `/Quota/Details/{id}`
- **Modifier un quota** : `/Quota/Edit/{id}`
- **Supprimer un quota** : `/Quota/Delete/{id}`

---

## ✅ Checklist d'utilisation

Avant de créer ou modifier un quota historique :

- [ ] Vérifier que le groupe "Douaniers" existe
- [ ] Vérifier qu'aucun quota n'existe déjà pour cette date
- [ ] S'assurer que les valeurs sont correctes (QuotaJour, QuotaNuit)
- [ ] Ajouter des commentaires si nécessaire pour référence future
- [ ] Utiliser "Groupes Non-CIT" pour gérer les quotas permanents actuels

---

*Document créé le : 2025-01-XX*
*Dernière mise à jour : 2025-01-XX*
