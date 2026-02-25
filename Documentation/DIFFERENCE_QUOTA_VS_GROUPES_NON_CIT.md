# Différence entre Quota (Historique) et Groupes Non-CIT

## 📋 Vue d'ensemble

Ces deux vues gèrent les quotas pour les groupes non-CIT, mais avec des objectifs et des fonctionnalités différents :

- **`/Quota`** : Gestion de l'**historique** des quotas journaliers (archivage et référence)
- **`/GroupeNonCit`** : Gestion des **quotas permanents** actuels (utilisation quotidienne)

---

## 🔍 Tableau comparatif

| Aspect | Quota (Historique) | Groupes Non-CIT |
|--------|-------------------|-----------------|
| **URL** | `/Quota` | `/GroupeNonCit` |
| **Objectif** | Historique et archivage | Gestion active des quotas permanents |
| **Type de quotas** | Quotas journaliers (par date) | Quotas permanents (toujours actifs) |
| **Période** | Spécifique à une date | Permanents (tous les jours) |
| **Décrémentation** | Peut être modifiée manuellement | Ne se décrémente pas (toujours disponible) |
| **Utilisation** | Référence historique | Utilisation quotidienne active |
| **Création** | Manuelle pour référence | Manuelle pour gestion active |
| **Modification** | Modifiable (historique) | Modifiable (quotas permanents) |
| **Suppression** | Soft delete (archivage) | Soft delete (désactivation) |
| **Données** | Table `QuotasJournaliers` | Table `GroupesNonCit` |

---

## 📊 Différences détaillées

### 1. **Objectif et utilisation**

#### Quota (Historique) - `/Quota`

- **Objectif** : Consulter et archiver l'historique des quotas journaliers
- **Utilisation** : Référence pour analyser les quotas passés
- **Cas d'usage** : 
  - Voir les quotas alloués à une date spécifique
  - Archiver les quotas d'une période
  - Analyser l'historique de consommation

#### Groupes Non-CIT - `/GroupeNonCit`

- **Objectif** : Gérer les quotas permanents des groupes non-CIT
- **Utilisation** : Configuration active pour les commandes quotidiennes
- **Cas d'usage** :
  - Définir les quotas permanents d'un groupe
  - Modifier les quotas actuels
  - Gérer les restrictions (standard uniquement)

---

### 2. **Type de quotas**

#### Quota (Historique)

- **Type** : Quotas journaliers (liés à une date spécifique)
- **Exemple** : 
  - Douaniers, 15/12/2025 : 50 plats jour, 30 plats nuit
  - Douaniers, 16/12/2025 : 60 plats jour, 40 plats nuit
- **Caractéristique** : Chaque quota est unique pour une date donnée

#### Groupes Non-CIT

- **Type** : Quotas permanents (toujours actifs)
- **Exemple** :
  - Douaniers : 50 plats jour (permanent), 30 plats nuit (permanent)
- **Caractéristique** : Les quotas s'appliquent à tous les jours

---

### 3. **Structure des données**

#### Quota (Historique)

**Table** : `QuotasJournaliers`

```csharp
- Id : Guid
- GroupeNonCitId : Guid (référence au groupe)
- Date : DateTime (date spécifique du quota)
- QuotaJour : int (quota pour cette date)
- QuotaNuit : int (quota pour cette date)
- PlatsConsommesJour : int (consommés ce jour-là)
- PlatsConsommesNuit : int (consommés ce jour-là)
- PlatsRestantsJour : int (calculé : QuotaJour - Consommés)
- PlatsRestantsNuit : int (calculé : QuotaNuit - Consommés)
```

**Relation** : Un quota historique est lié à un groupe et à une date spécifique.

#### Groupes Non-CIT

**Table** : `GroupesNonCit`

```csharp
- Id : Guid
- Nom : string (ex: "Douaniers")
- CodeGroupe : string (ex: "DOU")
- Description : string
- QuotaJournalier : int (quota permanent jour)
- QuotaNuit : int (quota permanent nuit)
- RestrictionFormuleStandard : bool
```

**Relation** : Un groupe a des quotas permanents qui s'appliquent tous les jours.

---

### 4. **Affichage dans les vues**

#### Quota (Historique)

**Colonnes du tableau** :
- Groupe
- **Date** (spécifique)
- Quota Jour
- Quota Nuit
- **Consommé Jour** (pour cette date)
- **Consommé Nuit** (pour cette date)
- **Restant Jour** (calculé)
- **Restant Nuit** (calculé)
- Actions (Voir, Modifier, Supprimer)

**Tri** : Par date décroissante (plus récent en premier)

#### Groupes Non-CIT

**Colonnes du tableau** :
- Groupe
- Description
- **Quota Jour** (permanent)
- **Quota Nuit** (permanent)
- **Standard Uniquement** (restriction)
- Actions (Voir détails, Modifier)

**Tri** : Par nom de groupe (alphabétique)

---

### 5. **Création et modification**

#### Quota (Historique)

**Création** :
- Sélectionner une **date spécifique**
- Définir les quotas pour cette date
- Optionnel : Définir les plats déjà consommés (par défaut 0)
- **Restriction** : Uniquement pour le groupe "Douaniers"

**Modification** :
- Modifier les quotas d'une date spécifique
- Modifier les plats consommés
- Changer la date (si aucun doublon)

#### Groupes Non-CIT

**Création** :
- Définir le nom du groupe
- Définir les **quotas permanents** (jour et nuit)
- Définir les restrictions (standard uniquement)
- **Pas de date** : Les quotas s'appliquent tous les jours

**Modification** :
- Modifier les quotas permanents
- Modifier les restrictions
- Modifier la description

---

### 6. **Utilisation dans les commandes**

