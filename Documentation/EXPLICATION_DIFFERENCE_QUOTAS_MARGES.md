# 📊 Différence entre Quotas et Marges

## 🎯 Vue d'ensemble

Les **quotas** et les **marges** sont deux concepts complémentaires dans le système de gestion des commandes instantanées. Ils fonctionnent ensemble pour contrôler le nombre de plats disponibles, mais ont des rôles et des caractéristiques différents.

---

## 🔑 Définitions

### 📦 **QUOTAS** (QuotaJourRestant / QuotaNuitRestant)

Les quotas représentent le **nombre initial de plats disponibles** pour chaque période. Ce sont les plats "principaux" ou "de base" prévus pour la journée.

- **QuotaJourRestant** : Nombre de plats disponibles pour la période **Jour** (midi, avant 18h)
- **QuotaNuitRestant** : Nombre de plats disponibles pour la période **Nuit** (soir, à partir de 18h)

### 🎁 **MARGES** (MargeJourRestante / MargeNuitRestante)

Les marges représentent des **plats supplémentaires** disponibles après épuisement des quotas. Ce sont des plats "de secours" ou "de réserve" pour gérer les imprévus.

- **MargeJourRestante** : Plats supplémentaires pour la période **Jour** (indépendante)
- **MargeNuitRestante** : Plats supplémentaires pour la période **Nuit** (indépendante)

---

## 🔄 Différences principales

| Aspect | QUOTAS | MARGES |
|--------|--------|--------|
| **Rôle** | Plats principaux/de base | Plats supplémentaires/de secours |
| **Utilisation** | Utilisés en premier | Utilisés après épuisement des quotas |
| **Priorité** | Priorité 1 | Priorité 2 |
| **Objectif** | Couvrir la demande normale | Gérer les imprévus et la demande supplémentaire |
| **Gestion** | Définis lors de la création des formules | Paramétrables par RH/Admin via "Gestion des Marges" |
| **Flexibilité** | Moins flexible (basé sur les quantités prévues) | Très flexible (ajustable selon les besoins) |

---

## 🔄 Ordre d'utilisation

### Lors de la validation d'une commande

#### Période Jour (avant 18h)
```
1️⃣ D'abord : Décrémenter QuotaJourRestant
   ↓
2️⃣ Ensuite : Si QuotaJourRestant = 0, décrémenter MargeJourRestante
   ↓
3️⃣ Si les deux sont à 0 : Plus de plats disponibles
```

#### Période Nuit (à partir de 18h)
```
1️⃣ D'abord : Décrémenter QuotaNuitRestant
   ↓
2️⃣ Ensuite : Si QuotaNuitRestant = 0, décrémenter MargeNuitRestante
   ↓
3️⃣ Si les deux sont à 0 : Plus de plats disponibles
```

---

## 📊 Exemple concret

### Scénario initial
- **Formule** : Standard 1 du lundi
- **QuotaJourRestant** : 10 plats (plats principaux pour le midi)
- **QuotaNuitRestant** : 8 plats (plats principaux pour le soir)
- **MargeJourRestante** : 3 plats (plats supplémentaires pour le midi)
- **MargeNuitRestante** : 2 plats (plats supplémentaires pour le soir)

### Déroulement

#### Matin (période Jour)
1. **Validation de 8 commandes** :
   - `QuotaJourRestant` : 10 → 2 (8 plats utilisés)
   - `MargeJourRestante` : 3 → 3 (non utilisée)
   - **Total disponible** : 2 + 3 = 5 plats

2. **Validation de 2 commandes supplémentaires** :
   - `QuotaJourRestant` : 2 → 0 (2 plats utilisés)
   - `MargeJourRestante` : 3 → 3 (non utilisée)
   - **Total disponible** : 0 + 3 = 3 plats

3. **Validation de 3 commandes supplémentaires** :
   - `QuotaJourRestant` : 0 (déjà épuisé)
   - `MargeJourRestante` : 3 → 0 (3 plats utilisés)
   - **Total disponible** : 0 + 0 = 0 plats
   - **Résultat** : ❌ Plus de commandes instantanées Jour possibles

#### Soir (période Nuit, après 18h)
1. **Validation de 8 commandes** :
   - `QuotaNuitRestant` : 8 → 0 (8 plats utilisés)
   - `MargeNuitRestante` : 2 → 2 (non utilisée)
   - **Total disponible** : 0 + 2 = 2 plats

2. **Validation de 2 commandes supplémentaires** :
   - `QuotaNuitRestant` : 0 (déjà épuisé)
   - `MargeNuitRestante` : 2 → 0 (2 plats utilisés)
   - **Total disponible** : 0 + 0 = 0 plats
   - **Résultat** : ❌ Plus de commandes instantanées Nuit possibles

