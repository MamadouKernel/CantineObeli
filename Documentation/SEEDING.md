# Configuration du Seeding de la Base de Données

## Problème Résolu

Le problème où les données de base étaient recréées à chaque démarrage de la solution a été corrigé.

## Améliorations Apportées

### 1. Vérification Robuste des Données Existantes

- **Menus de la semaine** : Vérification du nombre exact de menus (21 = 3 types × 7 jours)
- **Commandes de test** : Vérification du nombre de commandes existantes
- **Menus de la semaine + 1** : Même logique de vérification robuste

### 2. Messages de Log Informatifs

Le système affiche maintenant des messages clairs :
- `🌱 Début du seeding de la base de données...`
- `📋 Création des menus manquants pour la semaine courante (X/21 existants)`
- `✅ Menus de la semaine courante existent déjà (21/21)`
- `✅ Seeding de la base de données terminé`

### 3. Contrôle par Variable d'Environnement

Vous pouvez désactiver complètement le seeding en production :

```bash
# Désactiver le seeding
set OBELI_ENABLE_SEEDING=false

# Activer le seeding (par défaut)
set OBELI_ENABLE_SEEDING=true
```

## Comportement Actuel

### En Développement
- Le seeding s'exécute à chaque démarrage
- Seules les données manquantes sont créées
- Les données existantes sont préservées

### En Production
- Vous pouvez désactiver le seeding avec `OBELI_ENABLE_SEEDING=false`
- Les migrations de base de données s'exécutent toujours
- Aucune donnée de test n'est créée

## Données Créées par le Seeding

1. **Département par défaut** : "Direction Général"
2. **Fonction par défaut** : "Fonction Général"
3. **Utilisateur admin** : `admin` / `admin123`
4. **Utilisateur prestataire** : `prestataire` / `presta123`
5. **Menus de la semaine courante** : 21 menus (3 types × 7 jours)
6. **Menus de la semaine + 1** : 21 menus (3 types × 7 jours)
7. **Commandes de test** : 1 commande pour mardi

## Sécurité

⚠️ **IMPORTANT** : Changez immédiatement les mots de passe par défaut en production :
- `admin123` → Mot de passe sécurisé
- `presta123` → Mot de passe sécurisé

Vous pouvez définir le mot de passe du prestataire via la variable d'environnement :
```bash
set OBELI_PRESTA_DEFAULT_PWD=votre_mot_de_passe_securise
```
