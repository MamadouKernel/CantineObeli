# 💡 Proposition : Calcul Automatique des Marges

## 📋 Vue d'ensemble

Cette proposition décrit une fonctionnalité permettant de calculer automatiquement les marges de facturation en se basant sur :
1. **Historique des commandes** (semaines précédentes)
2. **Pourcentage configurable** (ex: 10%, 15%, 20%)
3. **Période de référence** (dernières 4 semaines, même jour de la semaine, etc.)

**Statut** : 🔴 Non implémenté (Proposition uniquement)

---

## 🎯 Objectifs

- Automatiser le calcul des marges jour et nuit
- Réduire le temps de configuration manuelle
- Baser les marges sur des données réelles (historique)
- Offrir plusieurs méthodes de calcul selon les besoins
- Permettre un aperçu avant application

---

## 🔧 Trois Options de Calcul Proposées

### Option 1 : Calcul basé sur l'historique récent (RECOMMANDÉ)

**Principe** :
- Analyser les commandes des 4 dernières semaines pour la même formule/jour
- Calculer la moyenne des commandes
- Appliquer un pourcentage de marge (ex: 15%)

**Exemple concret** :
```
Formule Améliorée - Lundi :
- Semaine -4 : 50 commandes
- Semaine -3 : 55 commandes
- Semaine -2 : 48 commandes
- Semaine -1 : 52 commandes

Moyenne = (50 + 55 + 48 + 52) / 4 = 51.25 ≈ 51
Marge 15% = 51 × 0.15 = 7.65 ≈ 8 plats supplémentaires
```

**Avantages** :
- ✅ Basé sur des données réelles
- ✅ S'adapte aux variations saisonnières
- ✅ Prend en compte les tendances récentes

**Inconvénients** :
- ❌ Nécessite un historique suffisant
- ❌ Ne fonctionne pas pour les nouvelles formules


### Option 2 : Calcul basé sur les quotas définis

**Principe** :
- Utiliser les quotas jour/nuit comme base
- Appliquer un pourcentage de marge

**Exemple concret** :
```
Quota Jour = 50
Marge 10% = 50 × 0.10 = 5 plats supplémentaires

Quota Nuit = 30
Marge 10% = 30 × 0.10 = 3 plats supplémentaires
```

**Avantages** :
- ✅ Simple et rapide
- ✅ Fonctionne même sans historique
- ✅ Basé sur les capacités définies

**Inconvénients** :
- ❌ Ne tient pas compte de la demande réelle
- ❌ Peut être imprécis si les quotas sont mal calibrés

---

### Option 3 : Calcul intelligent avec tendances (AVANCÉ)

**Principe** :
- Analyser les tendances (croissance/décroissance)
- Identifier les jours à forte demande
- Ajuster automatiquement les marges

**Exemple concret** :
```
Analyse des 8 dernières semaines :
- Tendance : +15% de croissance
- Jour : Lundi (forte demande)
→ Marge automatique : 20%

- Tendance : Stable
- Jour : Mercredi (demande moyenne)
→ Marge automatique : 15%

- Tendance : -10% de décroissance
- Jour : Vendredi (faible demande)
→ Marge automatique : 10%
```

**Avantages** :
- ✅ Très précis
- ✅ S'adapte automatiquement aux tendances
- ✅ Optimise les marges par jour

**Inconvénients** :
- ❌ Plus complexe à implémenter
- ❌ Nécessite beaucoup de données historiques

---

## 🎨 Proposition d'Interface Utilisateur

### Écran 1 : Configuration du Calcul Automatique

