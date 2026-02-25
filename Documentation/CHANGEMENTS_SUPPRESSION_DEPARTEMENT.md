# Changements Nécessaires : Suppression de Département

## ⚠️ ATTENTION

La suppression de l'entité "Département" nécessite des modifications importantes dans de nombreux fichiers. Voici un récapitulatif complet.

## Fichiers Modifiés ✅

1. ✅ `Models/Service.cs` - Relation changée de Departement vers Direction
2. ✅ `Models/Direction.cs` - Relation changée de Departements vers Services
3. ✅ `Models/Utilisateur.cs` - Relation changée de Departement vers Direction
4. ✅ `Data/ObeliDbContext.cs` - DbSet Departements supprimé, configurations mises à jour
5. ✅ `Models/Departement.cs` - SUPPRIMÉ
6. ✅ `Controllers/DepartementController.cs` - SUPPRIMÉ
7. ✅ `Views/Departement/` - SUPPRIMÉ (dossier complet)
8. ✅ `Controllers/ServiceController.cs` - Toutes références Departement → Direction
9. ✅ `Controllers/DirectionController.cs` - Références Departements → Services
10. ✅ `Views/Service/*.cshtml` - Toutes vues mises à jour
11. ✅ `Views/Direction/*.cshtml` - Toutes vues mises à jour
12. ✅ `Models/ViewModels/ReportingDashboardViewModel.cs` - Departement → Direction
13. ✅ `Program.cs` - Département par défaut → Direction par défaut

## Fichiers À Modifier ⚠️

### ViewModels

1. **`Models/ViewModels/CreateCommandeVisiteurViewModel.cs`**
   - `DepartementId` → `DirectionId`
   - `DepartementNom` → `DirectionNom`

2. **`Models/ViewModels/CreateUtilisateurViewModel.cs`**
   - `DepartementId` → `DirectionId`

3. **`Models/ViewModels/EditUtilisateurViewModel.cs`**
   - `DepartementId` → `DirectionId`

4. **`Models/ViewModels/ReportingIndicateursViewModel.cs`**
   - `CommandesParDepartement` → `CommandesParDirection`

5. **`Models/ViewModels/VisiteurSelectionViewModel.cs`**
   - `DepartementId` → `DirectionId`

### Controllers

6. **`Controllers/UtilisateurController.cs`**
   - Toutes les références à `Departements` → `Directions`
   - `DepartementId` → `DirectionId`
   - ViewBag.Departements → ViewBag.Directions

### Services

7. **`Services/Users/UserService.cs`**
   - `.Include(u => u.Departement)` → `.Include(u => u.Direction)`
   - `DepartementId` → `DirectionId`

8. **`Services/ReportingAutomatiqueService.cs`**
   - `.ThenInclude(u => u!.Departement)` → `.ThenInclude(u => u!.Direction)`
   - `Departement?.Nom` → `Direction?.Nom`

### Vues

9. **Toutes les vues utilisant Departement** :
   - `Views/Utilisateur/*.cshtml`
   - `Views/Commande/*.cshtml`
   - `Views/Visiteur/*.cshtml`
   - `Views/Reporting/*.cshtml`

## Nouvelle Hiérarchie

```
Direction
  └── Service
       └── Utilisateur (avec Fonction)
```

## Impact sur la Base de Données

### Tables à Supprimer
- `Departements`

### Colonnes à Modifier
- `Utilisateurs.DepartementId` → `Utilisateurs.DirectionId`
- `Services.DepartementId` → `Services.DirectionId`

### Migration Nécessaire

La migration doit :
1. Supprimer la table `Departements`
2. Supprimer la colonne `Utilisateurs.DepartementId`
3. Ajouter la colonne `Utilisateurs.DirectionId` (nullable)
4. Modifier `Services.DepartementId` en `Services.DirectionId`
5. Mettre à jour les clés étrangères

## Recommandation

⚠️ **ATTENTION** : Cette modification est très importante et impacte de nombreux fichiers.

**Options** :

1. **Option A - Continuer la suppression complète** :
   - Modifier tous les fichiers listés ci-dessus
   - Créer une migration complexe
   - Risque de perte de données si des départements existent

2. **Option B - Approche progressive** :
   - Garder temporairement Département
   - Migrer progressivement les données
   - Supprimer Département dans une phase ultérieure

3. **Option C - Renommer au lieu de supprimer** :
   - Renommer "Département" en "Direction"
   - Supprimer l'ancienne entité "Direction"
   - Moins de modifications nécessaires

**Quelle option préférez-vous ?**
