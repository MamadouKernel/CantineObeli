# 📊 Explication du Système de Quotas et Marges

## 📋 Vue d'ensemble

Le système de quotas et marges permet de gérer les commandes instantanées en limitant le nombre de plats disponibles par formule et par période (Jour/Nuit). Ce système empêche la création de commandes instantanées lorsque les quotas sont épuisés.

---

## 🎯 Objectif

Contrôler et limiter le nombre de commandes instantanées pouvant être créées pour chaque formule, en fonction de :
- La période de service (Jour = midi, Nuit = soir)
- L'heure de la journée (avant ou après 18h)
- Les quotas initiaux et les marges disponibles

---

## 🔑 Concepts clés

### 1. **Quotas**
Les quotas représentent le nombre initial de plats disponibles pour chaque période :

- **QuotaJourRestant** : Nombre de plats disponibles pour la période Jour (midi)
- **QuotaNuitRestant** : Nombre de plats disponibles pour la période Nuit (soir)

### 2. **Marges**
Les marges représentent des plats supplémentaires disponibles après épuisement des quotas :

- **MargeJourRestante** : Plats supplémentaires pour la période Jour (indépendante)
- **MargeNuitRestante** : Plats supplémentaires pour la période Nuit (indépendante)

**Important** : Les marges jour et nuit sont **indépendantes** et gérées séparément. Chaque marge peut être définie indépendamment de l'autre, sans dépendre d'une marge totale.

### 3. **Périodes de service**
- **Jour** : Période du midi (de 0h à 17h59)
- **Nuit** : Période du soir (à partir de 18h)

---

## 🔄 Fonctionnement

### 1. **Validation d'une commande par le PrestataireCantine**

Lorsqu'un PrestataireCantine valide une commande, le système décrémente automatiquement les quotas :

#### A. Période Jour (avant 18h)
```
1. Décrémenter d'abord QuotaJourRestant
   ↓
2. Si QuotaJourRestant = 0, décrémenter MargeJourRestante
   ↓
3. Si MargeJourRestante = 0, plus de plats disponibles pour Jour
```

#### B. Période Nuit (à partir de 18h)
```
1. Décrémenter d'abord QuotaNuitRestant
   ↓
2. Si QuotaNuitRestant = 0, décrémenter MargeNuitRestante
   ↓
3. Si MargeNuitRestante = 0, plus de plats disponibles pour Nuit
```

### 2. **Création d'une commande instantanée**

Avant de créer une commande instantanée, le système vérifie les quotas disponibles :

#### A. Vérification pour période Jour (avant 18h)
```
Total disponible = QuotaJourRestant + MargeJourRestante

Si Total disponible > 0 :
   ✅ Commande instantanée autorisée
Sinon :
   ❌ Commande instantanée bloquée jusqu'à 18h
```

#### B. Vérification pour période Nuit (à partir de 18h)
```
Total disponible = QuotaNuitRestant + MargeNuitRestante

Si Total disponible > 0 :
   ✅ Commande instantanée autorisée
Sinon :
   ❌ Commande instantanée bloquée
```

### 3. **Transition Jour → Nuit (18h)**

À 18h, le système passe automatiquement de la période Jour à la période Nuit :
- Les quotas Jour ne sont plus utilisables
- Les quotas Nuit deviennent actifs
- Les commandes instantanées pour Jour sont bloquées
- Les commandes instantanées pour Nuit sont autorisées

---

## 📊 Affichage dans l'interface PrestataireCantine

### Vue "Statistiques par Menu"

La vue affiche pour chaque formule :

#### Colonne "Jour"
- **Badge principal** : `QuotaJourRestant` (quota restant pour le midi)
- **Marge** : `MargeJourRestante` (si > 0, affichée en petit texte)
- **Statut "Épuisé"** : Si `QuotaJourRestant + MargeJourRestante = 0`

#### Colonne "Nuit"
- **Badge principal** : `QuotaNuitRestant` (quota restant pour le soir)
- **Marge** : `MargeNuitRestante` (si > 0, affichée en petit texte)
- **Statut "Épuisé"** : Si `QuotaNuitRestant + MargeNuitRestante = 0`

#### Colonne "Total"
- **Badge** : `(QuotaJourRestant + MargeJourRestante) + (QuotaNuitRestant + MargeNuitRestante)`

---

## 🔢 Gestion des marges

### Principe
Les marges jour et nuit sont **indépendantes** et doivent être définies séparément :

