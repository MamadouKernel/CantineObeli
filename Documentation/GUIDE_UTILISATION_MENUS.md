# Guide d'Utilisation - Gestion des Menus

## 🎯 Vue d'Ensemble

Le module de gestion des menus permet de créer, modifier, consulter et gérer l'historique des formules de repas pour la cantine.

---

## 📍 Accès au Module

**Navigation :** Menu principal > **FormuleJour** ou **Gestion des Formules**

**Rôles autorisés :**
- Administrateur
- Ressources Humaines
- Prestataire

---

## 🆕 Créer un Nouveau Menu

### Méthode 1 : Création Unitaire

1. Cliquer sur **Nouvelle Formule**
2. Remplir les informations :
   - **Date** : Date du menu (obligatoire)
   - **Nom de la formule** : Ex: "Menu du jour", "Formule Améliorée"
   - **Type de formule** : Sélectionner dans la liste (optionnel)
   - **Verrouillé** : Cocher pour empêcher les modifications

3. **Formule Améliorée** (optionnel) :
   - Entrée : Ex: "Salade verte"
   - Plat : Ex: "Poulet rôti"
   - Garniture : Ex: "Riz pilaf"
   - Dessert : Ex: "Fruit de saison"

4. **Formule Standard 1** (optionnel) :
   - Plat : Ex: "Sauce graine"
   - Garniture : Ex: "Viande de bœuf"

5. **Formule Standard 2** (optionnel) :
   - Plat : Ex: "Attieke"
   - Garniture : Ex: "Poisson grillé"

6. **Éléments Communs** (optionnel) :
   - Féculent : Ex: "Riz"
   - Légumes : Ex: "Légumes de saison"
   - Marge : 0-100%
   - Statut : Active/Inactive

7. **Historique** : Notes ou commentaires (optionnel)

8. Cliquer sur **Créer la formule**

---

### Méthode 2 : Création en Lot

**Idéal pour créer les menus d'une semaine ou d'un mois**

1. Cliquer sur **Création en Lot**
2. Définir la période :
   - **Date de début** : Premier jour
   - **Date de fin** : Dernier jour
   - ✅ **Exclure les weekends** : Cocher pour ignorer samedi/dimanche

3. Remplir les informations communes (identiques pour tous les jours)
4. Options :
   - ✅ **Remplacer les formules existantes** : Cocher pour écraser les menus existants

5. Cliquer sur **Créer les formules**

**Résultat :** Un menu sera créé pour chaque jour de la période avec les mêmes informations.

---

### Méthode 3 : Import Excel

**Idéal pour importer plusieurs menus différents**

1. Cliquer sur **Importer**
2. **Télécharger le modèle Excel** (première fois)
3. Remplir le fichier Excel :
   - Une ligne = un menu
   - Colonnes obligatoires : Date, NomFormule
   - Autres colonnes : Entree, Plat, Garniture, Dessert, PlatStandard1, etc.

4. Uploader le fichier rempli
5. Options :
   - **Date de début/fin** : Filtrer les menus à importer (optionnel)
   - ✅ **Remplacer les formules existantes**
   - ✅ **Ignorer les erreurs** : Continuer même si certaines lignes ont des erreurs

6. Cliquer sur **Importer**

**Format du fichier Excel :**
```
Date       | NomFormule        | Entree          | Plat           | ...
2024-01-15 | Formule Améliorée | Salade verte    | Poulet rôti    | ...
2024-01-16 | Formule Standard  |                 |                | ...
```

---

## ✏️ Modifier un Menu

1. Dans la liste des menus, cliquer sur l'icône **Modifier** (crayon) 📝
2. Modifier les champs souhaités
3. Cliquer sur **Enregistrer les modifications**

**Note :** Si le menu est verrouillé, décocher "Verrouillé" pour permettre les modifications futures.

---

## 👁️ Consulter les Détails d'un Menu

1. Dans la liste des menus, cliquer sur l'icône **Détails** (œil) 👁️
2. Consulter toutes les informations :
   - Informations générales
   - Formule Améliorée
   - Formules Standard 1 et 2
   - Éléments communs
   - Historique des modifications
   - Traçabilité (créé par, modifié par)

3. Actions disponibles :
   - **Modifier** : Accès direct à la modification
   - **Supprimer** : Supprimer le menu (avec confirmation)
   - **Retour à la liste** : Retour à la liste des menus

---

## 🗑️ Supprimer un Menu

1. Dans la liste des menus, cliquer sur l'icône **Supprimer** (poubelle) 🗑️
2. Confirmer la suppression