```
┌─────────────────────────────────────────────────────┐
│  Calcul Automatique des Marges                      │
├─────────────────────────────────────────────────────┤
│                                                      │
│  Période cible :                                     │
│  [22/03/2026] au [28/03/2026]                       │
│                                                      │
│  Méthode de calcul :                                │
│  ○ Basé sur l'historique (4 dernières semaines)     │
│  ● Basé sur les quotas définis                      │
│  ○ Calcul intelligent avec tendances                │
│                                                      │
│  Pourcentage de marge :                             │
│  Jour : [15] %    Nuit : [10] %                     │
│                                                      │
│  Options avancées :                                 │
│  ☑ Arrondir à l'entier supérieur                    │
│  ☑ Marge minimale : [2] plats                       │
│  ☑ Marge maximale : [20] plats                      │
│  ☐ Appliquer uniquement aux jours ouvrables         │
│                                                      │
│  [Calculer les Marges]  [Annuler]                   │
│                                                      │
└─────────────────────────────────────────────────────┘
```

### Écran 2 : Aperçu avant Application

```
┌─────────────────────────────────────────────────────────────────────┐
│  Aperçu des Marges Calculées                                        │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│  Date       Formule        Commandes  Quota  Marge    Marge         │
│                            Moyennes   Actuel Jour     Nuit          │
│  ────────────────────────────────────────────────────────────────   │
│  22/03  Amélioré          51         50     8 (15%)   5 (10%)      │
│  22/03  Standard 1        35         30     5 (15%)   3 (10%)      │
│  22/03  Standard 2        28         25     4 (15%)   3 (10%)      │
│  23/03  Amélioré          48         50     7 (15%)   5 (10%)      │
│  23/03  Standard 1        32         30     5 (15%)   3 (10%)      │
│  23/03  Standard 2        26         25     4 (15%)   3 (10%)      │
│  ...                                                                 │
│                                                                      │
│  Total marges : Jour = 156 plats | Nuit = 98 plats                 │
│                                                                      │
│  [Appliquer les Marges]  [Modifier]  [Annuler]                     │
│                                                                      │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 📊 Algorithmes Proposés

### Algorithme 1 : Basé sur l'historique

```csharp
/// <summary>
/// Calcule la marge automatique basée sur l'historique des commandes
/// </summary>
/// <param name="idFormule">ID de la formule</param>
/// <param name="dateFormule">Date de la formule</param>
/// <param name="periode">Période (Jour/Nuit)</param>
/// <param name="pourcentage">Pourcentage de marge à appliquer</param>
/// <returns>Marge calculée</returns>
public async Task<int> CalculerMargeAutomatique(
    Guid idFormule, 
    DateTime dateFormule, 
    Periode periode, 
    decimal pourcentage)
{
    // 1. Récupérer les commandes des 4 dernières semaines
    var dateDebut = dateFormule.AddDays(-28);
    var dateFin = dateFormule.AddDays(-1);
    
    var commandesHistorique = await _context.Commandes
        .Where(c => c.IdFormule == idFormule &&
                   c.DateConsommation >= dateDebut &&
                   c.DateConsommation <= dateFin &&
                   c.PeriodeService == (int)periode &&
                   c.Supprimer == 0 &&
                   c.StatusCommande == (int)StatutCommande.Precommander)
        .GroupBy(c => c.DateConsommation.Value.Date)
        .Select(g => g.Count())
        .ToListAsync();
    
    // 2. Calculer la moyenne
    if (!commandesHistorique.Any())
        return 0; // Pas d'historique
    
    var moyenne = commandesHistorique.Average();
    
    // 3. Appliquer le pourcentage
    var marge = (int)Math.Ceiling(moyenne * (pourcentage / 100));
    
    // 4. Appliquer les limites
    marge = Math.Max(2, marge); // Minimum 2
    marge = Math.Min(20, marge); // Maximum 20
    
    return marge;
}
```

### Algorithme 2 : Basé sur les quotas

```csharp
/// <summary>
/// Calcule la marge automatique basée sur les quotas définis
/// </summary>
/// <param name="quota">Quota actuel</param>
/// <param name="pourcentage">Pourcentage de marge à appliquer</param>
/// <returns>Marge calculée</returns>
public int CalculerMargeDepuisQuota(int quota, decimal pourcentage)
{
    if (quota <= 0)
        return 0;
    
    var marge = (int)Math.Ceiling(quota * (pourcentage / 100));
    
    // Appliquer les limites
    marge = Math.Max(2, marge); // Minimum 2
    marge = Math.Min(20, marge); // Maximum 20
    
    return marge;
}
```

### Algorithme 3 : Calcul intelligent avec tendances

```csharp
/// <summary>
/// Calcule la marge automatique avec analyse des tendances
/// </summary>
/// <param name="idFormule">ID de la formule</param>
/// <param name="dateFormule">Date de la formule</param>
/// <param name="periode">Période (Jour/Nuit)</param>
/// <returns>Marge calculée intelligemment</returns>
public async Task<int> CalculerMargeIntelligente(
    Guid idFormule, 
    DateTime dateFormule, 
    Periode periode)
{
    // 1. Analyser les 8 dernières semaines
    var dateDebut = dateFormule.AddDays(-56);
    
    var commandesParSemaine = await _context.Commandes
        .Where(c => c.IdFormule == idFormule &&
                   c.DateConsommation >= dateDebut &&
                   c.DateConsommation < dateFormule &&
                   c.PeriodeService == (int)periode &&
                   c.Supprimer == 0)
        .GroupBy(c => EF.Functions.DateDiffWeek(dateDebut, c.DateConsommation.Value))
        .Select(g => new { Semaine = g.Key, Total = g.Count() })
        .OrderBy(x => x.Semaine)
        .ToListAsync();
    
    if (commandesParSemaine.Count < 2)
        return 0;
    
    // 2. Calculer la tendance (croissance/décroissance)
    var derniereSemaine = commandesParSemaine.Last().Total;
    var moyenne = commandesParSemaine.Average(x => x.Total);
    
    // 3. Ajuster le pourcentage selon la tendance
    decimal pourcentage;
    if (derniereSemaine > moyenne * 1.2m)
        pourcentage = 20m; // Forte croissance → Marge 20%
    else if (derniereSemaine > moyenne)
        pourcentage = 15m; // Croissance modérée → Marge 15%
    else if (derniereSemaine < moyenne * 0.8m)
        pourcentage = 10m; // Décroissance → Marge 10%
    else
        pourcentage = 12m; // Stable → Marge 12%
    
    // 4. Calculer la marge
    var marge = (int)Math.Ceiling(moyenne * (pourcentage / 100));
    
    return Math.Max(2, Math.Min(20, marge));
}
```

---

## 🔄 Workflow Utilisateur Proposé

### Étape par Étape

```
1. Menu → Gestion des Marges
   ↓
