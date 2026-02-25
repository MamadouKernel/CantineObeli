# 📊 Explication : Extraction Commandes et Exporter en Excel

## 📋 Vue d'ensemble

Ces deux fonctionnalités permettent d'extraire et d'exporter les données des commandes vers Excel, mais avec des objectifs et des processus différents.

---

## 🔍 1. Extraction Commandes

### 📍 Accès
- **URL** : `/Extraction/Index`
- **Menu** : Point Financier → Extraction Commandes
- **Rôles autorisés** : `Administrateur`, `RH`, `PrestataireCantine`

### 🎯 Objectif
Extraire les **commandes précommandées** pour une période donnée, avec la possibilité de **définir/modifier les marges** avant l'extraction. Cette fonctionnalité est principalement utilisée pour la préparation des commandes futures et la gestion des marges.

### 🔄 Processus en 3 étapes

#### Étape 1 : Sélection de la période et définition des marges
1. L'utilisateur sélectionne une **période** (Date de début et Date de fin)
2. Le système affiche tous les **menus (formules)** de cette période
3. Pour chaque menu, l'utilisateur peut :
   - Voir la **marge actuelle** (en %)
   - **Modifier la marge** si nécessaire
4. L'utilisateur valide les marges

#### Étape 2 : Extraction des commandes
1. Le système recherche toutes les **commandes précommandées** (`StatusCommande = Precommander`) dans la période sélectionnée
2. Les commandes sont **groupées par formule**
3. Pour chaque formule, le système affiche :
   - **Date** de la formule
   - **Type de formule** (Standard 1, Standard 2, Amélioré)
   - **Nom du plat**
   - **Nombre de commandes** pour cette formule
   - **Marge** associée (en %)

#### Étape 3 : Export en Excel
1. L'utilisateur clique sur "Exporter en Excel"
2. Le système génère un fichier Excel avec toutes les commandes précommandées
3. Le fichier contient les colonnes suivantes :
   - Date Consommation
   - Code Commande
   - Type Client (CitUtilisateur, GroupeNonCit, Visiteur)
   - Client (Nom du client)
   - Matricule/Code Groupe
   - Site
   - Type Formule
   - Nom Plat
   - Quantité
   - Période (Jour/Nuit)
   - Marge (%)

### 📊 Données extraites
- **Type de commandes** : Uniquement les commandes **précommandées** (`StatusCommande = Precommander`)
- **Période** : Basée sur la `DateConsommation` des commandes
- **Filtres** : Aucun filtre supplémentaire (toutes les commandes précommandées de la période)

### 💡 Cas d'utilisation
- Préparer les commandes pour la semaine suivante
- Vérifier et ajuster les marges avant l'extraction
- Générer un fichier Excel pour le prestataire avec les commandes précommandées
- Analyser les commandes futures par formule

---

## 📥 2. Exporter en Excel (depuis la liste des commandes)

### 📍 Accès
- **URL** : `/Commande/ExporterExcel`
- **Menu** : Point Financier → Exporter en Excel
- **Rôles autorisés** : `Administrateur`, `RH`, `PrestataireCantine`

### 🎯 Objectif
Exporter **toutes les commandes** (tous statuts confondus) vers Excel, avec la possibilité d'appliquer des **filtres** (statut, dates, matricule).

### 🔄 Processus

1. L'utilisateur accède directement à l'export (ou depuis la page "Liste des Commandes" avec des filtres)
2. Le système exporte toutes les commandes selon les filtres appliqués :
   - **Statut** : Tous les statuts (Précommandée, Consommée, Annulée, Facturée, Exemptée, Indisponible, Non Récupérée)
   - **Date de début** (optionnel)
   - **Date de fin** (optionnel)
   - **Matricule** (optionnel, pour Admin/RH)
3. Le fichier Excel généré contient les colonnes suivantes :
   - Code Commande
   - Date (date de création)
   - Date Consommation
   - Client
   - Type Client
   - Site
   - Formule
   - Nom Plat
   - Statut
   - Période (Jour/Nuit)
   - Quantité
   - Montant
   - Instantanée (Oui/Non)

### 📊 Données exportées
- **Type de commandes** : **Toutes les commandes** (tous statuts)
- **Filtres disponibles** :
  - Statut de la commande
  - Date de début
  - Date de fin
  - Matricule (pour Admin/RH uniquement)
