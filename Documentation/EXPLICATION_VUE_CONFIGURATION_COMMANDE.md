# ⚙️ Explication : Vue "Configuration des Commandes"

## 📋 Vue d'ensemble

Cette vue permet aux **RH** et **Administrateurs** de configurer les paramètres de clôture et de confirmation automatique des commandes. Elle gère le système de blocage périodique des commandes et la confirmation automatique.

**URL** : `https://localhost:7021/ConfigurationCommande`

---

## 🎯 Objectif

Configurer les paramètres qui déterminent :
- **Quand** les commandes sont bloquées (jour et heure de clôture)
- **Si** les commandes sont automatiquement confirmées à l'heure de clôture
- **Tester** le statut actuel du blocage
- **Forcer** la confirmation automatique manuellement si nécessaire

---

## 🔐 Accès et autorisations

### Rôles autorisés
- ✅ **Administrateur**
- ✅ **RH** (Ressources Humaines)
- ❌ **PrestataireCantine** (non autorisé)
- ❌ **Employé** (non autorisé)

---

## 📊 Structure de la vue

La vue est divisée en **3 sections principales** :

### 1. **Statut actuel des commandes** 🔒

#### Affichage
- **Carte colorée** :
  - 🟢 **Vert** : Commandes **AUTORISÉES** (icône déverrouillée)
  - 🔴 **Rouge** : Commandes **BLOQUÉES** (icône verrouillée)

#### Informations affichées
- **État actuel** : Badge indiquant si les commandes sont "BLOQUÉES" ou "AUTORISÉES"
- **Prochaine clôture** : Date et heure de la prochaine clôture automatique (format : "lundi 25/12/2024 à 12:00")

#### Logique de blocage
Les commandes sont **bloquées** lorsque :
- On est **après** le jour et l'heure de clôture configurés
- Exemple : Si clôture = Vendredi 12:00, les commandes sont bloquées à partir de Vendredi 12:01 jusqu'au prochain jour de clôture

---

### 2. **Paramètres de clôture** ⚙️

#### Formulaire de configuration

##### **Jour de clôture** 📅
- **Type** : Liste déroulante (select)
- **Options** : Lundi, Mardi, Mercredi, Jeudi, Vendredi, Samedi, Dimanche
- **Valeur par défaut** : Vendredi
- **Clé de configuration** : `COMMANDE_JOUR_CLOTURE`
- **Description** : Jour de la semaine où les commandes sont automatiquement bloquées

##### **Heure de clôture** 🕐
- **Type** : Champ de saisie horaire (time input)
- **Format** : HH:mm (ex: 12:00, 16:30)
- **Valeur par défaut** : 12:00
- **Clé de configuration** : `COMMANDE_HEURE_CLOTURE`
- **Description** : Heure exacte du blocage des commandes

##### **Confirmation automatique** ✅
- **Type** : Case à cocher (checkbox)
- **Valeur par défaut** : Activée (true)
- **Clé de configuration** : `COMMANDE_AUTO_CONFIRMATION`
- **Description** : Si activée, les commandes précommandées sont automatiquement confirmées à l'heure de clôture
- **Fonctionnement** : Le service `CommandeAutomatiqueService` vérifie cette configuration et confirme les commandes si activée

#### Boutons d'action
- **💾 Sauvegarder** : Enregistre les modifications dans la base de données
- **↩️ Annuler** : Réinitialise le formulaire aux valeurs actuelles (sans sauvegarder)

---

### 3. **Actions et outils** 🛠️

#### **Tester le blocage** 🔍
- **Bouton** : "Tester le Blocage" (bleu/info)
- **Action** : Vérifie le statut actuel du blocage et affiche un message informatif
- **Résultat** :
  - Si bloqué : "Les commandes sont actuellement BLOQUÉES. Prochaine clôture: [date]"
  - Si autorisé : "Les commandes sont actuellement AUTORISÉES. Prochaine clôture: [date]"

#### **Forcer la confirmation** ⚡
- **Bouton** : "Forcer Confirmation" (jaune/warning)
- **Action** : Exécute manuellement la confirmation automatique des commandes
- **Confirmation requise** : Oui (popup de confirmation)
- **Fonctionnement** : Appelle `CommandeAutomatiqueService.ConfirmerCommandesAutomatiquementAsync()`
- **Résultat** :
  - Succès : "Confirmation automatique des commandes exécutée avec succès."
  - Aucune action : "Aucune commande à confirmer ou conditions non remplies."

#### **Réinitialiser la configuration** 🔄
- **Bouton** : "Réinitialiser" (rouge/danger)
- **Action** : Remet toutes les configurations aux valeurs par défaut
- **Confirmation requise** : Oui (popup de confirmation)
- **Valeurs par défaut** :
  - Jour de clôture : Vendredi
  - Heure de clôture : 12:00
  - Auto-confirmation : true (activée)

#### **Informations** ℹ️
- **Section** : Carte d'informations
- **Contenu** :
  - Explication du "Jour de clôture"
  - Explication de l'"Heure de clôture"
  - Explication de l'"Auto-confirmation"
  - Note : "Les commandes sont autorisées uniquement pour la semaine N+1"

---

## 🔄 Fonctionnement technique

### Stockage des configurations

Les configurations sont stockées dans la table `ConfigurationsCommande` avec les clés suivantes :

| Clé | Description | Valeur par défaut |
|-----|-------------|-------------------|
| `COMMANDE_JOUR_CLOTURE` | Jour de la semaine pour la clôture | "Friday" |
| `COMMANDE_HEURE_CLOTURE` | Heure de clôture (format HH:mm) | "12:00" |
| `COMMANDE_AUTO_CONFIRMATION` | Activation de la confirmation automatique | "true" |

