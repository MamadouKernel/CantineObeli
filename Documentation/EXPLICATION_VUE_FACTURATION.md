# Explication de la Vue Facturation

## 📋 Vue d'ensemble

La vue `https://localhost:7021/Facturation` est une interface de **gestion manuelle de la facturation** des commandes non consommées. Elle permet aux **Administrateurs et RH** d'analyser et d'appliquer la facturation sur une période donnée.

---

## 🎯 Objectif principal

Cette vue permet de :

1. **Analyser** les commandes non consommées sur une période spécifique
2. **Calculer** automatiquement quelles commandes doivent être facturées selon les règles configurées
3. **Visualiser** les commandes facturables et non facturables avec leurs motifs
4. **Appliquer manuellement** la facturation après validation

---

## 🔐 Accès et autorisations

- **Rôles autorisés** : `Administrateur`, `RH`
- **URL** : `/Facturation?dateDebut=YYYY-MM-DD&dateFin=YYYY-MM-DD`
- **Paramètres** :

  - `dateDebut` : Date de début de la période d'analyse (optionnel, par défaut : 1er jour du mois en cours)
  - `dateFin` : Date de fin de la période d'analyse (optionnel, par défaut : date du jour)

---

## 📊 Structure de la vue

### 1. **En-tête et filtres de période**

```html
<!-- Section de filtrage par période -->
- Date de début : Champ date pour sélectionner le début de la période
- Date de fin : Champ date pour sélectionner la fin de la période
- Bouton "Analyser" : Lance l'analyse des commandes non consommées
```

**Fonctionnement** :

- Par défaut, si aucune date n'est fournie, la période est le mois en cours (du 1er au jour actuel)
- L'utilisateur peut sélectionner une période personnalisée via les champs de date
- Le formulaire envoie une requête GET avec les paramètres `dateDebut` et `dateFin`

---

### 2. **Résumé de la facturation**

Cette section affiche un tableau récapitulatif avec 4 indicateurs :

#### a) Statut de la facturation

- **Activation** : Affiche si la facturation est ACTIVÉE ou DÉSACTIVÉE
- **Couleur** : Vert si activée, rouge si désactivée
- **Source** : Paramètre `FACTURATION_NON_CONSOMMEES_ACTIVE` dans `ConfigurationCommande`

#### b) Commandes facturables

- **Nombre** : Nombre de commandes qui seront facturées
- **Montant total** : Somme de tous les montants à facturer (en FCFA)
- **Calcul** : `MontantOriginal × (PourcentageFacturation / 100)`

#### c) Commandes non facturables

- **Nombre** : Nombre de commandes exemptées de facturation
- **Motif** : Raison de l'exemption (voir section "Règles de facturation")

#### d) Taux de facturation

- **Pourcentage** : Pourcentage du montant original qui sera facturé
- **Absences gratuites** : Nombre d'absences gratuites par utilisateur/mois
- **Source** : Paramètres `FACTURATION_POURCENTAGE` et `FACTURATION_ABSENCES_GRATUITES`

---

### 3. **Règles appliquées**

Cette section liste les règles de facturation actuellement configurées :

- **Pourcentage** : Pourcentage du montant original à facturer
- **Absences gratuites** : Nombre d'absences gratuites par utilisateur et par mois
- **Week-end** : Indique si les commandes du week-end sont facturées ou non
- **Jours fériés** : Indique si les commandes des jours fériés sont facturées ou non

---

### 4. **Tableau des commandes facturables**

Affiche toutes les commandes qui **seront facturées** si l'utilisateur clique sur "Appliquer la Facturation".

**Colonnes** :

- **Code** : Code de la commande + date de commande
- **Utilisateur** : Nom complet + email
- **Date Consommation** : Date prévue de consommation + jour de la semaine + badges (Week-end, Férié)
- **Formule** : Nom de la formule + plat
- **Montant Original** : Montant initial de la commande
- **Montant à Facturer** : Montant calculé selon le pourcentage + pourcentage appliqué
- **Retard** : Nombre de jours depuis la date de consommation

**Style** : Tableau avec en-tête vert (`table-success`)

---

### 5. **Tableau des commandes non facturables**

Affiche toutes les commandes qui **ne seront pas facturées** avec le motif d'exemption.

**Colonnes** :