- **MargeJourRestante** : Définie indépendamment pour la période Jour
- **MargeNuitRestante** : Définie indépendamment pour la période Nuit

### Initialisation
- Chaque marge peut être initialisée à 0 ou à une valeur positive
- Les deux marges sont gérées de manière totalement indépendante
- Aucune dépendance entre les marges jour et nuit

---

## 📝 Exemple concret

### Scénario initial
- **Formule** : Standard 1
- **QuotaJourRestant** : 10 plats
- **QuotaNuitRestant** : 8 plats
- **MargeJourRestante** : 2 plats (indépendante)
- **MargeNuitRestante** : 3 plats (indépendante)

### Déroulement

#### 1. Validation de 10 commandes Jour (avant 18h)
- `QuotaJourRestant` : 10 → 0
- `MargeJourRestante` : 2 → 2 (non utilisée)
- **Total disponible Jour** : 0 + 2 = 2 plats

#### 2. Validation de 2 commandes Jour supplémentaires
- `QuotaJourRestant` : 0 (déjà épuisé)
- `MargeJourRestante` : 2 → 0
- **Total disponible Jour** : 0 + 0 = 0 plats
- **Résultat** : ❌ Plus de commandes instantanées Jour possibles jusqu'à 18h

#### 3. À 18h, transition vers Nuit
- Les quotas Nuit deviennent actifs
- `QuotaNuitRestant` : 8 plats
- `MargeNuitRestante` : 2 plats
- **Total disponible Nuit** : 8 + 2 = 10 plats

#### 4. Validation de 8 commandes Nuit
- `QuotaNuitRestant` : 8 → 0
- `MargeNuitRestante` : 2 → 2 (non utilisée)
- **Total disponible Nuit** : 0 + 2 = 2 plats

#### 5. Validation de 2 commandes Nuit supplémentaires
- `QuotaNuitRestant` : 0 (déjà épuisé)
- `MargeNuitRestante` : 2 → 0
- **Total disponible Nuit** : 0 + 0 = 0 plats
- **Résultat** : ❌ Plus de commandes instantanées Nuit possibles

---

## 🚫 Blocage des commandes instantanées

### Conditions de blocage

#### Pour la période Jour (avant 18h)
```
Si (QuotaJourRestant + MargeJourRestante) <= 0 :
   ❌ Commande instantanée bloquée
   Message : "Les quotas pour la période Jour sont épuisés. 
              Impossible de créer une commande instantanée jusqu'à 18h."
```

#### Pour la période Nuit (à partir de 18h)
```
Si (QuotaNuitRestant + MargeNuitRestante) <= 0 :
   ❌ Commande instantanée bloquée
   Message : "Les quotas pour la période Nuit sont épuisés. 
              Impossible de créer une commande instantanée."
```

#### Tentative de créer une commande Nuit avant 18h
```
❌ Commande instantanée bloquée
Message : "Les commandes instantanées pour la période Nuit 
           ne sont disponibles qu'à partir de 18h."
```

---

## 🔧 Implémentation technique

### Modèle FormuleJour

```csharp
public class FormuleJour
{
    // Quotas initiaux
    public int? QuotaJourRestant { get; set; } = 0;
    public int? QuotaNuitRestant { get; set; } = 0;
    
    // Marges disponibles
    public int? MargeJourRestante { get; set; } = 0;
    public int? MargeNuitRestante { get; set; } = 0;
    
    // Marge totale (pour référence)
    public int? Marge { get; set; }
}
```

### Méthode de décrémentation

```csharp
private async Task DecrementerQuotasFormuleAsync(FormuleJour formule, Commande commande)
{
    var maintenant = DateTime.Now;
    var heureActuelle = maintenant.Hour;
    var periodeCommande = commande.PeriodeService;
    var quantite = commande.Quantite;
    
    bool estPeriodeJour = heureActuelle < 18;
    
    if (estPeriodeJour && periodeCommande == Periode.Jour)
    {
        // Décrémenter QuotaJourRestant d'abord
        if (formule.QuotaJourRestant > 0)
        {
            formule.QuotaJourRestant -= quantite;
        }
        // Puis MargeJourRestante si nécessaire
        else if (formule.MargeJourRestante > 0)
        {
            formule.MargeJourRestante -= quantite;
        }
    }
    else if (!estPeriodeJour && periodeCommande == Periode.Nuit)
    {
        // Décrémenter QuotaNuitRestant d'abord
        if (formule.QuotaNuitRestant > 0)
        {
            formule.QuotaNuitRestant -= quantite;
        }
        // Puis MargeNuitRestante si nécessaire
        else if (formule.MargeNuitRestante > 0)
        {
            formule.MargeNuitRestante -= quantite;
        }
    }
}
```