2. Sélectionner période (ex: semaine prochaine)
   ↓
3. Cliquer sur "Calcul Automatique" (nouveau bouton)
   ↓
4. Configurer :
   - Méthode de calcul (Historique/Quotas/Intelligent)
   - Pourcentage jour/nuit
   - Options avancées (min/max, arrondi)
   ↓
5. Cliquer sur "Calculer"
   ↓
6. Aperçu des marges calculées avec détails :
   - Commandes moyennes
   - Quotas actuels
   - Marges proposées
   ↓
7. Options :
   - Appliquer tout automatiquement
   - Modifier manuellement certaines valeurs
   - Annuler et recommencer
   ↓
8. Confirmation et sauvegarde
   ↓
9. Message de succès avec statistiques
```

---

## 📋 Fonctionnalités Additionnelles Proposées

### 1. Sauvegarde des Paramètres Préférés

Permettre de sauvegarder les paramètres de calcul pour réutilisation :

```csharp
public class ParametresCalculMarges
{
    public Guid Id { get; set; }
    public string Nom { get; set; } // Ex: "Configuration Standard"
    public MethodeCalcul Methode { get; set; } // Historique/Quotas/Intelligent
    public decimal PourcentageJour { get; set; } = 15m;
    public decimal PourcentageNuit { get; set; } = 10m;
    public int MargeMinimale { get; set; } = 2;
    public int MargeMaximale { get; set; } = 20;
    public bool ArrondirSuperieur { get; set; } = true;
    public bool JoursOuvrablesUniquement { get; set; } = false;
    public Guid UtilisateurId { get; set; }
    public DateTime CreatedOn { get; set; }
}
```

**Interface** :
```
Paramètres sauvegardés :
- Configuration Standard (15% jour, 10% nuit)
- Configuration Haute Saison (20% jour, 15% nuit)
- Configuration Basse Saison (10% jour, 5% nuit)

