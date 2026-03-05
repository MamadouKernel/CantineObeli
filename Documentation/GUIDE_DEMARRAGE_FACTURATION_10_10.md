# 🚀 Guide de Démarrage Rapide - Facturation Automatique 10/10

## ✅ Étape 1 : Configuration Email

### Option A : Gmail (Recommandé pour les tests)

1. **Activer la validation en 2 étapes** sur votre compte Gmail
2. **Générer un mot de passe d'application** :
   - Aller sur https://myaccount.google.com/apppasswords
   - Sélectionner "Autre (nom personnalisé)"
   - Entrer "Obeli Facturation"
   - Copier le mot de passe généré (16 caractères)

3. **Configurer dans `appsettings.json`** :
```json
{
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": "587",
    "SmtpUser": "votre-email@gmail.com",
    "SmtpPassword": "xxxx xxxx xxxx xxxx",
    "From": "noreply@obeli.com"
  }
}
```

### Option B : Autre fournisseur SMTP

Exemples de configuration :

**Outlook/Office 365** :
```json
{
  "Email": {
    "SmtpHost": "smtp.office365.com",
    "SmtpPort": "587",
    "SmtpUser": "votre-email@outlook.com",
    "SmtpPassword": "votre-mot-de-passe",
    "From": "noreply@obeli.com"
  }
}
```

**SendGrid** :
```json
{
  "Email": {
    "SmtpHost": "smtp.sendgrid.net",
    "SmtpPort": "587",
    "SmtpUser": "apikey",
    "SmtpPassword": "votre-api-key",
    "From": "noreply@obeli.com"
  }
}
```

---

## ✅ Étape 2 : Vérifier l'Enregistrement du Service

Le service est déjà enregistré dans `Program.cs` :
```csharp
builder.Services.AddScoped<INotificationService, NotificationService>();
```

✅ **Rien à faire, c'est déjà configuré !**

---

## ✅ Étape 3 : Ajouter le Paramètre d'Heure d'Exécution

### Option A : Via SQL (Rapide)

Exécuter le script `Scripts/AjouterParametreHeureFacturation.sql` :

```sql
INSERT INTO ConfigurationsCommande (Cle, Valeur, Description, CreatedOn, CreatedBy, Supprimer)
VALUES (
    'FACTURATION_HEURE_EXECUTION',
    '2',
    'Heure d''exécution de la facturation automatique (0-23)',
    GETUTCDATE(),
    'System',
    0
);
```

### Option B : Via l'Interface (Recommandé)

1. Se connecter en tant qu'Administrateur
2. Aller dans **Configuration des Commandes**
3. Ajouter une nouvelle configuration :
   - **Clé** : `FACTURATION_HEURE_EXECUTION`
   - **Valeur** : `2` (pour 2h du matin)
   - **Description** : Heure d'exécution de la facturation automatique (0-23)
4. Sauvegarder

---

## ✅ Étape 4 : Tester l'Envoi d'Email

### Test Manuel

1. Aller sur `/FacturationAutomatique/Index`
2. Cliquer sur **"Exécuter la Facturation"**
3. Sélectionner une période avec des commandes non consommées
4. Cliquer sur **"Exécuter la Facturation"**
5. Vérifier :
   - ✅ Message de succès affiché
   - ✅ Email reçu par les administrateurs
   - ✅ Logs dans la console

### Vérifier les Logs

Rechercher dans les logs :
```
📧 Notification de facturation envoyée à X destinataires
```

Si vous voyez :
```
⚠️ Configuration email manquante. Notification non envoyée.
```
→ Vérifier la configuration email dans `appsettings.json`

---

## ✅ Étape 5 : Consulter le Dashboard

1. Aller sur `/FacturationAutomatique/Dashboard`
2. Vous devriez voir :
   - ✅ 4 cartes de statistiques
   - ✅ Graphique d'évolution
   - ✅ Graphique de répartition
   - ✅ Graphique des montants
   - ✅ Tableau des dernières facturations

---

## ✅ Étape 6 : Vérifier le Service Automatique

### Vérifier que le service est démarré

Dans les logs au démarrage de l'application :
```
💰 Service de facturation automatique démarré
```

### Vérifier l'heure d'exécution

Le service s'exécute à l'heure configurée (par défaut 2h du matin).

Pour tester immédiatement :
1. Modifier `FACTURATION_HEURE_EXECUTION` à l'heure actuelle
2. Attendre 1 heure maximum
3. Vérifier les logs

---

## 🔍 Dépannage

### Problème : Email non envoyé

**Symptôme** : Message "Configuration email manquante"

**Solution** :
1. Vérifier que `appsettings.json` contient la section `Email`
2. Vérifier que tous les champs sont remplis
3. Pour Gmail, vérifier que vous utilisez un mot de passe d'application

### Problème : Erreur SMTP

**Symptôme** : Exception lors de l'envoi d'email

**Solutions** :
- Vérifier le port (587 pour TLS, 465 pour SSL)
- Vérifier que le pare-feu autorise les connexions SMTP
- Vérifier les identifiants

### Problème : Aucun destinataire trouvé

**Symptôme** : "Aucun destinataire trouvé pour la notification"

**Solution** :
1. Vérifier qu'il existe des utilisateurs avec rôle "Administrateur" ou "RH"
2. Vérifier que ces utilisateurs ont une adresse email valide
3. Vérifier que `Supprimer = 0` pour ces utilisateurs

### Problème : Dashboard vide

**Symptôme** : Graphiques sans données

**Solution** :
- C'est normal si aucune facturation n'a été effectuée
- Exécuter une facturation manuelle pour générer des données
- Les graphiques utilisent des données simulées par défaut (à remplacer par des données réelles)

---

## 📊 Exemple d'Email Reçu

```
De: noreply@obeli.com
À: admin@obeli.com
Sujet: ✅ Facturation Automatique - 05/03/2026

Résumé de la facturation

Commandes Facturées: 15
Commandes Exemptées: 3
Montant Total Facturé: 42,000 FCFA

La facturation automatique a été exécutée avec succès.
Consultez le tableau de bord pour plus de détails.

---
Ceci est un message automatique du système Obeli
Ne pas répondre à cet email
```

---

## 🎯 Checklist Finale

Avant de considérer la configuration terminée :

- [ ] Configuration email dans `appsettings.json`
- [ ] Service enregistré dans `Program.cs` ✅ (déjà fait)
- [ ] Paramètre `FACTURATION_HEURE_EXECUTION` ajouté
- [ ] Test d'envoi d'email réussi
- [ ] Dashboard accessible et fonctionnel
- [ ] Logs du service visibles au démarrage
- [ ] Au moins un administrateur avec email valide

---

## 🚀 Prochaines Étapes

Une fois la configuration terminée :

1. **Laisser le service tourner** : Il s'exécutera automatiquement à 2h du matin
2. **Consulter le dashboard** : Suivre les statistiques quotidiennes
3. **Vérifier les emails** : S'assurer de recevoir les notifications
4. **Ajuster l'heure** : Si nécessaire, modifier `FACTURATION_HEURE_EXECUTION`

---

## 📞 Support

En cas de problème :

1. Consulter les logs de l'application
2. Vérifier la configuration email
3. Tester l'exécution manuelle
4. Consulter `Documentation/AMELIORATIONS_FACTURATION_AUTOMATIQUE.md`

---

**Félicitations ! Votre système de facturation automatique 10/10 est prêt ! 🎉**
