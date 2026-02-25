# 💰 Explication : Vue "Paramètres de Facturation"

## 📋 Vue d'ensemble

Cette vue permet aux **RH** et **Administrateurs** de configurer les règles de facturation automatique des commandes non consommées. Elle définit les conditions et montants à facturer lorsque les utilisateurs commandent mais ne consomment pas leurs repas.

**URL** : `https://localhost:7021/ParametresFacturation`

---

## 🎯 Objectif

Configurer les paramètres qui déterminent :
- **Si** la facturation des commandes non consommées est activée
- **Combien** facturer (pourcentage du prix)
- **Quand** facturer (absences gratuites, délai d'annulation)
- **Quelles** commandes facturer (week-end, jours fériés)

---

## 🔐 Accès et autorisations

### Rôles autorisés
- ✅ **Administrateur**
- ✅ **RH** (Ressources Humaines)
- ❌ **PrestataireCantine** (non autorisé)
- ❌ **Employé** (non autorisé)

---

## 📊 Structure de la vue

La vue est organisée en **5 sections principales** :

### 1. **Activation de la facturation** 🔘

#### Fonctionnalité
- **Type** : Switch (interrupteur on/off)
- **Clé de configuration** : `FACTURATION_NON_CONSOMMEES_ACTIVE`
- **Valeur par défaut** : `false` (désactivé)

#### Description
Active ou désactive complètement la facturation des commandes non consommées.

**Si désactivé** :
- Aucune commande non consommée ne sera facturée
- Toutes les autres règles sont ignorées

**Si activé** :
- Les règles de facturation ci-dessous s'appliquent

---

### 2. **Montant de la Facturation** 💵

#### Pourcentage à facturer
- **Type** : Champ numérique (0-100)
- **Clé de configuration** : `FACTURATION_POURCENTAGE`
- **Valeur par défaut** : `100` (100% du prix)
- **Plage** : 0 à 100

#### Description
Définit le pourcentage du prix de la commande à facturer.

**Exemples** :
- **100%** : L'utilisateur paie le prix complet de la commande non consommée
- **50%** : L'utilisateur paie la moitié du prix
- **0%** : Aucune facturation (gratuit)

**Calcul** :
```
Montant à facturer = (Prix de la commande × Pourcentage) / 100
```

**Exemple concret** :
- Commande non consommée : 2800 FCFA (formule améliorée)
- Pourcentage configuré : 50%
- Montant facturé : (2800 × 50) / 100 = **1400 FCFA**

---

### 3. **Absences Gratuites** 🎁

#### Nombre d'absences gratuites par mois
- **Type** : Champ numérique (≥ 0)
- **Clé de configuration** : `FACTURATION_ABSENCES_GRATUITES`
- **Valeur par défaut** : `0` (aucune absence gratuite)

#### Description
Nombre de commandes non consommées tolérées gratuitement chaque mois par utilisateur.

**Fonctionnement** :
- Les premières absences du mois (selon le nombre configuré) ne sont **pas facturées**
- Les absences suivantes sont facturées selon le pourcentage configuré
- Le compteur se réinitialise chaque mois

**Exemple concret** :
- Configuration : 2 absences gratuites par mois
- Utilisateur A :
  - 1ère absence du mois : **Gratuite** ✅
  - 2ème absence du mois : **Gratuite** ✅
  - 3ème absence du mois : **Facturée** 💰
  - 4ème absence du mois : **Facturée** 💰

**Note** : Les absences gratuites sont comptées par ordre chronologique (les plus anciennes d'abord).

---

### 4. **Délai d'Annulation Gratuite** ⏰

#### Délai en heures avant la consommation
- **Type** : Champ numérique (≥ 0)
- **Clé de configuration** : `FACTURATION_DELAI_ANNULATION_GRATUITE`
- **Valeur par défaut** : `24` heures

#### Description
Si l'utilisateur annule sa commande avant ce délai, elle ne sera **pas facturée**.

**Fonctionnement** :
- L'utilisateur peut annuler sa commande gratuitement jusqu'à X heures avant la date/heure de consommation
- Après ce délai, l'annulation ne change rien : la commande sera facturée si non consommée

**Exemple concret** :
- Configuration : 24 heures
- Commande pour le **lundi 12h00**
- Annulation le **dimanche 10h00** (26h avant) : **Gratuite** ✅
- Annulation le **dimanche 14h00** (22h avant) : **Facturée** 💰 (trop tard)

**Note** : Ce délai s'applique uniquement aux annulations. Si la commande n'est simplement pas consommée (sans annulation), elle sera facturée selon les autres règles.

---

### 5. **Options Spéciales** ⚙️

#### Facturation week-end
- **Type** : Case à cocher
- **Clé de configuration** : `FACTURATION_WEEKEND`
- **Valeur par défaut** : `false` (non facturé)

#### Description
Détermine si les commandes non consommées du **samedi et dimanche** doivent être facturées.

**Si désactivé** :
- Les commandes du week-end ne sont **jamais facturées**, même si elles ne sont pas consommées
- Elles comptent quand même pour les absences gratuites

**Si activé** :
- Les commandes du week-end sont facturées selon les mêmes règles que les jours de semaine

---

#### Facturation jours fériés
- **Type** : Case à cocher
- **Clé de configuration** : `FACTURATION_JOURS_FERIES`
- **Valeur par défaut** : `false` (non facturé)

#### Description
Détermine si les commandes non consommées les **jours fériés** doivent être facturées.

**Si désactivé** :
- Les commandes des jours fériés ne sont **jamais facturées**, même si elles ne sont pas consommées
- Elles comptent quand même pour les absences gratuites

**Si activé** :
- Les commandes des jours fériés sont facturées selon les mêmes règles que les jours normaux

---

## 🔄 Fonctionnement technique

### Stockage des configurations

Les configurations sont stockées dans la table `ConfigurationsCommande` avec les clés suivantes :

| Clé | Description | Valeur par défaut |
|-----|-------------|-------------------|
| `FACTURATION_NON_CONSOMMEES_ACTIVE` | Activation de la facturation | "false" |
| `FACTURATION_POURCENTAGE` | Pourcentage à facturer (0-100) | "100" |
| `FACTURATION_ABSENCES_GRATUITES` | Nombre d'absences gratuites par mois | "0" |
| `FACTURATION_DELAI_ANNULATION_GRATUITE` | Délai d'annulation gratuite (heures) | "24" |
| `FACTURATION_WEEKEND` | Facturer le week-end | "false" |
| `FACTURATION_JOURS_FERIES` | Facturer les jours fériés | "false" |

### Ordre d'application des règles

Lors du calcul de la facturation, les règles sont appliquées dans cet ordre :

1. **Facturation désactivée** → Aucune facturation
2. **Week-end non facturé** → Gratuit si samedi/dimanche
3. **Jour férié non facturé** → Gratuit si jour férié
4. **Absences gratuites** → Gratuit si dans la limite mensuelle
5. **Délai d'annulation** → Gratuit si annulé à temps
6. **Facturation** → Appliquer le pourcentage configuré

---

## 📝 Exemples d'utilisation

### Scénario 1 : Activer la facturation complète

**Objectif** : Facturer 100% des commandes non consommées

1. Activer "Facturation des commandes non consommées"
2. Définir "Pourcentage à facturer" = 100
3. Définir "Absences gratuites" = 0
4. Cliquer sur "Enregistrer"

**Résultat** : Toutes les commandes non consommées seront facturées à 100% du prix.

---

### Scénario 2 : Facturation partielle avec tolérance

**Objectif** : Facturer 50% avec 2 absences gratuites par mois

1. Activer la facturation
2. Définir "Pourcentage" = 50
3. Définir "Absences gratuites" = 2
4. Cliquer sur "Enregistrer"

**Résultat** :
- Les 2 premières absences du mois : Gratuites
- Les absences suivantes : Facturées à 50% du prix

---

### Scénario 3 : Désactiver la facturation du week-end

**Objectif** : Ne jamais facturer les commandes du week-end

1. Activer la facturation
2. Décocher "Facturer les commandes non consommées le week-end"
3. Cliquer sur "Enregistrer"

**Résultat** : Les commandes du samedi et dimanche ne seront jamais facturées, même si non consommées.

---

### Scénario 4 : Configuration complète

**Configuration** :
- Facturation activée : ✅
- Pourcentage : 75%
- Absences gratuites : 3 par mois
- Délai annulation : 48 heures
- Week-end : Non facturé
- Jours fériés : Non facturé

**Résultat** :
- Les 3 premières absences du mois : Gratuites
- Les absences suivantes : Facturées à 75% du prix
- Annulation > 48h avant : Gratuite
- Week-end et jours fériés : Jamais facturés

---

## 🔗 Intégration avec d'autres services

### Service de facturation

Le service `FacturationService` utilise ces configurations pour :
- Calculer le montant à facturer pour chaque commande non consommée
- Déterminer si une commande doit être facturée ou non
- Appliquer les règles d'absences gratuites par utilisateur

### Service de facturation automatique

Le service `FacturationAutomatiqueService` utilise ces configurations pour :
- Déclencher automatiquement la facturation selon un planning
- Appliquer les règles configurées à toutes les commandes non consommées

### Vue PointConsommationCIT

La vue `PointConsommationCIT` utilise ces configurations pour :
- Afficher les montants à facturer
- Indiquer quelles commandes sont facturables ou non
- Calculer les totaux de facturation

---

## ⚠️ Points importants

### 1. **Absences gratuites par utilisateur**
- Les absences gratuites sont comptées **par utilisateur** et **par mois**
- Le compteur se réinitialise au début de chaque mois
- Les absences sont comptées dans l'ordre chronologique (les plus anciennes d'abord)

### 2. **Délai d'annulation**
- Le délai s'applique uniquement si l'utilisateur **annule** sa commande
- Si la commande n'est simplement pas consommée (sans annulation), elle sera facturée selon les autres règles

### 3. **Week-end et jours fériés**
- Si désactivés, ces commandes ne sont **jamais facturées**
- Elles comptent quand même pour les absences gratuites
- Elles peuvent être annulées gratuitement selon le délai configuré

### 4. **Validation**
- Le pourcentage doit être entre 0 et 100
- Les nombres d'absences gratuites et délai d'annulation doivent être ≥ 0

---

## 🐛 Dépannage

### Problème : La facturation ne fonctionne pas

**Solutions** :
1. Vérifier que "Facturation des commandes non consommées" est activée
2. Vérifier que le pourcentage est > 0
3. Vérifier que les commandes sont bien en statut "Non Récupérée" ou "Précommandée" (non consommée)

### Problème : Les absences gratuites ne fonctionnent pas

**Solutions** :
1. Vérifier que le nombre d'absences gratuites est > 0
2. Vérifier que le compteur est réinitialisé chaque mois
3. Vérifier que les absences sont comptées par utilisateur (par email)

### Problème : Le week-end est facturé alors qu'il ne devrait pas

**Solutions** :
1. Vérifier que "Facturer les commandes non consommées le week-end" est décochée
2. Vérifier que la date de consommation est bien un samedi ou dimanche
3. Recharger la page pour voir les paramètres actuels

---

## 📚 Références techniques

- **Contrôleur** : `ParametresFacturationController`
- **Service** : `IConfigurationService` / `ConfigurationService`
- **Service de facturation** : `IFacturationService` / `FacturationService`
- **Modèle** : `ConfigurationCommande`
- **Table** : `ConfigurationsCommande`

---

## ✅ Résumé

Cette vue permet de :
- ✅ Activer/désactiver la facturation des commandes non consommées
- ✅ Configurer le pourcentage à facturer (0-100%)
- ✅ Définir le nombre d'absences gratuites par mois
- ✅ Configurer le délai d'annulation gratuite
- ✅ Activer/désactiver la facturation du week-end
- ✅ Activer/désactiver la facturation des jours fériés
- ✅ Voir un résumé en temps réel des règles configurées

**Utilisateurs cibles** : RH et Administrateurs uniquement.

**Objectif principal** : Encourager les utilisateurs à annuler leurs commandes à temps s'ils ne peuvent pas être présents, tout en offrant une certaine flexibilité avec les absences gratuites.