#### Quota (Historique)

- **Non utilisé** pour les commandes actives
- Sert uniquement à l'**archivage** et à la **référence**
- Les commandes ne vérifient pas les quotas historiques

#### Groupes Non-CIT

- **Utilisé activement** pour les commandes
- Lors de la création d'une commande Douaniers :
  - Le système vérifie les quotas permanents du groupe
  - Les quotas permanents sont utilisés pour valider les commandes
  - Les restrictions (standard uniquement) sont appliquées

---

### 7. **Décrémentation** (Réduction automatique du quota)

#### Qu'est-ce que la "décrémentation" ?

**Décrémenter** signifie **réduire automatiquement** le quota lorsqu'une commande est validée.

**Exemple** :
- Quota initial : 50 plats
- Commande validée : 5 plats
- Quota après décrémentation : 45 plats (50 - 5 = 45)

#### Quota (Historique)

- **Les "Plats Consommés" peuvent être modifiés manuellement** lors de la création/modification
- **Pas de décrémentation automatique** : Vous saisissez vous-même combien de plats ont été consommés pour référence
- Les plats restants sont calculés automatiquement : `Restant = Quota - Consommé`
- **Exemple** : Vous créez un quota historique pour le 15/12/2025 avec 50 plats alloués, puis vous saisissez manuellement que 45 plats ont été consommés ce jour-là

#### Groupes Non-CIT

- **Ne se décrémente pas du tout** (ni automatiquement, ni manuellement)
- Les quotas sont **permanents** et **restent toujours identiques**
- **Exemple** : Si vous définissez 50 plats pour les Douaniers, ce quota reste toujours à 50 plats, même après des commandes
- Contrairement aux quotas des formules (`FormuleJour`) qui se décrémentent automatiquement lors de la validation des commandes

---

## 💡 Exemple concret

### Scénario : Gestion des quotas Douaniers

#### Dans Groupes Non-CIT (`/GroupeNonCit`)

**Configuration permanente** :
- Groupe : Douaniers
- Quota Jour : 50 plats (permanent)
- Quota Nuit : 30 plats (permanent)
- Standard Uniquement : Oui

**Utilisation** :
- Tous les jours, les Douaniers peuvent commander jusqu'à 50 plats pour le midi
- Tous les jours, les Douaniers peuvent commander jusqu'à 30 plats pour le soir
- Les quotas ne se décrémentent pas, ils sont toujours disponibles

#### Dans Quota (Historique) (`/Quota`)

**Archivage historique** :
- Date : 15/12/2025
- Groupe : Douaniers
- Quota Jour : 50 plats
- Quota Nuit : 30 plats
- Consommé Jour : 45 plats (saisi manuellement pour référence)
- Consommé Nuit : 25 plats (saisi manuellement pour référence)
- Restant Jour : 5 plats (calculé)
- Restant Nuit : 5 plats (calculé)

**Utilisation** :
- Référence historique pour analyser la consommation du 15/12/2025
- Ne sert pas à valider les commandes actuelles

---

## 🔄 Flux de travail recommandé

### Pour gérer les quotas actuels

1. **Aller dans `/GroupeNonCit`**
2. **Créer ou modifier** un groupe avec ses quotas permanents
3. Les quotas sont **immédiatement actifs** pour toutes les commandes

### Pour archiver l'historique

1. **Aller dans `/Quota`**
2. **Créer un quota historique** pour une date spécifique
3. **Saisir les plats consommés** si nécessaire (par défaut 0)
4. Le quota est **archivé** pour référence future

---

## ⚠️ Points importants

### 1. **Migration vers Groupes Non-CIT**

Les quotas permanents sont maintenant gérés dans "Groupes Non-CIT". La vue Quota sert uniquement à l'historique.

### 2. **Quotas permanents vs historiques**

- **Permanents** (`/GroupeNonCit`) : Utilisés pour les commandes actuelles
- **Historiques** (`/Quota`) : Archivage et référence uniquement

### 3. **Ne pas confondre**

- **Quota (Historique)** : Pour voir l'historique des quotas par date
- **Groupes Non-CIT** : Pour gérer les quotas permanents actuels

### 4. **Recommandation**

- **Utiliser `/GroupeNonCit`** pour la gestion quotidienne
- **Utiliser `/Quota`** uniquement pour archiver des données historiques

---

## 📝 Résumé

| Question | Quota (Historique) | Groupes Non-CIT |
|----------|-------------------|-----------------|
| **Quand l'utiliser ?** | Pour archiver l'historique | Pour gérer les quotas actuels |
| **Quelle date ?** | Date spécifique | Tous les jours (permanent) |
| **Se décrémente ?** | Les "Plats Consommés" peuvent être modifiés manuellement pour référence | Non, les quotas restent toujours identiques (permanent) |
| **Utilisé pour les commandes ?** | Non (archivage uniquement) | Oui (validation active) |
| **Qui peut créer ?** | Admin/RH (historique) | Admin/RH (quotas permanents) |

---

## ✅ Checklist : Quelle vue utiliser ?

### Utiliser `/GroupeNonCit` si :

- [ ] Vous voulez gérer les quotas permanents actuels
- [ ] Vous voulez créer ou modifier un groupe
- [ ] Vous voulez définir les quotas pour tous les jours
- [ ] Vous voulez gérer les restrictions (standard uniquement)

### Utiliser `/Quota` si :

- [ ] Vous voulez archiver l'historique des quotas
- [ ] Vous voulez voir les quotas d'une date spécifique
- [ ] Vous voulez enregistrer les plats consommés pour référence
- [ ] Vous voulez analyser l'historique de consommation

---

*Document créé le : 2025-01-XX*
*Dernière mise à jour : 2025-01-XX*

