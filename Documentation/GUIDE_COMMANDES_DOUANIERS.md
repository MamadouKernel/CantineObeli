# 🛡️ Guide: Workflow des Commandes Douaniers

## Date: 05/03/2026

---

## 🎯 Qu'est-ce qu'une Commande Douaniers?

Les **Commandes Douaniers** sont des commandes spéciales pour le personnel des douanes qui ont des règles particulières:

- ✅ Commandes instantanées (jour même)
- ✅ Quantités multiples autorisées
- ✅ Pas de limitation par période
- ✅ Validation spécifique requise

---

## 👥 Acteurs

### 1. Créateur de Commande
- Administrateur
- RH
- Prestataire Cantine

### 2. Validateur
- Administrateur
- RH
- Prestataire Cantine

### 3. Bénéficiaire
- Personnel des Douanes (groupe spécial)

---

## 📋 Workflow Complet

### Étape 1: Création de la Commande

**Accès:**
```
Menu → Commandes → Commande des Douaniers
```

**Formulaire:**
```
1. Sélectionner la formule du jour
2. Indiquer la quantité (peut être > 1)
3. Choisir la période (Jour/Nuit)
4. Sélectionner le site de livraison
5. Cliquer sur "Créer"
```

**Résultat:**
- Commande créée avec statut "Précommandée"
- Code commande généré automatiquement
- Notification envoyée au prestataire

### Étape 2: Validation de la Commande

**Accès:**
```
Menu → Commandes → Validation Douaniers
```

**Actions possibles:**

#### Option A: Valider
```
1. Trouver la commande dans la liste
2. Vérifier les détails
3. Cliquer sur "Valider"
4. Confirmer
```

**Résultat:**
- Statut passe à "Validée"
- Commande transmise au prestataire
- Notification envoyée

#### Option B: Annuler
```
1. Trouver la commande dans la liste
2. Cliquer sur "Annuler"
3. Saisir le motif d'annulation
4. Confirmer
```

**Résultat:**
- Statut passe à "Annulée"
- Motif enregistré
- Notification envoyée

### Étape 3: Préparation (Prestataire)

**Actions du prestataire:**
```
1. Recevoir la notification
2. Consulter les détails
3. Préparer les repas
4. Marquer comme "En préparation" (optionnel)
```

### Étape 4: Livraison

**Actions:**
```
1. Livrer les repas au site indiqué
2. Faire signer le bon de livraison
3. Marquer comme "Livrée"
```

### Étape 5: Consommation

**Actions:**
```
1. Les douaniers consomment les repas
2. Le prestataire marque comme "Consommée"
3. Points de consommation créés automatiquement
```

---

## 🔄 Schéma du Workflow

```
[Création]
    ↓
[Précommandée] ──→ [Annulée] (avec motif)
    ↓
[Validée]
    ↓
[En Préparation] (optionnel)
    ↓
[Livrée]
    ↓
[Consommée]
```

---

## 📝 Détails des Statuts

### 1. Précommandée
- **Description:** Commande créée, en attente de validation
- **Actions possibles:** Valider, Annuler, Modifier
- **Qui peut agir:** Admin, RH, Prestataire

### 2. Validée
- **Description:** Commande validée, prête pour préparation
- **Actions possibles:** Annuler (avec motif), Marquer en préparation
- **Qui peut agir:** Admin, RH, Prestataire

### 3. En Préparation (optionnel)
- **Description:** Repas en cours de préparation
- **Actions possibles:** Marquer comme livrée
- **Qui peut agir:** Prestataire

### 4. Livrée
- **Description:** Repas livrés au site
- **Actions possibles:** Marquer comme consommée
- **Qui peut agir:** Prestataire

### 5. Consommée
- **Description:** Repas consommés
- **Actions possibles:** Aucune (statut final)
- **Qui peut agir:** Personne (verrouillé)

### 6. Annulée
- **Description:** Commande annulée
- **Actions possibles:** Aucune (statut final)
- **Qui peut agir:** Personne (verrouillé)

---

## 🎯 Règles Spécifiques

### Quantités

- ✅ **Quantités multiples autorisées** (contrairement aux commandes normales)
- ✅ Pas de limite de quantité
- ⚠️ Vérifier la disponibilité avec le prestataire

### Timing

- ✅ **Commandes instantanées** (jour même)
- ✅ Pas de délai de 24h requis
- ⚠️ Sous réserve de disponibilité

### Validation

- ✅ **Validation obligatoire** avant préparation
- ⚠️ Ne pas oublier de valider
- ✅ Notification automatique au prestataire

### Annulation

- ✅ Possible à tout moment avant consommation
- ✅ Motif obligatoire
- ⚠️ Prévenir le prestataire si déjà en préparation

---

## 💡 Cas d'Usage

### Cas 1: Commande Normale

**Situation:** 10 douaniers ont besoin de déjeuner aujourd'hui

**Workflow:**
```
1. Créer commande: Quantité = 10, Période = Jour
2. Valider immédiatement
3. Prestataire prépare
4. Livraison au poste de douane
5. Marquer comme consommée
```

