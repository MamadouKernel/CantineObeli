# 🛡️ Explication de la Commande Douaniers

## 📋 Vue d'ensemble

La **commande Douaniers** est une fonctionnalité spécialisée qui permet aux **Prestataires de Cantine** de créer des commandes pour le groupe "Douaniers" (groupe non-CIT). Cette fonctionnalité se distingue par :

- **Quotas permanents** : Nombre fixe de plats autorisés par jour (configuré par Admin/RH)
- **Restriction aux plats standard** : Les Douaniers ne peuvent commander que des formules contenant des plats standard
- **Commande instantanée** : Les commandes sont créées pour le jour même uniquement
- **Validation par code** : Chaque commande génère un code unique pour validation

---

## 👥 Qui peut créer une commande Douaniers ?

### Rôles autorisés :
- ✅ **PrestataireCantine** : Accès complet à la création et validation
- ✅ **Administrateur** : Accès complet à la création et validation
- ✅ **RH** : Accès complet à la création et validation

### Rôles non autorisés :
- ❌ **Employé** - ne peut pas créer de commandes Douaniers

---

## 🔑 Caractéristiques principales

### 1. **Type de client**
- **Groupe non-CIT** : "Douaniers"
- Type de commande : `TypeClient = GroupeNonCit`
- Groupe prédéfini dans le système

### 2. **Quotas permanents**
- **Quota Jour** : Nombre maximum de plats pour le service du jour (ex: 50 plats)
- **Quota Nuit** : Nombre maximum de plats pour le service de nuit (ex: 30 plats)
- **Permanent** : Les quotas sont définis une seule fois et s'appliquent chaque jour
- **Modifiable** : Les Admin/RH peuvent ajuster les quotas dans Paramètres → Groupes Non-CIT

### 3. **Restriction aux plats standard**
- Les Douaniers ne peuvent commander que des formules contenant des **plats standard**
- **Les formules améliorées sont exclues** : Elles n'apparaissent pas dans la liste de sélection
- Vérification automatique : La formule doit contenir au moins un plat standard
- Double vérification : Même si une formule améliorée était sélectionnée, elle serait refusée

### 4. **Date de consommation**
- **Jour même uniquement** : Les commandes sont créées pour aujourd'hui
- Pas de commande anticipée possible
- Date de consommation = Date du jour

### 5. **Quantité**
- **Minimum** : 1 plat
- **Maximum** : 100 plats par commande
- **Vérification** : La quantité ne peut pas dépasser le quota restant

### 6. **Code de commande unique**
- Format : `DOU-YYYYMMDD-XXXXXXXX` (ex: `DOU-20241201-ABC12345`)
- Généré automatiquement
- Utilisé pour la validation de la commande

### 7. **Code de vérification**
- Format : `DOU-{Quantite}-{HHmm}` (ex: `DOU-10-1430`)
- Utilisé pour valider la commande lors de la consommation

---

## 📝 Processus de création

### Étape 1 : Accès à la fonctionnalité
1. **Connectez-vous** en tant que **PrestataireCantine**, **Administrateur** ou **RH**
2. **Allez dans** **Commandes** → **Commande des Douaniers**

