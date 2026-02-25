# 📊 Guide d'Utilisation - Quotas Permanents Douaniers

## 🎯 Vue d'ensemble

Le système de **quotas permanents** permet aux RH/Admin de définir un **nombre fixe de plats** que les Douaniers peuvent consommer **chaque jour**. Ces quotas sont définis **une seule fois** et s'appliquent automatiquement à tous les jours. Ils peuvent être **modifiés par les Admin/RH** dans les paramètres.

## 🔧 Configuration des Quotas

### 1. Modifier les Quotas Permanents (RH/Admin)

1. **Connectez-vous** en tant qu'**Administrateur** ou **RessourcesHumaines**
2. **Allez dans** **Paramètres** → **Groupes Non-CIT**
3. **Cliquez sur** l'icône **Modifier** (crayon) du groupe "Douaniers"
4. **Ajustez les quotas** :
   - **Quota Jour** : Nombre de plats pour le service du jour (ex: 50)
   - **Quota Nuit** : Nombre de plats pour le service de nuit (ex: 30)
   - **Restriction Standard** : Activez pour limiter aux plats standard uniquement
5. **Sauvegardez** les modifications

### 2. Voir les Statistiques

1. **Allez dans** **Paramètres** → **Groupes Non-CIT**
2. **Cliquez sur** l'icône **Détails** (œil) du groupe "Douaniers"
3. **Consultez** les statistiques en temps réel :
   - **Quota Total** vs **Plats Consommés**
   - **Graphiques de progression**
   - **Plats Restants**

## 📋 Utilisation des Quotas

### 1. Créer une Commande Douaniers

1. **Connectez-vous** en tant que **PrestataireCantine**
2. **Allez dans** **Commandes** → **Commandes Douaniers**
3. **Vérifiez** les quotas permanents affichés :
   - **Jour** : X/Y plats (X consommés, Y total)
   - **Nuit** : X/Y plats (X consommés, Y total)
4. **Créez la commande** :
   - **Formule du Jour** : Sélectionnez un menu standard
   - **Nombre de Plats** : Entrez la quantité (max = quota restant)
   - **Période** : Jour ou Nuit
   - **Site** : CIT Billing ou CIT Terminal

### 2. Validation de Commande

1. **Notez le code de commande** généré (ex: `DOU-20241201-ABC12345`)
2. **Allez dans** **Commandes** → **Validation Douaniers**
3. **Entrez le code** de commande
4. **Validez** pour confirmer la consommation

## 🚨 Contrôles Automatiques

### Quota Respecté ✅
- **Demande ≤ Quota restant** → Commande acceptée
- **Quota mis à jour** automatiquement
- **Code de commande** généré

### Quota Dépassé ❌
- **Demande > Quota restant** → Commande refusée
- **Message d'erreur** explicite
- **Aucune commande** créée

## 📊 Exemple Pratique

### Configuration Permanente
```
Quota Jour : 50 plats (permanent)
Quota Nuit : 30 plats (permanent)
S'applique : Tous les jours automatiquement
```

### Utilisation
```
1. Commande de 10 plats (Jour) → ✅ Acceptée
   Restant : 40 plats jour, 30 plats nuit

2. Commande de 45 plats (Jour) → ❌ Refusée
   Raison : 45 > 40 (quota restant)

3. Commande de 20 plats (Nuit) → ✅ Acceptée
   Restant : 40 plats jour, 10 plats nuit
```

## 🔍 Monitoring et Statistiques

### Vue des Quotas
- **Liste complète** des quotas par date
- **Statistiques visuelles** avec barres de progression
- **Indicateurs colorés** :
  - 🟢 Vert : Quota disponible
  - 🟡 Jaune : Quota utilisé partiellement
  - 🔴 Rouge : Quota épuisé

### Logs de Debug
- **Console de l'application** : Logs détaillés des vérifications
- **Messages explicites** en cas d'erreur
- **Suivi des mises à jour** des quotas

## ⚙️ Configuration Technique

### Modèle de Données
```csharp
GroupeNonCit {
    Id : Guid (identifiant du groupe)
    Nom : string (nom du groupe, ex: "Douaniers")
    QuotaJournalier : int? (nombre de plats autorisés pour le jour - permanent)
    QuotaNuit : int? (nombre de plats autorisés pour la nuit - permanent)
    RestrictionFormuleStandard : bool (limite aux plats standard uniquement)
    CodeGroupe : string (code du groupe, ex: "DOU")
}
```

### Règles Métier
1. **Quota permanent** défini une seule fois par groupe
2. **Quota strict** : impossible de dépasser
3. **Calcul automatique** des plats consommés par jour
4. **Restriction aux plats standard** pour les Douaniers
5. **Codes de commande uniques** avec préfixe "DOU-"

## 🎯 Points Clés à Retenir

1. **Le quota est un nombre FIXE PERMANENT** défini une seule fois par les RH/Admin
2. **Il s'applique AUTOMATIQUEMENT chaque jour** sans exception
3. **L'application bloque automatiquement** les dépassements
4. **Les statistiques sont calculées en temps réel** chaque jour
5. **Chaque commande génère un code unique** pour validation
6. **Les quotas peuvent être modifiés** via Paramètres → Groupes Non-CIT

## 🆘 Dépannage

### Problème : "Aucun quota défini"
**Solution** : Configurer les quotas permanents via Paramètres → Groupes Non-CIT → Modifier "Douaniers"

### Problème : "Quota insuffisant"
**Solution** : Vérifier les quotas restants ou augmenter le quota permanent via Paramètres → Groupes Non-CIT

### Problème : "Groupe Douaniers introuvable"
**Solution** : Le groupe est créé automatiquement au démarrage de l'application

---

**📞 Support** : En cas de problème, vérifiez les logs de l'application ou contactez l'équipe technique.
