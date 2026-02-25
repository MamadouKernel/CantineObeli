# 📚 Documentation Complète - Toutes les Vues de l'Application O'Beli K

## 📋 Vue d'ensemble

Cette documentation décrit **toutes les vues** de l'application **O'Beli K**, une application web de gestion de commandes de repas pour la Côte d'Ivoire, développée en **ASP.NET Core MVC**.

**Version** : 1.0  
**Date de mise à jour** : 2025-01-XX  
**Nombre total de vues** : ~87 vues

---

## 📑 Table des matières

1. [Vues d'authentification](#1-vues-dauthentification)
2. [Vue d'accueil](#2-vue-daccueil)
3. [Vues de gestion des commandes](#3-vues-de-gestion-des-commandes)
4. [Vues de gestion des menus (Formules)](#4-vues-de-gestion-des-menus-formules)
5. [Vues de gestion des utilisateurs](#5-vues-de-gestion-des-utilisateurs)
6. [Vues de points de consommation](#6-vues-de-points-de-consommation)
7. [Vues de facturation](#7-vues-de-facturation)
8. [Vues de reporting et statistiques](#8-vues-de-reporting-et-statistiques)
9. [Vues de configuration](#9-vues-de-configuration)
10. [Vues de gestion des prestataires](#10-vues-de-gestion-des-prestataires)
11. [Vues de diagnostics](#11-vues-de-diagnostics)
12. [Vues administratives](#12-vues-administratives)
13. [Vues partagées](#13-vues-partagées)

---

## 1. Vues d'authentification

### 1.1. Login (`/Auth/Login`)
**Fichier** : `Views/Auth/Login.cshtml`  
**Contrôleur** : `AuthController`  
**Accès** : Public (non authentifié)

#### Description
Page de connexion à l'application. Permet aux utilisateurs de s'authentifier avec leur matricule et mot de passe.

#### Fonctionnalités
- ✅ Saisie du matricule (UserName)
- ✅ Saisie du mot de passe
- ✅ Option "Se souvenir de moi" (cookie de 30 jours)
- ✅ Lien "Mot de passe oublié"
- ✅ Gestion des erreurs de connexion
- ✅ Redirection automatique si déjà connecté

#### Rôles autorisés
Tous les utilisateurs (public)

---

### 1.2. Profile (`/Auth/Profile`)
**Fichier** : `Views/Auth/Profile.cshtml`  
**Contrôleur** : `AuthController`  
**Accès** : Authentifié

#### Description
Page de profil utilisateur permettant de consulter ses informations personnelles.

#### Fonctionnalités
- ✅ Affichage des informations utilisateur (nom, prénoms, email, matricule)
- ✅ Affichage du rôle
- ✅ Lien vers "Modifier le profil"
- ✅ Lien vers "Changer le mot de passe"

#### Rôles autorisés
Tous les utilisateurs authentifiés

---

### 1.3. Edit Profile (`/Auth/EditProfile`)
**Fichier** : `Views/Auth/EditProfile.cshtml`  
**Contrôleur** : `AuthController`  
**Accès** : Authentifié

#### Description
Formulaire de modification du profil utilisateur (email principalement).

#### Fonctionnalités
- ✅ Modification de l'email
- ✅ Validation des données
- ✅ Mise à jour du profil

#### Rôles autorisés
Tous les utilisateurs authentifiés

---

### 1.4. Change Password (`/Auth/ChangePassword`)
**Fichier** : `Views/Auth/ChangePassword.cshtml`  
**Contrôleur** : `AuthController`  
**Accès** : Authentifié

#### Description
Formulaire de changement de mot de passe pour l'utilisateur connecté.

#### Fonctionnalités
- ✅ Saisie de l'ancien mot de passe
- ✅ Saisie du nouveau mot de passe (confirmation)
- ✅ Validation des mots de passe
- ✅ Hachage BCrypt du nouveau mot de passe

#### Rôles autorisés
Tous les utilisateurs authentifiés

---

## 2. Vue d'accueil

### 2.1. Home Index (`/Home` ou `/`)
**Fichier** : `Views/Home/Index.cshtml`  
**Contrôleur** : `HomeController`  
**Accès** : Authentifié

#### Description
Page d'accueil principale de l'application. Affiche différents contenus selon le rôle de l'utilisateur.

#### Pour les Employés, Administrateurs et RH
**Fonctionnalités** :
- ✅ **Menus de la semaine en cours** : Affichage des formules du lundi au dimanche
- ✅ **Mes commandes de la semaine** : Liste des commandes de l'utilisateur connecté
- ✅ **Annulation de commandes** : Possibilité d'annuler ses commandes précommandées (24h avant consommation)
- ✅ **Filtrage par date** : Affichage organisé par jour de la semaine

#### Pour les Prestataires de Cantine
**Fonctionnalités** :
- ✅ **Menus du jour** : Affichage des formules disponibles aujourd'hui
- ✅ **Commandes du jour** : Liste de toutes les commandes pour aujourd'hui (tous statuts)
- ✅ **Statistiques par formule** : Nombre de commandes par formule
- ✅ **Informations sur les quotas et marges** : Quotas restants, marges disponibles

#### Rôles autorisés
Tous les utilisateurs authentifiés (contenu adapté selon le rôle)

---

## 3. Vues de gestion des commandes

### 3.1. Liste des commandes (`/Commande`)
**Fichier** : `Views/Commande/Index.cshtml`  
**Contrôleur** : `CommandeController`  
**Accès** : Authentifié

#### Description
Liste principale des commandes avec filtrage et pagination.

#### Fonctionnalités
- ✅ **Filtrage par statut** : Précommandée, Consommée, Annulée
- ✅ **Filtrage par période** : Date de consommation
- ✅ **Pagination** : Navigation par pages
- ✅ **Recherche** : Par code de commande, utilisateur, formule
- ✅ **Actions** : Voir détails, Modifier, Annuler (selon permissions)
- ✅ **Export Excel** : Export des commandes filtrées

#### Rôles autorisés
Tous les utilisateurs authentifiés (filtrage selon le rôle)

---

### 3.2. Créer une commande (`/Commande/Create`)
**Fichier** : `Views/Commande/Create.cshtml`  
**Contrôleur** : `CommandeController`  
**Accès** : Authentifié

#### Description
Formulaire de création de commande pour la semaine N+1 (semaine suivante ouvrée).

#### Fonctionnalités
- ✅ **Sélection de formule** : Formules disponibles pour la semaine N+1
- ✅ **Sélection de date** : Dates disponibles (lundi au vendredi de la semaine suivante)
- ✅ **Sélection utilisateur** : 
  - Employé : Seulement lui-même
  - Admin/RH : Tous les utilisateurs
- ✅ **Quantité** : Nombre de plats (défaut : 1)
- ✅ **Validation** : Vérification des quotas et disponibilités
- ✅ **Blocage des commandes** : Gestion des périodes de blocage

#### Rôles autorisés
- ✅ Administrateur
- ✅ RH
- ✅ Employé (seulement ses propres commandes)

---

### 3.3. Modifier une commande (`/Commande/Edit/{id}`)
**Fichier** : `Views/Commande/Edit.cshtml`  
**Contrôleur** : `CommandeController`  
**Accès** : Authentifié

#### Description
Formulaire de modification d'une commande existante.

#### Fonctionnalités
- ✅ **Modification de la formule** : Changement de formule si disponible
- ✅ **Modification de la date** : Changement de date de consommation
- ✅ **Modification de la quantité** : Ajustement du nombre de plats
- ✅ **Restrictions temporelles** : 
  - Employé : 24h avant consommation ou semaine N+1
  - Admin : Pas de restriction
- ✅ **Validation** : Vérification des nouvelles données

#### Rôles autorisés
- ✅ Administrateur (sans restriction)
- ✅ RH (sans restriction)
- ✅ Employé (seulement ses commandes, avec restrictions temporelles)

---

### 3.4. Détails d'une commande (`/Commande/Details/{id}`)
**Fichier** : `Views/Commande/Details.cshtml`  
**Contrôleur** : `CommandeController`  
**Accès** : Authentifié

#### Description
Affichage détaillé d'une commande avec toutes ses informations.

#### Fonctionnalités
- ✅ **Informations complètes** : Code, date, formule, utilisateur, statut
- ✅ **Détails de la formule** : Plats, accompagnements, prix
- ✅ **Historique** : Modifications, annulations
- ✅ **Actions disponibles** : Selon le statut et les permissions

#### Rôles autorisés
Tous les utilisateurs authentifiés (selon les permissions)

---

### 3.5. Créer commande groupée (`/Commande/CreerCommandeGroupee`)
**Fichier** : `Views/Commande/CreerCommandeGroupee.cshtml`  
**Contrôleur** : `CommandeController`  
**Accès** : Administrateur, RH

#### Description
Création de commandes groupées pour plusieurs utilisateurs avec la même formule.

#### Fonctionnalités
- ✅ **Sélection multiple d'utilisateurs** : Liste avec cases à cocher
- ✅ **Sélection de formule** : Formule unique pour tous
- ✅ **Date unique** : Même date de consommation pour tous
- ✅ **Quantité par utilisateur** : Définition de la quantité pour chaque utilisateur
- ✅ **Création en masse** : Création de plusieurs commandes en une fois

#### Rôles autorisés
- ✅ Administrateur
- ✅ RH

---

### 3.6. Créer commande instantanée (`/Commande/CreerCommandeInstantanee`)
**Fichier** : `Views/Commande/CreerCommandeInstantanee.cshtml`  
**Contrôleur** : `CommandeController`  
**Accès** : PrestataireCantine, Administrateur, RH

#### Description
Création de commandes instantanées pour le jour même (avant 18h pour le midi, après 18h pour le soir).

#### Fonctionnalités
- ✅ **Commandes du jour** : Création pour aujourd'hui uniquement
- ✅ **Vérification des quotas** : Vérification des quotas et marges disponibles
- ✅ **Période de service** : 
  - Avant 18h : Période Jour (déjeuner)
  - Après 18h : Période Nuit (dîner)
- ✅ **Limite par utilisateur** : Un utilisateur = une commande instantanée par période/jour
- ✅ **Validation en temps réel** : Vérification de disponibilité

#### Rôles autorisés
- ✅ PrestataireCantine
- ✅ Administrateur
- ✅ RH

---

### 3.7. Créer commande Douaniers (`/Commande/CreerCommandeDouaniers`)
**Fichier** : `Views/Commande/CreerCommandeDouaniers.cshtml`  
**Contrôleur** : `CommandeController`  
**Accès** : PrestataireCantine, Administrateur, RH

#### Description
Interface spécialisée pour créer des commandes pour le groupe "Douaniers" (groupe non-CIT).

#### Fonctionnalités
- ✅ **Groupe Douaniers** : Commande automatique pour le groupe Douaniers
- ✅ **Vérification des quotas** : Vérification des quotas permanents du groupe
- ✅ **Restriction aux plats standard** : Seulement les formules contenant des plats standard
- ✅ **Commande instantanée** : Pour le jour même uniquement
- ✅ **Validation par code** : Génération d'un code de validation unique

#### Rôles autorisés
- ✅ PrestataireCantine
- ✅ Administrateur
- ✅ RH

---

### 3.8. Valider commande Douaniers (`/Commande/ValiderCommandeDouaniers`)
**Fichier** : `Views/Commande/ValiderCommandeDouaniers.cshtml`  
**Contrôleur** : `CommandeController`  
**Accès** : PrestataireCantine, Administrateur, RH

#### Description
Interface de validation des commandes Douaniers par code de vérification.

#### Fonctionnalités
- ✅ **Saisie du code** : Code de vérification de la commande
- ✅ **Validation** : Vérification et validation de la commande
- ✅ **Statut** : Changement du statut de la commande
- ✅ **Historique** : Traçabilité des validations

#### Rôles autorisés
- ✅ PrestataireCantine
- ✅ Administrateur
- ✅ RH

---

### 3.9. Vérifier commande (`/Commande/VerifierCommande`)
**Fichier** : `Views/Commande/VerifierCommande.cshtml`  
**Contrôleur** : `CommandeController`  
**Accès** : Authentifié

#### Description
Vérification d'une commande par code ou identifiant.

#### Fonctionnalités
- ✅ **Recherche par code** : Recherche par code de commande
- ✅ **Affichage des détails** : Informations de la commande
- ✅ **Statut** : Vérification du statut actuel

#### Rôles autorisés
Tous les utilisateurs authentifiés

---

### 3.10. Cumul Points Consommation (`/Commande/CumulPointsConsommation`)
**Fichier** : `Views/Commande/CumulPointsConsommation.cshtml`  
**Contrôleur** : `CommandeController`  
**Accès** : Administrateur, RH

#### Description
Vue agrégée montrant le cumul des points de consommation par utilisateur sur une période.

#### Fonctionnalités
- ✅ **Agrégation par utilisateur** : Regroupement des points par utilisateur
- ✅ **Filtrage par période** : Sélection de la période (début/fin)
- ✅ **Calcul des totaux** : Totaux par type de formule et statut
- ✅ **Export** : Export Excel des données

#### Rôles autorisés
- ✅ Administrateur
- ✅ RH

---

### 3.11. Mes Points Consommation (`/Commande/MesPointsConsommation`)
**Fichier** : `Views/Commande/MesPointsConsommation.cshtml`  
**Contrôleur** : `CommandeController`  
**Accès** : Authentifié

#### Description
Affichage des points de consommation de l'utilisateur connecté.

#### Fonctionnalités
- ✅ **Points personnels** : Seulement les points de l'utilisateur connecté
- ✅ **Par type de formule** : Standard et Améliorée
- ✅ **Par statut** : Consommée, Non Récupérée, Indisponible
- ✅ **Totaux** : Montant total calculé

#### Rôles autorisés
Tous les utilisateurs authentifiés (seulement leurs propres points)

---

## 4. Vues de gestion des menus (Formules)

### 4.1. Liste des menus (`/FormuleJour`)
**Fichier** : `Views/FormuleJour/Index.cshtml`  
**Contrôleur** : `FormuleJourController`  
**Accès** : Administrateur, RH

#### Description
Liste principale des formules/menus avec filtrage par période.

#### Fonctionnalités
- ✅ **Sélection de période** : Choix de la période à afficher
- ✅ **Affichage par semaine** : Groupement par semaine
- ✅ **Tri** : Par date et type de formule
- ✅ **Actions** : Créer, Modifier, Voir détails, Supprimer
- ✅ **Pagination** : Navigation par pages si nécessaire

#### Rôles autorisés
- ✅ Administrateur
- ✅ RH

---

### 4.2. Créer une formule (`/FormuleJour/Create`)
**Fichier** : `Views/FormuleJour/Create.cshtml`  
**Contrôleur** : `FormuleJourController`  
**Accès** : Administrateur, RH

#### Description
Formulaire de création d'une nouvelle formule (menu).

#### Fonctionnalités
- ✅ **Sélection de date** : Date de la formule
- ✅ **Type de formule** : 
  - Améliorée (1 plat)
  - Standard (2 plats)
- ✅ **Champs selon le type** :
  - Améliorée : Entrée, Plat, Garniture, Dessert, Féculent, Légumes
  - Standard : Plat Standard 1, Garniture 1, Plat Standard 2, Garniture 2, Féculent, Légumes
- ✅ **Quotas** : Quota Jour et Nuit (optionnels)
- ✅ **Marges** : Marges disponibles (optionnels)
- ✅ **Validation** : Vérification des champs obligatoires

#### Rôles autorisés
- ✅ Administrateur
- ✅ RH

---

### 4.3. Modifier une formule (`/FormuleJour/Edit/{id}`)
**Fichier** : `Views/FormuleJour/Edit.cshtml`  
**Contrôleur** : `FormuleJourController`  
**Accès** : Administrateur, RH

#### Description
Formulaire de modification d'une formule existante.

#### Fonctionnalités
- ✅ **Modification des plats** : Changement des plats et accompagnements
- ✅ **Modification des quotas** : Ajustement des quotas jour/nuit
- ✅ **Modification des marges** : Ajustement des marges
- ✅ **Date** : Changement de date si pas de commandes associées
- ✅ **Validation** : Vérification des modifications

#### Rôles autorisés
- ✅ Administrateur
- ✅ RH

---

### 4.4. Détails d'une formule (`/FormuleJour/Details/{id}`)
**Fichier** : `Views/FormuleJour/Details.cshtml`  
**Contrôleur** : `FormuleJourController`  
**Accès** : Administrateur, RH

#### Description
Affichage détaillé d'une formule avec toutes ses informations.

#### Fonctionnalités
- ✅ **Informations complètes** : Date, type, tous les plats
- ✅ **Quotas et marges** : Quotas disponibles, marges restantes
- ✅ **Statistiques** : Nombre de commandes associées
- ✅ **Actions** : Modifier, Supprimer

#### Rôles autorisés
- ✅ Administrateur
- ✅ RH

---

### 4.5. Importer des formules (`/FormuleJour/Import`)
**Fichier** : `Views/FormuleJour/Import.cshtml`  
**Contrôleur** : `FormuleJourController`  
**Accès** : Administrateur, RH

#### Description
Import en masse de formules depuis un fichier Excel.

#### Fonctionnalités
- ✅ **Upload de fichier** : Import depuis Excel (.xlsx)
- ✅ **Template** : Téléchargement d'un template Excel
- ✅ **Validation** : Vérification des données importées
- ✅ **Prévisualisation** : Aperçu avant import
- ✅ **Import en masse** : Création de plusieurs formules en une fois

#### Rôles autorisés
- ✅ Administrateur
- ✅ RH

---

### 4.6. Sélection de période (`/FormuleJour/SelectPeriod`)
**Fichier** : `Views/FormuleJour/SelectPeriod.cshtml`  
**Contrôleur** : `FormuleJourController`  
**Accès** : Administrateur, RH

#### Description
Sélection de la période pour afficher ou créer des formules.

#### Fonctionnalités
- ✅ **Sélection de période** : Choix de la semaine/mois
- ✅ **Navigation** : Semaine précédente/suivante
- ✅ **Redirection** : Vers la liste ou création

#### Rôles autorisés
- ✅ Administrateur
- ✅ RH

---

## 5. Vues de gestion des utilisateurs

### 5.1. Liste des utilisateurs (`/Utilisateur`)
**Fichier** : `Views/Utilisateur/Index.cshtml`  
**Contrôleur** : `UtilisateurController`  
**Accès** : Administrateur

#### Description
Liste principale des utilisateurs avec filtrage et recherche.

#### Fonctionnalités
- ✅ **Filtrage par rôle** : Administrateur, RH, Employé, PrestataireCantine
- ✅ **Recherche** : Par nom, prénoms, matricule, email
- ✅ **Pagination** : Navigation par pages
- ✅ **Actions** : Créer, Voir détails, Modifier, Réinitialiser mot de passe
- ✅ **Export** : Export Excel des utilisateurs

#### Rôles autorisés
- ✅ Administrateur uniquement

---

### 5.2. Créer un utilisateur (`/Utilisateur/Create`)
**Fichier** : `Views/Utilisateur/Create.cshtml`  
**Contrôleur** : `UtilisateurController`  
**Accès** : Administrateur

#### Description
Formulaire de création d'un nouvel utilisateur.

#### Fonctionnalités
- ✅ **Informations personnelles** : Nom, Prénoms, Matricule
- ✅ **Email** : Adresse email
- ✅ **Rôle** : Sélection du rôle (Administrateur, RH, Employé, PrestataireCantine)
- ✅ **Département** : Sélection du département
- ✅ **Fonction** : Sélection de la fonction
- ✅ **Mot de passe** : Génération automatique ou manuelle
- ✅ **Validation** : Vérification des données uniques

#### Rôles autorisés
- ✅ Administrateur uniquement

---

### 5.3. Modifier un utilisateur (`/Utilisateur/Edit/{id}`)
**Fichier** : `Views/Utilisateur/Edit.cshtml`  
**Contrôleur** : `UtilisateurController`  
**Accès** : Administrateur

#### Description
Formulaire de modification d'un utilisateur existant.

#### Fonctionnalités
- ✅ **Modification des informations** : Nom, prénoms, email
- ✅ **Changement de rôle** : Modification du rôle
- ✅ **Changement de département/fonction** : Mise à jour des attributs
- ✅ **Validation** : Vérification des données

#### Rôles autorisés
- ✅ Administrateur uniquement

---

### 5.4. Détails d'un utilisateur (`/Utilisateur/Details/{id}`)
**Fichier** : `Views/Utilisateur/Details.cshtml`  
**Contrôleur** : `UtilisateurController`  
**Accès** : Administrateur

#### Description
Affichage détaillé d'un utilisateur avec ses informations et statistiques.

#### Fonctionnalités
- ✅ **Informations complètes** : Toutes les données de l'utilisateur
- ✅ **Statistiques** : Nombre de commandes, points de consommation
- ✅ **Historique** : Dernières commandes
- ✅ **Actions** : Modifier, Réinitialiser mot de passe

#### Rôles autorisés
- ✅ Administrateur uniquement

---

### 5.5. Réinitialiser mot de passe (`/Utilisateur/ResetPassword/{id}`)
**Fichier** : `Views/Utilisateur/ResetPassword.cshtml`  
**Contrôleur** : `UtilisateurController`  
**Accès** : Administrateur

#### Description
Réinitialisation du mot de passe d'un utilisateur par l'administrateur.

#### Fonctionnalités
- ✅ **Nouveau mot de passe** : Saisie d'un nouveau mot de passe
- ✅ **Confirmation** : Confirmation du nouveau mot de passe
- ✅ **Notification** : Envoi d'email à l'utilisateur (optionnel)
- ✅ **Validation** : Vérification des règles de mot de passe

#### Rôles autorisés
- ✅ Administrateur uniquement

---

### 5.6. Liste simple (`/Utilisateur/List`)
**Fichier** : `Views/Utilisateur/List.cshtml`  
**Contrôleur** : `UtilisateurController`  
**Accès** : Administrateur

#### Description
Liste simplifiée des utilisateurs pour sélection (utilisée dans d'autres vues).

#### Fonctionnalités
- ✅ **Liste compacte** : Affichage simplifié
- ✅ **Filtrage** : Par rôle ou département
- ✅ **Sélection** : Pour intégration dans d'autres formulaires

#### Rôles autorisés
- ✅ Administrateur

---

## 6. Vues de points de consommation

### 6.1. Points de consommation CIT (`/PointsConsommation/PointConsommationCIT`)
**Fichier** : `Views/PointsConsommation/PointConsommationCIT.cshtml`  
**Contrôleur** : `PointsConsommationController`  
**Accès** : Administrateur, RH

#### Description
Vue administrative agrégée des points de consommation de tous les utilisateurs CIT.

#### Fonctionnalités
- ✅ **Agrégation par utilisateur** : Regroupement des points par utilisateur
- ✅ **Par type de formule** : Standard et Améliorée
- ✅ **Par statut** : 
  - Consommée (effectivement consommé)
  - Non Récupérée (commandé mais non récupéré, facturé)
  - Indisponible (commandé mais plat fini)
- ✅ **Calcul des montants** : Montant total par utilisateur
- ✅ **Export** : Export Excel des données
- ✅ **Recherche** : Recherche par nom, matricule

#### Rôles autorisés
- ✅ Administrateur
- ✅ RH

---

### 6.2. Mes points de consommation (`/PointsConsommation/MesPointsConsommation`)
**Fichier** : `Views/PointsConsommation/MesPointsConsommation.cshtml`  
**Contrôleur** : `PointsConsommationController`  
**Accès** : Authentifié

#### Description
Affichage des points de consommation de l'utilisateur connecté uniquement.

#### Fonctionnalités
- ✅ **Points personnels** : Seulement les points de l'utilisateur connecté
- ✅ **Détails** : Par formule, date, statut
- ✅ **Totaux** : Calcul des totaux par type et statut
- ✅ **Montant total** : Montant total facturé

#### Rôles autorisés
Tous les utilisateurs authentifiés (seulement leurs propres points)

---

### 6.3. Créer un point de consommation (`/PointsConsommation/Create`)
**Fichier** : `Views/PointsConsommation/Create.cshtml`  
**Contrôleur** : `PointsConsommationController`  
**Accès** : Administrateur, RH, PrestataireCantine

#### Description
Formulaire de création d'un point de consommation (validation d'une commande consommée).

#### Fonctionnalités
- ✅ **Sélection de commande** : Choix de la commande à valider
- ✅ **Statut** : Consommée, Non Récupérée, Indisponible
- ✅ **Date** : Date de consommation
- ✅ **Validation** : Création du point de consommation

#### Rôles autorisés
- ✅ Administrateur
- ✅ RH
- ✅ PrestataireCantine

---

### 6.4. Modifier un point de consommation (`/PointsConsommation/Edit/{id}`)
**Fichier** : `Views/PointsConsommation/Edit.cshtml`  
**Contrôleur** : `PointsConsommationController`  
**Accès** : Administrateur, RH

#### Description
Formulaire de modification d'un point de consommation existant.

#### Fonctionnalités
- ✅ **Modification du statut** : Changement du statut
- ✅ **Modification de la date** : Ajustement de la date
- ✅ **Validation** : Mise à jour du point

#### Rôles autorisés
- ✅ Administrateur
- ✅ RH

---

## 7. Vues de facturation

### 7.1. Facturation (`/Facturation`)
**Fichier** : `Views/Facturation/Index.cshtml`  
**Contrôleur** : `FacturationController`  
**Accès** : Administrateur, RH

#### Description
Vue principale de facturation affichant les commandes non consommées à facturer.

#### Fonctionnalités
- ✅ **Filtrage par période** : Sélection de la période (mois par défaut)
- ✅ **Commandes non consommées** : Liste des commandes à facturer
- ✅ **Calcul automatique** : Calcul des montants à facturer
- ✅ **Par utilisateur** : Regroupement par utilisateur
- ✅ **Par type de formule** : Standard et Améliorée
- ✅ **Totaux** : Montant total par utilisateur et global
- ✅ **Export** : Export Excel pour facturation

#### Rôles autorisés
- ✅ Administrateur
- ✅ RH

---

### 7.2. Paramètres de facturation (`/ParametresFacturation`)
**Fichier** : `Views/ParametresFacturation/Index.cshtml`  
**Contrôleur** : `ParametresFacturationController`  
**Accès** : Administrateur, RH

#### Description
Configuration des paramètres de facturation (prix des formules, règles de facturation).

#### Fonctionnalités
- ✅ **Prix des formules** : Configuration des prix Standard et Améliorée
- ✅ **Règles de facturation** : Paramètres de facturation
- ✅ **Activation/Désactivation** : Activation de la facturation automatique
- ✅ **Sauvegarde** : Mise à jour des paramètres

#### Rôles autorisés
- ✅ Administrateur
- ✅ RH

---

### 7.3. Facturation automatique (`/FacturationAutomatique`)
**Fichier** : `Views/FacturationAutomatique/Index.cshtml`  
**Contrôleur** : `FacturationAutomatiqueController`  
**Accès** : Administrateur, RH

#### Description
Configuration et gestion de la facturation automatique (génération automatique des factures).

#### Fonctionnalités
- ✅ **Activation/Désactivation** : Gestion de l'activation
- ✅ **Période** : Configuration de la période de facturation
- ✅ **Génération** : Génération manuelle ou automatique des factures
- ✅ **Historique** : Historique des facturations automatiques

#### Rôles autorisés
- ✅ Administrateur
- ✅ RH

---

### 7.4. Diagnostic facturation (`/DiagnosticFacturation`)
**Fichier** : `Views/DiagnosticFacturation/Index.cshtml`  
**Contrôleur** : `DiagnosticFacturationController`  
**Accès** : Administrateur, RH

#### Description
Outil de diagnostic pour analyser l'état des commandes et détecter les incohérences dans la facturation.

#### Fonctionnalités
- ✅ **Analyse des commandes** : Détection des incohérences
- ✅ **Comparaison** : Comparaison entre statut et état réel
- ✅ **Résumé** : Statistiques des commandes (Total, Précommandées, Consommées, Annulées)
- ✅ **Détails** : Liste détaillée des commandes avec problèmes potentiels
- ✅ **Export** : Export des données pour analyse

#### Rôles autorisés
- ✅ Administrateur
- ✅ RH

---

## 8. Vues de reporting et statistiques

### 8.1. Dashboard Reporting (`/Reporting/Dashboard`)
**Fichier** : `Views/Reporting/Dashboard.cshtml`  
**Contrôleur** : `ReportingController`  
**Accès** : Administrateur, RH

#### Description
Tableau de bord principal avec statistiques et graphiques de l'application.

#### Fonctionnalités
- ✅ **Statistiques globales** : Nombre d'utilisateurs, commandes, formules
- ✅ **Graphiques** : Graphiques de consommation, tendances
- ✅ **Filtrage par période** : Sélection de la période d'analyse
- ✅ **Export** : Export des rapports
- ✅ **Indicateurs clés** : KPIs principaux

#### Rôles autorisés
- ✅ Administrateur
- ✅ RH

---

### 8.2. Extraction des commandes (`/Extraction`)
**Fichier** : `Views/Extraction/Index.cshtml`  
**Contrôleur** : `ExtractionController`  
**Accès** : Administrateur, RH, PrestataireCantine

#### Description
Extraction et export des commandes avec définition de marges.

#### Fonctionnalités
- ✅ **Sélection de période** : Date de début et fin
- ✅ **Définition de marges** : Marges à ajouter par formule
- ✅ **Extraction** : Génération du fichier d'extraction
- ✅ **Prévisualisation** : Aperçu avant export
- ✅ **Export Excel** : Export des commandes avec marges

#### Rôles autorisés
- ✅ Administrateur
- ✅ RH
- ✅ PrestataireCantine

---

## 9. Vues de configuration

### 9.1. Configuration des commandes (`/ConfigurationCommande`)
**Fichier** : `Views/ConfigurationCommande/Index.cshtml`  
**Contrôleur** : `ConfigurationCommandeController`  
**Accès** : Administrateur, RH

#### Description
Configuration générale des paramètres de commandes (blocage, délais, etc.).

#### Fonctionnalités
- ✅ **Blocage des commandes** : Activation/désactivation du blocage
- ✅ **Périodes de blocage** : Configuration des dates de blocage
- ✅ **Délais** : Délais d'annulation, de modification
- ✅ **Paramètres généraux** : Autres paramètres système
- ✅ **Sauvegarde** : Mise à jour des configurations

#### Rôles autorisés
- ✅ Administrateur
- ✅ RH

---

### 9.2. Diagnostic configuration (`/DiagnosticConfig`)
**Fichier** : `Views/DiagnosticConfig/Index.cshtml`  
**Contrôleur** : `DiagnosticConfigController`  
**Accès** : Administrateur

#### Description
Outil de diagnostic pour vérifier la configuration système.

#### Fonctionnalités
- ✅ **Vérification des paramètres** : Contrôle de la configuration
- ✅ **Détection d'erreurs** : Identification des problèmes
- ✅ **Recommandations** : Suggestions de correction
- ✅ **Statut** : État de santé de la configuration

#### Rôles autorisés
- ✅ Administrateur uniquement

---

## 10. Vues de gestion des prestataires

### 10.1. Liste des prestataires (`/Prestataire`)
**Fichier** : `Views/Prestataire/Index.cshtml`  
**Contrôleur** : `PrestataireController`  
**Accès** : Administrateur

#### Description
Liste des prestataires de cantine.

#### Fonctionnalités
- ✅ **Liste des prestataires** : Tous les prestataires
- ✅ **Actions** : Créer, Modifier, Voir détails
- ✅ **Statistiques** : Nombre de commandes par prestataire

#### Rôles autorisés
- ✅ Administrateur

---

### 10.2. Gestion prestataire cantine (`/PrestataireCantine`)
**Fichier** : `Views/PrestataireCantine/List.cshtml`  
**Contrôleur** : `PrestataireCantineController`  
**Accès** : Administrateur, RH

#### Description
Gestion des prestataires de cantine (utilisateurs avec rôle PrestataireCantine).

#### Fonctionnalités
- ✅ **Liste des prestataires** : Tous les prestataires de cantine
- ✅ **Actions** : Créer, Modifier, Voir détails
- ✅ **Commandes** : Vue des commandes par prestataire
- ✅ **Exportations** : Historique des exports

#### Rôles autorisés
- ✅ Administrateur
- ✅ RH

---

### 10.3. Quantités commande prestataire (`/PrestataireCantine/QuantitesCommandePrestataire`)
**Fichier** : `Views/PrestataireCantine/QuantitesCommandePrestataire.cshtml`  
**Contrôleur** : `PrestataireCantineController`  
**Accès** : PrestataireCantine, Administrateur, RH

#### Description
Affichage des quantités de commandes par formule pour le prestataire connecté.

#### Fonctionnalités
- ✅ **Quantités par formule** : Nombre de commandes par formule
- ✅ **Par date** : Quantités pour une date spécifique
- ✅ **Export** : Export des quantités
- ✅ **Filtrage** : Par période ou formule

#### Rôles autorisés
- ✅ PrestataireCantine
- ✅ Administrateur
- ✅ RH

---

### 10.4. Générer commande prestataire (`/PrestataireCantine/GenererCommande`)
**Fichier** : `Views/PrestataireCantine/GenererCommande.cshtml`  
**Contrôleur** : `PrestataireCantineController`  
**Accès** : PrestataireCantine, Administrateur, RH

#### Description
Génération de commandes pour le prestataire (extraction des commandes à préparer).

#### Fonctionnalités
- ✅ **Sélection de date** : Date pour laquelle générer les commandes
- ✅ **Génération** : Création du fichier de commandes
- ✅ **Export** : Export Excel des commandes
- ✅ **Marges** : Application des marges définies

#### Rôles autorisés
- ✅ PrestataireCantine
- ✅ Administrateur
- ✅ RH

---

### 10.5. Gestion des marges (`/PrestataireCantine/GestionMarges`)
**Fichier** : `Views/PrestataireCantine/GestionMarges.cshtml`  
**Contrôleur** : `PrestataireCantineController`  
**Accès** : PrestataireCantine, Administrateur, RH

#### Description
Gestion des marges pour les formules (marges supplémentaires à prévoir).

#### Fonctionnalités
- ✅ **Définition de marges** : Marges par formule
- ✅ **Par date** : Marges pour une date spécifique
- ✅ **Modification** : Ajustement des marges
- ✅ **Validation** : Sauvegarde des marges

#### Rôles autorisés
- ✅ PrestataireCantine
- ✅ Administrateur
- ✅ RH

---

### 10.6. Exportations (`/PrestataireCantine/Exportations`)
**Fichier** : `Views/PrestataireCantine/Exportations.cshtml`  
**Contrôleur** : `PrestataireCantineController`  
**Accès** : PrestataireCantine, Administrateur, RH

#### Description
Historique des exportations de commandes par le prestataire.

#### Fonctionnalités
- ✅ **Historique** : Liste des exports effectués
- ✅ **Dates** : Date et heure des exports
- ✅ **Téléchargement** : Re-téléchargement des fichiers exportés
- ✅ **Filtrage** : Par période

#### Rôles autorisés
- ✅ PrestataireCantine
- ✅ Administrateur
- ✅ RH

---

## 11. Vues de diagnostics

### 11.1. Diagnostic commandes (`/DiagnosticCommande`)
**Fichier** : `Views/DiagnosticCommande08/` (dossier)  
**Contrôleur** : `DiagnosticCommandeController`  
**Accès** : Administrateur

#### Description
Outils de diagnostic pour analyser les commandes et détecter les problèmes.

#### Fonctionnalités
- ✅ **Analyse des commandes** : Détection des incohérences
- ✅ **Statistiques** : Statistiques détaillées
- ✅ **Vérifications** : Vérification de l'intégrité des données

#### Rôles autorisés
- ✅ Administrateur uniquement

---

### 11.2. Diagnostic utilisateurs (`/DiagnosticUser`)
**Fichier** : Vues dans le contrôleur  
**Contrôleur** : `DiagnosticUserController`  
**Accès** : Administrateur

#### Description
Outils de diagnostic pour analyser les utilisateurs et leurs données.

#### Fonctionnalités
- ✅ **Analyse des utilisateurs** : Détection des problèmes
- ✅ **Vérifications** : Vérification de l'intégrité
- ✅ **Statistiques** : Statistiques par utilisateur

#### Rôles autorisés
- ✅ Administrateur uniquement

---

## 12. Vues administratives

### 12.1. Administration (`/Admin`)
**Fichier** : `Views/Admin/Index.cshtml`  
**Contrôleur** : `AdminController`  
**Accès** : Administrateur

#### Description
Page d'administration principale avec statistiques et outils de maintenance.

#### Fonctionnalités
- ✅ **Statistiques système** : Nombre d'utilisateurs, commandes, formules
- ✅ **Outils de maintenance** : Nettoyage, réinitialisation
- ✅ **Gestion de base de données** : Opérations sur la DB
- ✅ **Logs** : Consultation des logs système

#### Rôles autorisés
- ✅ Administrateur uniquement

---

### 12.2. Nettoyage (`/Cleanup`)
**Fichier** : `Views/Cleanup/Index.cshtml`  
**Contrôleur** : `CleanupController`  
**Accès** : Administrateur

#### Description
Outils de nettoyage de la base de données (suppression de données obsolètes).

#### Fonctionnalités
- ✅ **Nettoyage sélectif** : Choix des données à nettoyer
- ✅ **Suppression** : Suppression de données obsolètes
- ✅ **Sauvegarde** : Option de sauvegarde avant nettoyage
- ✅ **Logs** : Traçabilité des opérations

#### Rôles autorisés
- ✅ Administrateur uniquement

---

### 12.3. Gestion des départements (`/Departement`)
**Fichier** : `Views/Departement/Index.cshtml`  
**Contrôleur** : `DepartementController`  
**Accès** : Administrateur, RH

#### Description
Gestion des départements de l'organisation.

#### Fonctionnalités
- ✅ **Liste des départements** : Tous les départements
- ✅ **Création** : Ajout de nouveaux départements
- ✅ **Modification** : Modification des départements existants
- ✅ **Suppression** : Suppression (soft delete)
- ✅ **Pagination** : Navigation par pages

#### Rôles autorisés
- ✅ Administrateur
- ✅ RH

---

### 12.4. Gestion des fonctions (`/Fonction`)
**Fichier** : `Views/Fonction/Index.cshtml`  
**Contrôleur** : `FonctionController`  
**Accès** : Administrateur, RH

#### Description
Gestion des fonctions (postes) dans l'organisation.

#### Fonctionnalités
- ✅ **Liste des fonctions** : Toutes les fonctions
- ✅ **Création** : Ajout de nouvelles fonctions
- ✅ **Modification** : Modification des fonctions existantes
- ✅ **Suppression** : Suppression (soft delete)
- ✅ **Pagination** : Navigation par pages

#### Rôles autorisés
- ✅ Administrateur
- ✅ RH

---

### 12.5. Groupes non-CIT (`/GroupeNonCit`)
**Fichier** : `Views/GroupeNonCit/Index.cshtml`  
**Contrôleur** : `GroupeNonCitController`  
**Accès** : Administrateur, RH

#### Description
Gestion des groupes non-CIT (groupes externes comme les Douaniers) avec quotas permanents.

#### Fonctionnalités
- ✅ **Liste des groupes** : Tous les groupes non-CIT
- ✅ **Quotas permanents** : Quotas jour et nuit (permanents, ne se décrémentent pas)
- ✅ **Restrictions** : Restriction aux formules standard
- ✅ **Création** : Ajout de nouveaux groupes
- ✅ **Modification** : Modification des quotas et restrictions
- ✅ **Détails** : Vue détaillée d'un groupe

**Important** : Cette vue remplace l'ancienne vue `/Quota` (historique) qui a été supprimée.

#### Rôles autorisés
- ✅ Administrateur
- ✅ RH

---

### 12.6. Gestion des marges (`/GestionMarges`)
**Fichier** : `Views/GestionMarges/Index.cshtml`  
**Contrôleur** : `GestionMargesController`  
**Accès** : Administrateur, RH

#### Description
Gestion globale des marges pour les formules (marges supplémentaires).

#### Fonctionnalités
- ✅ **Marges par formule** : Définition des marges
- ✅ **Par date** : Marges pour des dates spécifiques
- ✅ **Modification** : Ajustement des marges
- ✅ **Statistiques** : Vue d'ensemble des marges

#### Rôles autorisés
- ✅ Administrateur
- ✅ RH

---

### 12.7. Visiteurs (`/Visiteur`)
**Fichier** : `Views/Visiteur/List.cshtml`  
**Contrôleur** : `VisiteurController`  
**Accès** : Administrateur, RH

#### Description
Gestion des visiteurs et de leurs commandes.

#### Fonctionnalités
- ✅ **Liste des visiteurs** : Tous les visiteurs
- ✅ **Création** : Ajout de nouveaux visiteurs
- ✅ **Commandes** : Vue des commandes par visiteur
- ✅ **Filtrage** : Par département, date
- ✅ **Export** : Export des données visiteurs

#### Rôles autorisés
- ✅ Administrateur
- ✅ RH

---

## 13. Vues partagées

### 13.1. Layout principal (`Views/Shared/_Layout.cshtml`)
**Description** : Layout principal de l'application avec menu de navigation, header, footer.

**Fonctionnalités** :
- ✅ **Menu de navigation** : Menu principal avec tous les liens
- ✅ **Authentification** : Affichage selon le rôle
- ✅ **Notifications** : Zone de notifications
- ✅ **Header/Footer** : En-tête et pied de page
- ✅ **Responsive** : Design adaptatif

---

### 13.2. Messages d'erreur (`Views/Shared/Error.cshtml`)
**Description** : Page d'erreur générique pour les erreurs non gérées.

---

### 13.3. Page non trouvée (`Views/Shared/NotFound.cshtml`)
**Description** : Page 404 pour les ressources non trouvées.

---

### 13.4. Erreur serveur (`Views/Shared/ServerError.cshtml`)
**Description** : Page 500 pour les erreurs serveur.

---

### 13.5. Non autorisé (`Views/Shared/Unauthorized.cshtml`)
**Description** : Page 403 pour les accès non autorisés.

---

### 13.6. Messages de notification (`Views/Shared/_NotificationMessages.cshtml`)
**Description** : Partial view pour afficher les messages de notification (succès, erreur, info).

---

### 13.7. Pagination (`Views/Shared/_Pagination.cshtml`)
**Description** : Partial view pour la pagination des listes.

---

### 13.8. Scripts de validation (`Views/Shared/_ValidationScriptsPartial.cshtml`)
**Description** : Partial view pour les scripts de validation côté client (jQuery Validation).

---

## 📊 Résumé par catégorie

| Catégorie | Nombre de vues | Rôles principaux |
|-----------|---------------|------------------|
| Authentification | 4 | Public / Authentifié |
| Accueil | 1 | Tous (contenu adapté) |
| Commandes | 11 | Tous (selon permissions) |
| Menus (Formules) | 6 | Admin, RH |
| Utilisateurs | 6 | Admin uniquement |
| Points consommation | 4 | Admin, RH, Prestataire |
| Facturation | 4 | Admin, RH |
| Reporting | 2 | Admin, RH |
| Configuration | 2 | Admin, RH |
| Prestataires | 6 | Prestataire, Admin, RH |
| Diagnostics | 3 | Admin uniquement |
| Administration | 7 | Admin, RH |
| Partagées | 8 | Tous |
| **TOTAL** | **~64 vues** | |

---

## 🔐 Matrice des permissions par vue

| Vue | Admin | RH | Employé | Prestataire |
|-----|-------|----|---------|-------------| 
| Login | ✅ | ✅ | ✅ | ✅ |
| Home | ✅ | ✅ | ✅ | ✅ |
| Commandes (Liste) | ✅ | ✅ | ✅ | ✅ |
| Commandes (Créer) | ✅ | ✅ | ✅ | ❌ |
| Commandes (Instantanée) | ✅ | ✅ | ❌ | ✅ |
| Commandes (Douaniers) | ✅ | ✅ | ❌ | ✅ |
| Formules (CRUD) | ✅ | ✅ | ❌ | ❌ |
| Utilisateurs | ✅ | ❌ | ❌ | ❌ |
| Points Consommation | ✅ | ✅ | ✅* | ✅* |
| Facturation | ✅ | ✅ | ❌ | ❌ |
| Reporting | ✅ | ✅ | ❌ | ❌ |
| Configuration | ✅ | ✅ | ❌ | ❌ |
| Prestataires | ✅ | ✅ | ❌ | ✅* |
| Diagnostics | ✅ | ❌ | ❌ | ❌ |
| Administration | ✅ | ✅** | ❌ | ❌ |

*Seulement leurs propres données  
**Selon la vue

---

## 📝 Notes importantes

1. **Soft Delete** : La plupart des entités utilisent un soft delete (`Supprimer = 1`) plutôt qu'une suppression physique
2. **Pagination** : Beaucoup de listes utilisent la pagination pour améliorer les performances
3. **Filtrage** : Les listes principales offrent des fonctionnalités de filtrage et recherche
4. **Export Excel** : Beaucoup de vues offrent l'export Excel des données
5. **Responsive** : Toutes les vues sont responsives (Bootstrap 5)
6. **Notifications** : SignalR est utilisé pour les notifications en temps réel

---

**Document créé le : 2025-01-XX**  
**Dernière mise à jour : 2025-01-XX**  
**Auteur : Équipe de développement**