**⚠️ Attention :**
- Si des commandes sont liées à ce menu, la suppression sera refusée
- La suppression est logique (soft delete), le menu reste en base de données

---

## 📅 Consulter les Menus de la Semaine N+1

**Pour planifier les menus de la semaine suivante**

1. Cliquer sur **Semaine N+1**
2. Les menus de la semaine suivante s'affichent automatiquement
3. Créer les menus manquants si nécessaire

---

## 📜 Consulter l'Historique des Menus

**Pour voir tous les menus passés et futurs**

1. Cliquer sur **Historique**
2. Utiliser les filtres :
   - **Date de début** : Filtrer à partir d'une date
   - **Date de fin** : Filtrer jusqu'à une date
   - **Nom de formule** : Rechercher par nom

3. Consulter les statistiques :
   - Total de menus
   - Formules Améliorées
   - Formules Standard
   - Menus Verrouillés

4. Parcourir la timeline des menus
5. Cliquer sur **Détails** ou **Modifier** pour chaque menu

---

## 🔍 Filtrer les Menus

Dans la liste principale :

1. Utiliser les filtres :
   - **Date de début** : Afficher les menus à partir de cette date
   - **Date de fin** : Afficher les menus jusqu'à cette date

2. Cliquer sur **Filtrer**
3. Cliquer sur **Effacer** pour réinitialiser les filtres

---

## 💡 Conseils et Bonnes Pratiques

### Planification
- ✅ Créer les menus de la semaine N+1 avant le vendredi
- ✅ Utiliser la création en lot pour gagner du temps
- ✅ Vérifier les menus avant de les verrouiller

### Organisation
- ✅ Utiliser des noms de formule cohérents : "Formule Améliorée", "Formule Standard"
- ✅ Remplir l'historique pour noter les changements importants
- ✅ Verrouiller les menus validés pour éviter les modifications accidentelles

### Import Excel
- ✅ Télécharger le modèle Excel pour voir le format attendu
- ✅ Vérifier les dates (format YYYY-MM-DD)
- ✅ Tester avec quelques lignes avant d'importer un gros fichier
- ✅ Cocher "Ignorer les erreurs" pour importer les lignes valides même si certaines ont des erreurs

### Gestion des Erreurs
- ❌ **"Une formule existe déjà pour cette date"** : Cocher "Remplacer les formules existantes" ou choisir une autre date
- ❌ **"Impossible de supprimer cette formule car elle est liée à des commandes"** : Le menu ne peut pas être supprimé, le modifier à la place
- ❌ **"Le fichier doit être au format Excel"** : Vérifier que le fichier est .xlsx ou .xls

---

## 📊 Exemples d'Utilisation

### Exemple 1 : Créer les menus d'une semaine

**Scénario :** Créer les menus du lundi 15 au vendredi 19 janvier 2024

1. Cliquer sur **Création en Lot**
2. Date de début : 15/01/2024
3. Date de fin : 19/01/2024
4. Cocher "Exclure les weekends"
5. Remplir les informations communes
6. Cliquer sur **Créer les formules**

**Résultat :** 5 menus créés (lundi à vendredi)

---

### Exemple 2 : Importer les menus d'un mois

**Scénario :** Importer 30 menus différents depuis Excel

1. Télécharger le modèle Excel
2. Remplir 30 lignes avec les menus du mois
3. Uploader le fichier
4. Cocher "Ignorer les erreurs"
5. Cliquer sur **Importer**

**Résultat :** Les menus valides sont importés, les erreurs sont listées

---

### Exemple 3 : Modifier un menu de la semaine N+1

**Scénario :** Changer le plat du mardi prochain

1. Cliquer sur **Semaine N+1**
2. Trouver le menu du mardi
3. Cliquer sur **Modifier**
4. Changer le plat
5. Cliquer sur **Enregistrer les modifications**

**Résultat :** Le menu du mardi est mis à jour

---

## 🆘 Support

En cas de problème :
1. Vérifier les messages d'erreur affichés
2. Consulter ce guide
3. Contacter l'administrateur système

---

## 📝 Résumé des Actions Rapides

| Action | Bouton | Icône |
|--------|--------|-------|
| Créer un menu | Nouvelle Formule | ➕ |
| Créer en lot | Création en Lot | 📅 |
| Importer | Importer | 📥 |
| Voir détails | Détails | 👁️ |
| Modifier | Modifier | ✏️ |
| Supprimer | Supprimer | 🗑️ |
| Historique | Historique | 📜 |
| Semaine N+1 | Semaine N+1 | 📆 |

---

**Version :** 1.0  
**Date :** 5 février 2026  
**Statut :** ✅ Complet
