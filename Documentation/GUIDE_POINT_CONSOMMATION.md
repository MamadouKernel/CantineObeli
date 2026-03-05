# 📊 Guide: Créer un Point de Consommation

## Date: 05/03/2026

---

## 🎯 Qu'est-ce qu'un Point de Consommation?

Un **Point de Consommation** est un enregistrement qui marque qu'une commande a été effectivement consommée par un utilisateur. Il sert à:

- ✅ Suivre la consommation réelle des employés
- ✅ Calculer les statistiques de fréquentation
- ✅ Générer les rapports de consommation
- ✅ Facturer correctement les repas

---

## 📍 Comment Accéder?

### Via le Menu

```
Menu → Paramètres → Points de Consommation → Créer
```

### Via l'URL Directe

```
/PointsConsommation/Create
```

---

## 👥 Qui Peut Créer?

- ✅ Administrateur
- ✅ RH
- ✅ Prestataire Cantine
- ❌ Employé (ne peut pas créer)

---

## 📋 Formulaire de Création

### Champs Obligatoires

#### 1. Utilisateur
- **Type:** Liste déroulante
- **Description:** Sélectionner l'employé concerné
- **Recherche:** Tapez le nom ou le matricule
- **Exemple:** "KOUASSI Jean (MAT001)"

#### 2. Date de Consommation
- **Type:** Date
- **Description:** Date du repas consommé
- **Format:** JJ/MM/AAAA
- **Exemple:** 05/03/2026

#### 3. Période
- **Type:** Liste déroulante
- **Options:**
  - Jour (Déjeuner)
  - Nuit (Dîner)
- **Exemple:** Jour

#### 4. Formule
- **Type:** Liste déroulante
- **Description:** Menu consommé
- **Exemple:** "Menu Standard - Poulet Braisé"

### Champs Optionnels

#### 5. Quantité
- **Type:** Nombre
- **Valeur par défaut:** 1
- **Description:** Nombre de repas
- **Exemple:** 1

#### 6. Montant
- **Type:** Nombre décimal
- **Description:** Prix du repas
- **Calculé automatiquement** selon la formule
- **Exemple:** 2500 FCFA

#### 7. Commentaire
- **Type:** Texte libre
- **Description:** Notes additionnelles
- **Exemple:** "Repas pris en retard"

---

## 🔧 Processus de Création

### Méthode 1: Création Manuelle

**Étape 1:** Accéder au formulaire
```
Menu → Paramètres → Points de Consommation → Créer
```

**Étape 2:** Remplir les champs
```
1. Sélectionner l'utilisateur
2. Choisir la date
3. Sélectionner la période (Jour/Nuit)
4. Choisir la formule
5. Vérifier la quantité (1 par défaut)
6. Vérifier le montant (calculé auto)
7. Ajouter un commentaire (optionnel)
```

**Étape 3:** Valider
```
Cliquer sur "Créer" ou "Enregistrer"
```

**Étape 4:** Confirmation
```
Message de succès: "Point de consommation créé avec succès"
```

### Méthode 2: Création Automatique

Les points de consommation sont **créés automatiquement** quand:

1. ✅ Une commande est marquée comme "Consommée"
2. ✅ Le prestataire valide la consommation
3. ✅ Le système détecte une consommation via QR code (si implémenté)

---

## 💡 Cas d'Usage

### Cas 1: Consommation Normale

**Situation:** Un employé a consommé son repas normalement

**Action:**
1. Le prestataire marque la commande comme "Consommée"
2. Le système crée automatiquement le point de consommation
3. Aucune action manuelle nécessaire

### Cas 2: Consommation Sans Commande

**Situation:** Un employé a mangé sans avoir commandé (exception)

**Action:**
1. Aller dans "Créer Point de Consommation"
2. Sélectionner l'employé
3. Remplir les informations
4. Créer manuellement le point

**Commentaire:** "Consommation exceptionnelle sans commande préalable"

### Cas 3: Correction d'Erreur

**Situation:** Un point de consommation a été oublié