[Charger] [Modifier] [Supprimer] [Nouveau]
```

### 2. Historique des Calculs Automatiques

Garder une trace de tous les calculs effectués :

```csharp
public class HistoriqueCalculMarges
{
    public Guid Id { get; set; }
    public DateTime DateCalcul { get; set; }
    public DateTime PeriodeDebut { get; set; }
    public DateTime PeriodeFin { get; set; }
    public MethodeCalcul Methode { get; set; }
    public decimal PourcentageJour { get; set; }
    public decimal PourcentageNuit { get; set; }
    public int NombreFormules { get; set; }
    public int TotalMargesJour { get; set; }
    public int TotalMargesNuit { get; set; }
    public string UtilisateurNom { get; set; }
    public bool Applique { get; set; } // True si appliqué, False si annulé
}
```

**Interface** :
```
┌─────────────────────────────────────────────────────────────────┐
│  Historique des Calculs Automatiques                            │
├─────────────────────────────────────────────────────────────────┤
│  Date        Utilisateur  Méthode      Formules  Marges         │
│  ──────────────────────────────────────────────────────────────  │
│  05/03/2026  Admin        Historique   21        J:156, N:98    │
│  12/03/2026  RH_User      Quotas       21        J:140, N:85    │
│  19/03/2026  Admin        Intelligent  21        J:168, N:105   │
│                                                                  │
│  [Voir Détails] [Réappliquer] [Exporter]                       │
└─────────────────────────────────────────────────────────────────┘
```

### 3. Rapports et Statistiques

Afficher des statistiques sur l'efficacité des marges :

```csharp
public class StatistiquesMarges
{
    public int MargesDefinies { get; set; }
    public int MargesUtilisees { get; set; }
    public decimal TauxUtilisation { get; set; } // %
    public int MargesDepassees { get; set; } // Nombre de fois
    public List<FormuleStatistique> ParFormule { get; set; }
}

public class FormuleStatistique
{
    public string NomFormule { get; set; }
    public DateTime Date { get; set; }
    public int MargeDefinie { get; set; }
    public int MargeUtilisee { get; set; }
    public decimal TauxUtilisation { get; set; }
    public bool Depassee { get; set; }
}
```

**Interface** :
```
┌─────────────────────────────────────────────────────────────────┐
│  Statistiques d'Utilisation des Marges                          │
├─────────────────────────────────────────────────────────────────┤
│  Période : 01/03/2026 - 31/03/2026                              │
│                                                                  │
│  Marges définies : 252 plats                                    │
│  Marges utilisées : 187 plats (74%)                             │
│  Marges dépassées : 3 fois                                      │
│                                                                  │
│  Top 3 formules avec marges dépassées :                         │
│  1. Amélioré - Lundi (3 fois)                                   │
│  2. Standard 1 - Vendredi (2 fois)                              │
│  3. Amélioré - Mercredi (1 fois)                                │
│                                                                  │
│  💡 Recommandations :                                            │
│  - Augmenter marge "Amélioré - Lundi" de 8 à 12 plats          │
│  - Augmenter marge "Standard 1 - Vendredi" de 5 à 8 plats      │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

### 4. Notifications et Alertes

Système d'alertes pour optimiser les marges :

```csharp
public class AlerteMarge
{
    public Guid Id { get; set; }
    public TypeAlerte Type { get; set; } // Depassement/SousUtilisation/Recommandation
    public Guid IdFormule { get; set; }
    public DateTime DateFormule { get; set; }
    public string Message { get; set; }
    public string Recommandation { get; set; }
    public DateTime DateAlerte { get; set; }
    public bool Lue { get; set; }
}

public enum TypeAlerte
{
    Depassement,      // Marge dépassée
    SousUtilisation,  // Marge peu utilisée
    Recommandation    // Suggestion d'ajustement
}
```

