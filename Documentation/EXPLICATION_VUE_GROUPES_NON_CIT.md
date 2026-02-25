# 📊 Explication : Vue "Gestion des Groupes Non-CIT"

## 📋 Vue d'ensemble

Cette vue permet aux **RH** et **Administrateurs** de gérer les **groupes non-CIT**, c'est-à-dire les groupes d'utilisateurs externes à la CIT (Comité Interministériel de Travail) qui peuvent commander des repas. Ces groupes ont des quotas permanents et des restrictions spécifiques.

**URL** : `https://localhost:7021/GroupeNonCit`

---

## 🎯 Objectif

Gérer les groupes d'utilisateurs externes à la CIT qui peuvent commander des repas, avec :
- Des quotas permanents (jour et nuit)
- Des restrictions sur les formules (standard uniquement ou toutes les formules)
- Un suivi des commandes par groupe

---

## 🔐 Accès et autorisations

### Rôles autorisés
- ✅ **Administrateur**
- ✅ **RH** (Ressources Humaines)
- ❌ **PrestataireCantine** (non autorisé)
- ❌ **Employé** (non autorisé)

---

## 🔄 Gestion des groupes

### Principe
Les groupes non-CIT sont **créés manuellement** par les RH et Administrateurs. Il n'y a plus d'initialisation automatique.

