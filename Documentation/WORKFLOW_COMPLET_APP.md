# 🔄 WORKFLOW COMPLET DE L'APPLICATION O'BELI K

**Documentation complète des processus métier**  
**Version** : 1.0  
**Date** : 2025-01-XX

---

## 📋 Table des matières

1. [Structure du document](#structure-du-document)
2. [Workflow de commande hebdomadaire (Employés)](#1-workflow-de-commande-hebdomadaire-employés)
3. [Workflow de commande instantanée](#2-workflow-de-commande-instantanée)
4. [Workflow de commande Douaniers](#3-workflow-de-commande-douaniers)
5. [Workflow de modification/annulation de commande](#4-workflow-de-modificationannulation-de-commande)
6. [Workflow de fermeture automatique](#5-workflow-de-fermeture-automatique)
7. [Workflow de validation/consommation](#6-workflow-de-validationconsommation)
8. [Workflow de facturation automatique](#7-workflow-de-facturation-automatique)
9. [Workflow de gestion des menus](#8-workflow-de-gestion-des-menus)
10. [Workflow de gestion des quotas](#9-workflow-de-gestion-des-quotas)
11. [Workflow de reporting et statistiques](#10-workflow-de-reporting-et-statistiques)
12. [Calendrier récapitulatif hebdomadaire](#calendrier-récapitulatif-hebdomadaire)

---

## Structure du document

Chaque workflow est structuré avec les informations suivantes :

- **QUI** : Rôles autorisés à effectuer l'action
- **QUOI** : Action à réaliser
- **COMMENT** : Procédure détaillée étape par étape
- **QUAND** : Moments, délais, horaires précis
- **RÈGLES** : Contraintes et validations
- **RÉSULTAT** : État final après l'action

---

## 1. WORKFLOW DE COMMANDE HEBDOMADAIRE (EMPLOYÉS)

### 📌 QUI
- **Rôles autorisés** : `Employe`, `Administrateur`, `RH`
- **Principal acteur** : Employé CIT

### 🎯 QUOI
Créer une ou plusieurs commandes pour la semaine suivante (semaine N+1), un repas par jour ouvrable (lundi à vendredi).

### ⏰ QUAND

#### **Période de commande ouverte**
- **Début** : Lundi 00:00 de la semaine courante (semaine N)
- **Fin** : Vendredi 11:59:59 (avant 12:00) de la semaine courante
- **Durée** : 5 jours ouvrés (lundi à vendredi)

#### **Période de commande bloquée**
- **Vendredi 12:00** → Commandes bloquées automatiquement
- **Samedi** → Commandes bloquées
- **Dimanche** → Commandes bloquées
- **Réouverture** : Lundi 00:00 de la semaine suivante

#### **Délai de précommande**
- **Recommandation** : 48 heures avant 12:00 de la date de consommation
- **Exemple** : Pour consommer le lundi 15/01 à midi, commander avant le samedi 13/01 à 12:00
- **Note** : Le système n'empêche plus la création après ce délai (affichage informatif uniquement)

### 📝 COMMENT

#### Étape 1 : Accès à l'interface
```
1. Se connecter à l'application
2. Cliquer sur "Commandes" dans le menu principal
3. Cliquer sur "Commander"
```

#### Étape 2 : Vérification du blocage
```
Le système vérifie automatiquement :
- Jour actuel : vendredi après 12h ? → ❌ Bloqué
- Jour actuel : samedi ou dimanche ? → ❌ Bloqué
- Jour actuel : lundi à vendredi avant 12h ? → ✅ Autorisé
```

#### Étape 3 : Sélection de la semaine
```
1. Affichage par défaut : Semaine N+1 (semaine suivante)
2. Navigation possible entre semaines (flèches précédent/suivant)
3. Affichage des jours : Lundi à Vendredi uniquement
```

#### Étape 4 : Consultation des menus
```
Pour chaque jour de la semaine N+1 :
- Affichage des formules disponibles :
  * Formule Améliorée
  * Formule Standard 1
  * Formule Standard 2
- Détails de chaque formule :
  * Entrée
  * Plat principal
  * Dessert
  * Garniture
  * Boisson
```

#### Étape 5 : Création de commande
```
Pour chaque jour souhaité :

1. Cliquer sur "Commander" pour le jour souhaité
2. Sélectionner une formule parmi celles disponibles
3. Choisir la période :
   - Jour (Midi) - Déjeuner
   - Nuit (Soir) - Dîner
4. Choisir le site :
   - CIT Billing
   - CIT Terminal
5. Cliquer sur "Valider"

Répéter pour chaque jour de la semaine souhaité.
```

#### Étape 6 : Validation système
```
Le système effectue automatiquement :
1. Vérification de l'existence de l'utilisateur
2. Vérification des quotas (si groupe spécial)
3. Génération d'un code de commande unique
4. Création de la commande avec statut "Précommandée"
5. Notification en temps réel (SignalR)
```

### ✅ RÈGLES ET CONTRAINTES

#### Règle 1 : Limite par jour
- **Un seul repas par jour** : Un employé ne peut commander qu'**une seule commande par jour**
- Si une commande existe déjà pour un jour → Modification nécessaire

#### Règle 2 : Formule obligatoire
- Une formule doit être sélectionnée
- La formule doit exister pour la date de consommation

#### Règle 3 : Période obligatoire
- Jour ou Nuit doit être spécifié

#### Règle 4 : Site obligatoire
- CIT Billing ou CIT Terminal doit être spécifié

### 📊 RÉSULTAT

- **Statut initial** : `Precommander` (Précommandée)
- **Code de commande** : Généré automatiquement (format unique)
- **Date de consommation** : Date sélectionnée dans la semaine N+1
- **Notification** : Affichée en temps réel
- **Visibilité** : Commande visible dans "Mes Commandes"

---

## 2. WORKFLOW DE COMMANDE INSTANTANÉE

### 📌 QUI
- **Rôles autorisés** : `Administrateur`, `PrestataireCantine` uniquement
- **Rôles non autorisés** : `RH`, `Employe`
- **Principal acteur** : Prestataire de cantine

### 🎯 QUOI
Créer une commande pour un employé CIT **pour le jour même** (aujourd'hui), sans délai de précommande.

### ⏰ QUAND

#### **Période autorisée**
- **Jour** : Du lundi au vendredi
- **Heure** : Toute la journée (sauf périodes de blocage)
- **Date de consommation** : Uniquement aujourd'hui (jour J)

#### **Période bloquée**
- **Vendredi 12:00** → Impossible de créer une commande instantanée
- **Samedi** → Impossible
- **Dimanche** → Impossible

### 📝 COMMENT

#### Étape 1 : Accès à l'interface
```
1. Se connecter avec un compte Administrateur ou PrestataireCantine
2. Cliquer sur "Commandes" dans le menu principal
3. Cliquer sur "Commande instantanée"
```

#### Étape 2 : Vérification du blocage
```
Le système vérifie automatiquement :
- Jour actuel : vendredi après 12h ? → ❌ Bloqué
- Jour actuel : samedi ou dimanche ? → ❌ Bloqué
- Jour actuel : lundi à vendredi ? → ✅ Autorisé
```

#### Étape 3 : Saisie du matricule
```
1. Dans le champ "Rechercher un employé", saisir le matricule
   Exemple : "JD001"
2. Le système recherche automatiquement
3. Affichage des résultats :
   - Nom complet
   - Matricule
   - Département
   - Statut (actif/inactif)
```

#### Étape 4 : Sélection de l'employé
```
1. Cliquer sur l'employé dans les résultats de recherche
2. Vérification automatique :
   - Employé actif ? → ✅ Continu
   - Employé inactif ? → ❌ Erreur
```

#### Étape 5 : Choix de la formule
```
1. Affichage des formules disponibles pour AUJOURD'HUI uniquement
2. Sélectionner une formule :
   - Formule Améliorée
   - Formule Standard 1
   - Formule Standard 2
3. Visualiser les détails de la formule
```

#### Étape 6 : Sélection de la période
```
1. Choisir la période :
   - Midi (Jour) - pour le déjeuner
   - Soir (Nuit) - pour le dîner
```

#### Étape 7 : Sélection du site
```
1. Choisir le site :
   - CIT Billing
   - CIT Terminal
```

#### Étape 8 : Création
```
1. Cliquer sur "Créer la commande"
2. Le système vérifie :
   - Employé existe et est actif
   - Pas de commande existante en statut "Précommandée" ou "Consommée" pour cette période
   - Formule existe pour aujourd'hui
3. Génération du code de commande
4. Statut : "Précommandée"
5. Marqueur : Instantanee = true
```

### ✅ RÈGLES ET CONTRAINTES

#### Règle 1 : Limite par période
- **Une seule commande instantanée par période par jour** pour un employé
- Si commande "Précommandée" existe → ❌ Bloqué
- Si commande "Consommée" existe → ❌ Bloqué
- Si commande "Annulée" existe → ✅ Autorisé (remplacement possible)

#### Règle 2 : Date fixe
- Date de consommation : Toujours aujourd'hui (jour J)
- Impossible de commander pour demain ou plus tard

#### Règle 3 : Quantité fixe
- Quantité : Toujours 1 (un seul plat par commande)

#### Règle 4 : Type de client
- Uniquement pour les employés CIT (CitUtilisateur)
- Pas pour les groupes non-CIT ou visiteurs

### 📊 RÉSULTAT

- **Statut initial** : `Precommander` (Précommandée)
- **Date de consommation** : Aujourd'hui (jour J)
- **Marqueur** : `Instantanee = true`
- **Code de commande** : Généré automatiquement
- **Visibilité** : Commande visible dans "Mes Commandes" pour l'employé concerné

---

## 3. WORKFLOW DE COMMANDE DOUANIERS

### 📌 QUI
- **Rôles autorisés** : `Administrateur`, `PrestataireCantine` uniquement
- **Principal acteur** : Prestataire de cantine

### 🎯 QUOI
Créer une commande pour le groupe "Douaniers" avec vérification des quotas disponibles (jour ou nuit).

### ⏰ QUAND

#### **Période autorisée**
- **Jour** : Du lundi au vendredi
- **Heure** : Toute la journée (sauf périodes de blocage)
- **Date de consommation** : Uniquement aujourd'hui (jour J)

#### **Période bloquée**
- **Vendredi 12:00** → Impossible
- **Samedi** → Impossible
- **Dimanche** → Impossible

### 📝 COMMENT

#### Étape 1 : Accès à l'interface
```
1. Se connecter avec un compte Administrateur ou PrestataireCantine
2. Cliquer sur "Commandes" dans le menu principal
3. Cliquer sur "Commande Douaniers"
```

#### Étape 2 : Vérification du blocage
```
Le système vérifie automatiquement :
- Jour actuel : vendredi après 12h ? → ❌ Bloqué
- Jour actuel : samedi ou dimanche ? → ❌ Bloqué
- Jour actuel : lundi à vendredi ? → ✅ Autorisé
```

#### Étape 3 : Sélection de la date
```
1. Date de consommation : Fixée automatiquement à aujourd'hui (jour J)
2. Pas de modification possible
```

#### Étape 4 : Choix de la formule
```
1. Affichage des formules disponibles pour AUJOURD'HUI uniquement
2. Sélectionner une formule parmi celles proposées
```

#### Étape 5 : Sélection de la période
```
1. Choisir la période :
   - Jour (Midi) - pour le déjeuner
   - Nuit (Soir) - pour le dîner
```

#### Étape 6 : Sélection du site
```
1. Choisir le site :
   - CIT Billing
   - CIT Terminal
```

#### Étape 7 : Saisie de la quantité
```
1. Entrer la quantité souhaitée (entre 1 et 100)
2. Le système vérifie les quotas disponibles :
   - Quota total pour la période (Jour ou Nuit)
   - Plats déjà consommés aujourd'hui pour cette période
   - Quota restant disponible
3. Si quantité demandée > quota restant → ❌ Erreur
4. Si quantité demandée ≤ quota restant → ✅ Autorisé
```

#### Étape 8 : Création
```
1. Cliquer sur "Créer la commande"
2. Le système effectue :
   - Vérification finale des quotas
   - Création de la commande
   - Génération du code de commande
   - Statut : "Précommandée"
```

### ✅ RÈGLES ET CONTRAINTES

#### Règle 1 : Quotas obligatoires
- Un quota doit être défini pour le groupe "Douaniers" dans `/GroupeNonCit`
- Quota par période (Jour et Nuit séparés)
- Vérification en temps réel avant création

#### Règle 2 : Limite de quantité
- Minimum : 1 plat
- Maximum : 100 plats par commande
- Ou jusqu'à épuisement du quota disponible

#### Règle 3 : Date fixe
- Date de consommation : Toujours aujourd'hui
- Impossible de commander pour une autre date

#### Règle 4 : Groupe unique
- Uniquement pour le groupe "Douaniers"
- Pas pour d'autres groupes non-CIT

### 📊 RÉSULTAT

- **Statut initial** : `Precommander` (Précommandée)
- **Date de consommation** : Aujourd'hui (jour J)
- **Quantité** : Valeur saisie (entre 1 et 100)
- **Groupe** : Douaniers
- **Quota utilisé** : Mis à jour automatiquement

---

## 4. WORKFLOW DE MODIFICATION/ANNULATION DE COMMANDE

### 📌 QUI

#### **Modification**
- **Rôles autorisés** : `Employe` (pour ses propres commandes), `Administrateur`, `RH`
- **Administrateurs** : Peuvent modifier n'importe quand (sauf commandes consommées)

#### **Annulation**
- **Rôles autorisés** : `Employe` (pour ses propres commandes), `Administrateur`, `RH`, `PrestataireCantine`
- **Employés** : Sous contraintes de délai strictes

### 🎯 QUOI

Modifier ou annuler une commande existante (changer la formule, la période, le site, ou annuler complètement).

### ⏰ QUAND

#### **Modification - Règles de délai**

##### Pour les Employés et RH :
1. **Commandes de la semaine N+1** :
   - **Modifiable jusqu'à** : Dimanche 12:00 de la semaine N+1
   - **Exemple** : Commande pour lundi 15/01 → Modifiable jusqu'à dimanche 14/01 à 12:00

2. **Commandes de la semaine courante** :
   - **Modifiable jusqu'à** : 24 heures avant la date de consommation
   - **Exemple** : Commande pour mercredi 10/01 à midi → Modifiable jusqu'à mardi 09/01 à midi

3. **Commandes consommées** :
   - **JAMAIS modifiables** (même pour Administrateur)

##### Pour les Administrateurs :
- **Pas de restriction de délai** (sauf commandes consommées)
- Peuvent modifier n'importe quand, n'importe quelle commande

#### **Annulation - Règles de délai**

##### Pour les Employés :
1. **Commandes de la semaine courante** :
   - **Annulable jusqu'à** : 24 heures avant la date de consommation
   - **Exemple** : Commande pour mercredi 10/01 à midi → Annulable jusqu'à mardi 09/01 à midi
   - **Délai dépassé** → ❌ Message : "Délai de 24h dépassé. Il ne reste que Xh Ymin avant la consommation"

2. **Commandes de la semaine N+1** :
   - Généralement annulables (sous réserve des règles de modification)

##### Pour les Administrateurs, RH, Prestataires :
- **Pas de restriction de délai** (sauf commandes consommées)

### 📝 COMMENT

#### **Modification**

##### Étape 1 : Accès
```
1. Se connecter à l'application
2. Aller dans "Commandes" → "Mes Commandes" (pour employés)
   ou "Commandes" → "Liste des Commandes" (pour Admin/RH)
3. Trouver la commande à modifier
4. Cliquer sur "Modifier"
```

##### Étape 2 : Vérification des droits
```
Le système vérifie automatiquement :
1. Rôle de l'utilisateur :
   - Administrateur ? → ✅ Autorisé (sauf commandes consommées)
   - Employé ? → Vérifier si c'est sa propre commande
2. Statut de la commande :
   - Consommée ? → ❌ Impossible de modifier
   - Précommandée ou Annulée ? → Vérifier délais
3. Délais :
   - Semaine N+1 ? → Vérifier si avant dimanche 12:00
   - Semaine courante ? → Vérifier si avant 24h de la consommation
```

##### Étape 3 : Modification
```
1. Changer les informations souhaitées :
   - Formule
   - Période (Jour/Nuit)
   - Site (CIT Billing / CIT Terminal)
2. Cliquer sur "Enregistrer"
3. Le système valide :
   - Nouvelle formule existe pour la date
   - Aucun conflit
4. Mise à jour de la commande
5. Notification en temps réel
```

#### **Annulation**

##### Étape 1 : Accès
```
1. Se connecter à l'application
2. Aller dans "Commandes" → "Mes Commandes" (pour employés)
   ou "Commandes" → "Liste des Commandes" (pour Admin/RH/Prestataire)
3. Trouver la commande à annuler
4. Cliquer sur "Annuler"
```

##### Étape 2 : Vérification des droits et délais
```
Le système vérifie automatiquement :
1. Rôle de l'utilisateur :
   - Administrateur/RH/Prestataire ? → ✅ Autorisé (sauf consommées)
   - Employé ? → Vérifier délai de 24h
2. Statut de la commande :
   - Consommée ? → ❌ Impossible d'annuler
   - Précommandée ? → Vérifier délais
3. Délai pour employés :
   - Semaine courante ? → Vérifier si ≥ 24h avant consommation
   - Délai dépassé ? → ❌ Erreur avec message détaillé
```

##### Étape 3 : Confirmation
```
1. Afficher un message de confirmation
2. Pour les employés : Afficher le temps restant avant consommation
3. Cliquer sur "Confirmer l'annulation"
4. Le système :
   - Change le statut à "Annulée"
   - Enregistre la modification
   - Envoie notification aux prestataires (SignalR)
```

### ✅ RÈGLES ET CONTRAINTES

#### Règle 1 : Commandes consommées
- **JAMAIS modifiables ni annulables** (même pour Administrateur)
- Statut final, aucune modification possible

#### Règle 2 : Délai de 24h pour employés
- Pour annulation/modification de commandes semaine courante
- Calcul : Date de consommation - 24 heures
- Délai dépassé → Message d'erreur avec temps restant

#### Règle 3 : Semaine N+1
- Modifiable jusqu'au dimanche 12:00 de la semaine N+1
- Après cette limite → Plus modifiable par les employés

#### Règle 4 : Administrateurs
- Exception : Pas de restriction de délai
- Peuvent modifier/annuler n'importe quand (sauf consommées)

### 📊 RÉSULTAT

#### **Modification**
- **Statut** : Reste "Précommandée" (si elle l'était)
- **Informations** : Mises à jour (formule, période, site)
- **Historique** : Modification enregistrée (ModifiedOn, ModifiedBy)
- **Notification** : En temps réel

#### **Annulation**
- **Statut** : `Annulee` (Annulée)
- **Historique** : Annulation enregistrée
- **Notification** : Envoyée aux prestataires
- **Pour employés** : Possibilité de créer une nouvelle commande

---

## 5. WORKFLOW DE FERMETURE AUTOMATIQUE

### 📌 QUI
- **Acteur** : Système automatique (Service en arrière-plan)
- **Rôles impliqués** : Aucun (automatique)
- **Configuration** : `Administrateur` peut modifier les paramètres

### 🎯 QUOI
Fermer automatiquement les commandes de la semaine N+1 le vendredi à 12:00, les marquer comme confirmées et bloquer la création de nouvelles commandes.

### ⏰ QUAND

#### **Moment d'exécution**
- **Jour** : Vendredi
- **Heure** : 12:00 (midi)
- **Fréquence de vérification** : Toutes les 5 minutes
- **Action unique** : Une seule fois par jour (même si le service vérifie toutes les 5 minutes)

#### **Exemple concret**
```
Semaine courante : Semaine N (du lundi 08/01 au vendredi 12/01)
Semaine suivante : Semaine N+1 (du lundi 15/01 au vendredi 19/01)

Vendredi 12/01 à 12:00 → Fermeture automatique
- Toutes les commandes pour la semaine 15/01-19/01 sont confirmées
- Les commandes restent en statut "Précommandée" (seront marquées "Consommée" au scan)
- Blocage des nouvelles commandes jusqu'au lundi suivant
```

### 📝 COMMENT

#### **Processus automatique (aucune intervention humaine)**

##### Étape 1 : Vérification du moment
```
Le service FermetureAutomatiqueService s'exécute toutes les 5 minutes :
1. Vérifie si c'est vendredi
2. Vérifie si l'heure est 12:00
3. Si oui → Continue
4. Si non → Attend 5 minutes et revérifie
```

##### Étape 2 : Vérification d'exécution précédente
```
1. Vérifie si la fermeture a déjà été effectuée aujourd'hui
2. Clé de vérification : "FERMETURE_EFFECTUEE_YYYYMMDD"
3. Si déjà effectuée → Arrêt (pas de double exécution)
4. Si pas encore effectuée → Continue
```

##### Étape 3 : Calcul de la semaine N+1
```
1. Calcule le lundi de la semaine suivante (semaine N+1)
2. Calcule le vendredi de la semaine suivante
3. Détermine la plage de dates : lundi N+1 au vendredi N+1
```

##### Étape 4 : Traitement des commandes
```
Pour toutes les commandes de la semaine N+1 en statut "Précommandée" :
1. Les commandes RESTENT en statut "Précommandée"
   (Elles seront marquées "Consommée" au point de consommation)
2. Mise à jour de ModifiedOn et ModifiedBy
3. Les commandes sont maintenant "confirmées" (prêtes à être consommées)
```

##### Étape 5 : Enregistrement de l'exécution
```
1. Création d'un enregistrement : "FERMETURE_EFFECTUEE_YYYYMMDD"
2. Enregistrement de la date et heure
3. Log des statistiques :
   - Nombre de commandes confirmées
   - Dates de la semaine N+1
```

##### Étape 6 : Notification et logs
```
1. Logs détaillés dans l'application :
   - "Fermeture automatique terminée"
   - Nombre de commandes confirmées
   - Dates de la semaine N+1
2. Notification (si configurée)
```

#### **Configuration (par Administrateur)**

##### Étape 1 : Accès
```
1. Se connecter en tant qu'Administrateur
2. Aller dans "Paramètres" → "Configuration Commandes"
```

##### Étape 2 : Modification des paramètres
```
1. COMMANDE_JOUR_CLOTURE :
   - Valeur par défaut : "Friday" (Vendredi)
   - Options : Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday
   
2. COMMANDE_HEURE_CLOTURE :
   - Valeur par défaut : "12:00"
   - Format : HH:mm (24h)
   
3. COMMANDE_AUTO_CONFIRMATION :
   - Valeur par défaut : "true"
   - Options : true, false
```

### ✅ RÈGLES ET CONTRAINTES

#### Règle 1 : Exécution unique
- Une seule fermeture par jour maximum
- Vérification pour éviter les doubles exécutions

#### Règle 2 : Commandes concernées
- Uniquement les commandes de la semaine N+1
- Uniquement les commandes en statut "Précommandée"
- Les commandes déjà "Consommées" ou "Annulées" sont ignorées

#### Règle 3 : Statut après fermeture
- Les commandes RESTENT en statut "Précommandée"
- Elles seront marquées "Consommée" au point de consommation
- Si non scannées, elles restent "Précommandée" et seront facturées

#### Règle 4 : Blocage des nouvelles commandes
- Après fermeture, impossible de créer de nouvelles commandes
- Blocage jusqu'au lundi suivant à 00:00

### 📊 RÉSULTAT

- **Statut des commandes** : Restent "Précommandée" (seront "Consommée" au scan)
- **Blocage** : Nouvelles commandes bloquées jusqu'au lundi suivant
- **Enregistrement** : "FERMETURE_EFFECTUEE_YYYYMMDD" créé
- **Logs** : Statistiques enregistrées
- **Notification** : Envoyée (si configurée)

---

## 6. WORKFLOW DE VALIDATION/CONSOMMATION

### 📌 QUI

#### **Création manuelle de point de consommation**
- **Rôles autorisés** : `Administrateur`, `RH`
- **Principal acteur** : RH ou Admin

#### **Scan/Validation au point de consommation**
- **Rôles autorisés** : `PrestataireCantine`, `Administrateur`
- **Principal acteur** : Prestataire de cantine

### 🎯 QUOI

Valider qu'une commande a été consommée en créant un point de consommation, et changer le statut de la commande de "Précommandée" à "Consommée".

### ⏰ QUAND

#### **Création manuelle**
- **Quand** : À tout moment, après la consommation du repas
- **Utilité** : Corriger des erreurs, ajouter des consommations oubliées

#### **Scan/Validation**
- **Quand** : Au moment de la récupération du repas (jour de consommation)
- **Moment optimal** : À l'heure du repas (midi ou soir)

### 📝 COMMENT

#### **Création manuelle (Admin/RH)**

##### Étape 1 : Accès
```
1. Se connecter en tant qu'Administrateur ou RH
2. Aller dans "Points de consommation" → "Créer un point"
```

##### Étape 2 : Sélection de l'utilisateur
```
1. Rechercher ou sélectionner l'utilisateur :
   - Par nom
   - Par matricule
   - Dans la liste
2. Vérifier que l'utilisateur existe et est actif
```

##### Étape 3 : Sélection de la commande
```
1. Afficher les commandes de l'utilisateur :
   - Commandes en statut "Précommandée"
   - Commandes déjà "Consommées" (si correction)
2. Sélectionner la commande concernée
```

##### Étape 4 : Renseigner les informations
```
1. Type de formule : Automatiquement rempli depuis la commande
2. Nom du plat : Automatiquement rempli depuis la formule
3. Quantité : Automatiquement rempli depuis la commande
4. Lieu de consommation :
   - Restaurant CIT (par défaut)
   - Autre lieu (saisie manuelle)
5. Date de consommation : Date de la commande
6. Heure : Heure actuelle (modifiable)
```

##### Étape 5 : Sauvegarde
```
1. Cliquer sur "Créer"
2. Le système :
   - Crée le point de consommation
   - Change le statut de la commande à "Consommée" (si elle était "Précommandée")
   - Enregistre les informations
3. Confirmation affichée
```

#### **Scan/Validation (Prestataire)**

##### Étape 1 : Accès
```
1. Se connecter en tant que PrestataireCantine ou Administrateur
2. Aller dans "Points de consommation" → "Valider une consommation"
   (ou interface de scan si disponible)
```

##### Étape 2 : Scan ou recherche
```
Option A : Scan du code de commande
1. Scanner le code-barres ou QR code de la commande
2. Le système trouve automatiquement la commande

Option B : Recherche manuelle
1. Saisir le code de commande
2. Ou rechercher par nom d'utilisateur
3. Sélectionner la commande
```

##### Étape 3 : Vérification
```
Le système vérifie :
1. Commande existe et est en statut "Précommandée" ? → ✅
2. Date de consommation correspond à aujourd'hui ? → ✅
3. Point de consommation n'existe pas déjà ? → ✅
4. Si toutes les vérifications OK → Continue
5. Si problème → Afficher erreur
```

##### Étape 4 : Validation
```
1. Afficher les détails de la commande :
   - Utilisateur
   - Formule
   - Date
   - Période
2. Cliquer sur "Valider la consommation"
3. Le système :
   - Crée automatiquement le point de consommation
   - Change le statut de la commande à "Consommée"
   - Enregistre la date et heure de validation
```

### ✅ RÈGLES ET CONTRAINTES

#### Règle 1 : Un point par commande
- **Un seul point de consommation par commande**
- Si point existe déjà → Erreur ou affichage du point existant

#### Règle 2 : Changement de statut
- Lors de la création d'un point de consommation :
  - Si commande était "Précommandée" → Devient "Consommée"
  - Si commande était déjà "Consommée" → Point ajouté (cas de correction)

#### Règle 3 : Utilisateur obligatoire
- Un utilisateur doit être associé au point de consommation
- Les visiteurs et groupes non-CIT ne génèrent pas de points de consommation standard

#### Règle 4 : Date de consommation
- La date du point correspond à la date de consommation de la commande
- Modifiable lors de la création manuelle

### 📊 RÉSULTAT

- **Point de consommation** : Créé avec toutes les informations
- **Statut de la commande** : Change de "Précommandée" à "Consommée"
- **Traçabilité** : Date, heure, lieu enregistrés
- **Visibilité** : Point visible dans "Mes Points de Consommation" pour l'utilisateur

---

## 7. WORKFLOW DE FACTURATION AUTOMATIQUE

### 📌 QUI
- **Acteur** : Système automatique (Service en arrière-plan)
- **Configuration** : `Administrateur` peut activer/désactiver
- **Consultation** : `Administrateur`, `RH`

### 🎯 QUOI
Facturer automatiquement les commandes qui n'ont pas été consommées (pas de point de consommation créé) et dont la date de consommation est passée.

### ⏰ QUAND

#### **Moment d'exécution**
- **Fréquence** : Toutes les heures (vérification continue)
- **Déclenchement** : Automatique, 24/7
- **Condition** : Doit être activé dans les paramètres

#### **Commandes facturées**
- **Date de consommation** : Hier ou plus ancien
- **Statut** : "Précommandée" (n'a pas été consommée)
- **Point de consommation** : Aucun point de consommation associé
- **Montant** : > 0

#### **Exemple concret**
```
Aujourd'hui : Mercredi 10/01
Commande créée pour : Mardi 09/01 (hier)
Statut : Précommandée
Point de consommation : Aucun

Le service de facturation automatique :
1. Vérifie toutes les heures
2. Trouve cette commande (date consommation = hier)
3. Vérifie qu'elle est toujours "Précommandée"
4. Vérifie qu'aucun point de consommation n'existe
5. Crée un point de consommation avec lieu "FACTURATION - NON RÉCUPÉRÉE (Montant FCFA)"
6. Change le statut de la commande à "Facturée" (ou reste "Précommandée" selon implémentation)
```

### 📝 COMMENT

#### **Processus automatique (aucune intervention humaine)**

##### Étape 1 : Vérification de l'activation
```
Le service FacturationAutomatiqueService s'exécute toutes les heures :
1. Vérifie le paramètre "FACTURATION_NON_CONSOMMEES_ACTIVE"
2. Si "false" ou vide → Arrêt (facturation désactivée)
3. Si "true" → Continue
```

##### Étape 2 : Vérification d'exécution du jour
```
1. Vérifie si la facturation a déjà été effectuée aujourd'hui
2. Clé de vérification : "FACTURATION_EFFECTUEE_YYYYMMDD"
3. Si déjà effectuée → Arrêt (une seule facturation par jour)
4. Si pas encore effectuée → Continue
```

##### Étape 3 : Recherche des commandes facturables
```
1. Calcule la date limite : Aujourd'hui - 1 jour (hier ou plus ancien)
2. Recherche les commandes :
   - DateConsommation <= date limite (hier ou plus ancien)
   - Statut = "Précommandée"
   - Aucun point de consommation associé (Supprimer = 0)
   - Montant > 0
   - Supprimer = 0
```

##### Étape 4 : Calcul de la facturation
```
Pour chaque commande trouvée :
1. Calcul du montant à facturer :
   - Montant de la commande
   - Selon le type de formule
2. Comptage :
   - Nombre de commandes facturables
   - Nombre de commandes non facturables (montant = 0)
3. Calcul du montant total à facturer
```

##### Étape 5 : Application de la facturation
```
Pour chaque commande facturable :
1. Création d'un point de consommation :
   - TypeFormule : Récupéré de la commande
   - NomPlat : Récupéré de la formule
   - QuantiteConsommee : Quantité de la commande
   - LieuConsommation : "FACTURATION - NON RÉCUPÉRÉE (Montant FCFA)"
   - DateConsommation : Date de consommation de la commande
   - UtilisateurId : Utilisateur de la commande
   - CommandeId : ID de la commande
   - CreatedBy : "System_FacturationAutomatique"

2. Mise à jour du statut :
   - Le statut peut rester "Précommandée" ou passer à "Facturée"
   - (Selon l'implémentation exacte)
```

##### Étape 6 : Enregistrement de l'exécution
```
1. Création d'un enregistrement : "FACTURATION_EFFECTUEE_YYYYMMDD"
2. Enregistrement des statistiques :
   - Nombre de commandes facturables
   - Nombre de commandes non facturables
   - Montant total facturé
3. Date et heure enregistrées
```

##### Étape 7 : Logs et notification
```
1. Logs détaillés :
   - "Facturation automatique appliquée avec succès"
   - Nombre de commandes facturées
   - Montant total
2. Notification (si configurée)
```

#### **Configuration (par Administrateur)**

##### Étape 1 : Accès
```
1. Se connecter en tant qu'Administrateur
2. Aller dans "Paramètres" → "Configuration Commandes"
```

##### Étape 2 : Activation/Désactivation
```
1. Paramètre : "FACTURATION_NON_CONSOMMEES_ACTIVE"
2. Valeurs possibles :
   - "true" → Facturation automatique activée
   - "false" → Facturation automatique désactivée
3. Sauvegarder
```

#### **Facturation manuelle (par Admin/RH)**

##### Étape 1 : Accès
```
1. Se connecter en tant qu'Administrateur ou RH
2. Aller dans "Facturation" → "Facturation manuelle"
```

##### Étape 2 : Sélection de la période
```
1. Choisir la date de début
2. Choisir la date de fin
3. Cliquer sur "Rechercher"
```

##### Étape 3 : Consultation
```
1. Affichage des commandes facturables :
   - Date de consommation
   - Utilisateur
   - Formule
   - Montant
   - Statut
2. Prévisualisation du montant total
```

##### Étape 4 : Application
```
1. Vérifier la liste des commandes
2. Cliquer sur "Appliquer la facturation"
3. Confirmation demandée
4. Le système applique la facturation (même processus que l'automatique)
5. Confirmation affichée
```

### ✅ RÈGLES ET CONTRAINTES

#### Règle 1 : Activation obligatoire
- La facturation automatique doit être activée dans les paramètres
- Si désactivée, aucune facturation automatique n'est effectuée

#### Règle 2 : Exécution unique par jour
- Une seule facturation automatique par jour maximum
- Vérification pour éviter les doubles facturations

#### Règle 3 : Date de consommation passée
- Seules les commandes dont la date de consommation est passée (hier ou plus ancien) sont facturées
- Les commandes pour aujourd'hui ou le futur ne sont pas facturées

#### Règle 4 : Pas de point de consommation
- Seules les commandes sans point de consommation sont facturées
- Si un point existe déjà, la commande n'est pas facturée (déjà consommée)

#### Règle 5 : Montant > 0
- Seules les commandes avec un montant > 0 sont facturées
- Les commandes gratuites ne sont pas facturées

### 📊 RÉSULTAT

- **Points de consommation** : Créés avec lieu "FACTURATION - NON RÉCUPÉRÉE (Montant FCFA)"
- **Statut des commandes** : Marquées comme facturées (ou restent "Précommandée")
- **Enregistrement** : "FACTURATION_EFFECTUEE_YYYYMMDD" créé
- **Statistiques** : Nombre de commandes et montant total enregistrés
- **Visibilité** : Points de facturation visibles dans "Mes Points de Consommation" pour l'utilisateur

---

## 8. WORKFLOW DE GESTION DES MENUS

### 📌 QUI
- **Rôles autorisés** : `Administrateur`, `RH`
- **Principal acteur** : RH (gestion quotidienne), Administrateur (configuration)

### 🎯 QUOI
Créer, modifier et gérer les menus (formules) pour chaque jour, définir les plats disponibles pour chaque formule.

### ⏰ QUAND

#### **Création des menus**
- **Quand** : Avant le début de la semaine concernée
- **Recommandation** : Vendredi après-midi ou lundi matin pour la semaine N+1
- **Fréquence** : Hebdomadaire (une fois par semaine)

#### **Modification des menus**
- **Quand** : À tout moment, avant la date de consommation
- **Limitation** : Les menus déjà consommés ne peuvent pas être modifiés

### 📝 COMMENT

#### **Création d'un menu pour un jour**

##### Étape 1 : Accès
```
1. Se connecter en tant qu'Administrateur ou RH
2. Aller dans "Menus" → "Gérer les menus"
   ou "Menus" → "Créer un menu"
```

##### Étape 2 : Sélection de la date
```
1. Choisir la date pour laquelle créer le menu
2. Vérifier qu'un menu n'existe pas déjà pour cette date
3. Si menu existe → Option de modification
```

##### Étape 3 : Création des formules
```
Pour chaque type de formule (Améliorée, Standard 1, Standard 2) :

1. Sélectionner le type de formule
2. Renseigner les composants :
   - Entrée : Sélectionner dans la liste
   - Plat principal : Sélectionner dans la liste
   - Dessert : Sélectionner dans la liste
   - Garniture : Sélectionner dans la liste
   - Boisson : Sélectionner dans la liste
3. Définir le prix (si applicable)
4. Activer/Désactiver la formule
5. Répéter pour chaque type de formule
```

##### Étape 4 : Sauvegarde
```
1. Vérifier toutes les formules
2. Cliquer sur "Enregistrer"
3. Le système :
   - Crée les formules pour la date
   - Valide les données
   - Enregistre dans la base de données
4. Confirmation affichée
```

#### **Modification d'un menu existant**

##### Étape 1 : Accès
```
1. Aller dans "Menus" → "Gérer les menus"
2. Trouver le menu à modifier (par date)
3. Cliquer sur "Modifier"
```

##### Étape 2 : Modification
```
1. Modifier les composants des formules souhaitées
2. Changer les plats, prix, activation
3. Cliquer sur "Enregistrer"
```

#### **Consultation des menus (Employés)**

##### Étape 1 : Accès
```
1. Se connecter en tant qu'Employé
2. Aller dans "Menus" → "Voir les menus"
```

##### Étape 2 : Navigation
```
1. Sélectionner la semaine souhaitée
2. Affichage des menus jour par jour :
   - Date
   - Formules disponibles
   - Détails de chaque formule
3. Navigation entre semaines possible
```

### ✅ RÈGLES ET CONTRAINTES

#### Règle 1 : Date unique
- Un seul menu par date
- Impossible de créer deux menus pour la même date

#### Règle 2 : Formules obligatoires
- Au moins une formule doit être définie pour chaque jour
- Les formules doivent avoir tous les composants requis

#### Règle 3 : Modification limitée
- Les menus pour des dates passées ne peuvent pas être modifiés
- Les menus avec commandes associées peuvent être modifiés (avec précaution)

### 📊 RÉSULTAT

- **Menu créé** : Disponible pour la date sélectionnée
- **Visibilité** : Visible par les employés dans "Voir les menus"
- **Utilisation** : Utilisable lors de la création de commandes pour cette date

---

## 9. WORKFLOW DE GESTION DES QUOTAS

### 📌 QUI
- **Rôles autorisés** : `Administrateur`, `RH`
- **Principal acteur** : RH (gestion quotidienne)

### 🎯 QUOI
Définir et gérer les quotas de repas pour les groupes non-CIT (notamment les Douaniers), par période (Jour/Nuit).

### ⏰ QUAND

#### **Création/Modification des quotas**
- **Quand** : Avant le début de la période concernée
- **Recommandation** : Au début de chaque semaine ou mois
- **Fréquence** : Selon les besoins (hebdomadaire, mensuelle, etc.)

#### **Consultation des quotas**
- **Quand** : À tout moment
- **Utilité** : Vérifier les quotas disponibles avant de créer une commande Douaniers

### 📝 COMMENT

#### **Création d'un quota pour un groupe**

##### Étape 1 : Accès
```
1. Se connecter en tant qu'Administrateur ou RH
2. Aller dans "Gestion" → "Groupes Non-CIT"
   ou "Configuration" → "Quotas"
```

##### Étape 2 : Sélection du groupe
```
1. Sélectionner le groupe (ex: "Douaniers")
2. Si le groupe n'existe pas → Créer le groupe d'abord
```

##### Étape 3 : Définition des quotas
```
Pour chaque période (Jour et Nuit) :

1. Quota Jour (Midi) :
   - Entrer le nombre de plats disponibles
   - Exemple : 50 plats

2. Quota Nuit (Soir) :
   - Entrer le nombre de plats disponibles
   - Exemple : 30 plats

3. Ces quotas sont PERMANENTS (actifs pour tous les jours)
```

##### Étape 4 : Sauvegarde
```
1. Vérifier les valeurs
2. Cliquer sur "Enregistrer"
3. Le système :
   - Enregistre les quotas
   - Active les quotas pour le groupe
```

#### **Consultation des quotas disponibles**

##### Étape 1 : Accès
```
1. Se connecter en tant que PrestataireCantine ou Administrateur
2. Aller dans "Commandes" → "Commande Douaniers"
```

##### Étape 2 : Vérification automatique
```
Lors de la création d'une commande Douaniers :
1. Le système affiche automatiquement :
   - Quota total pour la période (Jour ou Nuit)
   - Plats déjà consommés aujourd'hui pour cette période
   - Quota restant disponible
2. Si quantité demandée > quota restant → ❌ Erreur
3. Si quantité demandée ≤ quota restant → ✅ Autorisé
```

### ✅ RÈGLES ET CONTRAINTES

#### Règle 1 : Quotas permanents
- Les quotas sont permanents (pas de date d'expiration)
- Ils s'appliquent à tous les jours
- Modification possible à tout moment

#### Règle 2 : Vérification en temps réel
- Lors de la création d'une commande Douaniers, vérification automatique
- Blocage si quota insuffisant

#### Règle 3 : Comptage quotidien
- Le système compte les plats consommés par jour
- Chaque jour, le quota se réinitialise (quotas quotidiens)

### 📊 RÉSULTAT

- **Quotas définis** : Disponibles pour le groupe
- **Vérification** : Automatique lors de la création de commandes
- **Suivi** : Comptage automatique des consommations

---

## 10. WORKFLOW DE REPORTING ET STATISTIQUES

### 📌 QUI
- **Rôles autorisés** : `Administrateur`, `RH`
- **Principal acteur** : RH (rapports réguliers), Administrateur (statistiques globales)

### 🎯 QUOI
Consulter les rapports, statistiques, et exporter les données pour analyse et suivi.

### ⏰ QUAND

#### **Consultation des statistiques**
- **Quand** : À tout moment
- **Fréquence** : Selon les besoins (quotidien, hebdomadaire, mensuel)

#### **Génération de rapports**
- **Quand** : À la fin de chaque période (semaine, mois)
- **Recommandation** : Fin de semaine pour rapport hebdomadaire, fin de mois pour rapport mensuel

### 📝 COMMENT

#### **Consultation des statistiques**

##### Étape 1 : Accès
```
1. Se connecter en tant qu'Administrateur ou RH
2. Aller dans "Rapports" → "Statistiques"
   ou "Dashboard" → "Statistiques"
```

##### Étape 2 : Sélection de la période
```
1. Choisir la date de début
2. Choisir la date de fin
3. Cliquer sur "Afficher"
```

##### Étape 3 : Consultation
```
Affichage des statistiques :
- Nombre total de commandes
- Nombre de commandes par statut
- Nombre de commandes par type de formule
- Revenus totaux
- Consommations par période (Jour/Nuit)
- Consommations par site
- Top utilisateurs
- Etc.
```

#### **Export Excel**

##### Étape 1 : Accès
```
1. Aller dans "Rapports" → "Export"
   ou dans une liste (commandes, points de consommation, etc.)
2. Cliquer sur "Exporter en Excel"
```

##### Étape 2 : Sélection
```
1. Choisir les données à exporter :
   - Toutes les commandes
   - Commandes d'une période
   - Points de consommation
   - Utilisateurs
   - etc.
2. Choisir la période (si applicable)
```

##### Étape 3 : Génération
```
1. Cliquer sur "Générer l'export"
2. Le système :
   - Génère le fichier Excel
   - Télécharge le fichier
3. Ouvrir le fichier Excel pour consultation
```

#### **Rapport de facturation**

##### Étape 1 : Accès
```
1. Aller dans "Facturation" → "Rapport de facturation"
```

##### Étape 2 : Sélection
```
1. Choisir la période
2. Filtrer par utilisateur, département, etc. (si disponible)
3. Cliquer sur "Générer le rapport"
```

##### Étape 3 : Consultation
```
Affichage :
- Commandes facturées
- Montant total facturé
- Détail par utilisateur
- Détail par période
```

### ✅ RÈGLES ET CONTRAINTES

#### Règle 1 : Accès restreint
- Seuls les Administrateurs et RH peuvent consulter les rapports
- Les employés ne voient que leurs propres statistiques

#### Règle 2 : Période limitée
- Les rapports peuvent être générés pour toute période
- Les données disponibles dépendent des données en base

### 📊 RÉSULTAT

- **Statistiques** : Affichées à l'écran
- **Export Excel** : Fichier téléchargé
- **Rapports** : Générés et consultables

---

## 📅 CALENDRIER RÉCAPITULATIF HEBDOMADAIRE

### Vue d'ensemble de la semaine type

```
┌─────────────────────────────────────────────────────────────────┐
│                    SEMAINE TYPE - WORKFLOW                      │
└─────────────────────────────────────────────────────────────────┘

LUNDI (Semaine N)
├─ 00:00 → Ouverture des commandes pour semaine N+1
├─ Toute la journée → Employés peuvent commander
├─ Délai recommandé : 48h avant 12h de la date de consommation
└─ Actions possibles :
   ✅ Création de commandes
   ✅ Modification de commandes (semaine N+1)
   ✅ Annulation de commandes (semaine N+1)

MARDI à JEUDI (Semaine N)
├─ Toute la journée → Commandes ouvertes
├─ Actions possibles :
   ✅ Création de commandes
   ✅ Modification de commandes (semaine N+1)
   ✅ Annulation de commandes (semaine N+1, ou semaine courante si ≥24h)
   ✅ Commandes instantanées (Admin/Prestataire)
   ✅ Commandes Douaniers (Admin/Prestataire)
   ✅ Validation de consommations
   ✅ Facturation automatique (toutes les heures)

VENDREDI (Semaine N)
├─ 00:00 - 11:59 → Dernières heures pour commander
├─ 12:00 → 🔒 FERMETURE AUTOMATIQUE
│          ├─ Commandes semaine N+1 confirmées
│          ├─ Nouvelles commandes bloquées
│          └─ Blocage jusqu'au lundi suivant
├─ Après 12:00 → Commandes bloquées
└─ Actions possibles :
   ✅ Validation de consommations
   ✅ Facturation automatique (toutes les heures)
   ❌ Création de nouvelles commandes (bloquée)

SAMEDI et DIMANCHE
├─ Commandes complètement bloquées
└─ Actions possibles :
   ✅ Validation de consommations (si service ouvert)
   ✅ Facturation automatique (toutes les heures)
   ❌ Création de commandes (bloquée)

LUNDI SUIVANT (Semaine N+1)
├─ 00:00 → Réouverture des commandes (pour semaine N+2)
├─ Les commandes de cette semaine peuvent être consommées
├─ Validation des commandes au point de consommation
└─ Actions possibles :
   ✅ Création de commandes (semaine N+2)
   ✅ Validation de consommations (semaine N+1)
   ✅ Commandes instantanées
   ✅ Commandes Douaniers
```

### Horaires clés

| Moment | Action | Acteur |
|--------|--------|--------|
| **Lundi 00:00** | Ouverture des commandes | Système |
| **Vendredi 12:00** | Fermeture automatique | Système (automatique) |
| **Toutes les heures** | Facturation automatique | Système (automatique) |
| **Toute la journée** | Commandes instantanées | Admin/Prestataire |
| **Midi/Soir** | Validation de consommations | Prestataire/Admin |

### Délais récapitulatifs

| Action | Délai | Qui |
|--------|-------|-----|
| **Créer une commande** | Avant vendredi 12h (semaine N) | Employé, Admin, RH |
| **Modifier une commande** | Jusqu'au dimanche 12h (semaine N+1) | Employé, Admin, RH |
| **Annuler une commande** | 24h avant la consommation | Employé |
| **Annuler une commande** | Sans restriction (sauf consommée) | Admin, RH, Prestataire |
| **Créer commande instantanée** | Jour même, avant 12h vendredi | Admin, Prestataire |
| **Valider une consommation** | Jour de consommation | Prestataire, Admin |

---

## 📝 NOTES IMPORTANTES

### Statuts de commande

1. **Précommandée** (Precommander) : Commande créée, en attente
2. **Consommée** (Consommee) : Repas récupéré et validé
3. **Annulée** (Annulee) : Commande annulée
4. **Facturée** : Commande non consommée, facturée (selon implémentation)

### Services automatiques

1. **FermetureAutomatiqueService** : S'exécute toutes les 5 minutes, ferme le vendredi 12h
2. **FacturationAutomatiqueService** : S'exécute toutes les heures, facture les non-consommées

### Points de consommation

- Créés automatiquement lors de la validation d'une commande
- Créés automatiquement lors de la facturation (lieu "FACTURATION")
- Peuvent être créés manuellement par Admin/RH

---

**Fin du document**

**Dernière mise à jour** : 2025-01-XX  
**Version** : 1.0