**Action:**
1. Vérifier qu'il n'existe pas déjà
2. Créer le point manuellement
3. Indiquer la raison dans le commentaire

**Commentaire:** "Correction - Point oublié le 05/03/2026"

### Cas 4: Visiteur

**Situation:** Un visiteur a consommé un repas

**Action:**
1. Créer une commande visiteur d'abord
2. Marquer la commande comme consommée
3. Le point est créé automatiquement

---

## ⚠️ Règles Importantes

### Unicité

- ❌ **Un seul point par utilisateur/date/période**
- ⚠️ Le système empêche les doublons
- ✅ Vérifiez avant de créer manuellement

### Cohérence

- ✅ La date doit correspondre à une formule existante
- ✅ La période doit correspondre à la formule
- ✅ L'utilisateur doit être actif

### Traçabilité

- ✅ Chaque point enregistre qui l'a créé
- ✅ Chaque point enregistre quand il a été créé
- ✅ Les modifications sont tracées

---

## 📊 Visualisation

### Consulter les Points

```
Menu → Commandes → Point Consommation CIT
```

### Statistiques

```
Menu → Commandes → Statistiques des Consommations
```

### Rapports

```
Menu → Commandes → Repporting
```

---

## 🔍 Vérifications

### Avant de Créer

1. ✅ Vérifier qu'une commande existe
2. ✅ Vérifier que le point n'existe pas déjà
3. ✅ Vérifier la date et la période
4. ✅ Vérifier que la formule existe pour cette date

### Après Création

1. ✅ Vérifier le message de confirmation
2. ✅ Consulter la liste des points
3. ✅ Vérifier les statistiques mises à jour

---

## 🚫 Erreurs Courantes

### Erreur 1: "Point de consommation déjà existant"

**Cause:** Un point existe déjà pour cet utilisateur/date/période

**Solution:**
- Vérifier dans la liste des points
- Si c'est une erreur, supprimer l'ancien point
- Recréer si nécessaire

### Erreur 2: "Utilisateur introuvable"

**Cause:** L'utilisateur n'existe pas ou est désactivé

**Solution:**
- Vérifier que l'utilisateur est actif
- Vérifier l'orthographe du matricule
- Contacter l'administrateur si nécessaire

### Erreur 3: "Formule introuvable"

**Cause:** Aucune formule n'existe pour cette date

**Solution:**
- Vérifier la date sélectionnée
- Créer une formule pour cette date d'abord
- Réessayer

### Erreur 4: "Date invalide"

**Cause:** La date est dans un format incorrect

**Solution:**
- Utiliser le format JJ/MM/AAAA
- Utiliser le sélecteur de date
- Vérifier que la date n'est pas trop ancienne

---

## 📞 Support

**Accès:** Administrateur, RH, Prestataire uniquement

**En cas de problème:**
- Contactez votre Administrateur
- Vérifiez les logs système
- Consultez la documentation technique

---

## 🎓 Bonnes Pratiques

### 1. Privilégier l'Automatique

- ✅ Laissez le système créer les points automatiquement
- ⚠️ Ne créez manuellement qu'en cas d'exception
- ✅ Documentez toujours les créations manuelles

### 2. Vérifier Avant de Créer

- ✅ Toujours vérifier qu'un point n'existe pas déjà
- ✅ Vérifier la cohérence des données
- ✅ Ajouter un commentaire explicatif

### 3. Traçabilité

- ✅ Toujours indiquer un commentaire pour les créations manuelles
- ✅ Noter la raison de la création
- ✅ Garder une trace des corrections

### 4. Contrôle Régulier

- ✅ Vérifier régulièrement les points créés
- ✅ Comparer avec les commandes
- ✅ Corriger les incohérences rapidement

---

## 📈 Statistiques

Les points de consommation alimentent:

- 📊 Tableau de bord de consommation
- 📈 Graphiques de fréquentation
- 💰 Calculs de facturation
- 📋 Rapports mensuels
- 🎯 Indicateurs de performance

---

**Date:** 05/03/2026  
**Version:** 1.0  
**Auteur:** Équipe O'Beli