### Méthode de vérification

```csharp
private async Task<(bool Disponible, string Message)> VerifierQuotasDisponiblesAsync(
    FormuleJour formule, Periode periode)
{
    var maintenant = DateTime.Now;
    var heureActuelle = maintenant.Hour;
    var estPeriodeJour = heureActuelle < 18;
    
    if (estPeriodeJour && periode == Periode.Jour)
    {
        var totalDisponible = (formule.QuotaJourRestant ?? 0) + (formule.MargeJourRestante ?? 0);
        if (totalDisponible <= 0)
        {
            return (false, "Les quotas pour la période Jour sont épuisés...");
        }
    }
    else if (!estPeriodeJour && periode == Periode.Nuit)
    {
        var totalDisponible = (formule.QuotaNuitRestant ?? 0) + (formule.MargeNuitRestante ?? 0);
        if (totalDisponible <= 0)
        {
            return (false, "Les quotas pour la période Nuit sont épuisés...");
        }
    }
    else if (estPeriodeJour && periode == Periode.Nuit)
    {
        return (false, "Les commandes instantanées pour la période Nuit ne sont disponibles qu'à partir de 18h.");
    }
    
    return (true, string.Empty);
}
```

---

## 📈 Flux complet

### 1. Initialisation (création/modification d'une formule)
```
Formule créée/modifiée
    ↓
QuotaJourRestant = X (valeur initiale)
QuotaNuitRestant = Y (valeur initiale)
MargeJourRestante = A (valeur indépendante, définie séparément)
MargeNuitRestante = B (valeur indépendante, définie séparément)
```

### 2. Validation d'une commande
```
PrestataireCantine valide une commande
    ↓
Déterminer la période (Jour/Nuit) selon l'heure
    ↓
Décrémenter QuotaJourRestant ou QuotaNuitRestant
    ↓
Si quota = 0, décrémenter MargeJourRestante ou MargeNuitRestante
    ↓
Sauvegarder dans la base de données
```

### 3. Création d'une commande instantanée
```
Utilisateur tente de créer une commande instantanée
    ↓
Vérifier l'heure (avant/après 18h)
    ↓
Vérifier les quotas disponibles
    ↓
Si disponible :
   ✅ Créer la commande
Sinon :
   ❌ Bloquer avec message d'erreur
```

---

## 🎯 Points clés à retenir

1. **Décrémentation automatique** : Les quotas sont décrémentés uniquement lors de la **validation** d'une commande par le PrestataireCantine, pas lors de la création.

2. **Ordre de décrémentation** :
   - D'abord le quota principal (QuotaJourRestant ou QuotaNuitRestant)
   - Ensuite la marge (MargeJourRestante ou MargeNuitRestante)

3. **Transition à 18h** : À 18h, le système passe automatiquement de la période Jour à la période Nuit.

4. **Marge = 0** : Si la marge totale est 0, les marges jour et nuit sont aussi 0.

5. **Blocage** : Les commandes instantanées sont bloquées si le total disponible (quota + marge) est ≤ 0.

6. **Affichage** : La vue PrestataireCantine affiche les quotas restants en temps réel.

---

## 🆘 Dépannage

### Problème : "Les quotas sont toujours à 0"
**Solutions** :
- Vérifier que les quotas initiaux ont été définis lors de la création des formules
- Vérifier que la migration a été appliquée
- Initialiser manuellement les quotas dans la base de données

### Problème : "Les marges ne se décrémentent pas"
**Solutions** :
- Vérifier que la marge totale (Marge) est > 0
- Vérifier que les marges jour/nuit ont été initialisées
- Vérifier que la validation de commande appelle bien `DecrementerQuotasFormuleAsync`

### Problème : "Les commandes instantanées sont toujours bloquées"
**Solutions** :
- Vérifier les quotas disponibles dans la vue "Statistiques par Menu"
- Vérifier l'heure actuelle (avant/après 18h)
- Vérifier que la période de la commande correspond à l'heure

---

**Dernière mise à jour** : Décembre 2024