### Calcul de la prochaine clôture

Le système calcule automatiquement la prochaine date de clôture en fonction de :
1. Le jour de clôture configuré
2. L'heure de clôture configurée
3. La date et l'heure actuelles

**Exemple** :
- Configuration : Vendredi 12:00
- Aujourd'hui : Mercredi 10:00
- Prochaine clôture : Vendredi 12:00 (dans 2 jours)

### Vérification du blocage

Le système vérifie si les commandes sont bloquées en comparant :
- La date/heure actuelle
- La date/heure de la dernière clôture
- La date/heure de la prochaine clôture

**Logique** :
- Si `maintenant >= dernière_clôture` ET `maintenant < prochaine_clôture` → **BLOQUÉ**
- Sinon → **AUTORISÉ**

---

## 📝 Exemples d'utilisation

### Scénario 1 : Configurer la clôture hebdomadaire

**Objectif** : Bloquer les commandes tous les vendredis à 16h00

1. Accéder à `/ConfigurationCommande`
2. Sélectionner "Vendredi" dans "Jour de clôture"
3. Saisir "16:00" dans "Heure de clôture"
4. Cliquer sur "Sauvegarder"
5. Le système affiche : "Configuration mise à jour avec succès."

**Résultat** : Les commandes seront bloquées automatiquement tous les vendredis à 16h00.

---

### Scénario 2 : Désactiver la confirmation automatique

**Objectif** : Confirmer manuellement les commandes au lieu de l'automatique

1. Accéder à `/ConfigurationCommande`
2. Décocher "Confirmation automatique des commandes"
3. Cliquer sur "Sauvegarder"

**Résultat** : Les commandes ne seront plus confirmées automatiquement. Il faudra utiliser le bouton "Forcer Confirmation" manuellement.

---

### Scénario 3 : Tester le statut actuel

**Objectif** : Vérifier si les commandes sont actuellement bloquées

1. Accéder à `/ConfigurationCommande`
2. Observer la carte "Statut des Commandes" (vert = autorisé, rouge = bloqué)
3. Cliquer sur "Tester le Blocage" pour obtenir un message détaillé

**Résultat** : Un message s'affiche indiquant le statut actuel et la prochaine clôture.

---

### Scénario 4 : Forcer la confirmation manuellement

**Objectif** : Confirmer immédiatement toutes les commandes précommandées

1. Accéder à `/ConfigurationCommande`
2. Cliquer sur "Forcer Confirmation"
3. Confirmer l'action dans la popup
4. Attendre le message de succès

**Résultat** : Toutes les commandes précommandées éligibles sont confirmées immédiatement.

---

## 🔗 Intégration avec d'autres services

### Service de fermeture automatique

Le service `FermetureAutomatiqueService` utilise ces configurations pour :
- Détecter quand fermer les commandes
- Bloquer la création de nouvelles commandes après la clôture

### Service de confirmation automatique

Le service `CommandeAutomatiqueService` utilise ces configurations pour :
- Vérifier si la confirmation automatique est activée
- Confirmer automatiquement les commandes à l'heure de clôture

### Contrôleur de commandes

Le `CommandeController` utilise ces configurations pour :
- Vérifier si les commandes sont bloquées avant de permettre la création
- Afficher des messages d'avertissement aux utilisateurs

---

## ⚠️ Points importants

### 1. **Blocage automatique**
- Le blocage est vérifié en temps réel lors de chaque tentative de création de commande
- Le statut affiché dans la vue est calculé dynamiquement

### 2. **Confirmation automatique**
- La confirmation automatique ne fonctionne que si elle est activée dans la configuration
- Elle s'exécute via le service `CommandeAutomatiqueService` qui peut être appelé manuellement ou automatiquement

### 3. **Réinitialisation**
- La réinitialisation remet toutes les configurations aux valeurs par défaut
- Cette action est irréversible (sauf nouvelle configuration manuelle)

### 4. **Validation**
- Le jour de clôture doit être un jour de la semaine valide (Monday-Sunday)
- L'heure de clôture doit être au format HH:mm (ex: 12:00, 16:30)

---

## 🐛 Dépannage

### Problème : Les commandes ne sont pas bloquées

**Solutions** :
1. Vérifier que le jour et l'heure de clôture sont correctement configurés
2. Utiliser "Tester le Blocage" pour voir le statut actuel
3. Vérifier que la date/heure du serveur est correcte

### Problème : La confirmation automatique ne fonctionne pas

**Solutions** :
1. Vérifier que "Confirmation automatique" est activée (case cochée)
2. Vérifier que le service `CommandeAutomatiqueService` est en cours d'exécution
3. Utiliser "Forcer Confirmation" pour tester manuellement

### Problème : La prochaine clôture affichée est incorrecte

**Solutions** :
1. Vérifier que le jour et l'heure de clôture sont correctement configurés
2. Vérifier que la date/heure du serveur est correcte
3. Recharger la page pour recalculer

---

## 📚 Références techniques

- **Contrôleur** : `ConfigurationCommandeController`
- **Service** : `IConfigurationService` / `ConfigurationService`
- **Service automatique** : `ICommandeAutomatiqueService` / `CommandeAutomatiqueService`
- **Modèle** : `ConfigurationCommande`
- **Table** : `ConfigurationsCommande`

---

## ✅ Résumé

Cette vue permet de :
- ✅ Configurer le jour et l'heure de clôture des commandes
- ✅ Activer/désactiver la confirmation automatique
- ✅ Voir le statut actuel du blocage
- ✅ Tester le système de blocage
- ✅ Forcer la confirmation manuellement
- ✅ Réinitialiser les configurations par défaut

**Utilisateurs cibles** : RH et Administrateurs uniquement.

