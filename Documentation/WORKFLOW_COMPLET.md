# 📋 Workflow Complet - Application O'Beli K

## 🎯 Vue d'ensemble du projet

**O'Beli K** est une application web de gestion de commandes de repas pour la Côte d'Ivoire, développée en **ASP.NET Core MVC** avec **Entity Framework Core** et **SQL Server**.

### Technologies principales
- **Backend** : ASP.NET Core 8.0 (MVC)
- **Base de données** : SQL Server avec Entity Framework Core
- **Authentification** : Cookie Authentication (sans Identity)
- **Frontend** : Bootstrap 5, jQuery, Font Awesome
- **Temps réel** : SignalR (notifications)
- **Export** : EPPlus (Excel)
- **Hachage** : BCrypt pour les mots de passe

---

## 🏗️ Architecture du système

### Structure des dossiers
```
Obeli_K/
├── Controllers/          # Contrôleurs MVC (29 contrôleurs)
├── Models/              # Modèles de données et ViewModels
├── Views/               # Vues Razor (87 fichiers)
├── Services/            # Services métier (12 services)
├── Data/                # DbContext et configuration EF
├── Enums/               # Énumérations
├── Hubs/                # SignalR Hubs
├── wwwroot/             # Fichiers statiques (CSS, JS, images)
└── Migrations/          # Migrations Entity Framework
```

### Modèles de données principaux
- **Utilisateur** : Employés, RH, Administrateurs, Prestataires
- **Commande** : Commandes de repas (précommandées, consommées, annulées)
- **FormuleJour** : Menus quotidiens (Améliorée, Standard 1, Standard 2)
- **PointConsommation** : Validation des repas consommés
- **GroupeNonCit** : Groupes spéciaux (Douaniers, etc.)
- **QuotaJournalier** : Quotas de repas par groupe
- **PrestataireCantine** : Gestion des prestataires
- **ConfigurationCommande** : Paramètres système

---

## 👥 Rôles et permissions

### 1. **Administrateur** (Admin)
- ✅ Accès complet à toutes les fonctionnalités
- ✅ Gestion des utilisateurs, départements, fonctions
- ✅ Configuration système
- ✅ Gestion des formules et commandes
- ✅ Reporting et statistiques
- ✅ Export de données
- ✅ Pas de restriction de délai pour annulation

### 2. **RH** (Ressources Humaines)
- ✅ Gestion des formules
- ✅ Gestion des commandes
- ✅ Points de consommation
- ✅ Reporting
- ✅ Configuration des commandes
- ❌ Gestion des utilisateurs
- ❌ Paramètres système avancés

### 3. **Employé**
- ✅ Consulter les menus de la semaine
- ✅ Créer des commandes (semaine N+1)
- ✅ Voir ses commandes
- ✅ Annuler ses commandes (24h avant consommation)
- ❌ Gestion des formules
- ❌ Accès aux statistiques

### 4. **PrestataireCantine**
- ✅ Voir les menus du jour
- ✅ Voir les statistiques de commandes
- ✅ Créer des commandes instantanées
- ✅ Gérer les marges
- ✅ Exporter les commandes
- ❌ Accès aux commandes des employés

---

## 🔐 Workflow d'authentification

### 1. Connexion
```
1. Utilisateur accède à /Auth/Login
2. Saisit son matricule (UserName) et mot de passe
3. Système vérifie les identifiants via BCrypt
4. Création des claims (ID, nom, rôle)
5. Si Admin → tous les rôles sont ajoutés automatiquement
6. Cookie d'authentification créé (1h par défaut, extensible avec "Se souvenir de moi")
7. Redirection vers /Home/Index
```

### 2. Gestion des sessions
- **Durée par défaut** : 1 heure
- **"Se souvenir de moi"** : Extension à 30 jours
- **Expiration glissante** : Activée
- **Déconnexion** : `/Auth/Logout` supprime le cookie