---

## 💡 Analogie simple

Imaginez un restaurant :

### 🍽️ **QUOTAS** = Les plats préparés à l'avance
- Ce sont les plats que le chef a préparés en prévision de la demande normale
- Exemple : 10 plats préparés pour le midi, 8 pour le soir
- Si tous les plats sont servis, il faut utiliser les marges

### 🥘 **MARGES** = Les plats de secours
- Ce sont des plats supplémentaires préparés pour gérer les imprévus
- Exemple : 3 plats supplémentaires pour le midi, 2 pour le soir
- Utilisés seulement si les quotas sont épuisés

---

## 🎯 Calcul du total disponible

### Pour la période Jour
```
Total disponible Jour = QuotaJourRestant + MargeJourRestante
```

### Pour la période Nuit
```
Total disponible Nuit = QuotaNuitRestant + MargeNuitRestante
```

### Exemple
- **QuotaJourRestant** : 10
- **MargeJourRestante** : 3
- **Total disponible Jour** : 10 + 3 = **13 plats**

---

## 🔧 Gestion et paramétrage

### Quotas
- **Définis** : Lors de la création/modification des formules
- **Basés sur** : Les quantités prévues pour chaque période
- **Gérés par** : PrestataireCantine lors de la génération des commandes
- **Modification** : Via la gestion des quantités par date

### Marges
- **Définies** : Par les RH et Administrateurs
- **Basées sur** : Les besoins supplémentaires et les imprévus
- **Gérées par** : RH/Admin via "Gestion des Marges"
- **Modification** : Via l'interface dédiée "Gestion des Marges"

---

## 📋 Résumé visuel

```
┌─────────────────────────────────────────────────┐
│           SYSTÈME DE QUOTAS ET MARGES            │
├─────────────────────────────────────────────────┤
│                                                  │
│  PÉRIODE JOUR (Midi, avant 18h)                 │
│  ┌──────────────────────────────────────────┐   │
│  │ 1️⃣ QUOTA JOUR (10 plats)                  │   │
│  │    → Utilisé en premier                   │   │
│  │    → Plats principaux                     │   │
│  └──────────────────────────────────────────┘   │
│           ↓ (si épuisé)                          │
│  ┌──────────────────────────────────────────┐   │
│  │ 2️⃣ MARGE JOUR (3 plats)                   │   │
│  │    → Utilisé ensuite                      │   │
│  │    → Plats supplémentaires                │   │
│  └──────────────────────────────────────────┘   │
│                                                  │
│  PÉRIODE NUIT (Soir, à partir de 18h)           │
│  ┌──────────────────────────────────────────┐   │
│  │ 1️⃣ QUOTA NUIT (8 plats)                   │   │
│  │    → Utilisé en premier                   │   │
│  │    → Plats principaux                     │   │
│  └──────────────────────────────────────────┘   │
│           ↓ (si épuisé)                          │
│  ┌──────────────────────────────────────────┐   │
│  │ 2️⃣ MARGE NUIT (2 plats)                   │   │
│  │    → Utilisé ensuite                      │   │
│  │    → Plats supplémentaires                │   │
│  └──────────────────────────────────────────┘   │
│                                                  │
└─────────────────────────────────────────────────┘
```

---

## ✅ Points clés à retenir

1. **Quotas** = Plats principaux, utilisés en premier
2. **Marges** = Plats supplémentaires, utilisés après épuisement des quotas
3. **Ordre** : Quotas d'abord, puis marges
4. **Total disponible** = Quota + Marge
5. **Gestion** : Quotas par PrestataireCantine, Marges par RH/Admin
6. **Indépendance** : Marges jour et nuit sont indépendantes

---

## 🔍 Questions fréquentes

### Q1 : Pourquoi avoir deux systèmes (quotas et marges) ?
**R** : Pour gérer à la fois la demande normale (quotas) et les imprévus (marges), avec une flexibilité accrue.

### Q2 : Que se passe-t-il si les quotas sont épuisés mais qu'il reste des marges ?
**R** : Les commandes instantanées peuvent encore être créées en utilisant les marges disponibles.

### Q3 : Les marges sont-elles obligatoires ?
**R** : Non, elles peuvent être à 0. Mais elles permettent de gérer les imprévus et la demande supplémentaire.

### Q4 : Peut-on avoir des quotas à 0 et des marges > 0 ?
**R** : Oui, mais ce n'est pas recommandé. Les quotas devraient normalement être > 0 pour couvrir la demande de base.

### Q5 : Les quotas et marges sont-ils liés ?
**R** : Non, ils sont indépendants. Les marges sont utilisées seulement après épuisement des quotas, mais leurs valeurs sont définies séparément.

