# Champs Obligatoires vs Optionnels - Création d'Utilisateur

## Champs Obligatoires ✅

### Informations Personnelles
- **Nom** : Obligatoire
- **Prénoms** : Obligatoire
- **Matricule (UserName)** : Obligatoire et unique

### Rôle et Affectation
- **Rôle** : Obligatoire
- **Département** : Obligatoire (doit exister)
- **Fonction** : Obligatoire (doit exister)

### Sécurité
- **Mot de passe** : Obligatoire (minimum 6 caractères)
- **Confirmation mot de passe** : Obligatoire

## Champs Optionnels 📝

### Informations de Contact
- **Email** : Optionnel (mais doit être unique si fourni)
- **Téléphone** : Optionnel
- **Lieu** : Optionnel

### Configuration
- **Site** : Optionnel
- **Code Commande** : Optionnel (legacy)

## Logique de Validation

### Validation des Champs Obligatoires
```csharp
// Vérification de présence
if (string.IsNullOrWhiteSpace(utilisateur.Nom))
    ModelState.AddModelError("Nom", "Le nom est obligatoire.");

if (string.IsNullOrWhiteSpace(utilisateur.UserName))
    ModelState.AddModelError("UserName", "Le matricule est obligatoire.");
```

### Validation des Relations
```csharp
// Vérification que le département existe
var departementExiste = await _context.Departements
    .AnyAsync(d => d.Id == utilisateur.DepartementId && d.Supprimer == 0);
```

### Validation d'Unicité (Conditionnelle)
```csharp
// Email : uniquement si fourni
if (!string.IsNullOrWhiteSpace(utilisateur.Email))
{
    var emailExiste = await _context.Utilisateurs
        .AnyAsync(u => u.Email == utilisateur.Email && u.Supprimer == 0);
}

// Matricule : toujours vérifié (obligatoire)
var matriculeExiste = await _context.Utilisateurs
    .AnyAsync(u => u.UserName == utilisateur.UserName && u.Supprimer == 0);
```

## Interface Utilisateur

### Indication Visuelle
- **Champs obligatoires** : Astérisque (*) rouge
- **Champs optionnels** : Texte "(optionnel)" en gris

### Exemples
```html
<!-- Obligatoire -->
<label asp-for="Nom" class="form-label">Nom *</label>

<!-- Optionnel -->
<label asp-for="Email" class="form-label">Email <small class="text-muted">(optionnel)</small></label>
```

## Cas d'Usage

### Utilisateur avec Email
```
Nom: Dupont *
Prénoms: Jean *
Matricule: JDP001 *
Email: jean.dupont@entreprise.com (optionnel)
Département: Direction Général *
Fonction: Fonction Général *
```

### Utilisateur sans Email
```
Nom: Martin *
Prénoms: Pierre *
Matricule: PMT002 *
Email: (vide - OK)
Département: Direction Général *
Fonction: Fonction Général *
```

## Règles Métier

### Matricule (UserName)
- ✅ **Obligatoire** : Chaque utilisateur doit avoir un matricule unique
- ✅ **Unique** : Pas de doublons dans le système
- ✅ **Format** : Pas d'espaces (nettoyage automatique)

### Email
- ✅ **Optionnel** : Tous les utilisateurs n'ont pas d'email
- ✅ **Unique** : Si fourni, doit être unique
- ✅ **Format** : Validation du format email si fourni

### Relations
- ✅ **Département** : Doit exister et ne pas être supprimé
- ✅ **Fonction** : Doit exister et ne pas être supprimé
- ✅ **Rôle** : Doit être une valeur valide de l'enum

## Messages d'Erreur

### Champs Obligatoires
- "Le nom est obligatoire."
- "Le matricule est obligatoire."
- "Le département est obligatoire."

### Validation d'Unicité
- "Ce matricule est déjà utilisé."
- "Cette adresse email est déjà utilisée."

### Relations
- "Le département sélectionné n'existe pas."
- "La fonction sélectionnée n'existe pas."
