# 📖 Manuel d'Utilisation Complet - Application O'Beli K

**Version** : 1.0  
**Date** : 2025-01-XX  
**Public cible** : Administrateurs, RH, Support Applicatif, Utilisateurs finaux

---

## 📑 Table des matières

1. [Introduction](#1-introduction)
2. [Guide de démarrage rapide](#2-guide-de-démarrage-rapide)
3. [Connexion et authentification](#3-connexion-et-authentification)
4. [Guide pour les Administrateurs](#4-guide-pour-les-administrateurs)
5. [Guide pour les RH](#5-guide-pour-les-rh)
6. [Guide pour les Employés](#6-guide-pour-les-employés)
7. [Guide pour les Prestataires de Cantine](#7-guide-pour-les-prestataires-de-cantine)
8. [Procédures courantes](#8-procédures-courantes)
9. [Dépannage et FAQ](#9-dépannage-et-faq)
10. [Glossaire et termes techniques](#10-glossaire-et-termes-techniques)

---

## 1. Introduction

### 1.1. Qu'est-ce que O'Beli K ?

**O'Beli K** est une application web de gestion de commandes de repas pour la Côte d'Ivoire. Elle permet de :

- ✅ Gérer les menus quotidiens (formules)
- ✅ Permettre aux employés de commander leurs repas
- ✅ Suivre les consommations et points de repas
- ✅ Gérer la facturation
- ✅ Générer des rapports et statistiques

### 1.2. Navigateurs supportés

- ✅ Google Chrome (recommandé)
- ✅ Microsoft Edge
- ✅ Mozilla Firefox
- ✅ Safari

**Note** : JavaScript doit être activé dans votre navigateur.

### 1.3. URL d'accès

**URL de production** : `https://[URL_PRODUCTION]`  
**URL de développement/test** : `https://localhost:7021`

---

## 2. Guide de démarrage rapide

### 2.1. Première connexion

1. **Ouvrez votre navigateur** et accédez à l'URL de l'application
2. **Cliquez sur "Se connecter"** si vous n'êtes pas encore connecté
3. **Saisissez vos identifiants** :
   - **Matricule** : Votre matricule d'employé
   - **Mot de passe** : Votre mot de passe (fourni par l'administrateur)
4. **Cochez "Se souvenir de moi"** si vous utilisez votre ordinateur personnel
5. **Cliquez sur "Connexion"**

### 2.2. Que faire en cas de problème de connexion ?

- ❌ **"Identifiants incorrects"** → Vérifiez votre matricule et mot de passe
- ❌ **"Compte désactivé"** → Contactez votre administrateur
- ❌ **"Mot de passe oublié"** → Utilisez le lien "Mot de passe oublié"

### 2.3. Navigation dans l'application

L'application dispose d'un **menu principal** en haut de la page :

- **Accueil** : Tableau de bord
- **Commandes** : Gestion des commandes
- **Menus** : Consultation des menus (Employés) ou Gestion (Admin/RH)
- **Paramètres** : Configuration (Admin/RH uniquement)
- **Mon profil** : Vos informations personnelles

---

## 3. Connexion et authentification

### 3.1. Se connecter à l'application

#### Procédure étape par étape

1. Accédez à la page de connexion
2. Saisissez votre **matricule** (exemple : `EMP001`)
3. Saisissez votre **mot de passe**
4. (Optionnel) Cochez **"Se souvenir de moi"** pour rester connecté 30 jours
5. Cliquez sur **"Connexion"**

#### Durée de session

- **Session normale** : 1 heure d'inactivité
- **"Se souvenir de moi"** : 30 jours (même après fermeture du navigateur)

### 3.2. Réinitialiser son mot de passe

#### Si vous avez oublié votre mot de passe

1. Sur la page de connexion, cliquez sur **"Mot de passe oublié"**
2. Saisissez votre **adresse email** (celle enregistrée dans votre profil)
3. Cliquez sur **"Envoyer"**
4. **Vérifiez votre boîte email** (vérifiez aussi les spams)
5. **Cliquez sur le lien** dans l'email (valide 24 heures)
6. Saisissez votre **nouveau mot de passe** (2 fois pour confirmation)
7. Cliquez sur **"Réinitialiser"**

**Important** : 
- Le lien est valide pendant **24 heures** seulement
- Le lien ne peut être utilisé qu'**une seule fois**
- Si vous ne recevez pas l'email, contactez votre administrateur

### 3.3. Changer son mot de passe (utilisateur connecté)

1. Cliquez sur votre **nom** en haut à droite → **"Mon profil"**
2. Cliquez sur **"Changer le mot de passe"**
3. Saisissez votre **mot de passe actuel**
4. Saisissez votre **nouveau mot de passe** (2 fois)
5. Cliquez sur **"Changer le mot de passe"**

### 3.4. Déconnexion

1. Cliquez sur votre **nom** en haut à droite
2. Cliquez sur **"Déconnexion"**

---

## 4. Guide pour les Administrateurs

### 4.1. Vue d'ensemble des fonctionnalités

En tant qu'**Administrateur**, vous avez accès à **toutes les fonctionnalités** de l'application :

- ✅ Gestion des utilisateurs
- ✅ Gestion des départements et fonctions
- ✅ Gestion des formules (menus)
- ✅ Gestion des commandes
- ✅ Configuration système
- ✅ Reporting et statistiques
- ✅ Facturation
- ✅ Gestion des prestataires

### 4.2. Gestion des utilisateurs

#### 4.2.1. Créer un nouvel utilisateur

**Procédure complète** :

1. **Menu** → **Paramètres** → **Utilisateurs**
2. Cliquez sur **"Créer un utilisateur"**
3. Remplissez le formulaire :
   - **Matricule** ⚠️ (obligatoire, unique)
   - **Nom** ⚠️ (obligatoire)
   - **Prénoms** ⚠️ (obligatoire)
   - **Email** (recommandé)
   - **Département** (sélectionner dans la liste)
   - **Fonction** (sélectionner dans la liste)
   - **Rôle** :
     - **Employé** : Utilisateur standard
     - **RH** : Ressources Humaines (gestion des formules, commandes, reporting)
     - **Administrateur** : Accès complet
     - **PrestataireCantine** : Pour les prestataires de cantine
4. **Mot de passe** :
   - Option 1 : Laissez vide → Le système génère un mot de passe temporaire
   - Option 2 : Définissez un mot de passe personnalisé
5. Cliquez sur **"Créer"**

**⚠️ Important** :
- Le matricule doit être **unique** (pas de doublon)
- Si un mot de passe temporaire est généré, **communiquez-le** à l'utilisateur
- L'utilisateur devra **changer son mot de passe** à la première connexion

#### 4.2.2. Modifier un utilisateur

1. **Menu** → **Paramètres** → **Utilisateurs**
2. Cliquez sur le bouton **"Modifier"** (icône crayon) de l'utilisateur
3. Modifiez les informations nécessaires
4. Cliquez sur **"Enregistrer"**

#### 4.2.3. Réinitialiser le mot de passe d'un utilisateur

1. **Menu** → **Paramètres** → **Utilisateurs**
2. Cliquez sur **"Réinitialiser mot de passe"** (icône clé)
3. Saisissez un **nouveau mot de passe** (2 fois)
4. Cliquez sur **"Réinitialiser"**

**Note** : L'utilisateur recevra un email avec le nouveau mot de passe.

#### 4.2.4. Désactiver un utilisateur (suppression)

1. **Menu** → **Paramètres** → **Utilisateurs**
2. Cliquez sur **"Supprimer"** (icône poubelle)
3. Confirmez la suppression

**Note** : La suppression est "douce" (soft delete). L'utilisateur est désactivé mais ses données sont conservées.

### 4.3. Gestion des départements et fonctions

#### 4.3.1. Créer un département

1. **Menu** → **Paramètres** → **Départements**
2. Cliquez sur **"Créer un département"**
3. Saisissez le **nom** du département
4. Cliquez sur **"Créer"**

#### 4.3.2. Créer une fonction

1. **Menu** → **Paramètres** → **Fonctions**
2. Cliquez sur **"Créer une fonction"**
3. Saisissez le **nom** de la fonction
4. Cliquez sur **"Créer"**

### 4.4. Gestion des formules (menus)

#### 4.4.1. Créer une formule

**Procédure détaillée** :

1. **Menu** → **Menus** → **Gérer les formules**
2. Sélectionnez la **période** (semaine) à afficher
3. Cliquez sur **"Créer une formule"**
4. Remplissez le formulaire :
   - **Date** ⚠️ : Date du menu
   - **Type de formule** :
     - **Améliorée** : Menu complet (entrée, plat, dessert)
     - **Standard 1** : Premier plat standard
     - **Standard 2** : Deuxième plat standard
   - **Selon le type** :
     - **Améliorée** : Entrée, Plat, Garniture, Dessert, Féculent, Légumes
     - **Standard** : Plat Standard 1/2, Garniture 1/2, Féculent, Légumes
   - **Quotas** (optionnel) :
     - Quota Jour (midi)
     - Quota Nuit (soir)
   - **Marges** (optionnel) : Marges supplémentaires
5. Cliquez sur **"Créer"**

#### 4.4.2. Importer des formules depuis Excel

1. **Menu** → **Menus** → **Importer**
2. Cliquez sur **"Télécharger le template Excel"** (si nécessaire)
3. Remplissez le fichier Excel avec vos formules
4. Cliquez sur **"Parcourir"** et sélectionnez votre fichier
5. Cliquez sur **"Importer"**
6. Vérifiez les erreurs éventuelles
7. Confirmez l'importation

**Format Excel requis** :
- Colonnes : Date, Type, Entrée, Plat, Garniture, Dessert, etc.
- Format de date : JJ/MM/AAAA

### 4.5. Configuration des commandes

#### 4.5.1. Configurer les périodes de blocage

1. **Menu** → **Paramètres** → **Configuration des commandes**
2. Configurez :
   - **Jour de blocage** : Généralement "Vendredi"
   - **Heure de blocage** : Généralement "12:00"
   - **Activation du blocage** : Cochez pour activer
3. Cliquez sur **"Enregistrer"**

**Effet** : Les commandes seront bloquées automatiquement chaque vendredi à 12h00.

### 4.6. Gestion des groupes non-CIT (Douaniers)

#### 4.6.1. Configurer les quotas Douaniers

1. **Menu** → **Paramètres** → **Quotas Permanents des Groupes**
2. Si le groupe "Douaniers" n'existe pas, cliquez sur **"Créer un groupe"**
3. Remplissez :
   - **Nom** : "Douaniers"
   - **Code** : "DOU" (optionnel)
   - **Quota Jour** : Nombre de plats pour le midi (ex: 50)
   - **Quota Nuit** : Nombre de plats pour le soir (ex: 30)
   - **Restriction aux formules standard** : Cochez (recommandé)
4. Cliquez sur **"Créer"** ou **"Enregistrer"**

**Note** : Les quotas sont **permanents** (ne se décrémentent pas). Ils s'appliquent tous les jours.

### 4.7. Reporting et statistiques

#### 4.7.1. Consulter le dashboard

1. **Menu** → **Reporting** → **Dashboard**
2. Sélectionnez une **période** (dates de début et fin)
3. Consultez les statistiques :
   - Nombre de commandes
   - Répartition par type de formule
   - Consommations par période
   - Graphiques et tendances

#### 4.7.2. Exporter des données

1. Dans la vue souhaitée (Commandes, Points de consommation, etc.)
2. Appliquez les **filtres** nécessaires
3. Cliquez sur **"Exporter Excel"**
4. Le fichier Excel sera téléchargé

---

## 5. Guide pour les RH

### 5.1. Vue d'ensemble des fonctionnalités

En tant que **RH**, vous avez accès à la plupart des fonctionnalités sauf :

- ❌ Gestion des utilisateurs
- ❌ Paramètres système avancés

**Vous pouvez** :
- ✅ Gérer les formules (menus)
- ✅ Gérer les commandes
- ✅ Gérer les points de consommation
- ✅ Consulter les rapports
- ✅ Configurer les commandes
- ✅ Gérer les départements et fonctions

### 5.2. Gestion des formules (menus)

**Procédure identique à la section 4.4** (Administrateurs).

### 5.3. Gestion des commandes

#### 5.3.1. Créer une commande groupée

**Cas d'usage** : Créer plusieurs commandes en une fois pour plusieurs employés avec la même formule.

1. **Menu** → **Commandes** → **Commande groupée**
2. Sélectionnez la **formule** (date + type)
3. Sélectionnez la **date de consommation**
4. **Cochez les utilisateurs** concernés (liste à gauche)
5. Pour chaque utilisateur, spécifiez :
   - **Quantité** (généralement 1)
   - **Période** (Jour ou Nuit)
   - **Site** (CIT Billing ou CIT Terminal)
6. Cliquez sur **"Créer les commandes"**

### 5.4. Gestion des points de consommation

#### 5.4.1. Valider une consommation manuellement

1. **Menu** → **Points de consommation** → **Créer un point**
2. Sélectionnez l'**utilisateur**
3. Sélectionnez la **commande** à valider
4. Remplissez :
   - **Type de formule** : Standard ou Améliorée
   - **Statut** :
     - **Consommée** : Repas effectivement consommé
     - **Non Récupérée** : Commandé mais non récupéré (sera facturé)
     - **Indisponible** : Plat fini, commande annulée
   - **Date et heure** : Date de consommation
5. Cliquez sur **"Créer"**

#### 5.4.2. Consulter les points de consommation

1. **Menu** → **Points de consommation** → **Points de consommation CIT**
2. Utilisez les **filtres** :
   - Période (dates)
   - Recherche par nom ou matricule
3. Consultez le résumé par utilisateur :
   - Standard Consommée / Non Récupérée / Indisponible
   - Améliorée Consommée / Non Récupérée / Indisponible
   - Montant total

### 5.5. Facturation

#### 5.5.1. Consulter la facturation

1. **Menu** → **Facturation** → **Facturation**
2. Sélectionnez une **période** (mois par défaut)
3. Consultez :
   - Liste des commandes non consommées à facturer
   - Montant par utilisateur
   - Total général
4. Cliquez sur **"Exporter Excel"** pour obtenir le fichier de facturation

**Note** : Seules les commandes **non consommées** (non récupérées) sont facturées.

---

## 6. Guide pour les Employés

### 6.1. Vue d'ensemble des fonctionnalités

En tant qu'**Employé**, vous pouvez :

- ✅ Consulter les menus de la semaine
- ✅ Créer des commandes (semaine N+1)
- ✅ Voir vos commandes
- ✅ Annuler vos commandes (24h avant consommation)
- ✅ Consulter vos points de consommation
- ❌ Gérer les formules
- ❌ Voir les commandes des autres utilisateurs

### 6.2. Consulter les menus

#### 6.2.1. Voir les menus de la semaine en cours

1. **Page d'accueil** (après connexion)
2. La section **"Menus de la semaine"** affiche automatiquement :
   - Les formules du lundi au dimanche de la semaine en cours
   - Pour chaque formule : Type (Améliorée/Standard), Plats, Accompagnements

### 6.3. Créer une commande

#### 6.3.1. Commander pour la semaine N+1

**⚠️ Important** : Les commandes doivent être faites **au moins 48h avant** la date de consommation.

**Procédure étape par étape** :

1. **Menu** → **Commandes** → **Commander**
2. **Vérifiez le message** :
   - ✅ Si "Commandes disponibles" → Continuez
   - ❌ Si "Commandes bloquées" → Les commandes sont fermées (généralement vendredi après 12h)
3. Sélectionnez une **formule** dans la liste déroulante :
   - Les formules affichées sont celles de la **semaine N+1** (lundi à vendredi)
   - Format : "DD/MM/YYYY (Jour) - Nom Formule (Type)"
4. Remplissez :
   - **Date de consommation** : Sélectionnez dans le calendrier (date de la formule choisie)
   - **Période** :
     - **Jour** : Déjeuner (midi)
     - **Nuit** : Dîner (soir)
   - **Site** :
     - **CIT Billing** : Site principal
     - **CIT Terminal** : Site secondaire
   - **Quantité** : Nombre de plats (généralement 1)
5. Cliquez sur **"Créer la commande"**
6. **Confirmation** : Un message de succès s'affiche avec votre code de commande

**⚠️ Règles importantes** :
- Les commandes sont **bloquées le vendredi après 12h00**
- Les commandes rouvrent le **lundi matin**
- Vous ne pouvez pas commander pour la semaine en cours (seulement N+1)

#### 6.3.2. Commander depuis la page d'accueil

Vous pouvez aussi créer une commande directement depuis la page d'accueil :

1. Sur la page d'accueil, dans la section **"Menus de la semaine"**
2. Trouvez la formule souhaitée
3. Cliquez sur **"Commander"** (si disponible)
4. Remplissez les informations (période, site)
5. Cliquez sur **"Confirmer"**

### 6.4. Voir mes commandes

#### 6.4.1. Consulter toutes mes commandes

1. **Menu** → **Commandes** → **Mes commandes**
2. Utilisez les **filtres** :
   - **Statut** : Précommandée, Consommée, Annulée
   - **Période** : Dates de début et fin
3. Consultez la liste avec :
   - Code de commande
   - Date de consommation
   - Formule
   - Statut
   - Actions disponibles (Modifier, Annuler, Voir détails)

#### 6.4.2. Voir mes commandes depuis la page d'accueil

Sur la page d'accueil, la section **"Mes commandes de la semaine"** affiche automatiquement vos commandes de la semaine en cours.

### 6.5. Modifier une commande

#### 6.5.1. Changer la formule ou la date

**⚠️ Restrictions** :
- Vous pouvez modifier seulement les commandes **précommandées**
- Vous devez modifier **au moins 24h avant** la date de consommation
- Exception : Commandes de la semaine N+1 (toujours modifiables)

**Procédure** :

1. **Menu** → **Commandes** → **Mes commandes**
2. Trouvez la commande à modifier
3. Cliquez sur **"Modifier"** (icône crayon)
4. Modifiez :
   - Formule
   - Date de consommation
   - Période
   - Site
5. Cliquez sur **"Enregistrer"**

### 6.6. Annuler une commande

#### 6.6.1. Annuler une commande précommandée

**⚠️ Restrictions** :
- Vous pouvez annuler seulement les commandes **précommandées**
- Vous devez annuler **au moins 24h avant** la date de consommation
- Exception : Commandes de la semaine N+1 (toujours annulables)

**Procédure** :

1. **Menu** → **Commandes** → **Mes commandes**
2. Trouvez la commande à annuler
3. Cliquez sur **"Annuler"** (icône poubelle)
4. **Confirmez** l'annulation
5. Un message de confirmation s'affiche

**Alternative** : Depuis la page d'accueil, vous pouvez annuler directement depuis la section "Mes commandes de la semaine".

### 6.7. Consulter mes points de consommation

#### 6.7.1. Voir mon historique de consommation

1. **Menu** → **Points de consommation** → **Mes points de consommation**
2. Consultez :
   - **Par type de formule** : Standard / Améliorée
   - **Par statut** : Consommée / Non Récupérée / Indisponible
   - **Montant total** : Montant facturé
3. Utilisez les **filtres** pour sélectionner une période

**Note** : Cette vue montre seulement **vos propres points** de consommation.

---

## 7. Guide pour les Prestataires de Cantine

### 7.1. Vue d'ensemble des fonctionnalités

En tant que **Prestataire de Cantine**, vous pouvez :

- ✅ Voir les menus du jour
- ✅ Voir les commandes du jour
- ✅ Créer des commandes instantanées
- ✅ Créer des commandes pour les Douaniers
- ✅ Gérer les marges
- ✅ Exporter les commandes
- ❌ Voir les commandes des employés (sauf commandes du jour)

### 7.2. Consulter les menus du jour

#### 7.2.1. Voir les formules disponibles aujourd'hui

1. **Page d'accueil** (après connexion)
2. La section **"Menus du jour"** affiche automatiquement :
   - Toutes les formules disponibles pour aujourd'hui
   - Quotas restants
   - Marges disponibles
   - Nombre de commandes par formule

### 7.3. Créer une commande instantanée

#### 7.3.1. Commander pour un employé (jour même)

**⚠️ Restrictions** :
- Commandes pour le **jour même uniquement**
- **Avant 18h** : Commandes pour le déjeuner (période Jour)
- **Après 18h** : Commandes pour le dîner (période Nuit)
- Un utilisateur = **une seule commande instantanée par période/jour**

**Procédure** :

1. **Menu** → **Commandes** → **Commande instantanée**
2. Sélectionnez le **type de client** : "Employé CIT"
3. Remplissez :
   - **Utilisateur** : Recherchez et sélectionnez l'employé
   - **Formule** : Sélectionnez une formule du jour
   - **Période** : Jour ou Nuit (selon l'heure actuelle)
   - **Site** : CIT Billing ou CIT Terminal
4. Cliquez sur **"Créer la commande"**
5. **Vérification automatique** :
   - Quotas disponibles
   - Marges disponibles
   - Limite par utilisateur

### 7.4. Créer une commande pour les Douaniers

#### 7.4.1. Commander pour le groupe Douaniers

**Procédure détaillée** :

1. **Menu** → **Commandes** → **Commande Douaniers**
2. Sélectionnez la **formule** (doit contenir des plats standard)
3. Remplissez :
   - **Période** : Jour (midi) ou Nuit (soir)
   - **Site** : CIT Billing ou CIT Terminal
   - **Quantité** : Nombre de plats (1-100)
4. Cliquez sur **"Créer la commande"**
5. **Vérification automatique** :
   - Quotas permanents du groupe Douaniers
   - Plats déjà consommés aujourd'hui pour cette période
   - Quota restant disponible
6. Si le quota est suffisant :
   - Commande créée avec succès
   - Un **code de vérification** est généré (ex: DOU-50-1430)
7. **Communiquez le code** au client Douaniers

**⚠️ Important** :
- Les Douaniers ne peuvent commander que des formules avec **plats standard**
- Les quotas sont **permanents** (ne se décrémentent pas automatiquement)
- Un code de vérification est généré pour chaque commande

#### 7.4.2. Valider une commande Douaniers

1. **Menu** → **Commandes** → **Valider commande Douaniers**
2. Saisissez le **code de vérification** (ex: DOU-50-1430)
3. Cliquez sur **"Valider"**
4. La commande est validée et le statut change

### 7.5. Gérer les marges

#### 7.5.1. Définir des marges pour les formules

**Cas d'usage** : Prévoir des plats supplémentaires pour pallier les imprévus.

1. **Menu** → **Prestataires** → **Gestion des marges**
2. Sélectionnez une **date**
3. Pour chaque formule, définissez :
   - **Marge Jour** : Nombre de plats supplémentaires pour le midi
   - **Marge Nuit** : Nombre de plats supplémentaires pour le soir
4. Cliquez sur **"Enregistrer"**

**Note** : Les marges sont utilisées après épuisement des quotas principaux.

### 7.6. Exporter les commandes

#### 7.6.1. Exporter les commandes pour préparation

1. **Menu** → **Prestataires** → **Extraction**
2. Sélectionnez une **période** (dates de début et fin)
3. (Optionnel) Définissez des **marges** supplémentaires
4. Cliquez sur **"Exporter"**
5. Le fichier Excel est téléchargé avec :
   - Liste des commandes
   - Quantités par formule
   - Détails des plats

---

## 8. Procédures courantes

### 8.1. Scénarios fréquents pour les Administrateurs

#### Scénario 1 : Ajouter un nouvel employé dans le système

1. **Créer le département** (s'il n'existe pas)
   - Paramètres → Départements → Créer
2. **Créer la fonction** (s'il n'existe pas)
   - Paramètres → Fonctions → Créer
3. **Créer l'utilisateur**
   - Paramètres → Utilisateurs → Créer
   - Remplir : Matricule, Nom, Prénoms, Email, Département, Fonction, Rôle "Employé"
4. **Communiquer les identifiants** :
   - Matricule : [Matricule saisi]
   - Mot de passe : [Mot de passe généré ou défini]

#### Scénario 2 : Planifier les menus de la semaine

1. **Menu** → **Menus** → **Gérer les formules**
2. Pour chaque jour de la semaine (lundi à vendredi) :
   - Créer les formules (Améliorée, Standard 1, Standard 2)
   - Renseigner tous les plats et accompagnements
3. **Vérifier** que toutes les formules sont créées
4. **Communiquer** aux employés que les menus sont disponibles

#### Scénario 3 : Résoudre un problème de mot de passe oublié

1. **Option 1** : L'utilisateur utilise "Mot de passe oublié"
   - S'il reçoit l'email → Suivre la procédure normale
   - S'il ne reçoit pas l'email → Vérifier l'email dans son profil
2. **Option 2** : Administrateur réinitialise
   - Paramètres → Utilisateurs → Réinitialiser mot de passe
   - Définir un nouveau mot de passe
   - Communiquer le mot de passe à l'utilisateur

### 8.2. Scénarios fréquents pour les RH

#### Scénario 1 : Créer des commandes pour plusieurs employés

**Cas d'usage** : Plusieurs employés veulent la même formule.

1. **Menu** → **Commandes** → **Commande groupée**
2. Sélectionner la formule et la date
3. Cocher tous les employés concernés
4. Remplir les informations (période, site) pour chacun
5. Créer toutes les commandes en une fois

#### Scénario 2 : Valider les consommations de la semaine

1. **Menu** → **Points de consommation** → **Créer un point**
2. Pour chaque commande consommée :
   - Sélectionner l'utilisateur
   - Sélectionner la commande
   - Statut : "Consommée"
   - Enregistrer
3. Pour les commandes non récupérées :
   - Statut : "Non Récupérée" (sera facturée)

### 8.3. Scénarios fréquents pour les Employés

#### Scénario 1 : Commander pour toute la semaine N+1

1. **Menu** → **Commandes** → **Commander**
2. Pour chaque jour de la semaine (lundi à vendredi) :
   - Sélectionner une formule
   - Choisir la période (Jour/Nuit)
   - Choisir le site
   - Créer la commande
3. **Vérifier** vos commandes dans "Mes commandes"

#### Scénario 2 : Modifier une commande avant le délai

1. **Menu** → **Commandes** → **Mes commandes**
2. Trouver la commande à modifier
3. Cliquer sur "Modifier"
4. Changer la formule, la date, ou la période
5. Enregistrer

**⚠️ Rappel** : Vous devez modifier au moins 24h avant la date de consommation.

### 8.4. Scénarios fréquents pour les Prestataires

#### Scénario 1 : Gérer les commandes du jour

1. **Page d'accueil** → Consulter les menus et commandes du jour
2. **Créer des commandes instantanées** si nécessaire
3. **Exporter les commandes** pour préparation
4. **Valider les commandes Douaniers** avec les codes

#### Scénario 2 : Gérer les quotas et marges

1. **Menu** → **Prestataires** → **Gestion des marges**
2. Définir les marges pour les formules du jour
3. **Menu** → **Paramètres** → **Quotas Permanents** (si besoin d'ajuster les quotas Douaniers)

---

## 9. Dépannage et FAQ

### 9.1. Problèmes de connexion

#### ❌ "Identifiants incorrects"

**Solutions** :
1. Vérifiez que vous utilisez votre **matricule** (pas votre nom)
2. Vérifiez que le **Caps Lock** n'est pas activé
3. Vérifiez l'orthographe du mot de passe
4. Contactez votre administrateur si le problème persiste

#### ❌ "Compte désactivé"

**Solutions** :
1. Contactez votre **administrateur** pour réactiver votre compte
2. Vérifiez que vous n'avez pas été supprimé du système

#### ❌ "Mot de passe oublié" - Email non reçu

**Solutions** :
1. Vérifiez votre **dossier spam/courrier indésirable**
2. Vérifiez que votre **email** est correct dans votre profil
3. Attendez quelques minutes (envoi peut prendre du temps)
4. Contactez votre **administrateur** pour réinitialisation manuelle

### 9.2. Problèmes de commandes

#### ❌ "Les commandes sont bloquées"

**Explication** :
- Les commandes sont **automatiquement bloquées** le vendredi après 12h00
- Elles **rouvrent le lundi matin**

**Solutions** :
- Attendez le lundi pour commander
- Si vous devez commander en urgence, contactez un **Administrateur** (peut créer une commande instantanée)

#### ❌ "Impossible de créer une commande - Délai insuffisant"

**Explication** :
- Vous devez commander **au moins 48h avant** la date de consommation

**Solutions** :
1. Choisissez une date **plus tardive** (semaine N+1)
2. Si c'est urgent, contactez un **Prestataire de Cantine** (peut créer une commande instantanée)

#### ❌ "Impossible d'annuler - Délai dépassé"

**Explication** :
- Vous devez annuler **au moins 24h avant** la date de consommation

**Solutions** :
1. Si c'est justifié, contactez un **Administrateur** (peut annuler sans restriction)
2. Sinon, la commande sera facturée si non récupérée

#### ❌ "Quota insuffisant" (pour Douaniers)

**Explication** :
- Le quota permanent du groupe Douaniers est épuisé pour cette période

**Solutions** :
1. Contactez un **Administrateur** pour augmenter le quota
2. Vérifiez les commandes déjà créées aujourd'hui
3. Réessayez pour la période suivante (Nuit si Jour épuisé)

### 9.3. Problèmes de facturation

#### ❌ "Ma commande est facturée alors que je l'ai consommée"

**Explication** :
- La commande n'a pas été marquée comme "Consommée" dans les points de consommation

**Solutions** :
1. Contactez un **RH** ou **Administrateur**
2. Demandez la validation manuelle de votre consommation
3. La facturation sera ajustée

#### ❌ "Je vois un montant incorrect dans ma facture"

**Solutions** :
1. Consultez vos **points de consommation** (Mes points de consommation)
2. Vérifiez les commandes **"Non Récupérées"** (facturées)
3. Contactez un **RH** ou **Administrateur** pour correction

### 9.4. Problèmes d'affichage

#### ❌ "Les menus ne s'affichent pas"

**Solutions** :
1. Vérifiez que les **menus sont créés** par les RH/Admin
2. Actualisez la page (F5)
3. Vérifiez que vous êtes sur la bonne période (semaine en cours pour consultation, N+1 pour commande)
4. Contactez un **Administrateur** si le problème persiste

#### ❌ "Je ne vois pas mes commandes"

**Solutions** :
1. Vérifiez les **filtres** (statut, période)
2. Actualisez la page (F5)
3. Vérifiez que vous avez bien créé des commandes
4. Contactez le **support** si nécessaire

### 9.5. FAQ (Foire Aux Questions)

#### ❓ Puis-je commander pour aujourd'hui ?

**Réponse** :
- **Employé** : Non, seulement pour la semaine N+1
- **Prestataire/Admin** : Oui, via "Commande instantanée"

#### ❓ Puis-je modifier ma commande après l'avoir créée ?

**Réponse** :
- Oui, **si c'est au moins 24h avant** la date de consommation
- Exception : Commandes de la semaine N+1 (toujours modifiables)

#### ❓ Que se passe-t-il si je ne récupère pas ma commande ?

**Réponse** :
- La commande sera marquée comme **"Non Récupérée"**
- Elle sera **facturée** (vous serez débité du montant)

#### ❓ Comment savoir si ma commande a été validée ?

**Réponse** :
1. Consultez "Mes commandes"
2. Le statut est :
   - **Précommandée** : En attente
   - **Consommée** : Validée
   - **Annulée** : Annulée

#### ❓ Qu'est-ce qu'un point de consommation ?

**Réponse** :
- C'est l'enregistrement de votre consommation effective d'un repas
- Il est créé soit **manuellement** (par RH/Prestataire), soit **automatiquement** (fermeture automatique le vendredi)

#### ❓ Comment fonctionnent les quotas Douaniers ?

**Réponse** :
- Les quotas sont **permanents** (ne se décrémentent pas)
- Ils s'appliquent **tous les jours** (pas par date)
- Exemple : Quota Jour = 50 → 50 plats disponibles **chaque jour** pour le midi

---

## 10. Glossaire et termes techniques

### 10.1. Termes généraux

- **Formule** : Menu du jour (repas proposé)
- **Commande** : Réservation d'un repas par un utilisateur
- **Point de consommation** : Validation qu'un repas a été consommé
- **Quota** : Nombre maximum de plats disponibles
- **Marge** : Nombre de plats supplémentaires prévus (pour imprévus)

### 10.2. Types de formules

- **Formule Améliorée** : Menu complet avec entrée, plat, dessert
- **Formule Standard 1** : Premier plat standard avec accompagnements
- **Formule Standard 2** : Deuxième plat standard (alternative)

### 10.3. Statuts de commande

- **Précommandée** : Commande créée, en attente de consommation
- **Consommée** : Commande validée, repas consommé
- **Annulée** : Commande annulée par l'utilisateur ou le prestataire
- **Non Récupérée** : Commande non récupérée (sera facturée)

### 10.4. Périodes

- **Jour** : Période du déjeuner (midi)
- **Nuit** : Période du dîner (soir)

### 10.5. Sites

- **CIT Billing** : Site principal
- **CIT Terminal** : Site secondaire

### 10.6. Rôles

- **Administrateur** : Accès complet à toutes les fonctionnalités
- **RH** : Ressources Humaines (gestion des formules, commandes, reporting)
- **Employé** : Utilisateur standard (commandes personnelles)
- **PrestataireCantine** : Prestataire de cantine (commandes instantanées, export)

### 10.7. Groupes spéciaux

- **Douaniers** : Groupe non-CIT avec quotas permanents
- **Groupe Non-CIT** : Groupes externes à la CIT

---

## 📞 Contact et support

### Support technique

- **Email support** : [EMAIL_SUPPORT]
- **Téléphone** : [TELEPHONE_SUPPORT]
- **Horaires** : [HORAIRES]

### Contacts par rôle

- **Pour les Administrateurs** : [CONTACT_ADMIN]
- **Pour les RH** : [CONTACT_RH]
- **Pour les Employés** : [CONTACT_EMPLOYE]
- **Pour les Prestataires** : [CONTACT_PRESTATAIRE]

---

## ✅ Checklist de démarrage rapide

### Pour un nouvel Administrateur

- [ ] Se connecter avec les identifiants fournis
- [ ] Changer le mot de passe
- [ ] Créer les départements nécessaires
- [ ] Créer les fonctions nécessaires
- [ ] Créer les utilisateurs (RH, Employés)
- [ ] Configurer les paramètres de commande (blocage vendredi 12h)
- [ ] Créer les types de formules
- [ ] Configurer les quotas Douaniers (si nécessaire)
- [ ] Tester la création d'une formule
- [ ] Tester la création d'une commande

### Pour un nouvel Employé

- [ ] Se connecter avec les identifiants fournis
- [ ] Changer le mot de passe
- [ ] Consulter les menus de la semaine
- [ ] Créer une première commande
- [ ] Consulter "Mes commandes"
- [ ] Comprendre les délais (48h pour commander, 24h pour annuler)

### Pour un nouvel RH

- [ ] Se connecter avec les identifiants fournis
- [ ] Changer le mot de passe
- [ ] Créer une formule de test
- [ ] Créer une commande groupée
- [ ] Valider un point de consommation
- [ ] Consulter le reporting

### Pour un nouveau Prestataire

- [ ] Se connecter avec les identifiants fournis
- [ ] Changer le mot de passe
- [ ] Consulter les menus du jour
- [ ] Créer une commande instantanée
- [ ] Créer une commande Douaniers
- [ ] Exporter les commandes

---

## 📝 Notes importantes

1. **Sauvegarde** : Les données sont sauvegardées automatiquement. Aucune action manuelle nécessaire.

2. **Sécurité** :
   - Ne partagez **jamais** vos identifiants
   - Changez votre mot de passe régulièrement
   - Déconnectez-vous si vous utilisez un ordinateur partagé

3. **Performance** :
   - Utilisez les **filtres** pour améliorer les performances
   - Évitez de charger trop de données en même temps

4. **Best practices** :
   - Commandez en **avance** (semaine N+1)
   - Vérifiez régulièrement **vos commandes**
   - Contactez le support en cas de problème

---

**Document créé le : 2025-01-XX**  
**Dernière mise à jour : 2025-01-XX**  
**Version : 1.0**

**Pour toute question ou suggestion d'amélioration de ce manuel, contactez le support.**

