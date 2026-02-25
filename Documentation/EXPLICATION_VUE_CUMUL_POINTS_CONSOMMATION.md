# 📊 Explication de la Vue "Statistiques des Consommations"

## 🎯 Vue d'ensemble

La vue **Statistiques des Consommations** (`https://localhost:7021/Commande/CumulPointsConsommation`) est une interface de **statistiques et d'analyse** qui permet aux **Administrateurs** et aux **RH** de consulter des **données agrégées** sur toutes les consommations de repas dans le système. Cette vue fournit une vue d'ensemble globale avec des statistiques, des répartitions et des tendances.

---

## 🔐 Accès et Autorisations

### Rôles autorisés
- ✅ **Administrateur**
- ✅ **RH** (Ressources Humaines)

### Accès refusé
- ❌ **Employé**
- ❌ **PrestataireCantine**

---

## 📋 Fonctionnalités principales

### 1. **Période par défaut (Cycle de facturation)**

La vue utilise une période par défaut basée sur un **cycle de facturation mensuel** :

- **Date de début** : Le **17 du mois précédent**
- **Date de fin** : Le **16 du mois en cours**

**Exemple** : Si nous sommes le 20 janvier 2024 :
- Date de début : 17 décembre 2023
- Date de fin : 16 janvier 2024

Cette période peut être modifiée via les filtres de date.

---

### 2. **Statistiques globales (5 cartes)**

#### Carte 1 : Total des repas consommés
- **Icône** : ✅ (check-circle)
- **Valeur** : Nombre de repas effectivement consommés (statut = Consommée)
- **Couleur** : Vert (success)
- **Signification** : Repas qui ont été validés et effectivement consommés par les utilisateurs

#### Carte 2 : Total des repas non récupérés
- **Icône** : ⚠️ (exclamation-triangle)
- **Valeur** : Nombre de repas commandés mais non récupérés
- **Couleur** : Jaune (warning)
- **Signification** : Repas qui ont été commandés mais :
  - N'ont pas été récupérés avant 23h59 (statut = Non Récupérée)
  - Ou ont été facturés (statut = Précommandée avec lieu contenant "FACTURATION")

#### Carte 3 : Total des repas indisponibles
- **Icône** : ❌ (times-circle)
- **Valeur** : Nombre de repas commandés mais indisponibles (statut = Indisponible)
- **Couleur** : Rouge (danger)
- **Signification** : Repas qui ont été commandés mais les plats étaient finis (quotas épuisés). Ces repas ne peuvent pas être honorés et ne sont pas facturables.

#### Carte 4 : Coût total
- **Icône** : 💰 (money-bill-wave)
- **Valeur** : Montant total facturable en FCFA
- **Couleur** : Bleu (primary)
- **Calcul** : Somme des coûts des repas consommés + somme des coûts des repas non récupérés
- **Note** : Les repas indisponibles ne sont pas inclus dans ce calcul (coût = 0 FCFA)

#### Carte 5 : Utilisateurs actifs
- **Icône** : 👥 (users)
- **Valeur** : Nombre d'utilisateurs distincts ayant consommé dans la période
- **Couleur** : Bleu clair (info)

---

### 3. **Répartition par Statut**

Affiche la répartition des points de consommation selon le statut de la commande :

#### Statuts possibles (Enum StatutCommande)

L'enum `StatutCommande` contient les valeurs suivantes :

1. **Precommander = 0** : **Précommandée**
   - Commande créée à l'avance (au moins 48h avant la date de consommation)
   - En attente de validation/consommation
   - Peut être récupérée le jour prévu ou facturée si non récupérée

2. **Consommee = 1** : **Consommée**
   - Repas effectivement consommé et validé par le prestataire
   - Le point de consommation est créé et facturable
   - Les quotas sont décrémentés lors de la validation

3. **Annulee = 2** : **Annulée**
   - Commande annulée par l'utilisateur ou le prestataire
   - Peut être annulée dans les 24h pour les commandes de la semaine en cours
   - Non facturable (coût = 0 FCFA)

4. **Facturee = 3** : **Facturée**
   - Commande facturée (généralement après consommation)
   - Le point de consommation est créé et facturable
   - Utilisé pour le suivi comptable

