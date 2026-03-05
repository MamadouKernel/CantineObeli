# 🔍 Guide: Diagnostic Configuration

## ✅ Statut: IMPLÉMENTÉ ET FONCTIONNEL

---

## 🎯 Objectif

Cette fonctionnalité permet de **diagnostiquer la configuration** du système, notamment:
- Vérifier les configurations de facturation
- Tester la sauvegarde en base de données
- Identifier les problèmes de configuration
- Initialiser les configurations manquantes

---

## 🔐 Accès

**Rôles autorisés:**
- ✅ Administrateur uniquement

**URL d'accès:** `/DiagnosticConfig/Index`

**Menu:** Paramètres → Diagnostic Configuration (ou navigation directe)

---

## 📊 Qu'est-ce que le Diagnostic Configuration ?

C'est un **outil de dépannage** qui permet aux administrateurs de:

### 1. Vérifier les Configurations
- Lister toutes les configurations de facturation
- Voir les valeurs actuelles
- Identifier les configurations manquantes

### 2. Tester la Base de Données
- Tester la sauvegarde en base de données
- Vérifier la connexion
- Détecter les problèmes de permissions

### 3. Initialiser les Configurations
- Créer les configurations par défaut
- Réparer les configurations manquantes
- Réinitialiser le système

---

## 🎛️ Interface de Diagnostic

### Page Principale

```
┌──────────────────────────────────────────────┐
│ 🔍 Diagnostic des Configurations             │
├──────────────────────────────────────────────┤
│                                              │
│ 📊 Total des configurations: 5               │
│                                              │
│ [🧪 Test de Sauvegarde]                      │
│ [➕ Initialiser Configurations]              │
│                                              │
│ ┌──────────────────────────────────────────┐ │
│ │ Clé                    │ Valeur │ Desc   │ │
│ ├──────────────────────────────────────────┤ │
│ │ FACTURATION_ACTIVE     │ true   │ ...    │ │
│ │ FACTURATION_MONTANT    │ 5.00   │ ...    │ │
│ │ FACTURATION_EXEMPTION  │ true   │ ...    │ │
│ └──────────────────────────────────────────┘ │
│                                              │
│ ℹ️ Instructions:                             │
│ 1. Si aucune config → Initialiser           │
│ 2. Si problème → Test de Sauvegarde         │
│ 3. Retourner sur Paramètres Facturation     │
└──────────────────────────────────────────────┘
```

---

## 🚀 Comment Utiliser

### Étape 1: Accéder au Diagnostic

1. Connectez-vous avec un compte **Administrateur**
2. Naviguez vers `/DiagnosticConfig/Index`
3. Vous verrez la page de diagnostic

### Étape 2: Analyser les Résultats

#### Cas 1: Configurations Trouvées ✅

```
Total des configurations trouvées: 5

┌────────────────────────────────────────┐
│ FACTURATION_ACTIVE     │ true          │
│ FACTURATION_MONTANT    │ 5.00          │
│ FACTURATION_EXEMPTION  │ true          │
│ FACTURATION_WEEKEND    │ false         │
│ FACTURATION_DOUANIERS  │ true          │
└────────────────────────────────────────┘
```

**Résultat:** ✅ Tout fonctionne correctement

---

#### Cas 2: Aucune Configuration ❌

```
Total des configurations trouvées: 0

┌────────────────────────────────────────┐
│ AUCUNE CONFIGURATION TROUVÉE           │
│ ❌ Les configurations n'existent pas   │
└────────────────────────────────────────┘
```

**Action:** Cliquez sur "Initialiser Configurations"

---

#### Cas 3: Configurations Incomplètes ⚠️

```
Total des configurations trouvées: 2

┌────────────────────────────────────────┐
│ FACTURATION_ACTIVE     │ true          │
│ FACTURATION_MONTANT    │ 5.00          │
└────────────────────────────────────────┘
```

**Action:** Cliquez sur "Initialiser Configurations" pour compléter

---

### Étape 3: Actions Disponibles

#### Action 1: Test de Sauvegarde 🧪

**Objectif:** Tester si la base de données accepte les sauvegardes

**Utilisation:**
1. Cliquez sur **"Test de Sauvegarde"**
2. Le système crée une configuration de test
3. Message de succès ou d'erreur s'affiche

**Résultat Succès:**
```
✅ Test de sauvegarde réussi !
```

**Résultat Échec:**
```
❌ Test de sauvegarde échoué : [Message d'erreur]
```

---

#### Action 2: Initialiser Configurations ➕