### Groupe unique : Douaniers
**Important** : Seul le groupe **"Douaniers"** est conservé dans le système. Tous les autres groupes (Forces de l'Ordre, Sécurité, Visiteurs Officiels, etc.) sont automatiquement supprimés lors du chargement de la page, sauf s'ils ont des commandes associées.

### Création d'un nouveau groupe
1. Cliquer sur le bouton **"Créer un nouveau groupe"** dans l'en-tête
2. Remplir le formulaire :
   - **Nom du Groupe** (obligatoire) : Ex: "Douaniers"
   - **Code Groupe** (optionnel) : Ex: "DOU"
   - **Description** (optionnel) : Description du groupe
   - **Quota Permanent Jour** : Nombre de plats disponibles par jour pour le midi
   - **Quota Permanent Nuit** : Nombre de plats disponibles par jour pour le soir
   - **Restriction aux formules standard** : Cocher si le groupe ne peut commander que des formules standard
3. Cliquer sur **"Créer le groupe"**

### Groupe par défaut : Douaniers
Le système conserve uniquement le groupe **"Douaniers"** :
- **Nom** : "Douaniers"
- **Code** : "DOU" (recommandé)
- **Quota Jour** : À définir manuellement (ex: 50)
- **Quota Nuit** : À définir manuellement (ex: 30)
- **Restriction Standard** : Oui (recommandé)

**Note** : Les autres groupes sont automatiquement supprimés s'ils n'ont pas de commandes associées.

---

## 📊 Structure du tableau

Le tableau affiche **6 colonnes** :

### 1. **Groupe** 👥
- **Type** : Affichage (badge + nom)
- **Contenu** : 
  - Badge avec icône utilisateurs
  - Nom du groupe (ex: "Douaniers")
  - Code du groupe (si défini, ex: "Code: DOU")
- **Exemple** : 
  ```
  [👥] Douaniers
       Code: DOU
  ```

### 2. **Description** ℹ️
- **Type** : Affichage (texte)
- **Contenu** : Description du groupe
- **Exemple** : "Groupe des agents des douanes"
- **Si vide** : "Aucune description"

### 3. **Quota Jour** ☀️
- **Type** : Affichage (badge)
- **Contenu** : Nombre de plats disponibles par jour pour la période Jour (midi)
- **Affichage** :
  - Si défini : Badge jaune avec la valeur + "(permanent)"
  - Si non défini : Badge gris "Non défini"
- **Exemple** : "50 (permanent)" ou "Non défini"
- **Caractéristique** : **Quota permanent** (ne se décrémente pas, toujours disponible)

### 4. **Quota Nuit** 🌙
- **Type** : Affichage (badge)
- **Contenu** : Nombre de plats disponibles par jour pour la période Nuit (soir)
- **Affichage** :
  - Si défini : Badge bleu avec la valeur
  - Si non défini : Badge gris "Non défini"
- **Exemple** : "30" ou "Non défini"
- **Caractéristique** : **Quota permanent** (ne se décrémente pas, toujours disponible)

### 5. **Standard Uniquement** 🛡️
- **Type** : Affichage (badge)
- **Contenu** : Indique si le groupe est limité aux formules standard uniquement
- **Affichage** :
  - Si `true` : Badge vert "✅ Oui"
  - Si `false` : Badge gris "❌ Non"
- **Signification** :
  - **Oui** : Le groupe ne peut commander que des formules standard (Standard 1, Standard 2)
  - **Non** : Le groupe peut commander toutes les formules (Standard + Amélioré)

### 6. **Actions** ⚙️
- **Type** : Boutons d'action
- **Actions disponibles** :
  - **👁️ Voir les détails** : Affiche les détails du groupe et les statistiques de consommation
  - **✏️ Modifier** : Permet de modifier les paramètres du groupe (nom, description, quotas, restrictions)

### Bouton "Créer un nouveau groupe" ➕
- **Emplacement** : En-tête de la page (à droite)
- **Action** : Ouvre le formulaire de création d'un nouveau groupe
- **Fonctionnalité** : Permet aux RH et Administrateurs de créer manuellement de nouveaux groupes avec leurs quotas

---

## 🔍 Différence avec les quotas des formules

### Quotas des Groupes Non-CIT (cette vue)
- **Type** : Quotas **permanents** et **fixes**
- **Gestion** : Par groupe (ex: Douaniers = 50 plats/jour)
- **Décrémentation** : **Ne se décrémente pas** (toujours disponible)
- **Utilisation** : Pour les commandes de type "Groupe Non-CIT"
- **Exemple** : Les Douaniers ont toujours 50 plats disponibles par jour

### Quotas des Formules (FormuleJour)
- **Type** : Quotas **journaliers** et **variables**
- **Gestion** : Par formule et par date (ex: Standard 1 du 22/12 = 10 plats)
- **Décrémentation** : **Se décrémente** lors de la validation des commandes
- **Utilisation** : Pour les commandes instantanées des employés CIT
- **Exemple** : Si 5 commandes sont validées, il reste 5 plats

---

## 💡 Cas d'utilisation

### Scénario 1 : Créer un nouveau groupe (ex: Douaniers)
1. Accéder à `/GroupeNonCit`
2. Cliquer sur **"Créer un nouveau groupe"**
3. Remplir le formulaire :
   - Nom : "Douaniers"
   - Code : "DOU"
   - Description : "Groupe des agents des douanes"
   - Quota Jour : 50
   - Quota Nuit : 30
   - Restriction Standard : ✅ Oui
4. Cliquer sur **"Créer le groupe"**
5. Le groupe apparaît dans la liste avec ses quotas

### Scénario 2 : Voir tous les groupes
1. Accéder à `/GroupeNonCit`
2. Le système affiche tous les groupes non-CIT créés
3. Voir les quotas et restrictions de chaque groupe

### Scénario 3 : Modifier les quotas d'un groupe
1. Cliquer sur le bouton **✏️ Modifier** pour un groupe
2. Modifier les quotas jour/nuit si nécessaire
3. Sauvegarder les modifications

### Scénario 4 : Voir les statistiques d'un groupe
1. Cliquer sur le bouton **👁️ Voir les détails** pour un groupe
2. Voir les statistiques de consommation du jour
3. Voir l'historique des commandes

---

## 🔧 Fonctionnalités

### 1. Création d'un nouveau groupe
- **Bouton "Créer un nouveau groupe"** : Ouvre le formulaire de création
- **Formulaire** : Permet de définir le nom, code, description, quotas jour/nuit, et restrictions
- **Validation** : Vérifie que le nom du groupe n'existe pas déjà

### 2. Modification des groupes
- **Éditable** : Nom, Description, Quota Jour, Quota Nuit, Restriction Formule Standard, Code Groupe
- **Accès** : Via le bouton "Modifier" (✏️)

### 3. Consultation des détails
- **Statistiques** : Nombre de plats consommés aujourd'hui (Jour et Nuit)
- **Historique** : Liste des commandes du groupe
- **Accès** : Via le bouton "Voir les détails" (👁️)

---

## 📝 Exemple concret

### Groupe "Douaniers"
- **Nom** : Douaniers
- **Code** : DOU
- **Description** : Groupe des agents des douanes
- **Quota Jour** : 50 plats (permanent)
- **Quota Nuit** : 30 plats (permanent)
- **Standard Uniquement** : ✅ Oui

### Utilisation
- Les Douaniers peuvent commander jusqu'à **50 plats par jour** pour le midi
- Les Douaniers peuvent commander jusqu'à **30 plats par jour** pour le soir
- Les quotas sont **permanents** : ils ne se décrémentent pas, ils sont toujours disponibles
- Les Douaniers ne peuvent commander que des **formules standard** (pas d'amélioré)

---

## 🔄 Relation avec les commandes

### Type de commande
Les groupes non-CIT sont utilisés pour les commandes de type `TypeClientCommande.GroupeNonCit`.

### Création de commande
Lors de la création d'une commande pour un groupe non-CIT :
1. Sélectionner le type de client : "Groupe non-CIT"
2. Sélectionner le groupe (ex: Douaniers)
3. Le système vérifie les quotas permanents du groupe
4. Si la restriction "Standard Uniquement" est activée, seules les formules standard sont disponibles

### Exemple : Commande Douaniers
- **Type Client** : Groupe Non-CIT
- **Groupe** : Douaniers
- **Formules disponibles** : Standard 1, Standard 2 uniquement (pas d'Amélioré)
- **Quota** : Vérifie les quotas permanents (50 jour, 30 nuit)
- **Gratuit** : Les commandes Douaniers sont généralement gratuites (Montant = 0)

---

## ⚠️ Points importants

### Quotas permanents
- Les quotas des groupes non-CIT sont **permanents** et **ne se décrémentent pas**
- Contrairement aux quotas des formules qui se décrémentent lors de la validation
- Les quotas des groupes sont des **limites quotidiennes** qui se réinitialisent chaque jour

### Restrictions
- **Standard Uniquement = Oui** : Le groupe ne peut commander que des formules standard
- **Standard Uniquement = Non** : Le groupe peut commander toutes les formules

### Création
- Les groupes sont créés **manuellement** par les RH et Administrateurs
- Le bouton "Créer un nouveau groupe" ouvre le formulaire de création
- Chaque groupe peut avoir des quotas et restrictions personnalisés
- Les quotas sont définis lors de la création ou peuvent être modifiés ensuite

---

## 🎯 Résumé

Cette vue permet de :
- ✅ **Voir** tous les groupes non-CIT (Douaniers, Forces de l'Ordre, Sécurité, Visiteurs Officiels)
- ✅ **Consulter** les quotas permanents de chaque groupe
- ✅ **Vérifier** les restrictions (standard uniquement ou toutes formules)
- ✅ **Modifier** les paramètres des groupes (quotas, restrictions, description)
- ✅ **Consulter** les statistiques de consommation par groupe

Les quotas des groupes non-CIT sont **permanents** et **fixes**, contrairement aux quotas des formules qui sont **journaliers** et **variables**.