### Étape 2 : Vérification des quotas
- Le système affiche automatiquement les quotas permanents :
  - **Jour** : X/Y plats (X consommés aujourd'hui, Y quota total)
  - **Nuit** : X/Y plats (X consommés aujourd'hui, Y quota total)
  - **Plats restants** : Calculé automatiquement

### Étape 3 : Sélection de la formule
- **Formules disponibles** : Uniquement les formules du jour (standard uniquement)
- **Filtrage automatique** : 
  - Les formules améliorées sont **exclues** de la liste
  - Seules les formules standard sont proposées
- **Affichage** : Nom de la formule avec détails du menu

### Étape 4 : Configuration de la commande
- **Période de service** :
  - **Jour** : Pour le déjeuner (midi)
  - **Nuit** : Pour le dîner (soir)
- **Site** :
  - **CIT Terminal**
  - **CIT Billing**
- **Quantité** : Nombre de plats (1 à 100, max = quota restant)

### Étape 5 : Validation et création
- **Vérifications automatiques** :
  - ✅ Formule existe et contient des plats standard
  - ✅ Quota disponible suffisant
  - ✅ Quantité dans la plage autorisée (1-100)
  - ✅ Commandes non bloquées
- **Création de la commande** :
  - Code de commande généré
  - Code de vérification généré
  - Commande marquée comme instantanée (`Instantanee = true`)
  - Montant = 0 (plats gratuits pour les Douaniers)

---

## ⚠️ Règles et validations

### 1. **Quotas obligatoires**
- Les quotas doivent être configurés par Admin/RH
- Si aucun quota n'est défini → ❌ Commande refusée
- Message d'erreur : "Aucun quota n'a été défini pour les Douaniers"

### 2. **Respect des quotas**
- **Quota respecté** : Demande ≤ Quota restant → ✅ Commande acceptée
- **Quota dépassé** : Demande > Quota restant → ❌ Commande refusée
- Message d'erreur : "Quota insuffisant pour les Douaniers. Demande: X plats, Disponible: Y plats"

### 3. **Restriction aux plats standard**
- **Les formules améliorées sont exclues** : Elles n'apparaissent pas dans la liste de sélection
- La formule doit contenir au moins un plat standard
- Vérification des champs : `PlatStandard1`, `PlatStandard2`, ou `Plat`
- Si aucun plat standard → ❌ Commande refusée
- Si formule améliorée détectée → ❌ Commande refusée
- Messages d'erreur :
  - "Les formules améliorées ne sont pas autorisées pour les Douaniers"
  - "Cette formule ne contient pas de plats standard"

### 4. **Quantité**
- **Minimum** : 1 plat
- **Maximum** : 100 plats par commande
- **Validation** : Quantité entre 1 et 100

### 5. **Date**
- **Jour même uniquement** : DateConsommation = DateTime.Today
- Pas de commande anticipée possible

### 6. **Blocage des commandes**
- Si les commandes sont bloquées (vendredi 12h, weekend) → ❌ Commande refusée
- Message d'erreur : "Les commandes sont actuellement bloquées"

---

## 🔍 Calcul des quotas

### Quota permanent
```
Quota permanent = Valeur configurée dans GroupeNonCit
- QuotaJournalier : Nombre de plats autorisés pour le jour (permanent)
- QuotaNuit : Nombre de plats autorisés pour la nuit (permanent)
```

### Plats consommés aujourd'hui
```
Plats consommés = Somme des Quantite de toutes les commandes Douaniers
- DateConsommation = Aujourd'hui
- PeriodeService = Jour ou Nuit (selon la période)
- Supprimer = 0 (non supprimées)
```

### Quota restant
```
Quota restant = Quota permanent - Plats consommés aujourd'hui
```

### Exemple
```
Configuration permanente :
- Quota Jour : 50 plats
- Quota Nuit : 30 plats

Aujourd'hui :
- Commandes Jour : 10 + 15 + 5 = 30 plats consommés
- Quota restant Jour : 50 - 30 = 20 plats
- Commandes Nuit : 5 + 10 = 15 plats consommés
- Quota restant Nuit : 30 - 15 = 15 plats

Nouvelle commande :
- Demande : 25 plats (Jour) → ❌ Refusée (25 > 20)
- Demande : 15 plats (Jour) → ✅ Acceptée (15 ≤ 20)
```

---

## 📊 Exemple complet

### Scénario : Création d'une commande Douaniers

```
1. PrestataireCantine/Administrateur/RH accède à "Commande des Douaniers"
2. Vérification des quotas affichés :
   - Jour : 30/50 plats (30 consommés, 50 total, 20 restants)
   - Nuit : 15/30 plats (15 consommés, 30 total, 15 restants)
3. Sélection de la formule : "Formule Standard 1" (contient des plats standard)
4. Configuration :
   - Période : Jour (déjeuner)
   - Site : CIT Terminal
   - Quantité : 15 plats
5. Vérifications automatiques :
   - ✅ Formule existe et contient des plats standard
   - ✅ Quota restant Jour : 20 plats ≥ 15 plats demandés
   - ✅ Quantité dans la plage (1-100)
   - ✅ Commandes non bloquées
6. Clique sur "Créer la commande"
7. Résultat :
   - ✅ Commande créée avec succès
   - Code : DOU-20241201-ABC12345
   - Code vérification : DOU-15-1430
   - Statut : Précommandée
   - Type : GroupeNonCit
   - Quantité : 15 plats
   - Montant : 0 FCFA (gratuit)
   - Groupe : Douaniers
   - Instantanée : Oui
```

---

## ✅ Validation de commande

### Processus de validation
1. **Accès** : Commandes → Validation Douaniers
2. **Saisie du code** : Entrer le code de commande (ex: `DOU-20241201-ABC12345`)
3. **Vérification** :
   - ✅ Code existe
   - ✅ C'est bien une commande Douaniers
   - ✅ Commande non déjà validée
4. **Validation** : Confirmer la consommation
5. **Résultat** : Commande validée et marquée comme consommée

---

## 🔧 Configuration des quotas (Admin/RH)

### Modifier les quotas permanents
1. **Connectez-vous** en tant qu'**Administrateur** ou **RH**
2. **Allez dans** **Paramètres** → **Groupes Non-CIT**
3. **Cliquez sur** l'icône **Modifier** (crayon) du groupe "Douaniers"
4. **Ajustez les quotas** :
   - **Quota Jour** : Nombre de plats pour le service du jour
   - **Quota Nuit** : Nombre de plats pour le service de nuit
   - **Restriction Standard** : Activez pour limiter aux plats standard uniquement
5. **Sauvegardez** les modifications

### Voir les statistiques
1. **Allez dans** **Paramètres** → **Groupes Non-CIT**
2. **Cliquez sur** l'icône **Détails** (œil) du groupe "Douaniers"
3. **Consultez** les statistiques en temps réel :
   - **Quota Total** vs **Plats Consommés**
   - **Plats Restants**
   - **Historique des commandes**

---

## 📊 Tableau comparatif

| **Caractéristique** | **Commande Normale** | **Commande Instantanée** | **Commande Douaniers** |
|---------------------|---------------------|--------------------------|------------------------|
| **Type de client** | Employés CIT | Employés CIT | Groupe non-CIT (Douaniers) |
| **Quantité** | 1 (fixe) | 1 (fixe) | 1-100 (variable) |
| **Date consommation** | Semaine N+1 | Aujourd'hui | Aujourd'hui uniquement |
| **Délai** | 48h avant 12h00 | Aucun délai | Aucun délai |
| **Création par** | Employés, Admin, RH | Admin, Prestataire | Admin, RH, Prestataire |
| **Marqueur** | `Instantanee = false` | `Instantanee = true` | `Instantanee = true` |
| **TypeClient** | CitUtilisateur | CitUtilisateur | GroupeNonCit |
| **Quotas** | Non | Non | Oui (permanents) |
| **Restriction** | Toutes formules | Formules du jour | Plats standard uniquement (améliorées exclues) |
| **Montant** | Selon formule | Selon formule | 0 (gratuit) |
| **Code commande** | CMD-YYYYMMDD-XXXX | CMD-YYYYMMDD-XXXX | DOU-YYYYMMDD-XXXXXXXX |

---

## 🎯 Points clés à retenir

1. **Quotas permanents** : Définis une seule fois, s'appliquent chaque jour automatiquement
2. **Restriction standard** : Seules les formules avec plats standard sont autorisées
3. **Jour même uniquement** : Pas de commande anticipée possible
4. **Validation par code** : Chaque commande génère un code unique pour validation
5. **Gratuit** : Les Douaniers ne paient pas (Montant = 0)
6. **Rôles autorisés** : PrestatairesCantine, Administrateurs et RH peuvent créer des commandes Douaniers
7. **Quotas stricts** : Impossible de dépasser le quota restant

---

## 🆘 Dépannage

### Problème : "Aucun quota défini"
**Solution** : Configurer les quotas permanents via Paramètres → Groupes Non-CIT → Modifier "Douaniers"

### Problème : "Quota insuffisant"
**Solution** : 
- Vérifier les quotas restants affichés
- Réduire la quantité demandée
- Ou augmenter le quota permanent via Paramètres → Groupes Non-CIT

### Problème : "Cette formule ne contient pas de plats standard"
**Solution** : Sélectionner une autre formule qui contient des plats standard

### Problème : "Groupe Douaniers introuvable"
**Solution** : Le groupe est créé automatiquement au démarrage de l'application. Si absent, contacter l'équipe technique.

### Problème : "Les commandes sont actuellement bloquées"
**Solution** : Attendre la réouverture (après vendredi 12h ou weekend)

---

## 📞 Support

En cas de problème, vérifiez :
1. Les logs de l'application
2. La configuration des quotas dans Paramètres → Groupes Non-CIT
3. Les formules disponibles pour aujourd'hui
4. Les quotas restants affichés sur l'interface

---

**Dernière mise à jour** : Décembre 2024