**Objectif:** Créer toutes les configurations par défaut

**Utilisation:**
1. Cliquez sur **"Initialiser Configurations"**
2. Le système crée les configurations manquantes
3. Retournez sur le diagnostic pour vérifier

**Configurations créées:**
- `FACTURATION_NON_CONSOMMEES_ACTIVE` (true)
- `FACTURATION_NON_CONSOMMEES_MONTANT` (5.00)
- `FACTURATION_EXEMPTION_DOUANIERS` (true)
- `FACTURATION_EXEMPTION_WEEKEND` (false)
- Et autres...

---

## 📊 Informations Affichées

### Tableau des Configurations

| Colonne | Description | Exemple |
|---------|-------------|---------|
| **Clé** | Nom de la configuration | `FACTURATION_ACTIVE` |
| **Valeur** | Valeur actuelle | `true` |
| **Description** | Description de la config | "Active la facturation" |
| **Créé le** | Date de création | 2026-03-05 10:00:00 |
| **Modifié le** | Date de modification | 2026-03-05 14:30:00 |
| **Créé par** | Utilisateur créateur | "system" |
| **Modifié par** | Utilisateur modificateur | "admin" |

### Badges de Valeur

- 🟢 **Badge vert** (`true`) - Configuration activée
- ⚫ **Badge gris** (`false`) - Configuration désactivée
- 🔵 **Badge bleu** (autre) - Valeur numérique ou texte

---

## 🔍 Cas d'Usage

### Cas 1: Vérification Après Installation

**Situation:** Première installation du système

**Actions:**
1. Accéder au diagnostic
2. Vérifier que les configurations existent
3. Si manquantes → Initialiser
4. Tester la sauvegarde

**Résultat attendu:** Toutes les configurations présentes

---

### Cas 2: Problème de Facturation

**Situation:** La facturation ne fonctionne pas

**Actions:**
1. Accéder au diagnostic
2. Vérifier les configurations de facturation
3. Vérifier les valeurs (true/false)
4. Tester la sauvegarde
5. Réinitialiser si nécessaire

**Résultat attendu:** Configurations correctes et sauvegarde OK

---

### Cas 3: Migration de Base de Données

**Situation:** Après une migration ou restauration

**Actions:**
1. Accéder au diagnostic
2. Vérifier que les configurations ont été migrées
3. Tester la sauvegarde
4. Initialiser si manquantes

**Résultat attendu:** Toutes les configurations restaurées

---

## 🛠️ Dépannage

### Problème 1: "Aucune configuration trouvée"

**Cause:** Les configurations n'ont pas été initialisées

**Solution:**
1. Cliquez sur "Initialiser Configurations"
2. Attendez la confirmation
3. Rafraîchissez la page
4. Vérifiez que les configurations apparaissent

---

### Problème 2: "Test de sauvegarde échoué"

**Cause:** Problème de connexion à la base de données

**Solutions possibles:**

**A. Vérifier la connexion DB**
```
1. Vérifiez appsettings.json
2. Testez la connexion SQL Server
3. Vérifiez les permissions
```

**B. Vérifier les migrations**
```bash
dotnet ef database update
```

**C. Vérifier les logs**
```
Consultez les logs de l'application
Recherchez les erreurs SQL
```

---

### Problème 3: "Configurations incomplètes"

**Cause:** Certaines configurations manquent

**Solution:**
1. Cliquez sur "Initialiser Configurations"
2. Les configurations manquantes seront créées
3. Les configurations existantes ne seront pas modifiées

---

### Problème 4: "Erreur lors du diagnostic"

**Cause:** Exception dans le code

**Solution:**
1. Consultez les logs
2. Vérifiez le message d'erreur affiché
3. Contactez le support technique

---

## ⚙️ Configuration Technique

### Contrôleur

**Fichier:** `Controllers/DiagnosticConfigController.cs`

**Actions:**
- `Index()` - Afficher le diagnostic
- `TestSave()` - Tester la sauvegarde

### Vue

**Fichier:** `Views/DiagnosticConfig/Index.cshtml`

### Base de Données

**Table:** `ConfigurationsCommande`

**Colonnes:**
- `Id` (Guid) - Identifiant unique
- `Cle` (string) - Nom de la configuration
- `Valeur` (string) - Valeur de la configuration
- `Description` (string) - Description
- `CreatedOn` (DateTime) - Date de création
- `ModifiedOn` (DateTime) - Date de modification
- `CreatedBy` (string) - Créateur
- `ModifiedBy` (string) - Modificateur
- `Supprimer` (int) - Soft delete (0=actif, 1=supprimé)