- **Code** : Code de la commande + date de commande
- **Utilisateur** : Nom complet + email
- **Date Consommation** : Date prévue de consommation + jour de la semaine + badges (Week-end, Férié)
- **Formule** : Nom de la formule + plat
- **Motif** : Raison de l'exemption (badge gris)
- **Retard** : Nombre de jours depuis la date de consommation

**Style** : Tableau avec en-tête jaune (`table-warning`)

**Motifs possibles** :

- `"Facturation désactivée"` : La facturation est désactivée dans les paramètres
- `"Week-end non facturé"` : La commande est un samedi/dimanche et la facturation du week-end est désactivée
- `"Jour férié non facturé"` : La commande est un jour férié et la facturation des jours fériés est désactivée
- `"Absence gratuite (X/Y)"` : L'utilisateur a encore des absences gratuites disponibles (X = absences utilisées, Y = total d'absences gratuites)

---

### 6. **Bouton d'application**

#### a) Si la facturation est activée ET il y a des commandes facturables

- Affiche un bouton vert "Appliquer la Facturation (X commandes)"
- **Action** : Envoie une requête POST à `/Facturation/Appliquer`
- **Confirmation** : Demande confirmation avant d'appliquer ("Êtes-vous sûr de vouloir appliquer la facturation ?")
- **Résultat** : Crée des points de consommation pour chaque commande facturée

#### b) Si la facturation est désactivée

- Affiche un message d'alerte jaune
- Lien vers `/ParametresFacturation` pour activer la facturation

#### c) Si aucune commande facturable

- Le bouton n'apparaît pas

---

## 🔄 Flux de traitement

### Étape 1 : Chargement de la page (GET `/Facturation`)

```csharp
1. Le contrôleur reçoit les paramètres dateDebut et dateFin (ou utilise les valeurs par défaut)
2. Appel à FacturationService.GetCommandesNonConsommeesAsync(dateDebut, dateFin)
   - Recherche toutes les commandes avec :
     * Statut "Précommandée" OU "Consommée"
     * Date de consommation passée (avant aujourd'hui)
     * Type client = CIT Utilisateur
     * Pas de point de consommation (non validée par prestataire)
3. Appel à FacturationService.CalculerFacturationAsync(commandes)
   - Applique les règles de facturation
   - Sépare les commandes facturables et non facturables
4. Affichage du résultat dans la vue
```

### Étape 2 : Application de la facturation (POST `/Facturation/Appliquer`)

```csharp
1. Le contrôleur reçoit les paramètres dateDebut et dateFin
2. Vérification que la facturation est activée
3. Récupération des commandes non consommées
4. Calcul de la facturation
5. Appel à FacturationService.AppliquerFacturationAsync(commandes, resultat)
   - Pour chaque commande facturable :
     * Crée un PointConsommation avec le type "FACTURATION - NON RÉCUPÉRÉE"
     * Le montant facturé est enregistré dans LieuConsommation
     * NE CHANGE PAS le statut de la commande (reste "Précommandée")
   - Pour chaque commande non facturable :
     * Met à jour ModifiedOn et ModifiedBy
     * NE CHANGE PAS le statut de la commande
6. Sauvegarde dans la base de données
7. Redirection vers la page Index avec un message de succès
```

---

## 📐 Règles de facturation (ordre d'application)

Les règles sont appliquées dans l'ordre suivant pour chaque commande :

### 1. Vérification de l'activation

- Si la facturation est désactivée → **Toutes les commandes sont non facturables**

### 2. Vérification du week-end

- Si la commande est un samedi/dimanche ET la facturation du week-end est désactivée → **Non facturable** (motif : "Week-end non facturé")

### 3. Vérification des jours fériés

- Si la commande est un jour férié ET la facturation des jours fériés est désactivée → **Non facturable** (motif : "Jour férié non facturé")

### 4. Gestion des absences gratuites