### 3. Réinitialisation de mot de passe
```
1. Utilisateur clique sur "Mot de passe oublié"
2. Saisit son email
3. Système génère un token SHA-256 (valide 24h)
4. Email envoyé avec lien de réinitialisation
5. Utilisateur clique sur le lien
6. Saisit nouveau mot de passe
7. Token vérifié et invalidé après utilisation
```

---

## 📅 Workflow de gestion des menus (Formules)

### Création de formules (Admin/RH)

#### 1. Accès
```
Paramètres → Formules → Gérer les formules
```

#### 2. Processus de création
```
1. Sélectionner une date
2. Choisir le type de formule :
   - Formule Améliorée (entrée, plat, dessert, garniture)
   - Formule Standard 1 (plat standard 1, garniture standard 1)
   - Formule Standard 2 (plat standard 2, garniture standard 2)
3. Remplir les champs :
   - Entrée (si améliorée)
   - Plat principal
   - Garniture
   - Dessert (si améliorée)
   - Féculent
   - Légumes
4. Sauvegarder
```

#### 3. Types de formules
- **Améliorée** : Menu complet avec entrée, plat, dessert
- **Standard 1** : Plat standard avec garniture
- **Standard 2** : Alternative au Standard 1

#### 4. Gestion
- **Modification** : Possible jusqu'à la date de consommation
- **Suppression** : Soft delete (Supprimer = 1)
- **Historique** : Suivi des modifications dans le champ Historique

---

## 🛒 Workflow de commande

### A. Commande par semaine (Employés)

#### 1. Accès
```
Commandes → Commander
```

#### 2. Processus
```
1. Vérification du blocage des commandes
   - Si vendredi après 12h → Commandes bloquées
   - Si samedi/dimanche → Commandes bloquées
   - Sinon → Affichage des menus semaine N+1

2. Sélection de la semaine
   - Par défaut : Semaine N+1 (lundi à vendredi)
   - Possibilité de naviguer entre semaines

3. Pour chaque jour (Lundi à Vendredi) :
   a. Voir les formules disponibles
   b. Sélectionner une formule
   c. Choisir la période (Jour/Nuit)
   d. Choisir le site (CIT Billing / CIT Terminal)
   e. Cliquer sur "Commander"

4. Validation
   - Vérification des délais (48h avant consommation)
   - Vérification des quotas (pour groupes spéciaux)
   - Création de la commande avec statut "Précommandée"
   - Génération d'un code de commande unique

5. Confirmation
   - Notification en temps réel (SignalR)
   - Affichage dans "Mes Commandes"
```

#### 3. Règles de blocage
```
- Vendredi 12h00 → Fermeture automatique
- Samedi et Dimanche → Commandes bloquées
- Lundi → Réouverture pour semaine N+1
- Configuration modifiable par Admin/RH
```

### B. Commande instantanée (Prestataire/Admin)

#### 1. Accès
```
Commandes → Commande instantanée
```

#### 2. Processus
```
1. Sélectionner le type de client :
   - Employé CIT
   - Groupe Non-CIT (Douaniers, etc.)
   - Visiteur

2. Si Employé CIT :
   - Sélectionner l'utilisateur
   - Choisir la formule du jour
   - Période et site
   - Créer la commande

3. Si Groupe Non-CIT :
   - Sélectionner le groupe
   - Vérifier les quotas disponibles
   - Choisir la formule
   - Spécifier la quantité
   - Créer la commande

4. Si Visiteur :
   - Saisir nom et téléphone
   - Choisir la formule
   - Créer la commande
```

### C. Commande pour Douaniers

#### 1. Accès
```
Commandes → Commande Douaniers
```

#### 2. Processus
```
1. Sélectionner la date (jour même uniquement)
2. Choisir la formule disponible
3. Spécifier la période (Jour/Nuit)
4. Spécifier le site
5. Entrer la quantité (1-100)
6. Vérification automatique des quotas :
   - Quota total par période
   - Plats déjà consommés aujourd'hui
   - Quota restant disponible
7. Si quota suffisant → Création de la commande
8. Si quota insuffisant → Message d'erreur
```

