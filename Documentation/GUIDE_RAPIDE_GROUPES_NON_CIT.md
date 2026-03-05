# 🚀 Guide Rapide: Groupes Non-CIT

## ✅ OUI, c'est implémenté !

---

## 📍 Comment y accéder ?

### Méthode 1: Via le Menu
```
Menu → Paramètres → Groupes Non-CIT
```

### Méthode 2: URL Directe
```
https://localhost:7021/GroupeNonCit/Index
```

---

## 👥 Qui peut l'utiliser ?

- ✅ **Administrateur**
- ✅ **RH (Ressources Humaines)**
- ❌ Utilisateur normal
- ❌ Prestataire

---

## ⚡ Actions Rapides

### 1️⃣ Créer un Groupe

```
1. Cliquez sur "Créer un nouveau groupe"
2. Remplissez:
   - Nom: "Douaniers"
   - Quota Journalier: 50
   - Quota Nuit: 20
3. Cliquez "Créer"
```

**Temps:** 30 secondes

---

### 2️⃣ Modifier les Quotas

```
1. Trouvez le groupe dans la liste
2. Cliquez "Modifier"
3. Changez les quotas
4. Cliquez "Enregistrer"
```

**Temps:** 20 secondes

---

### 3️⃣ Voir les Statistiques

```
1. Trouvez le groupe dans la liste
2. Cliquez "Détails"
3. Consultez:
   - Plats consommés aujourd'hui
   - Quota restant
```

**Temps:** 10 secondes

---

## 📊 Exemple Concret

### Créer un Groupe "Douaniers"

**Formulaire:**
```
┌─────────────────────────────────────┐
│ Nom: Douaniers                      │
│ Description: Personnel douanier     │
│ Code Groupe: DOU                    │
│ Quota Journalier: 50                │
│ Quota Nuit: 20                      │
│ ☑ Restriction Formule Standard      │
│                                     │
│ [Créer]  [Annuler]                  │
└─────────────────────────────────────┘
```

**Résultat:**
- ✅ 50 repas maximum par jour
- ✅ 20 repas maximum par nuit
- ✅ Formules standard uniquement

---

## 🎯 Cas d'Usage Fréquents

### Cas 1: Augmenter les Quotas

**Situation:** Les douaniers ont besoin de plus de repas

**Solution:**
```
1. Modifier le groupe "Douaniers"
2. Quota Journalier: 50 → 75
3. Enregistrer
```

---

### Cas 2: Créer un Groupe VIP

**Situation:** Accueillir des visiteurs officiels

**Solution:**
```
1. Créer "Visiteurs VIP"
2. Quota Journalier: 30
3. Quota Nuit: 10
4. ☐ Pas de restriction (toutes formules)
```

---

### Cas 3: Désactiver un Groupe

**Situation:** Un groupe n'est plus utilisé

**Solution:**
```
1. Vérifier qu'il n'a pas de commandes
2. Cliquer "Supprimer"
3. Confirmer
```

---

## 🔢 Champs du Formulaire

| Champ | Obligatoire | Exemple | Description |
|-------|-------------|---------|-------------|
| **Nom** | ✅ Oui | "Douaniers" | Nom du groupe |
| **Description** | ❌ Non | "Personnel douanier" | Description |
| **Code Groupe** | ❌ Non | "DOU" | Code unique |
| **Quota Journalier** | ❌ Non | 50 | Repas/jour |
| **Quota Nuit** | ❌ Non | 20 | Repas/nuit |
| **Restriction** | ❌ Non | ☑ | Formules standard uniquement |

---

## ⚠️ Erreurs Courantes

### ❌ "Un groupe avec ce nom existe déjà"

**Solution:** Choisissez un autre nom

---

### ❌ "Impossible de supprimer le groupe"

**Raison:** Le groupe a des commandes  
**Solution:** Supprimez d'abord les commandes

---

### ❌ "Accès non autorisé"

**Raison:** Vous n'êtes pas Admin ou RH  
**Solution:** Connectez-vous avec le bon compte

---

## 📱 Interface

### Page Liste

```
┌────────────────────────────────────────────────┐
│ Groupes Non-CIT                                │
│                                                │
│ [+ Créer un nouveau groupe]                    │
│                                                │
│ ┌──────────────────────────────────────────┐  │
│ │ Douaniers                                │  │
│ │ Quota Jour: 50 | Quota Nuit: 20          │  │
│ │ [Modifier] [Détails] [Supprimer]         │  │
│ └──────────────────────────────────────────┘  │
│                                                │
│ ┌──────────────────────────────────────────┐  │
│ │ Forces de l'Ordre                        │  │
│ │ Quota Jour: 40 | Quota Nuit: 15          │  │
│ │ [Modifier] [Détails] [Supprimer]         │  │
│ └──────────────────────────────────────────┘  │
└────────────────────────────────────────────────┘
```

---

## 🎓 Formation 5 Minutes

### Étape 1: Accès (30s)
- Connectez-vous comme Admin/RH
- Allez dans Paramètres → Groupes Non-CIT

### Étape 2: Création (2min)
- Cliquez "Créer un nouveau groupe"
- Remplissez le formulaire
- Testez avec un groupe exemple

### Étape 3: Modification (1min)
- Modifiez les quotas d'un groupe
- Enregistrez les changements

### Étape 4: Consultation (1min)
- Consultez les détails d'un groupe
- Vérifiez les statistiques

### Étape 5: Suppression (30s)
- Supprimez un groupe de test
- Comprenez les restrictions

**Total: 5 minutes** ⏱️

---

## 💡 Astuces Pro

### Astuce 1: Quotas Réalistes
```
Historique moyen + 20% de marge
Exemple: 40 repas/jour → Quota: 50
```

### Astuce 2: Codes Courts
```
✅ "DOU", "FO", "SEC"
❌ "DOUANIERS_2024"
```

### Astuce 3: Révision Mensuelle
```
Consultez les statistiques chaque mois
Ajustez les quotas si nécessaire
```

---

## 📞 Support

### Questions Fréquentes

**Q: Puis-je avoir des quotas illimités ?**  
R: Oui, laissez les champs vides (null)

**Q: Les quotas se réinitialisent quand ?**  
R: Chaque jour à minuit

**Q: Puis-je supprimer un groupe avec des commandes ?**  
R: Non, supprimez d'abord les commandes

---

## ✅ Checklist Rapide

- [ ] Je suis connecté comme Admin/RH
- [ ] J'ai accédé à /GroupeNonCit/Index
- [ ] J'ai créé au moins un groupe
- [ ] J'ai défini les quotas
- [ ] J'ai testé la modification
- [ ] J'ai consulté les statistiques

---

## 🎉 C'est Tout !

Vous savez maintenant utiliser la fonctionnalité **Quotas Permanents Groupes Non-CIT**.

**Temps de maîtrise:** 5-10 minutes  
**Difficulté:** ⭐⭐☆☆☆ (Facile)  
**Utilité:** ⭐⭐⭐⭐⭐ (Essentielle)

---

**Besoin d'aide ?** Consultez le guide complet: `GUIDE_QUOTAS_GROUPES_NON_CIT.md`

**Date:** 05/03/2026  
**Version:** 1.0.0  
**Statut:** ✅ PRÊT À L'EMPLOI