- Les commandes sont groupées par utilisateur (email)
- Pour chaque utilisateur, les commandes sont triées par date de consommation (plus ancienne en premier)
- Les X premières commandes (où X = nombre d'absences gratuites) sont exemptées → **Non facturable** (motif : "Absence gratuite (X/Y)")
- Les commandes suivantes sont facturables

### 5. Calcul du montant à facturer

- Si la commande est facturable : `MontantAFacturer = MontantOriginal × (PourcentageFacturation / 100)`

---

## 💡 Exemple concret

### Scénario

- **Période** : 1er au 23 décembre 2025
- **Paramètres** :

  - Facturation activée : Oui
  - Pourcentage : 100%
  - Absences gratuites : 2 par utilisateur/mois
  - Week-end : Non facturé
  - Jours fériés : Non facturé

### Commandes trouvées

1. **Commande A** (Jean Dupont, 5 décembre, samedi) → **Non facturable** (Week-end)
2. **Commande B** (Jean Dupont, 10 décembre, mercredi) → **Non facturable** (Absence gratuite 1/2)
3. **Commande C** (Jean Dupont, 15 décembre, lundi) → **Non facturable** (Absence gratuite 2/2)
4. **Commande D** (Jean Dupont, 20 décembre, samedi) → **Non facturable** (Week-end)
5. **Commande E** (Marie Martin, 12 décembre, vendredi) → **Non facturable** (Absence gratuite 1/2)
6. **Commande F** (Marie Martin, 18 décembre, jeudi) → **Non facturable** (Absence gratuite 2/2)
7. **Commande G** (Marie Martin, 22 décembre, lundi) → **Facturable** (100% = 2800 FCFA)

### Résultat

- **Commandes facturables** : 1 (Commande G)
- **Commandes non facturables** : 6
- **Montant total à facturer** : 2800 FCFA

---

## 🔍 Différences avec la facturation automatique

| Aspect | Facturation (Manuelle) | Facturation Automatique |
|--------|------------------------|------------------------|
| **Déclenchement** | Manuel par Admin/RH | Automatique (service en arrière-plan) |
| **Période** | Personnalisable | 7 derniers jours |
| **Validation** | Requiert confirmation | Exécutée automatiquement |
| **Vue** | `/Facturation` | `/FacturationAutomatique` |
| **Contrôle** | Total | Automatique selon planning |

---

## ⚠️ Points importants

1. **Les commandes facturées ne changent pas de statut** :
   - Elles restent "Précommandées" même après facturation
   - Un point de consommation est créé pour tracer la facturation

2. **Les commandes déjà validées sont exclues** :
   - Si une commande a un point de consommation (validée par prestataire), elle n'apparaît pas dans la liste

3. **Seules les commandes CIT sont facturées** :
   - Les commandes des visiteurs, douaniers, etc. ne sont pas incluses

4. **Les absences gratuites sont par utilisateur et par mois** :
   - Chaque utilisateur a son propre quota d'absences gratuites
   - Le quota est réinitialisé chaque mois

5. **Le délai d'annulation gratuite n'est pas utilisé dans cette vue** :
   - Ce paramètre est utilisé lors de l'annulation d'une commande, pas lors de la facturation

---

## 🎨 Éléments visuels

- **Couleurs** :

  - Vert : Commandes facturables, succès
  - Jaune : Commandes non facturables, avertissements
  - Rouge : Erreurs, désactivation
  - Bleu : Informations, filtres

- **Badges** :

  - `bg-warning` : Jour de la semaine
  - `bg-info` : Week-end
  - `bg-danger` : Jour férié
  - `bg-secondary` : Motif d'exemption
  - `bg-danger` : Retard (jours)

---

## 📝 Notes techniques

- **Service utilisé** : `IFacturationService`
- **Modèles** : `FacturationResult`, `CommandeFacturable`, `CommandeNonFacturable`, `CommandeNonConsommeeViewModel`
- **Base de données** : Les points de consommation sont créés dans la table `PointsConsommation`
- **Logs** : Toutes les opérations sont loggées pour traçabilité

---

## 🔗 Liens connexes

- **Paramètres de facturation** : `/ParametresFacturation`
- **Facturation automatique** : `/FacturationAutomatique`
- **Configuration des commandes** : `/ConfigurationCommande`

---

## ✅ Checklist avant d'appliquer la facturation

- [ ] Vérifier que la période sélectionnée est correcte
- [ ] Vérifier que les règles de facturation sont correctement configurées
- [ ] Examiner le nombre de commandes facturables
- [ ] Vérifier le montant total à facturer
- [ ] Examiner les motifs d'exemption pour s'assurer qu'ils sont corrects
- [ ] Confirmer l'application de la facturation

---

*Document créé le : 2025-01-XX*
*Dernière mise à jour : 2025-01-XX*