---

## ✅ Workflow de validation (Points de consommation)

### 1. Validation manuelle
```
1. Accès : Points de consommation → Créer un point
2. Sélectionner l'utilisateur
3. Sélectionner la commande
4. Renseigner :
   - Type de formule
   - Nom du plat
   - Lieu de consommation
   - Date et heure
5. Sauvegarder
```

### 2. Validation automatique (Fermeture automatique)
```
1. Vendredi 12h00 → Service de fermeture automatique s'exécute
2. Pour chaque commande précommandée de la semaine N+1 :
   a. Statut → "Consommée"
   b. Création automatique d'un point de consommation
   c. Lieu : "Restaurant CIT"
   d. Date : Date de consommation de la commande
3. Notification envoyée
```

### 3. Consultation des points
```
Points de consommation → Mes points de consommation
- Filtrage par période
- Résumé par utilisateur
- Export possible
```

---

## 💰 Workflow de facturation

### A. Facturation automatique

#### 1. Déclenchement
```
- Service s'exécute toutes les heures
- Vérifie les commandes non consommées de la veille ou plus anciennes
- Facture uniquement si activé dans les paramètres
```

#### 2. Processus
```
1. Vérification de l'activation
   - Paramètre : FACTURATION_NON_CONSOMMEES_ACTIVE

2. Recherche des commandes facturables :
   - Date de consommation < aujourd'hui
   - Statut = "Précommandée"
   - Pas de point de consommation associé
   - Montant > 0

3. Calcul de la facturation :
   - Montant total à facturer
   - Nombre de commandes facturables
   - Nombre de commandes non facturables

4. Application :
   - Mise à jour du statut
   - Enregistrement de la facturation
   - Notification

5. Enregistrement :
   - Marque la facturation comme effectuée pour la journée
```

### B. Facturation manuelle

#### 1. Accès
```
Facturation → Facturation manuelle
```

#### 2. Processus
```
1. Sélectionner la période
2. Voir les commandes facturables
3. Prévisualiser le montant
4. Appliquer la facturation
5. Confirmation
```

---

## 🔒 Workflow de fermeture automatique

### 1. Configuration
```
Paramètres → Configuration des commandes
- Jour de clôture : Vendredi (par défaut)
- Heure de clôture : 12:00 (par défaut)
- Auto-confirmation : Activée (par défaut)
```

### 2. Processus automatique
```
1. Service vérifie toutes les 5 minutes
2. Si vendredi 12h00 :
   a. Calcul de la semaine N+1 (lundi à vendredi)
   b. Pour chaque commande précommandée :
      - Statut → "Consommée"
      - Création point de consommation
   c. Enregistrement de la fermeture
   d. Notification
3. Blocage des nouvelles commandes jusqu'au lundi
```

### 3. Test de blocage
```
Admin/RH peut tester le blocage manuellement :
1. Aller dans Configuration des commandes
2. Cliquer sur "Test de blocage"
3. Vérifier l'affichage du message
4. Vérifier la prochaine date d'ouverture
```

---

## 📊 Workflow de reporting

### 1. Dashboard (Admin/RH)
```
Reporting → Dashboard
- Statistiques globales
- Filtres : dates, sites, départements
- Graphiques de consommation
- Export Excel
```

### 2. Reporting automatique
```
Service s'exécute quotidiennement :
1. Génération de rapports
2. Envoi par email (si configuré)
3. Archivage
```

### 3. Extraction de données
```
Extraction → Exporter
- Sélection de la période
- Filtres par site, département
- Export Excel
- Export CSV
```

---

## 👥 Workflow de gestion des utilisateurs (Admin uniquement)

