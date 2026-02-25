# Guide Complet des Fonctionnalités - O'Beli

## 📊 Vue d'Ensemble

**Application** : O'Beli - Système de Gestion de Cantine CIT  
**Version** : 1.0  
**Date** : 10 février 2026  
**Total Fonctionnalités** : 95+

---

## 📑 Table des Matières

1. [Authentification](#authentification)
2. [Gestion des Utilisateurs](#gestion-des-utilisateurs)
3. [Gestion des Directions](#gestion-des-directions)
4. [Gestion des Services](#gestion-des-services)
5. [Gestion des Fonctions](#gestion-des-fonctions)
6. [Gestion des Menus](#gestion-des-menus)
7. [Commandes](#commandes)
8. [Commandes Visiteurs](#commandes-visiteurs)
9. [Points de Consommation](#points-de-consommation)
10. [Reporting](#reporting)
11. [Extraction et Facturation](#extraction-et-facturation)
12. [Prestataires](#prestataires)
13. [Quotas](#quotas)
14. [Configuration](#configuration)
15. [Administration](#administration)

---

## 1. Authentification

### 1.1 Connexion
- **URL** : `/Auth/Login`
- **Méthode** : GET/POST
- **Rôles** : Tous
- **Description** : Connexion au système avec matricule et mot de passe
- **Paramètres** :
  - Matricule (requis)
  - MotDePasse (requis)
  - SeSouvenirDeMoi (optionnel)

### 1.2 Déconnexion
- **URL** : `/Auth/Logout`
- **Méthode** : GET
- **Rôles** : Tous connectés
- **Description** : Déconnexion et suppression de la session

### 1.3 Profil Utilisateur
- **URL** : `/Auth/Profile`
- **Méthode** : GET
- **Rôles** : Tous connectés
- **Description** : Consulter son profil personnel

### 1.4 Modifier Profil
- **URL** : `/Auth/EditProfile`
- **Méthode** : GET/POST
- **Rôles** : Tous connectés
- **Description** : Modifier ses informations personnelles
- **Paramètres** : Nom, Prenoms, Email, PhoneNumber, Lieu, DirectionId, FonctionId, Site

### 1.5 Changer Mot de Passe
- **URL** : `/Auth/ChangePassword`
- **Méthode** : GET/POST
- **Rôles** : Tous connectés
- **Description** : Changer son propre mot de passe
- **Paramètres** : MotDePasseActuel, NouveauMotDePasse, Confirmation

### 1.6 Mot de Passe Oublié
- **URL** : `/Auth/Forgot`
- **Méthode** : GET/POST
- **Rôles** : Tous
- **Description** : Demander un lien de réinitialisation

### 1.7 Réinitialiser Mot de Passe
- **URL** : `/Auth/Reset`
- **Méthode** : GET/POST
- **Rôles** : Tous
- **Description** : Réinitialiser avec token reçu

---

## 2. Gestion des Utilisateurs

### 2.1 Liste des Utilisateurs
- **URL** : `/Utilisateur/List`
- **Méthode** : GET
- **Rôles** : Admin, RH
- **Description** : Afficher tous les utilisateurs actifs avec pagination

### 2.2 Créer Utilisateur
- **URL** : `/Utilisateur/Create`
- **Méthode** : GET/POST
- **Rôles** : Admin, RH
- **Description** : Créer un nouvel utilisateur
- **Paramètres** : Nom, Prenoms, UserName, Email, PhoneNumber, MotDePasse, Role, DirectionId, FonctionId, Site

### 2.3 Détails Utilisateur
- **URL** : `/Utilisateur/Details/{id}`
- **Méthode** : GET
- **Rôles** : Admin, RH
- **Description** : Afficher les détails complets d'un utilisateur

### 2.4 Modifier Utilisateur
- **URL** : `/Utilisateur/Edit/{id}`
- **Méthode** : GET/POST
- **Rôles** : Admin, RH
- **Description** : Modifier les informations d'un utilisateur

### 2.5 Supprimer Utilisateur
- **URL** : `/Utilisateur/Delete/{id}`
- **Méthode** : POST
- **Rôles** : Admin, RH
- **Description** : Supprimer un utilisateur (soft delete)

### 2.6 Réinitialiser Mots de Passe
- **URL** : `/Utilisateur/ResetPassword`
- **Méthode** : GET/POST
- **Rôles** : Admin, RH
- **Description** : Réinitialiser les mots de passe de plusieurs utilisateurs

---

## 3. Gestion des Directions

### 3.1 Liste des Directions
- **URL** : `/Direction/List`
- **Méthode** : GET
- **Rôles** : Admin, RH
- **Description** : Afficher toutes les directions avec pagination (5 par page)

### 3.2 Créer Direction
- **URL** : `/Direction/Create`
- **Méthode** : GET/POST
- **Rôles** : Admin, RH
- **Description** : Créer une nouvelle direction organisationnelle
- **Paramètres** : Nom, Code, Description, Responsable, Email

### 3.3 Détails Direction
- **URL** : `/Direction/Details/{id}`
- **Méthode** : GET
- **Rôles** : Admin, RH
- **Description** : Afficher les détails d'une direction et ses services

### 3.4 Modifier Direction
- **URL** : `/Direction/Edit/{id}`
- **Méthode** : GET/POST
- **Rôles** : Admin, RH
- **Description** : Modifier les informations d'une direction

### 3.5 Supprimer Direction
- **URL** : `/Direction/Delete/{id}`
- **Méthode** : POST
- **Rôles** : Admin, RH
- **Description** : Supprimer une direction (avec vérification des dépendances)

### 3.6 API Liste Directions
- **URL** : `/Direction/GetDirections`
- **Méthode** : GET (API JSON)
- **Rôles** : Admin, RH
- **Description** : API pour obtenir toutes les directions actives

---

## 4. Gestion des Services

### 4.1 Liste des Services
- **URL** : `/Service/List`
- **Méthode** : GET
- **Rôles** : Admin, RH
- **Description** : Afficher tous les services avec pagination (5 par page)

### 4.2 Créer Service
- **URL** : `/Service/Create`
- **Méthode** : GET/POST
- **Rôles** : Admin, RH
- **Description** : Créer un nouveau service rattaché à une direction
- **Paramètres** : Nom, Code, Description, DirectionId, Responsable, Email

### 4.3 Détails Service
- **URL** : `/Service/Details/{id}`
- **Méthode** : GET
- **Rôles** : Admin, RH
- **Description** : Afficher les détails d'un service

### 4.4 Modifier Service
- **URL** : `/Service/Edit/{id}`
- **Méthode** : GET/POST
- **Rôles** : Admin, RH
- **Description** : Modifier les informations d'un service

### 4.5 Supprimer Service
- **URL** : `/Service/Delete/{id}`
- **Méthode** : POST
- **Rôles** : Admin, RH
- **Description** : Supprimer un service (avec vérification des dépendances)

### 4.6 API Services par Direction
- **URL** : `/Service/GetServicesByDirection`
- **Méthode** : GET (API JSON)
- **Rôles** : Admin, RH
- **Description** : API pour obtenir les services d'une direction

---

## 5. Gestion des Fonctions

### 5.1 Liste des Fonctions
- **URL** : `/Fonction/List`
- **Méthode** : GET
- **Rôles** : Admin, RH
- **Description** : Afficher toutes les fonctions (postes) avec pagination

### 5.2 Créer Fonction
- **URL** : `/Fonction/Create`
- **Méthode** : GET/POST
- **Rôles** : Admin, RH
- **Description** : Créer une nouvelle fonction
- **Paramètres** : Nom, Description

### 5.3 Détails Fonction
- **URL** : `/Fonction/Details/{id}`
- **Méthode** : GET
- **Rôles** : Admin, RH
- **Description** : Afficher les détails d'une fonction et utilisateurs associés

### 5.4 Modifier Fonction
- **URL** : `/Fonction/Edit/{id}`
- **Méthode** : GET/POST
- **Rôles** : Admin, RH
- **Description** : Modifier une fonction existante

### 5.5 Supprimer Fonction
- **URL** : `/Fonction/Delete/{id}`
- **Méthode** : POST
- **Rôles** : Admin, RH
- **Description** : Supprimer une fonction (avec vérification des dépendances)

---

## 6. Gestion des Menus

### 6.1 Liste des Menus
- **URL** : `/FormuleJour/Index`
- **Méthode** : GET
- **Rôles** : Admin, RH, Prestataire
- **Description** : Afficher tous les menus du jour avec filtres par date
- **Filtres** : DateDebut, DateFin, TypeFormule

### 6.2 Créer Menu
- **URL** : `/FormuleJour/Create`
- **Méthode** : GET/POST
- **Rôles** : Admin, RH, Prestataire
- **Description** : Créer un nouveau menu pour une date spécifique
- **Paramètres** : Date, NomFormule, Plat, Garniture, Entree, Dessert, PlatStandard1, GarnitureStandard1, PlatStandard2, GarnitureStandard2

### 6.3 Créer Menus Multiples
- **URL** : `/FormuleJour/CreateMultiDay`
- **Méthode** : GET/POST
- **Rôles** : Admin, RH, Prestataire
- **Description** : Créer des menus pour plusieurs jours consécutifs

### 6.4 Détails Menu
- **URL** : `/FormuleJour/Details/{id}`
- **Méthode** : GET
- **Rôles** : Admin, RH, Prestataire
- **Description** : Afficher les détails d'un menu

### 6.5 Modifier Menu
- **URL** : `/FormuleJour/Edit/{id}`
- **Méthode** : GET/POST
- **Rôles** : Admin, RH, Prestataire
- **Description** : Modifier un menu existant

### 6.6 Supprimer Menu
- **URL** : `/FormuleJour/Delete/{id}`
- **Méthode** : POST
- **Rôles** : Admin, RH, Prestataire
- **Description** : Supprimer un menu (avec vérification des commandes associées)

### 6.7 Importer Menus Excel
- **URL** : `/FormuleJour/Import`
- **Méthode** : GET/POST
- **Rôles** : Admin, RH, Prestataire
- **Description** : Importer des menus depuis un fichier Excel (.xlsx)

### 6.8 Sélectionner Période
- **URL** : `/FormuleJour/SelectPeriod`
- **Méthode** : GET/POST
- **Rôles** : Admin, RH, Prestataire
- **Description** : Sélectionner une période pour créer des menus

---

## 7. Commandes

### 7.1 Liste des Commandes
- **URL** : `/Commande/Index`
- **Méthode** : GET
- **Rôles** : Tous connectés
- **Description** : Afficher toutes les commandes avec filtres multiples
- **Filtres** : Status, DateDebut, DateFin, TypeClient, Site, Page

### 7.2 Passer Commande
- **URL** : `/Commande/Create`
- **Méthode** : GET/POST
- **Rôles** : Tous connectés
- **Description** : Créer une nouvelle commande personnelle (délai 12h minimum)
- **Paramètres** : IdFormule, DateConsommation, Quantite, PeriodeService

### 7.3 Détails Commande
- **URL** : `/Commande/Details/{id}`
- **Méthode** : GET
- **Rôles** : Tous connectés
- **Description** : Afficher les détails complets d'une commande

### 7.4 Modifier Commande
- **URL** : `/Commande/Edit/{id}`
- **Méthode** : GET/POST
- **Rôles** : Tous connectés
- **Description** : Modifier une commande existante (si délai respecté)

### 7.5 Annuler Commande
- **URL** : `/Commande/Cancel/{id}`
- **Méthode** : POST
- **Rôles** : Tous connectés
- **Description** : Annuler une commande (si délai respecté)

### 7.6 Supprimer Commande
- **URL** : `/Commande/Delete/{id}`
- **Méthode** : POST
- **Rôles** : Admin, RH
- **Description** : Supprimer définitivement une commande

### 7.7 Vérifier Commande
- **URL** : `/Commande/VerifierCommande`
- **Méthode** : GET/POST
- **Rôles** : Admin, RH, Prestataire
- **Description** : Vérifier une commande par son code

### 7.8 Commande Instantanée
- **URL** : `/Commande/CreerCommandeInstantanee`
- **Méthode** : GET/POST
- **Rôles** : Admin, RH, Prestataire
- **Description** : Créer une commande avec délai < 12h
- **Paramètres** : IdFormule, DateConsommation, UtilisateurId, Quantite

### 7.9 Commande Groupée
- **URL** : `/Commande/CreerCommandeGroupee`
- **Méthode** : GET/POST
- **Rôles** : Admin, RH
- **Description** : Créer une commande pour un groupe d'utilisateurs
- **Paramètres** : DirectionId, DateDebut, DateFin, TypeFormule, NombrePersonnes

### 7.10 Commande Douaniers
- **URL** : `/Commande/CreerCommandeDouaniers`
- **Méthode** : GET/POST
- **Rôles** : Admin, RH, Prestataire
- **Description** : Créer une commande pour les douaniers (quota spécial)
- **Paramètres** : Date, NombreDouaniers, TypeFormule

### 7.11 Valider Commande Douaniers
- **URL** : `/Commande/ValiderCommandeDouaniers`
- **Méthode** : GET/POST
- **Rôles** : Admin, RH, Prestataire
- **Description** : Valider et finaliser les commandes des douaniers

### 7.12 Exporter Excel
- **URL** : `/Commande/ExporterExcel`
- **Méthode** : GET
- **Rôles** : Admin, RH
- **Description** : Exporter les commandes filtrées en Excel
- **Paramètres** : DateDebut, DateFin, Status, TypeClient

### 7.13 API Formules Disponibles
- **URL** : `/Commande/GetFormulesDisponibles`
- **Méthode** : POST (API JSON)
- **Rôles** : Tous connectés
- **Description** : API pour obtenir les formules d'une période

---

## 8. Commandes Visiteurs

### 8.1 Liste Commandes Visiteurs
- **URL** : `/Visiteur/Commands`
- **Méthode** : GET
- **Rôles** : Admin, RH
- **Description** : Afficher les commandes des visiteurs avec filtres
- **Filtres** : Page, DirectionId, DateDebut, DateFin

### 8.2 Créer Commande Visiteur
- **URL** : `/Visiteur/Create`
- **Méthode** : GET/POST
- **Rôles** : Admin, RH
- **Description** : Créer une commande pour un visiteur externe
- **Paramètres** : VisiteurNom, VisiteurTelephone, DirectionId, DateDebut, DateFin, NombreVisiteurs, TypeFormule

### 8.3 API Créer Commande
- **URL** : `/Visiteur/CreateCommande`
- **Méthode** : POST (API JSON)
- **Rôles** : Admin, RH
- **Description** : API pour créer une commande visiteur

### 8.4 Liste Formules Visiteurs
- **URL** : `/Visiteur/List`
- **Méthode** : GET
- **Rôles** : Admin, RH
- **Description** : Afficher les formules disponibles pour visiteurs

### 8.5 API Formules Visiteurs
- **URL** : `/Visiteur/GetFormules`
- **Méthode** : POST (API JSON)
- **Rôles** : Admin, RH
- **Description** : API pour obtenir les formules disponibles

---

## 9. Points de Consommation

### 9.1 Mes Points de Consommation
- **URL** : `/Commande/MesPointsConsommation`
- **Méthode** : GET
- **Rôles** : Tous connectés
- **Description** : Voir ses propres points de consommation par période
- **Filtres** : DateDebut, DateFin

### 9.2 Point Consommation CIT
- **URL** : `/PointsConsommation/PointConsommationCIT`
- **Méthode** : GET
- **Rôles** : Admin, RH
- **Description** : Voir les points de consommation de tous les utilisateurs CIT
- **Filtres** : DateDebut, DateFin, DirectionId

### 9.3 Créer Point Consommation
- **URL** : `/PointsConsommation/Create`
- **Méthode** : GET/POST
- **Rôles** : Admin, RH
- **Description** : Créer un nouveau point de consommation
- **Paramètres** : UtilisateurId, Date, TypeConsommation, Quantite

### 9.4 Modifier Point Consommation
- **URL** : `/PointsConsommation/Edit/{id}`
- **Méthode** : GET/POST
- **Rôles** : Admin, RH
- **Description** : Modifier un point de consommation existant

### 9.5 Cumul Points Consommation
- **URL** : `/Commande/CumulPointsConsommation`
- **Méthode** : GET
- **Rôles** : Admin, RH
- **Description** : Statistiques détaillées des consommations
- **Filtres** : DateDebut, DateFin, DirectionId, Site

---

## 10. Reporting

### 10.1 Tableau de Bord
- **URL** : `/Reporting/Dashboard`
- **Méthode** : GET
- **Rôles** : Admin, RH, Prestataire
- **Description** : Tableau de bord avec indicateurs et graphiques
- **Filtres** : DateDebut, DateFin, Site, DirectionId, FonctionId

### 10.2 Exporter CSV
- **URL** : `/Reporting/ExporterCsv`
- **Méthode** : POST
- **Rôles** : Admin, RH, Prestataire
- **Description** : Exporter le rapport en format CSV

### 10.3 Exporter PDF
- **URL** : `/Reporting/ExporterPdf`
- **Méthode** : POST
- **Rôles** : Admin, RH, Prestataire
- **Description** : Exporter le rapport en format PDF

### 10.4 API Données Participation
- **URL** : `/Reporting/GetParticipationData`
- **Méthode** : GET (API JSON)
- **Rôles** : Admin, RH, Prestataire
- **Description** : API pour les données de participation par jour

---

## 11. Extraction et Facturation

### 11.1 Extraction Commandes
- **URL** : `/Extraction/Index`
- **Méthode** : GET/POST
- **Rôles** : Admin, RH
- **Description** : Extraire les commandes pour facturation avec marges
- **Paramètres** : DateDebut, DateFin, TypeFormule, Marges

### 11.2 Définir Marges
- **URL** : `/Extraction/DefinirMarges`
- **Méthode** : GET/POST
- **Rôles** : Admin, RH
- **Description** : Définir les marges de facturation par formule

### 11.3 Gestion des Marges
- **URL** : `/GestionMarges/Index`
- **Méthode** : GET/POST
- **Rôles** : Admin, RH
- **Description** : Gérer les marges de facturation globales
- **Paramètres** : MargeAmeliore, MargeStandard1, MargeStandard2

### 11.4 Gestion Facturation
- **URL** : `/Facturation/Index`
- **Méthode** : GET
- **Rôles** : Admin, RH
- **Description** : Interface de gestion de la facturation
- **Filtres** : Mois, Annee

### 11.5 Paramètres Facturation
- **URL** : `/ParametresFacturation/Index`
- **Méthode** : GET/POST
- **Rôles** : Admin, RH
- **Description** : Configurer les paramètres de facturation automatique
- **Paramètres** : TarifAmeliore, TarifStandard1, TarifStandard2, JourFacturation

### 11.6 Diagnostic Facturation
- **URL** : `/DiagnosticFacturation/Index`
- **Méthode** : GET
- **Rôles** : Admin, RH
- **Description** : Diagnostiquer les problèmes de facturation

### 11.7 Facturation Automatique
- **URL** : `/FacturationAutomatique/Index`
- **Méthode** : GET/POST
- **Rôles** : Admin, RH
- **Description** : Gérer la facturation automatique mensuelle

---

## 12. Prestataires

### 12.1 Générer Commande Prestataire
- **URL** : `/PrestataireCantine/GenererCommande`
- **Méthode** : GET/POST
- **Rôles** : Admin, RH
- **Description** : Générer une commande pour le prestataire de cantine
- **Paramètres** : DateDebut, DateFin, PrestataireId

### 12.2 Quantités Commande
- **URL** : `/PrestataireCantine/QuantitesCommandePrestataire`
- **Méthode** : GET
- **Rôles** : Admin, RH
- **Description** : Voir les quantités par formule pour le prestataire

### 12.3 Gestion Marges Prestataire
- **URL** : `/PrestataireCantine/GestionMarges`
- **Méthode** : GET/POST
- **Rôles** : Admin, RH
- **Description** : Gérer les marges spécifiques du prestataire

### 12.4 Liste Prestataires
- **URL** : `/PrestataireCantine/List`
- **Méthode** : GET
- **Rôles** : Admin, RH
- **Description** : Liste de tous les prestataires de cantine

---

## 13. Quotas

### 13.1 Quotas Permanents Groupes
- **URL** : `/GroupeNonCit/Index`
- **Méthode** : GET
- **Rôles** : Admin, RH
- **Description** : Gérer les quotas des groupes non-CIT (douaniers, etc.)

### 13.2 Créer Quota Groupe
- **URL** : `/GroupeNonCit/Create`
- **Méthode** : GET/POST
- **Rôles** : Admin, RH
- **Description** : Créer un quota pour un groupe spécifique
- **Paramètres** : NomGroupe, TypeGroupe, QuotaJournalier, Description

### 13.3 Modifier Quota Groupe
- **URL** : `/GroupeNonCit/Edit/{id}`
- **Méthode** : GET/POST
- **Rôles** : Admin, RH
- **Description** : Modifier un quota de groupe existant

### 13.4 Supprimer Quota Groupe
- **URL** : `/GroupeNonCit/Delete/{id}`
- **Méthode** : POST
- **Rôles** : Admin, RH
- **Description** : Supprimer un quota de groupe

### 13.5 Détails Quota Groupe
- **URL** : `/GroupeNonCit/Details/{id}`
- **Méthode** : GET
- **Rôles** : Admin, RH
- **Description** : Voir les détails et l'historique d'un quota

---

## 14. Configuration

### 14.1 Configuration Commandes
- **URL** : `/ConfigurationCommande/Index`
- **Méthode** : GET/POST
- **Rôles** : Admin, RH
- **Description** : Configurer les délais et règles de commande
- **Paramètres** : DelaiCommandeHeures, DelaiAnnulationHeures, DelaiModificationHeures

### 14.2 Diagnostic Configuration
- **URL** : `/DiagnosticConfig/Index`
- **Méthode** : GET
- **Rôles** : Admin, RH
- **Description** : Diagnostiquer la configuration du système

### 14.3 Initialisation Config
- **URL** : `/InitConfig/Index`
- **Méthode** : GET/POST
- **Rôles** : Admin
- **Description** : Initialiser la configuration par défaut

---

## 15. Administration

### 15.1 Administration DB
- **URL** : `/Admin/Index`
- **Méthode** : GET
- **Rôles** : Admin
- **Description** : Interface d'administration de la base de données

### 15.2 Statistiques Système
- **URL** : `/Admin/GetStatistics`
- **Méthode** : GET (API JSON)
- **Rôles** : Admin
- **Description** : Obtenir les statistiques du système

### 15.3 Nettoyage Base
- **URL** : `/Cleanup/Index`
- **Méthode** : GET/POST
- **Rôles** : Admin
- **Description** : Nettoyer les données obsolètes de la base
- **Paramètres** : Type, DateLimite

### 15.4 Diagnostic Commandes
- **URL** : `/DiagnosticCommande/Index`
- **Méthode** : GET
- **Rôles** : Admin, RH
- **Description** : Diagnostiquer les problèmes de commandes

### 15.5 Diagnostic Utilisateurs
- **URL** : `/DiagnosticUser/Index`
- **Méthode** : GET
- **Rôles** : Admin, RH
- **Description** : Diagnostiquer les problèmes utilisateurs

---

## 📊 Statistiques

- **Total Modules** : 15
- **Total Fonctionnalités** : 95+
- **Rôles** : 4 (Admin, RH, Prestataire, Employé)
- **Types d'Actions** : CRUD, API, Export, Diagnostic, Configuration

---

## 🔐 Matrice des Rôles

| Rôle | Accès |
|------|-------|
| **Administrateur** | Accès complet à toutes les fonctionnalités |
| **RH** | Gestion utilisateurs, directions, services, commandes, reporting |
| **Prestataire Cantine** | Menus, commandes, vérification, reporting |
| **Employé** | Commandes personnelles, profil, points de consommation |

---

## 📝 Notes

- Toutes les suppressions sont des "soft deletes" (marquage Supprimer = 1)
- Les délais de commande sont configurables (par défaut 12h)
- Les quotas des douaniers sont gérés séparément
- La facturation peut être automatique ou manuelle
- Les exports sont disponibles en Excel, CSV et PDF

---

**Document généré le** : 10 février 2026  
**Version** : 1.0
