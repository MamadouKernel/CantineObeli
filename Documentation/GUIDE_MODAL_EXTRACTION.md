# 📊 Guide: Modal d'Extraction des Commandes

## Date: 05/03/2026

---

## 🎯 Objectif

Apprendre à utiliser la modal d'extraction pour exporter les commandes.

---

## 📍 Comment Accéder à la Modal?

### Méthode 1: Via le Menu Principal

1. Cliquer sur "Commandes" dans le menu principal
2. Sélectionner "Extraction Commandes"
3. La page d'extraction s'ouvre
4. Cliquer sur le bouton "Extraire" ou "Exporter"
5. La modal s'affiche

### Méthode 2: Via l'URL Directe

Accéder directement à: `/Extraction/Index`

### Méthode 3: Depuis la Liste des Commandes

1. Aller dans "Commandes" → "Liste des Commandes"
2. Cliquer sur "Exporter en Excel" (si disponible)
3. La modal d'extraction s'affiche

---

## 🎨 Contenu de la Modal

### Filtres Disponibles

La modal d'extraction permet de filtrer les commandes à exporter:

#### 1. Période
- **Date de début:** Date de début de la période
- **Date de fin:** Date de fin de la période
- **Raccourcis:** Aujourd'hui, Cette semaine, Ce mois

#### 2. Statut
- ☐ Précommandées
- ☐ Consommées
- ☐ Annulées
- ☐ Toutes

#### 3. Type de Client
- ☐ Employés CIT
- ☐ Groupes Non-CIT
- ☐ Visiteurs
- ☐ Tous

#### 4. Site
- ☐ Site 1
- ☐ Site 2
- ☐ Tous les sites

#### 5. Période de Service
- ☐ Jour (Déjeuner)
- ☐ Nuit (Dîner)
- ☐ Toutes

---

## 📥 Formats d'Export

### Format Excel (.xlsx)
- ✅ Recommandé pour l'analyse
- ✅ Compatible avec Microsoft Excel, LibreOffice
- ✅ Conserve le formatage

### Format CSV (.csv)
- ✅ Recommandé pour l'import dans d'autres systèmes
- ✅ Format universel
- ✅ Léger et rapide

---

## 📋 Colonnes Exportées

L'export contient les colonnes suivantes:

1. **Code Commande** - Identifiant unique
2. **Date Commande** - Date de création
3. **Date Consommation** - Date prévue
4. **Statut** - Précommandée/Consommée/Annulée
5. **Utilisateur** - Nom complet (si applicable)
6. **Matricule** - Matricule employé (si applicable)
7. **Type Client** - CIT/Groupe/Visiteur
8. **Formule** - Nom du menu
9. **Période** - Jour/Nuit
10. **Site** - Lieu de livraison
11. **Quantité** - Nombre de repas
12. **Montant** - Prix total
13. **Motif Annulation** - Si annulée

---

## 🔧 Utilisation Pas à Pas

### Étape 1: Ouvrir la Modal

```
Menu → Commandes → Extraction Commandes
```

### Étape 2: Définir les Filtres

```
1. Sélectionner la période (ex: 01/03/2026 - 31/03/2026)
2. Cocher les statuts souhaités (ex: Consommées uniquement)
3. Sélectionner le type de client (ex: Tous)
4. Choisir le site (ex: Tous les sites)
5. Sélectionner la période de service (ex: Toutes)
```

### Étape 3: Choisir le Format

```
☐ Excel (.xlsx) - Recommandé
☐ CSV (.csv)
```

### Étape 4: Exporter

```
Cliquer sur "Exporter" ou "Télécharger"
```

### Étape 5: Téléchargement

```
Le fichier se télécharge automatiquement
Nom du fichier: Commandes_YYYYMMDD_HHMMSS.xlsx
```

---

## 💡 Cas d'Usage

### Cas 1: Rapport Mensuel

**Objectif:** Exporter toutes les commandes consommées du mois

**Filtres:**
- Période: 01/03/2026 - 31/03/2026
- Statut: Consommées uniquement
- Type: Tous
- Format: Excel

### Cas 2: Facturation Prestataire

**Objectif:** Exporter les commandes pour facturation

**Filtres:**
- Période: Mois en cours
- Statut: Consommées uniquement
- Type: Tous
- Site: Tous
- Format: Excel

### Cas 3: Analyse des Annulations

**Objectif:** Analyser les commandes annulées

**Filtres:**
- Période: Dernier trimestre
- Statut: Annulées uniquement
- Type: Tous
- Format: Excel (pour analyse)

### Cas 4: Export pour Comptabilité

**Objectif:** Données pour le service comptable

**Filtres:**
- Période: Mois précédent
- Statut: Consommées uniquement
- Type: Employés CIT uniquement
- Format: CSV (pour import)

---

## ⚠️ Points d'Attention

### Performances

- ⚠️ Les exports de grandes périodes peuvent prendre du temps
- ✅ Limitez la période si possible (1 mois recommandé)
- ✅ Utilisez les filtres pour réduire le volume

### Données Sensibles

- ⚠️ Les exports contiennent des données personnelles
- ✅ Respectez la confidentialité
- ✅ Ne partagez pas les fichiers sans autorisation
- ✅ Supprimez les fichiers après utilisation

### Format

- ✅ Excel: Meilleur pour l'analyse et les rapports
- ✅ CSV: Meilleur pour l'import dans d'autres systèmes
- ⚠️ CSV peut avoir des problèmes d'encodage avec les accents

---

## 🔍 Dépannage

### La Modal ne S'Ouvre Pas

**Solutions:**
1. Vérifiez que JavaScript est activé
2. Rafraîchissez la page (F5)
3. Videz le cache du navigateur
4. Essayez un autre navigateur

### L'Export est Vide

**Solutions:**
1. Vérifiez les filtres (peut-être trop restrictifs)
2. Vérifiez la période sélectionnée
3. Vérifiez qu'il y a des commandes dans cette période

### Le Téléchargement Échoue

**Solutions:**
1. Vérifiez votre connexion internet
2. Réduisez la période d'export
3. Essayez le format CSV au lieu d'Excel
4. Contactez le support technique

---

## 📞 Support

**Accès:** Administrateur, RH uniquement

**En cas de problème:**
- Contactez votre Administrateur système
- Email: support@obeli.com
- Téléphone: +225 XX XX XX XX

---

## 🎓 Astuces

### Astuce 1: Exports Réguliers

Créez des exports réguliers (hebdomadaires/mensuels) pour:
- Suivi de la consommation
- Analyse des tendances
- Archivage des données

### Astuce 2: Nommage des Fichiers

Renommez vos exports avec des noms explicites:
- `Commandes_Mars2026_Consommees.xlsx`
- `Facturation_Prestataire_T1_2026.xlsx`
- `Annulations_Analyse_2026.xlsx`

### Astuce 3: Modèles Excel

Créez des modèles Excel avec:
- Tableaux croisés dynamiques
- Graphiques automatiques
- Formules de calcul

---

**Date:** 05/03/2026  
**Version:** 1.0  
**Auteur:** Équipe O'Beli