---

## 📋 Configurations de Facturation

### Configurations Standard

1. **FACTURATION_NON_CONSOMMEES_ACTIVE**
   - Type: Boolean (true/false)
   - Description: Active la facturation des commandes non consommées
   - Par défaut: true

2. **FACTURATION_NON_CONSOMMEES_MONTANT**
   - Type: Decimal
   - Description: Montant à facturer par commande non consommée
   - Par défaut: 5.00

3. **FACTURATION_EXEMPTION_DOUANIERS**
   - Type: Boolean (true/false)
   - Description: Exempte les douaniers de la facturation
   - Par défaut: true

4. **FACTURATION_EXEMPTION_WEEKEND**
   - Type: Boolean (true/false)
   - Description: Exempte les commandes du week-end
   - Par défaut: false

5. **FACTURATION_HEURE_EXECUTION**
   - Type: Integer (0-23)
   - Description: Heure d'exécution de la facturation automatique
   - Par défaut: 2

---

## 🎓 Formation

### Pour les Administrateurs

**Objectifs:**
1. Comprendre le diagnostic
2. Savoir interpréter les résultats
3. Résoudre les problèmes courants
4. Initialiser les configurations

**Durée:** 10 minutes

**Étapes:**
1. Accéder au diagnostic
2. Analyser les configurations
3. Tester la sauvegarde
4. Initialiser si nécessaire
5. Vérifier le résultat

---

## 📝 Bonnes Pratiques

### Quand Utiliser le Diagnostic ?

✅ **Utiliser dans ces cas:**
- Après l'installation initiale
- Après une migration de base de données
- En cas de problème de facturation
- Avant une mise à jour majeure
- Pour vérifier l'intégrité des données

❌ **Ne pas utiliser pour:**
- Modifier les configurations (utiliser Paramètres Facturation)
- Supprimer des configurations
- Tester les fonctionnalités métier

### Fréquence de Vérification

- **Installation:** Obligatoire
- **Mensuel:** Recommandé
- **Après incident:** Obligatoire
- **Avant mise à jour:** Recommandé

---

## ✅ Checklist de Diagnostic

### Vérification Initiale

- [ ] Accéder à `/DiagnosticConfig/Index`
- [ ] Vérifier le nombre de configurations
- [ ] Vérifier que toutes les clés sont présentes
- [ ] Vérifier les valeurs (true/false/montants)

### Test de Fonctionnement

- [ ] Cliquer sur "Test de Sauvegarde"
- [ ] Vérifier le message de succès
- [ ] Rafraîchir la page
- [ ] Vérifier que la config de test apparaît

### Initialisation (si nécessaire)

- [ ] Cliquer sur "Initialiser Configurations"
- [ ] Attendre la confirmation
- [ ] Retourner sur le diagnostic
- [ ] Vérifier que toutes les configs sont créées

### Vérification Finale

- [ ] Toutes les configurations présentes
- [ ] Test de sauvegarde réussi
- [ ] Aucune erreur affichée
- [ ] Retourner sur Paramètres Facturation
- [ ] Tester la modification d'une configuration

---

## 🔗 Liens Utiles

### Pages Connexes

- **Paramètres Facturation:** `/ParametresFacturation/Index`
- **Initialisation Config:** `/InitConfig/Index`
- **Diagnostic Facturation:** `/DiagnosticFacturation/Index`

### Documentation

- Guide Paramètres Facturation
- Guide Configuration Commandes
- Guide Facturation Automatique

---

## 🎉 Résumé

La fonctionnalité **"Diagnostic Configuration"** est:

✅ **Implémentée** - Code complet et testé  
✅ **Fonctionnelle** - Toutes les actions disponibles  
✅ **Accessible** - Pour Administrateurs uniquement  
✅ **Utile** - Outil de dépannage essentiel  
✅ **Simple** - Interface claire et intuitive  
✅ **Documentée** - Guide complet disponible

**Prête à l'emploi !** 🚀

---

## 📞 Support

### En Cas de Problème

1. **Vérifier les logs** de l'application
2. **Consulter ce guide** pour les solutions
3. **Tester la sauvegarde** pour identifier le problème
4. **Initialiser les configurations** si nécessaire
5. **Contacter le support** si le problème persiste

---

**Date:** 05/03/2026  
**Version:** 1.0.0  
**Auteur:** Kiro AI Assistant  
**Statut:** ✅ PRODUCTION READY