**Exemples d'alertes** :
```
⚠️ Dépassement : La formule "Amélioré - Lundi" a dépassé sa marge 3 fois ce mois
   💡 Suggestion : Augmenter la marge de 15% à 20%

ℹ️ Sous-utilisation : La formule "Standard 2 - Mercredi" n'utilise que 30% de sa marge
   💡 Suggestion : Réduire la marge de 10 à 5 plats

📊 Recommandation : Tendance à la hausse détectée pour "Amélioré - Vendredi"
   💡 Suggestion : Passer de 15% à 18% de marge
```

---

## 🔄 Cas d'Usage Pratiques

### Cas 1 : Nouvelle Période (Utilisation Standard)

**Situation** : Définir les marges pour la semaine prochaine

**Solution** :
1. Accéder à "Gestion des Marges"
2. Sélectionner période : 22/03 au 28/03
3. Cliquer sur "Calcul Automatique"
4. Choisir méthode : "Historique"
5. Pourcentage : 15% jour, 10% nuit
6. Cliquer sur "Calculer"
7. Vérifier l'aperçu
8. Cliquer sur "Appliquer"

**Résultat** : Marges définies en 2 minutes au lieu de 15 minutes manuellement

### Cas 2 : Ajustement Saisonnier (Haute Saison)

**Situation** : Période de forte affluence (fin de mois, événements)

**Solution** :
1. Charger paramètres sauvegardés : "Configuration Haute Saison"
2. Méthode : "Calcul Intelligent"
3. Le système détecte automatiquement la croissance
4. Applique 20% au lieu de 15%
5. Aperçu et validation

**Résultat** : Marges adaptées automatiquement aux tendances

### Cas 3 : Nouvelle Formule (Pas d'Historique)

**Situation** : Nouvelle formule ajoutée, pas de données historiques

**Solution** :
1. Méthode : "Basé sur les quotas"
2. Pourcentage conservateur : 10%
3. Aperçu des marges calculées
4. Ajustement manuel si nécessaire
5. Application

**Résultat** : Marges de base définies, ajustables après quelques semaines

### Cas 4 : Optimisation Continue

**Situation** : Améliorer les marges basées sur les statistiques

**Solution** :
1. Consulter "Statistiques d'Utilisation"
2. Identifier les formules avec marges dépassées
3. Relancer calcul automatique avec pourcentages ajustés
4. Appliquer les nouvelles marges

**Résultat** : Optimisation continue basée sur les données réelles

---

## ✅ Avantages de cette Approche

### Gains de Temps
- ⏱️ Réduction de 80% du temps de configuration
- 🚀 Configuration d'une semaine en 2 minutes au lieu de 15 minutes
- 🔄 Réutilisation des paramètres sauvegardés

### Précision
- 📊 Basé sur des données réelles (historique)
- 🎯 Adaptation aux tendances et variations
- 📈 Amélioration continue avec statistiques

### Flexibilité
- 🔧 Plusieurs méthodes de calcul disponibles
- ⚙️ Paramètres personnalisables
- ✏️ Modification manuelle toujours possible

### Contrôle
- 👁️ Aperçu avant application
- 📋 Historique des calculs
- 🔔 Alertes et recommandations

### Traçabilité
- 📝 Historique complet des calculs
- 👤 Utilisateur et date enregistrés
- 📊 Statistiques d'utilisation

---

## 🚀 Plan d'Implémentation Recommandé

### Phase 1 : MVP (Minimum Viable Product)

**Objectif** : Fonctionnalité de base opérationnelle

**Fonctionnalités** :
- ✅ Calcul basé sur l'historique (4 semaines)
- ✅ Pourcentage configurable jour/nuit
- ✅ Aperçu avant application
- ✅ Limites min/max (2-20 plats)
- ✅ Sauvegarde des marges calculées

