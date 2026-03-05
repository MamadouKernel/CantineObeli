# 📋 Guide: Annuler vs Supprimer une Commande

## Date: 05/03/2026

---

## 🎯 Objectif

Comprendre la différence entre "Annuler" et "Supprimer" une commande dans O'Beli.

---

## 🔄 Annuler une Commande

### Qu'est-ce que c'est?

**Annuler** une commande est un "soft delete" - la commande reste dans la base de données mais est marquée comme annulée.

### Caractéristiques

✅ **Garde l'historique** - La commande reste visible dans l'historique  
✅ **Motif requis** - Vous devez indiquer pourquoi vous annulez  
✅ **Traçabilité** - On peut voir qui a annulé et quand  
✅ **Statistiques** - La commande est comptée dans les statistiques d'annulation  
✅ **Réversible** - Peut potentiellement être réactivée (selon les règles métier)

### Qui peut annuler?

- ✅ Administrateur
- ✅ RH
- ✅ Prestataire Cantine
- ⚠️ Employé (uniquement ses propres commandes, selon délai)

### Quand utiliser?

- Changement de menu par l'utilisateur
- Absence imprévue
- Erreur de commande
- Annulation par le prestataire (rupture de stock, etc.)

### Comment annuler?

1. Aller dans "Commandes" → "Liste des Commandes"
2. Trouver la commande à annuler
3. Cliquer sur "Annuler"
4. **Saisir obligatoirement un motif d'annulation**
5. Confirmer

### Exemple de Motif

```
Motifs valides:
- "Absence pour congé maladie"
- "Changement de menu demandé par l'utilisateur"
- "Rupture de stock - Plat non disponible"
- "Erreur de saisie - Mauvaise date"
```

---

## 🗑️ Supprimer une Commande

### Qu'est-ce que c'est?

**Supprimer** une commande est également un "soft delete" dans O'Beli, mais avec une intention différente.

### Caractéristiques

❌ **Masquée de l'interface** - La commande n'apparaît plus dans les listes normales  
❌ **Pas de motif requis** - Suppression directe  
⚠️ **Traçabilité limitée** - Marquée comme supprimée mais reste en base  
⚠️ **Statistiques** - Exclue des statistiques normales  
❌ **Non réversible** - Ne peut pas être réactivée facilement

### Qui peut supprimer?

- ✅ Administrateur
- ✅ RH
- ❌ Prestataire Cantine (ne peut pas supprimer)
- ❌ Employé (ne peut pas supprimer)

### Quand utiliser?

- Commande créée par erreur (doublon)
- Commande de test
- Nettoyage de données erronées
- Correction d'erreurs administratives

### Comment supprimer?

1. Aller dans "Commandes" → "Liste des Commandes"
2. Trouver la commande à supprimer
3. Cliquer sur "Supprimer"
4. Confirmer (pas de motif requis)

---

## 📊 Tableau Comparatif

| Critère | Annuler | Supprimer |
|---------|---------|-----------|
| **Garde l'historique** | ✅ Oui | ⚠️ Oui mais masqué |
| **Motif requis** | ✅ Oui | ❌ Non |
| **Visible dans les listes** | ✅ Oui (avec statut "Annulée") | ❌ Non |
| **Statistiques** | ✅ Comptée | ❌ Exclue |
| **Réversible** | ✅ Potentiellement | ❌ Non |
| **Qui peut le faire** | Admin, RH, Prestataire, (Employé) | Admin, RH uniquement |
| **Usage typique** | Annulation normale | Correction d'erreur |

---

## 🎯 Règles Métier Importantes

### Commandes Consommées

❌ **Ne peuvent JAMAIS être annulées ou supprimées**  
Une fois qu'une commande est marquée comme "Consommée", elle est verrouillée.

### Commandes Passées

⚠️ **Peuvent être modifiées/annulées/supprimées uniquement par un Administrateur**  
Les commandes dont la date de consommation est passée nécessitent des droits Admin.

### Délai de Modification

⚠️ **Règle des 24h** (selon configuration)  
Les commandes peuvent généralement être modifiées jusqu'à 24h avant la date de consommation.

---

## 💡 Recommandations

### Utilisez "Annuler" quand:

1. ✅ L'utilisateur change d'avis
2. ✅ Le prestataire ne peut pas honorer la commande
3. ✅ Vous voulez garder une trace de l'annulation
4. ✅ Vous voulez des statistiques d'annulation
5. ✅ C'est une annulation "normale" dans le workflow

### Utilisez "Supprimer" quand:

1. ✅ C'est une commande de test
2. ✅ C'est un doublon créé par erreur
3. ✅ C'est une erreur de saisie administrative
4. ✅ Vous voulez nettoyer les données
5. ✅ La commande ne devrait jamais avoir existé

---

## ⚠️ Attention

### Annulation

- Toujours indiquer un motif clair et précis
- Le motif sera visible dans l'historique
- Les statistiques d'annulation sont suivies

### Suppression

- Utilisez avec précaution
- Réservé aux Administrateurs et RH
- Pas de retour en arrière facile
- Peut affecter les statistiques

---

## 📞 Support

En cas de doute:
- Contactez votre Administrateur
- Consultez le service RH
- Préférez "Annuler" si vous n'êtes pas sûr

---

## 🔍 Exemples Pratiques

### Exemple 1: Employé Absent

**Situation:** Un employé est malade et ne viendra pas manger  
**Action:** ✅ **ANNULER** la commande  
**Motif:** "Absence pour congé maladie"  
**Pourquoi:** C'est une annulation normale, on veut garder la trace

### Exemple 2: Doublon

**Situation:** Une commande a été créée deux fois par erreur  
**Action:** ✅ **SUPPRIMER** le doublon  
**Pourquoi:** C'est une erreur, la commande ne devrait pas exister

### Exemple 3: Changement de Menu

**Situation:** L'utilisateur veut changer de formule  
**Action:** ✅ **ANNULER** l'ancienne commande  
**Motif:** "Changement de formule demandé par l'utilisateur"  
**Pourquoi:** C'est une annulation normale, on veut garder la trace

### Exemple 4: Commande de Test

**Situation:** Commande créée pour tester le système  
**Action:** ✅ **SUPPRIMER** la commande de test  
**Pourquoi:** C'est une commande de test, elle ne doit pas apparaître dans les statistiques

---

**Date:** 05/03/2026  
**Version:** 1.0  
**Auteur:** Équipe O'Beli