- **Tri** : Par date de création (plus récentes en premier)

### 💡 Cas d'utilisation
- Exporter l'historique complet des commandes
- Analyser les commandes par statut
- Générer des rapports pour la comptabilité
- Exporter les commandes d'un utilisateur spécifique

---

## 🔄 Différences principales

| Critère | Extraction Commandes | Exporter en Excel |
|---------|---------------------|-------------------|
| **Type de commandes** | Uniquement précommandées | Tous les statuts |
| **Gestion des marges** | ✅ Oui (définition/modification) | ❌ Non |
| **Processus** | 3 étapes (Période → Marges → Extraction → Export) | 1 étape (Export direct) |
| **Filtres** | Période uniquement | Statut, Dates, Matricule |
| **Objectif** | Préparation des commandes futures | Export de l'historique complet |
| **Colonnes Excel** | 11 colonnes (inclut Marge %) | 13 colonnes (inclut Statut, Montant, Instantanée) |

---

## 📝 Exemple d'utilisation

### Scénario 1 : Extraction Commandes
**Contexte** : Le prestataire veut préparer les commandes pour la semaine prochaine.

1. Accéder à "Extraction Commandes"
2. Sélectionner la période : Du 20/01/2024 au 26/01/2024
3. Vérifier/modifier les marges pour chaque formule
4. Valider les marges
5. Voir les commandes extraites groupées par formule
6. Exporter en Excel pour envoyer au prestataire

**Résultat** : Fichier Excel avec uniquement les commandes précommandées de la semaine prochaine, avec les marges définies.

### Scénario 2 : Exporter en Excel
**Contexte** : L'administrateur veut un rapport complet des commandes du mois dernier.

1. Accéder à "Exporter en Excel" (ou depuis "Liste des Commandes" avec filtres)
2. Le système exporte toutes les commandes (tous statuts)
3. Télécharger le fichier Excel

**Résultat** : Fichier Excel avec toutes les commandes du mois, incluant les statuts, montants, etc.

---

## 🔧 Aspects techniques

### Extraction Commandes
- **Contrôleur** : `ExtractionController`
- **Actions principales** :
  - `Index()` (GET) : Affiche le formulaire de sélection de période
  - `Index(ExtractionViewModel)` (POST) : Étape 1 - Affiche les menus avec marges
  - `ValiderMarges(DefinirMargesViewModel)` (POST) : Valide et sauvegarde les marges
  - `ExtraireCommandes(ExtractionModalViewModel)` (POST) : Étape 2 - Extrait les commandes
  - `ExporterExcel(ExtractionModalViewModel)` (POST) : Étape 3 - Exporte en Excel

### Exporter en Excel
- **Contrôleur** : `CommandeController`
- **Action** : `ExporterExcel(string? status, DateTime? dateDebut, DateTime? dateFin, string? matricule)`
- **Filtres** : Appliqués directement dans la requête LINQ

---

## ⚠️ Notes importantes

1. **Extraction Commandes** :
   - Ne concerne que les commandes **précommandées**
   - Permet de **modifier les marges** avant l'extraction
   - Processus en plusieurs étapes

2. **Exporter en Excel** :
   - Exporte **toutes les commandes** (tous statuts)
   - **Pas de gestion des marges**
   - Export direct sans étapes intermédiaires

3. **Fichiers Excel générés** :
   - Format : `.xlsx` (Excel 2007+)
   - Bibliothèque utilisée : `ClosedXML`
   - Nom du fichier : 
     - Extraction : `Extraction_Commandes_YYYYMMDD_YYYYMMDD.xlsx`
     - Export : `Commandes_YYYYMMDD_HHMMSS.xlsx`

---

## 🎯 Quand utiliser chaque fonctionnalité ?

### Utiliser "Extraction Commandes" si :
- ✅ Vous voulez extraire uniquement les commandes précommandées
- ✅ Vous devez définir/modifier les marges avant l'extraction
- ✅ Vous préparez les commandes pour une période future
- ✅ Vous voulez voir les commandes groupées par formule avec leurs marges

### Utiliser "Exporter en Excel" si :
- ✅ Vous voulez exporter toutes les commandes (tous statuts)
- ✅ Vous avez besoin d'un export rapide sans gestion de marges
- ✅ Vous voulez appliquer des filtres (statut, dates, matricule)
- ✅ Vous générez un rapport complet pour la comptabilité