**Durée estimée** : 2-3 jours de développement

**Fichiers à créer/modifier** :
- `Controllers/GestionMargesController.cs` (ajouter actions)
- `Models/ViewModels/CalculAutomatiqueMargesViewModel.cs` (nouveau)
- `Views/GestionMarges/CalculAutomatique.cshtml` (nouveau)
- `Views/GestionMarges/AperçuMarges.cshtml` (nouveau)

### Phase 2 : Améliorations

**Objectif** : Enrichir l'expérience utilisateur

**Fonctionnalités** :
- ✅ Calcul basé sur quotas
- ✅ Sauvegarde des paramètres préférés
- ✅ Historique des calculs
- ✅ Export des résultats en Excel

**Durée estimée** : 2 jours de développement

**Fichiers à créer/modifier** :
- `Models/ParametresCalculMarges.cs` (nouveau)
- `Models/HistoriqueCalculMarges.cs` (nouveau)
- `Controllers/GestionMargesController.cs` (enrichir)
- `Views/GestionMarges/Historique.cshtml` (nouveau)

### Phase 3 : Fonctionnalités Avancées

**Objectif** : Intelligence et optimisation

**Fonctionnalités** :
- ✅ Calcul intelligent avec tendances
- ✅ Rapports et statistiques d'utilisation
- ✅ Notifications et alertes
- ✅ Recommandations automatiques

**Durée estimée** : 3-4 jours de développement

**Fichiers à créer/modifier** :
- `Models/StatistiquesMarges.cs` (nouveau)
- `Models/AlerteMarge.cs` (nouveau)
- `Services/AnalyseMargesService.cs` (nouveau)
- `Controllers/StatistiquesMargesController.cs` (nouveau)
- `Views/StatistiquesMarges/Index.cshtml` (nouveau)

---

## 📊 Estimation Globale

### Temps de Développement
- **Phase 1 (MVP)** : 2-3 jours
- **Phase 2 (Améliorations)** : 2 jours
- **Phase 3 (Avancé)** : 3-4 jours
- **Total** : 7-9 jours de développement

### Complexité
- **Phase 1** : 🟢 Faible à Moyenne
- **Phase 2** : 🟡 Moyenne
- **Phase 3** : 🔴 Moyenne à Élevée

### Priorité Recommandée
1. **Phase 1** : Haute priorité (gain immédiat)
2. **Phase 2** : Moyenne priorité (confort utilisateur)
3. **Phase 3** : Basse priorité (optimisation)

---

## 💡 Recommandation Finale

Je recommande de commencer par la **Phase 1 (MVP)** avec l'**Option 1 (Historique)** car :

1. ✅ Gain de temps immédiat pour les utilisateurs
2. ✅ Basé sur des données réelles (historique)
3. ✅ Complexité raisonnable
4. ✅ Valeur ajoutée significative
5. ✅ Fondation solide pour les phases suivantes

Les phases 2 et 3 peuvent être ajoutées progressivement selon les retours utilisateurs et les besoins identifiés.

---

## 📝 Notes Importantes

### Prérequis
- Historique de commandes suffisant (minimum 4 semaines)
- Quotas définis pour les formules
- Accès aux rôles Administrateur et RH

### Limitations
- Ne fonctionne pas pour les nouvelles formules sans historique (utiliser Option 2)
- Nécessite une validation manuelle de l'aperçu
- Les marges calculées sont des suggestions, pas des obligations

### Points d'Attention
- Toujours afficher un aperçu avant application
- Permettre la modification manuelle après calcul
- Conserver l'historique des calculs pour audit
- Limiter les marges (min/max) pour éviter les valeurs aberrantes

---

**Date de création** : 05/03/2026  
**Statut** : 🔴 Proposition non implémentée  
**Priorité** : Moyenne  
**Impact** : Élevé (gain de temps significatif)