### 1. Création d'utilisateur
```
1. Paramètres → Utilisateurs → Créer
2. Renseigner :
   - Nom et prénoms (obligatoires)
   - Matricule (UserName) - obligatoire et unique
   - Email
   - Téléphone
   - Département (obligatoire)
   - Fonction (obligatoire)
   - Site
   - Rôle
   - Mot de passe (généré ou personnalisé)
3. Option "Forcer réinitialisation" si nécessaire
4. Sauvegarder
```

### 2. Modification
```
1. Sélectionner l'utilisateur
2. Modifier les informations
3. Possibilité de réinitialiser le mot de passe
4. Sauvegarder
```

### 3. Suppression
```
- Soft delete uniquement (Supprimer = 1)
- Les commandes associées sont conservées
- L'utilisateur ne peut plus se connecter
```

---

## 🏢 Workflow de gestion des groupes Non-CIT

### 1. Création de groupe
```
1. Paramètres → Groupes Non-CIT → Créer
2. Renseigner :
   - Nom du groupe
   - Description
   - Type de groupe (Douaniers, etc.)
3. Sauvegarder
```

### 2. Gestion des quotas
```
1. Sélectionner le groupe
2. Définir les quotas :
   - Quota jour (nombre de plats)
   - Quota nuit (nombre de plats)
   - Date d'application
3. Possibilité de quotas permanents
4. Sauvegarder
```

### 3. Consultation
```
- Voir les quotas actuels
- Voir les plats consommés
- Voir les plats restants
- Historique des commandes
```

---

## 🍽️ Workflow Prestataire Cantine

### 1. Vue du jour
```
1. Connexion en tant que PrestataireCantine
2. Accueil affiche :
   - Menus du jour
   - Statistiques par formule
   - Commandes par période (Jour/Nuit)
   - Marges configurées
```

### 2. Gestion des marges
```
1. Prestataire Cantine → Ajouter marges
2. Sélectionner la période
3. Pour chaque formule :
   - Définir la marge
4. Sauvegarder
5. Si export déjà effectué → Modification restreinte
```

### 3. Export des commandes
```
1. Prestataire Cantine → Exporter commandes
2. Sélectionner la période
3. Vérifier les commandes
4. Générer l'export Excel
5. Télécharger le fichier
6. L'export est enregistré (évite les doublons)
```

---

## 🔄 Workflow de cycle de vie d'une commande

### États d'une commande
```
1. Précommandée (0)
   - Commande créée
   - En attente de consommation
   - Peut être annulée (24h avant)

2. Consommée (1)
   - Repas récupéré
   - Point de consommation créé
   - Ne peut plus être modifiée

3. Annulée (2)
   - Commande annulée
   - Motif enregistré
   - Peut être remplacée par une nouvelle commande
```

### Transitions
```
Précommandée → Consommée :
  - Validation manuelle
  - Fermeture automatique (vendredi 12h)

Précommandée → Annulée :
  - Annulation manuelle (24h avant)
  - Annulation admin (sans restriction)

Consommée → (aucune transition possible)
Annulée → (aucune transition possible)
```

---

## ⚙️ Services en arrière-plan

### 1. FermetureAutomatiqueService
```
- Fréquence : Vérification toutes les 5 minutes
- Action : Fermeture des commandes vendredi 12h
- Logs : Détails de chaque opération
```

### 2. FacturationAutomatiqueService
```
- Fréquence : Vérification toutes les heures
- Action : Facturation des commandes non consommées
- Condition : Activée dans les paramètres
```

### 3. ReportingAutomatiqueService
```
- Fréquence : Quotidienne
- Action : Génération de rapports
- Notification : Email (si configuré)
```

---

## 📱 Notifications en temps réel (SignalR)

### Types de notifications
```
1. Nouvelle commande créée
2. Commande annulée
3. Fermeture automatique effectuée
4. Facturation appliquée
5. Nouvelle formule ajoutée
```

### Implémentation
```
- Hub : /hubs/notifications
- Connexion automatique au chargement
- Messages en temps réel
- Badge de notification
```

---

## 🔍 Workflow de diagnostic

