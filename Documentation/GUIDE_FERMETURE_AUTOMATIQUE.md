# 🔒 Guide - Fermeture Automatique des Commandes

## 🎯 Vue d'ensemble

Le système de **fermeture automatique** ferme automatiquement les commandes de la semaine N+1 **le vendredi à 12h00**. Cette fermeture est gérée par le service `FermetureAutomatiqueService` qui s'exécute en arrière-plan.

## ⚙️ Fonctionnement

### 🕐 Déclenchement
- **Jour** : Vendredi
- **Heure** : 12h00
- **Fréquence** : Vérification toutes les 5 minutes
- **Action** : Fermeture automatique des commandes semaine N+1

### 🔄 Processus de Fermeture

1. **Vérification du moment** : Le service vérifie si c'est vendredi 12h
2. **Calcul de la semaine N+1** : Détermine les dates de la semaine suivante
3. **Traitement des commandes** :
   - ✅ **Commandes précommandées** → Passent en "Consommée"
   - 🍽️ **Points de consommation** → Créés automatiquement
   - 📊 **Statistiques** → Mises à jour
4. **Enregistrement** : Marque que la fermeture a été effectuée
5. **Notification** : Logs détaillés de l'opération

## 🔧 Configuration

### Paramètres par Défaut
```
Jour de clôture: Friday (Vendredi)
Heure de clôture: 12:00
Auto-confirmation: true
```

### Modification des Paramètres
1. **Connectez-vous** en tant qu'**Administrateur**
2. **Allez dans** **Paramètres** → **Configuration Commandes**
3. **Modifiez** les paramètres selon vos besoins :
   - `COMMANDE_JOUR_CLOTURE` : Jour de fermeture
   - `COMMANDE_HEURE_CLOTURE` : Heure de fermeture
   - `COMMANDE_AUTO_CONFIRMATION` : Activation auto-confirmation

## 📊 Actions Automatiques

### Commandes Précommandées
- **Statut** : `Precommander` → `Consommee`
- **Point de consommation** : Créé automatiquement
- **Utilisateur** : Assigné selon la commande
- **Lieu** : "Restaurant CIT"

### Points de Consommation
- **Type de formule** : Récupéré depuis la commande
- **Nom du plat** : Extrait de la formule
- **Quantité** : Copiée depuis la commande
- **Date** : Date de consommation de la commande

## 🔍 Monitoring et Logs

### Logs de Démarrage
```
🚀 Service de fermeture automatique démarré
```

### Logs de Fermeture
```
🔒 Début de la fermeture automatique des commandes pour la semaine N+1
✅ Fermeture automatique terminée:
   📊 Commandes confirmées: X
   ❌ Commandes annulées: Y
   📅 Semaine N+1: DD/MM/YYYY au DD/MM/YYYY
```

### Logs de Points de Consommation
```
🍽️ Point de consommation créé pour la commande {ID}: {NomPlat}
```

## 🧪 Test du Système

### Test Manuel
1. **Créez des commandes** pour la semaine N+1
2. **Attendez vendredi 12h** ou modifiez l'heure système
3. **Vérifiez les logs** de l'application
4. **Contrôlez** que les commandes sont confirmées

### Test Automatique
```powershell
# Exécuter le script de test
.\Scripts\TestFermetureAutomatique.ps1
```

## 📋 Vérifications Post-Fermeture

### Dans l'Interface
1. **Commandes** → **Liste des Commandes**
2. **Filtrer** par statut "Consommée"
3. **Vérifier** que les commandes semaine N+1 sont confirmées

### Dans la Base de Données
```sql
-- Vérifier les commandes confirmées
SELECT * FROM Commandes 
WHERE StatusCommande = 2 -- Consommee
AND DateConsommation >= '2024-12-09' -- Lundi semaine N+1
AND DateConsommation <= '2024-12-13'; -- Vendredi semaine N+1

-- Vérifier les points de consommation créés
SELECT * FROM PointsConsommation 
WHERE CreatedBy = 'FermetureAutomatiqueService'
AND CreatedOn >= '2024-12-06'; -- Date de fermeture
```

## 🚨 Dépannage

### Problème : Fermeture non effectuée
**Vérifications :**
1. **Logs de l'application** : Chercher les messages de fermeture
2. **Configuration** : Vérifier les paramètres de clôture
3. **Service** : Vérifier que `FermetureAutomatiqueService` est actif
4. **Base de données** : Chercher `FERMETURE_EFFECTUEE_YYYYMMDD`

### Problème : Commandes non confirmées
**Solutions :**
1. **Vérifier les logs** pour les erreurs
2. **Contrôler** que les commandes existent pour la semaine N+1
3. **Vérifier** que le statut initial est "Précommandée"
4. **Exécuter manuellement** si nécessaire

### Problème : Points de consommation manquants
**Solutions :**
1. **Vérifier** que les formules existent
2. **Contrôler** les logs de création de points
3. **Vérifier** que les commandes ont un utilisateur assigné

## 📈 Statistiques

### Métriques Disponibles
- **Commandes confirmées** : Nombre de commandes passées en "Consommée"
- **Points créés** : Nombre de points de consommation générés
- **Erreurs** : Nombre d'erreurs lors de la fermeture
- **Durée** : Temps d'exécution de la fermeture

### Historique
- **Date de fermeture** : Enregistrée dans `ConfigurationCommande`
- **Détails** : Nombre de commandes traitées
- **Erreurs** : Logs d'erreurs si problème

## 🎯 Points Clés

1. **Automatique** : Aucune intervention manuelle requise
2. **Fiable** : Vérification toutes les 5 minutes
3. **Traçable** : Logs détaillés de toutes les opérations
4. **Configurable** : Paramètres modifiables par l'admin
5. **Sécurisé** : Une seule fermeture par jour maximum

---

**📞 Support** : En cas de problème, vérifiez les logs de l'application ou contactez l'équipe technique.