### Cas 2: Commande Urgente

**Situation:** 5 douaniers supplémentaires arrivent à l'improviste

**Workflow:**
```
1. Créer commande: Quantité = 5, Période = Jour
2. Appeler le prestataire pour confirmer disponibilité
3. Valider si OK
4. Préparation express
5. Livraison
```

### Cas 3: Annulation

**Situation:** Mission annulée, les douaniers ne viennent pas

**Workflow:**
```
1. Aller dans "Validation Douaniers"
2. Trouver la commande
3. Cliquer sur "Annuler"
4. Motif: "Mission annulée - Douaniers non présents"
5. Confirmer
6. Appeler le prestataire si déjà en préparation
```

### Cas 4: Modification de Quantité

**Situation:** Besoin de 15 repas au lieu de 10

**Workflow:**
```
1. Annuler la commande initiale
   Motif: "Modification quantité - 15 au lieu de 10"
2. Créer nouvelle commande avec quantité = 15
3. Valider
4. Prévenir le prestataire
```

---

## ⚠️ Points d'Attention

### Avant Création

- ✅ Vérifier la disponibilité des formules du jour
- ✅ Confirmer le nombre exact de douaniers
- ✅ Vérifier le site de livraison
- ✅ Contacter le prestataire si quantité importante

### Après Création

- ✅ **NE PAS OUBLIER DE VALIDER**
- ✅ Vérifier que la notification est partie
- ✅ Confirmer avec le prestataire si nécessaire

### En Cas de Problème

- ✅ Contacter immédiatement le prestataire
- ✅ Annuler si nécessaire avec motif clair
- ✅ Créer une nouvelle commande si besoin

---

## 🚫 Erreurs Courantes

### Erreur 1: Oubli de Validation

**Symptôme:** La commande reste en "Précommandée"

**Impact:** Le prestataire ne reçoit pas la commande

**Solution:**
```
1. Aller dans "Validation Douaniers"
2. Trouver la commande
3. Cliquer sur "Valider"
```

### Erreur 2: Mauvaise Quantité

**Symptôme:** Pas assez ou trop de repas

**Solution:**
```
1. Annuler la commande (motif: "Erreur quantité")
2. Créer nouvelle commande avec bonne quantité
3. Valider immédiatement
4. Prévenir le prestataire
```

### Erreur 3: Mauvais Site

**Symptôme:** Livraison au mauvais endroit

**Solution:**
```
1. Contacter le prestataire IMMÉDIATEMENT
2. Corriger l'adresse de livraison
3. Annuler et recréer si nécessaire
```

### Erreur 4: Doublon

**Symptôme:** Deux commandes pour le même groupe

**Solution:**
```
1. Identifier le doublon
2. Annuler la commande en trop
3. Motif: "Doublon - Commande créée par erreur"
4. Garder une seule commande
```

---

## 📞 Contacts

### En Cas d'Urgence

**Prestataire Cantine:**
- Téléphone: +225 XX XX XX XX
- Email: prestataire@obeli.com

**Administrateur Système:**
- Téléphone: +225 XX XX XX XX
- Email: admin@obeli.com

### Support Technique

- Email: support@obeli.com
- Heures: Lun-Ven 8h-17h

---

## 🎓 Bonnes Pratiques

### 1. Communication

- ✅ Toujours prévenir le prestataire pour les grandes quantités
- ✅ Confirmer la livraison par téléphone
- ✅ Donner un contact sur place

### 2. Anticipation

- ✅ Créer les commandes le plus tôt possible
- ✅ Prévoir une marge sur les quantités
- ✅ Avoir un plan B en cas de problème

### 3. Suivi

- ✅ Vérifier le statut régulièrement
- ✅ Confirmer la livraison
- ✅ Marquer comme consommée rapidement

### 4. Documentation

- ✅ Noter les quantités réelles consommées
- ✅ Documenter les problèmes rencontrés
- ✅ Partager les retours d'expérience

---

## 📊 Statistiques

Les commandes douaniers sont suivies séparément:

- 📈 Nombre de commandes par mois
- 📊 Quantités moyennes
- ⏱️ Délais de validation
- ❌ Taux d'annulation
- ✅ Taux de satisfaction

---

## 🔍 FAQ

**Q: Peut-on commander pour plusieurs jours?**  
R: Non, les commandes douaniers sont uniquement pour le jour même.

**Q: Y a-t-il une limite de quantité?**  
R: Non, mais vérifiez la disponibilité avec le prestataire.

**Q: Qui paie les repas?**  
R: Selon l'accord avec les douanes (à vérifier avec l'administration).

**Q: Peut-on modifier une commande validée?**  
R: Non, il faut annuler et recréer.

**Q: Combien de temps pour préparer?**  
R: Minimum 2h, idéalement 4h pour grandes quantités.

---

**Date:** 05/03/2026  
**Version:** 1.0  
**Auteur:** Équipe O'Beli