5. **Exemptee = 4** : **Exemptée**
   - **Commande exemptée de paiement**
   - Utilisée pour les commandes qui ne doivent pas être facturées
   - **Cas d'utilisation** :
     - Repas offerts par l'entreprise (événements, réunions, etc.)
     - Repas gratuits pour certains utilisateurs (selon les règles métier)
     - Commandes spéciales non facturables
     - Commandes pour des invités VIP ou des occasions spéciales
   - **Important** : 
     - Le statut "Exemptée" peut être appliqué manuellement par un administrateur ou automatiquement par le système selon les règles métier
     - Dans le système de facturation automatique, les commandes non facturables sont loggées comme "exemptées" mais peuvent conserver leur statut d'origine (ex: "Précommandée")
     - Ce statut indique explicitement qu'une commande ne doit pas être facturée, même si elle a été consommée
   - **Coût** : 0 FCFA (non facturable)
   - **Note** : Ce statut est disponible dans l'enum mais peut ne pas être utilisé fréquemment dans le système actuel

6. **Indisponible = 5** : **Indisponible**
   - Commande précommandée mais les plats étaient finis (quotas épuisés)
   - La commande a été créée mais ne peut pas être honorée
   - Non facturable (coût = 0 FCFA)
   - Le statut est automatiquement changé par le système lorsque les quotas sont épuisés

7. **NonRecuperer = 6** : **Non Récupérée**
   - Commande précommandée qui n'a pas été récupérée avant 23h59
   - Le statut est automatiquement changé par le `ChangementStatutAutomatiqueService` à 23h59
   - Généralement facturable (selon les règles de facturation)
   - Un point de consommation avec "FACTURATION" dans le lieu peut être créé