### 1. Diagnostic des commandes
```
Diagnostic → Commandes
- Vérification des incohérences
- Commandes orphelines
- Statuts invalides
- Corrections automatiques
```

### 2. Diagnostic des utilisateurs
```
Diagnostic → Utilisateurs
- Utilisateurs sans département
- Utilisateurs sans fonction
- Doublons
- Corrections
```

### 3. Diagnostic de configuration
```
Diagnostic → Configuration
- Paramètres manquants
- Valeurs invalides
- Initialisation
```

### 4. Diagnostic de facturation
```
Diagnostic → Facturation
- Commandes facturables
- Incohérences
- Corrections
```

---

## 🗄️ Workflow de base de données

### 1. Migrations
```
1. Création : dotnet ef migrations add NomMigration
2. Application : Automatique au démarrage (Program.cs)
3. Vérification : Logs de connexion
```

### 2. Seeding
```
1. Département par défaut : "Direction Général"
2. Fonction par défaut : "Fonction Général"
3. Utilisateur admin :
   - UserName: admin
   - Password: admin123
   - ⚠️ À changer immédiatement
4. Configurations par défaut
```

### 3. Nettoyage
```
Admin → Nettoyer base de données
- Suppression des données de test
- Conservation des comptes admin
- Conservation des données de référence
```

---

## 📤 Workflow d'export

### 1. Export Excel (Commandes)
```
1. Commandes → Exporter Excel
2. Sélectionner la période
3. Filtres optionnels
4. Génération du fichier
5. Téléchargement
```

### 2. Export Prestataire
```
1. Prestataire Cantine → Exporter
2. Sélectionner la période
3. Vérification des marges
4. Génération avec marges
5. Téléchargement
6. Enregistrement de l'export (évite doublons)
```

### 3. Export Reporting
```
1. Reporting → Exporter
2. Sélectionner le type de rapport
3. Filtres
4. Génération
5. Téléchargement
```

---

## 🌍 Adaptation géographique

### Saisons (Côte d'Ivoire)
```
- Grande saison sèche (Décembre-Mars) : ☀️ Orange
- Grande saison des pluies (Avril-Juillet) : 🌧️ Bleu
- Petite saison sèche (Août-Septembre) : ☀️ Or
- Petite saison des pluies (Octobre-Novembre) : 🌦️ Vert
```

### Localisation
```
- Culture : fr-FR
- Fuseau horaire : UTC (à configurer selon besoin)
- Format de dates : dd/MM/yyyy
```

---

## 🔐 Sécurité

### 1. Authentification
```
- BCrypt pour hachage des mots de passe (work factor: 12)
- Cookies sécurisés (HttpOnly, SameSite)
- Expiration des sessions
- Protection CSRF (AntiForgeryToken)
```

### 2. Autorisation
```
- Vérification des rôles sur chaque action
- [Authorize(Roles = "Admin")] sur les contrôleurs
- Vérification dans les vues
- Redirection si non autorisé
```

### 3. Validation
```
- Validation côté serveur (ModelState)
- Validation côté client (JavaScript)
- Sanitization des entrées
- Protection SQL Injection (EF Core)
```

---

## 🚀 Déploiement

### 1. Prérequis
```
- .NET 8.0 Runtime
- SQL Server (local ou distant)
- IIS ou serveur web compatible
```

### 2. Configuration
```
1. appsettings.json :
   - ConnectionString
   - Configuration des services
   - Paramètres d'email (si nécessaire)

2. Variables d'environnement :
   - OBELI_ENABLE_SEEDING (true/false)
```

### 3. Démarrage
```
1. Application des migrations automatiques
2. Seeding conditionnel
3. Démarrage des services en arrière-plan
4. Initialisation SignalR
```

---

## 📝 Logs et monitoring

### 1. Logging
```
- Niveaux : Information, Warning, Error
- Logs détaillés pour chaque opération
- Traçabilité des actions utilisateurs
- Logs des services automatiques
```

### 2. Monitoring
```
- Vérification de la connexion DB au démarrage
- Logs des erreurs avec stack traces
- Notifications en cas d'échec
```

---

## 🎯 Points d'attention

### 1. Délais de commande
```
- 48h avant consommation : Dernier délai pour commander
- 24h avant consommation : Dernier délai pour annuler (sauf admin)
- Vendredi 12h : Fermeture automatique
```

### 2. Quotas
```
- Vérification automatique pour groupes Non-CIT
- Quotas par période (Jour/Nuit)
- Calcul en temps réel
- Messages d'erreur clairs
```

### 3. Facturation
```
- Seulement les commandes non consommées
- Vérification de l'absence de point de consommation
- Montant > 0
- Activation manuelle requise
```

### 4. Exports
```
- Prévention des doublons
- Enregistrement des exports
- Restrictions de modification après export
```

---

## 📚 Documentation complémentaire

- `WORKFLOWS_TEST_ROLES.md` : Tests par rôle
- `GUIDE_FERMETURE_AUTOMATIQUE.md` : Guide de fermeture
- `GUIDE_QUOTAS_DOUANIERS.md` : Guide des quotas
- `CHAMPS_OBLIGATOIRES_UTILISATEUR.md` : Champs obligatoires
- `SEEDING.md` : Documentation du seeding
- `NETTOYAGE_BASE_DONNEES.md` : Guide de nettoyage

---

## 🔄 Cycle de vie complet d'une commande (Exemple)

```
Jour 1 (Lundi) - 10h00
├─ Employé consulte les menus de la semaine N+1
├─ Sélectionne une formule pour jeudi
├─ Crée une commande → Statut: "Précommandée"
└─ Notification envoyée

Jour 2 (Mardi) - 14h00
├─ Employé peut encore modifier/annuler (délai 24h non atteint)
└─ Commande visible dans "Mes Commandes"

Jour 3 (Mercredi) - 12h00
├─ Délai de 24h avant consommation atteint
├─ Annulation encore possible (jusqu'à 24h avant)
└─ Modification possible

Jour 4 (Jeudi) - 08h00
├─ Délai de 24h avant consommation dépassé
├─ Annulation impossible (sauf admin)
└─ Commande verrouillée

Jour 4 (Jeudi) - 12h00
├─ Employé récupère son repas
├─ Point de consommation créé manuellement
├─ Statut → "Consommée"
└─ Commande finalisée

OU (si non récupérée)

Jour 5 (Vendredi) - 12h00
├─ Fermeture automatique
├─ Toutes les commandes précommandées → "Consommée"
├─ Points de consommation créés automatiquement
└─ Blocage des nouvelles commandes

Jour 6 (Samedi) - 00h00
├─ Si commande non récupérée jeudi
├─ Facturation automatique (si activée)
├─ Statut reste "Précommandée" mais facturée
└─ Montant facturé à l'employé
```

---

## ✅ Checklist de démarrage

### Pour un nouvel administrateur
```
[ ] Se connecter avec admin/admin123
[ ] Changer le mot de passe admin
[ ] Créer les départements nécessaires
[ ] Créer les fonctions nécessaires
[ ] Créer les utilisateurs (RH, Employés)
[ ] Configurer les paramètres de commande
[ ] Créer les types de formules
[ ] Créer les groupes Non-CIT (si nécessaire)
[ ] Configurer les quotas
[ ] Tester la création d'une formule
[ ] Tester la création d'une commande
[ ] Vérifier les services automatiques
```

### Pour un nouvel employé
```
[ ] Se connecter avec ses identifiants
[ ] Vérifier son profil
[ ] Consulter les menus de la semaine
[ ] Créer une commande test
[ ] Vérifier dans "Mes Commandes"
[ ] Tester l'annulation (si dans les délais)
```

---

**Document créé le** : 2025-01-XX  
**Version** : 1.0  
**Application** : O'Beli K - Système de gestion de commandes de repas