#### Informations affichées
- **Statut** : Nom du statut (selon l'enum)
- **Pourcentage** : Pourcentage du total des points
- **Nombre de points** : Nombre de points dans ce statut
- **Coût total** : Montant total pour ce statut (peut être 0 pour certains statuts)

#### Tri
- Trié par nombre de points décroissant (du plus fréquent au moins fréquent)

#### Statuts facturables vs non facturables

**Statuts facturables** (inclus dans le calcul des coûts) :
- ✅ **Consommée** (1) : Facturable
- ✅ **Facturée** (3) : Facturable
- ✅ **Précommandée** (0) avec lieu contenant "FACTURATION" : Facturable
- ✅ **Non Récupérée** (6) : Généralement facturable

**Statuts non facturables** (coût = 0 FCFA) :
- ❌ **Annulée** (2) : Non facturable
- ❌ **Exemptée** (4) : Non facturable (exemptée de paiement)
- ❌ **Indisponible** (5) : Non facturable
- ❌ **Précommandée** (0) sans facturation : Non facturable (en attente)

---

### 4. **Répartition par Formule**

Affiche la répartition des consommations selon le type de formule :

#### Informations affichées
- **Nom de la formule** : Ex. "Standard 1", "Standard 2", "Amélioré"
- **Pourcentage** : Pourcentage du total des consommations
- **Nombre de consommations** : Nombre de fois que cette formule a été consommée
- **Coût total** : Montant total pour cette formule

#### Tri
- Trié par nombre de consommations décroissant (formule la plus populaire en premier)

---

### 5. **Évolution Temporelle (7 derniers jours)**

Affiche l'évolution des consommations sur les **7 derniers jours** de la période :

#### Informations affichées
- **Date** : Date de consommation (format dd/MM/yyyy)
- **Jour de la semaine** : Nom du jour (lundi, mardi, etc.)
- **Nombre de points** : Nombre de points consommés ce jour
- **Coût total** : Montant total pour ce jour

#### Utilisation
- Permet d'identifier les jours de forte/faible consommation
- Aide à détecter des tendances ou des anomalies

---

### 6. **Top 5 Utilisateurs (Anonymisés)**

Affiche les **5 utilisateurs** ayant le plus de points de consommation :

#### Anonymisation
- Les utilisateurs sont affichés comme : `"Utilisateur {8 premiers caractères du GUID}..."`
- Exemple : `"Utilisateur 12345678..."`

#### Informations affichées
- **Rang** : Position dans le classement (#1, #2, etc.)
- **Icône** : Différente pour chaque rang (crown, medal, award, star, certificate)
- **Couleur** : Différente pour chaque rang (warning, secondary, success, info, primary)
- **Nom** : Nom anonymisé de l'utilisateur
- **Dernière consommation** : Date de la dernière consommation
- **Nombre de points** : Total des points de consommation
- **Coût total** : Montant total facturable

#### Tri
- Trié par nombre de points décroissant

---

### 7. **Tableau détaillé par Utilisateur (Anonymisé)**

Affiche un tableau complet avec **tous les utilisateurs** ayant consommé dans la période :

#### Colonnes du tableau
1. **#** : Rang de l'utilisateur
2. **Utilisateur** : Nom anonymisé avec avatar
3. **Points** : Nombre total de points de consommation
4. **Coût Total** : Montant total facturable
5. **Dernière Consommation** : Date et heure de la dernière consommation
6. **Coût Moyen** : Coût moyen par point (`Coût Total / Points`)

#### Tri
- Trié par nombre de points décroissant

#### Anonymisation
- Les utilisateurs sont affichés comme : `"Utilisateur {8 premiers caractères du GUID}..."`
- Permet de voir les statistiques sans exposer les identités

---

## 💰 Calcul des coûts

### Méthode `CalculerCoutPoint`

Pour chaque point de consommation, le coût est calculé selon les règles suivantes :

#### 1. **Points facturables**
Un point est facturable si :
- La commande associée a le statut **Consommée** (1)
- La commande associée a le statut **Facturée** (3)
- La commande associée a le statut **Précommandée** (0) mais le lieu de consommation contient "FACTURATION"
- La commande associée a le statut **Non Récupérée** (6) - généralement facturable selon les règles de facturation

#### 2. **Montant pour les facturations**
Si le lieu de consommation contient "FACTURATION", le montant est extrait du texte :
- Format : `FACTURATION (XXXX F CFA)`
- Le montant entre parenthèses est utilisé directement

#### 3. **Montant standard**
Pour les autres points facturables :
- **Formule Standard** (Standard 1, Standard 2) : **550 FCFA** par unité
- **Formule Améliorée** : **2800 FCFA** par unité
- **Montant** = `Quantité consommée × Prix unitaire`

#### 4. **Points non facturables**
- Si la commande n'est pas dans un statut facturable : **0 FCFA**
- **Statuts non facturables** :
  - **Annulée** (2) : Commande annulée
  - **Exemptée** (4) : Commande exemptée de paiement (repas offerts, gratuits, etc.)
  - **Indisponible** (5) : Plats finis, commande non honorée
  - **Précommandée** (0) sans facturation : En attente de consommation

---

## 📊 Structure des données

### ViewModel (objet dynamique)

```csharp
{
    DateDebut: DateTime,
    DateFin: DateTime,
    TotalPoints: int,
    TotalCout: decimal,
    TotalUtilisateurs: int,
    CumulParUtilisateur: List<{
        UtilisateurId: Guid,
        NomComplet: string, // Anonymisé
        NombrePoints: int,
        CoutTotal: decimal,
        DerniereConsommation: DateTime
    }>,
    CumulParFormule: List<{
        FormuleNom: string,
        NombreConsommations: int,
        CoutTotal: decimal,
        Pourcentage: double
    }>,
    CumulParJour: List<{
        Date: DateTime,
        NombrePoints: int,
        CoutTotal: decimal
    }>,
    CumulParStatut: List<{
        Statut: string,
        NombrePoints: int,
        CoutTotal: decimal,
        Pourcentage: double
    }>,
    Periode: string // Format: "Du dd/MM/yyyy au dd/MM/yyyy"
}
```

---

## 🔄 Flux de données

### 1. **Récupération des points de consommation**
```
Tous les points de consommation dans la période
    ↓
Filtrage : Supprimer = 0, DateConsommation dans la période
    ↓
Inclusion des relations : Commande, FormuleJour
    ↓
Tri par DateConsommation croissante
```

### 2. **Calcul des statistiques globales**
```
TotalPoints = Nombre total de repas consommés
TotalCout = Somme de tous les coûts
TotalUtilisateurs = Nombre d'utilisateurs distincts
```

### 3. **Groupements**
```
Par Utilisateur :
    ↓
GroupBy(UtilisateurId)
    ↓
Calcul : NombrePoints, CoutTotal, DerniereConsommation
    ↓
Tri par NombrePoints décroissant

Par Formule :
    ↓
GroupBy(NomFormule)
    ↓
Calcul : NombreConsommations, CoutTotal, Pourcentage
    ↓
Tri par NombreConsommations décroissant

Par Jour :
    ↓
GroupBy(DateConsommation.Date)
    ↓
Calcul : NombrePoints, CoutTotal
    ↓
Tri par Date croissante

Par Statut :
    ↓
GroupBy(StatusCommande)
    ↓
Calcul : NombrePoints, CoutTotal, Pourcentage
    ↓
Tri par NombrePoints décroissant
```

---

## 🎨 Interface utilisateur

### Design
- **Cartes statistiques** : 4 cartes colorées avec icônes
- **Sections** : Répartitions organisées en cartes avec en-têtes
- **Tableau** : Tableau responsive avec hover effects
- **Couleurs** :
  - Primary (bleu) : Total des Repas, Top utilisateurs
  - Success (vert) : Coût Total, Formules
  - Info (bleu clair) : Utilisateurs Actifs, Évolution temporelle
  - Warning (jaune) : Coût Moyen, Top utilisateurs (rang 1)

### Responsive
- Layout en grille Bootstrap (col-md-6, col-md-3)
- Tableau avec scroll horizontal si nécessaire
- Cartes adaptatives selon la taille d'écran

### Interactions
- Hover sur les cartes (effet de translation)
- Filtres de date avec bouton "Filtrer"
- Tableau avec hover sur les lignes

---

## ⚠️ Points importants

### 1. **Anonymisation des utilisateurs**
- Les noms des utilisateurs sont anonymisés pour préserver la confidentialité
- Format : `"Utilisateur {8 premiers caractères du GUID}..."`
- L'ID utilisateur est conservé pour les calculs mais n'est pas affiché

### 2. **Période par défaut**
- La période par défaut suit un cycle de facturation mensuel (17 du mois n-1 au 16 du mois en cours)
- Cette période peut être modifiée via les filtres

### 3. **Calcul des coûts**
- Seuls les points facturables sont inclus dans les calculs de coût
- Les points non facturables (annulés, etc.) ont un coût de 0 FCFA

### 4. **Évolution temporelle**
- Affiche uniquement les **7 derniers jours** de la période
- Permet de voir les tendances récentes sans surcharger l'interface

### 5. **Top 5 Utilisateurs**
- Affiche uniquement les **5 premiers** utilisateurs
- Le tableau détaillé affiche **tous** les utilisateurs

---

## 🔍 Cas d'utilisation

### 1. **Analyse globale des consommations**
Un administrateur veut avoir une vue d'ensemble des consommations sur le cycle de facturation en cours.

### 2. **Identification des tendances**
L'administrateur consulte l'évolution temporelle pour identifier les jours de forte consommation.

### 3. **Analyse des formules populaires**
L'administrateur consulte la répartition par formule pour savoir quelles formules sont les plus demandées.

### 4. **Vérification des statuts**
L'administrateur consulte la répartition par statut pour vérifier la proportion de commandes consommées vs non récupérées.

### 5. **Préparation de rapports**
L'administrateur utilise les statistiques pour préparer des rapports de gestion ou de facturation.

---

## 📝 Notes techniques

### Contrôleur
- **Méthode** : `CumulPointsConsommation(DateTime? dateDebut, DateTime? dateFin)`
- **Autorisation** : `[Authorize(Roles = "Administrateur,RH")]`
- **Logique** : Récupération, groupement, calcul des statistiques

### Méthodes utilitaires
- `CalculerCoutPoint(PointConsommation pc)` : Calcule le coût d'un point de consommation
- `GetPrixFormuleStandard(string nomFormule)` : Retourne le prix unitaire selon le type de formule

### Performance
- Les requêtes utilisent `Include` pour charger les relations nécessaires
- Les groupements et calculs sont effectués en mémoire après récupération des données
- Les données sont triées une seule fois pour optimiser les performances

### Vue
- **Type** : `dynamic` (objet anonyme)
- **Layout** : `_Layout.cshtml`
- **Sections** : Statistiques, Répartitions, Évolution, Top utilisateurs, Tableau détaillé

---

## 🚀 Actions futures possibles

- Export Excel des statistiques
- Graphiques visuels (Chart.js, etc.)
- Filtres supplémentaires (par département, par site, etc.)
- Comparaison avec les périodes précédentes
- Alertes sur les anomalies (forte consommation, etc.)
- Désanonymisation optionnelle pour les administrateurs
- Export PDF des rapports

---

## 📚 Voir aussi

- [Explication des Points de Consommation](./EXPLICATION_POINTS_CONSOMMATION.md)
- [Explication de la Vue Point Consommation CIT](./EXPLICATION_VUE_POINT_CONSOMMATION_CIT.md)
- [Système de Facturation](./Services/FacturationService.cs)
- [Service de Facturation Automatique](./Services/FacturationAutomatiqueService.cs)

