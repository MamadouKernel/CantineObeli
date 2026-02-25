# Standards de Codage - Projet Obeli_K

## 📋 Vue d'Ensemble

Ce document définit les standards de codage à respecter pour le projet Obeli_K. Tous les développeurs doivent suivre ces règles pour maintenir la cohérence et la qualité du code.

## 🎯 Principes Fondamentaux

### SOLID Principles

1. **S**ingle Responsibility Principle : Une classe = une responsabilité
2. **O**pen/Closed Principle : Ouvert à l'extension, fermé à la modification
3. **L**iskov Substitution Principle : Les sous-types doivent être substituables
4. **I**nterface Segregation Principle : Interfaces spécifiques plutôt que générales
5. **D**ependency Inversion Principle : Dépendre des abstractions, pas des implémentations

### DRY (Don't Repeat Yourself)

❌ **Mauvais** :
```csharp
if (string.IsNullOrWhiteSpace(model.Nom))
    ModelState.AddModelError(nameof(model.Nom), "Le nom est obligatoire.");
// Répété dans 10 méthodes différentes
```

✅ **Bon** :
```csharp
public class CreateUtilisateurValidator : AbstractValidator<CreateUtilisateurViewModel>
{
    public CreateUtilisateurValidator()
    {
        RuleFor(x => x.Nom).NotEmpty().WithMessage(ErrorMessages.RequiredField);
    }
}
```

### KISS (Keep It Simple, Stupid)

❌ **Mauvais** :
```csharp
public async Task<IActionResult> Create(CreateUtilisateurViewModel model)
{
    // 500 lignes de code dans une seule méthode
}
```

✅ **Bon** :
```csharp
public async Task<IActionResult> Create(CreateUtilisateurViewModel model)
{
    if (!ModelState.IsValid)
        return View(model);
        
    var result = await _userService.CreateAsync(model);
    
    if (result.Success)
    {
        TempData["SuccessMessage"] = SuccessMessages.UserCreated;
        return RedirectToAction(nameof(Index));
    }
    
    ModelState.AddModelError("", result.ErrorMessage);
    return View(model);
}
```

## 📝 Conventions de Nommage

### Classes et Interfaces

```csharp
// ✅ PascalCase pour les classes
public class CommandeService { }
public class UtilisateurRepository { }

// ✅ Préfixe "I" pour les interfaces
public interface ICommandeService { }
public interface IUtilisateurRepository { }

// ✅ Suffixe descriptif
public class CreateUtilisateurViewModel { }
public class CommandeListViewModel { }
public class UtilisateurValidator { }
```

### Méthodes

```csharp
// ✅ PascalCase, verbe d'action
public async Task<Commande> CreateCommandeAsync(CreateCommandeViewModel model) { }
public bool IsCommandeBloquee() { }
public decimal CalculerMontantFacturation(Commande commande) { }

// ❌ Éviter les noms vagues
public async Task DoStuff() { } // Mauvais
public async Task Process() { } // Mauvais
```

### Variables et Paramètres

```csharp
// ✅ camelCase pour les variables locales et paramètres
var utilisateurId = Guid.NewGuid();
var commandeList = new List<Commande>();

// ✅ Noms descriptifs
var commandesNonConsommees = await GetCommandesNonConsommeesAsync();

// ❌ Éviter les abréviations
var cmd = new Commande(); // Mauvais
var usr = new Utilisateur(); // Mauvais
```

### Constantes

```csharp
// ✅ PascalCase pour les constantes publiques
public const string CommandeJourCloture = "COMMANDE_JOUR_CLOTURE";
public const int DefaultPageSize = 10;

// ✅ UPPER_CASE pour les constantes privées (optionnel)
private const int MAX_RETRY_COUNT = 3;
```

### Propriétés

```csharp
// ✅ PascalCase
public Guid CommandeId { get; set; }
public string Nom { get; set; } = null!;
public DateTime DateCreation { get; set; }

// ✅ Cohérence dans le nommage des IDs
public Guid UtilisateurId { get; set; } // Suffixe "Id"
public Guid FormuleId { get; set; }     // Pas "IdFormule"
```

## 🏗️ Structure des Fichiers

### Organisation des Dossiers

```
Obeli_K/
├── Constants/           # Constantes centralisées
├── Controllers/         # Contrôleurs MVC
├── Data/               # DbContext et configurations EF
├── Enums/              # Énumérations
├── Models/             # Entités de domaine
│   ├── Enums/         # Enums spécifiques aux modèles
│   └── ViewModels/    # ViewModels pour les vues
├── Repositories/       # Pattern Repository
│   ├── Interfaces/
│   └── Implementations/
├── Services/           # Logique métier
│   ├── Configuration/
│   ├── Security/
│   └── Users/
├── Validators/         # Validateurs FluentValidation
└── Views/             # Vues Razor
```

### Ordre des Membres dans une Classe

```csharp
public class ExempleService
{
    // 1. Constantes privées
    private const int MaxRetryCount = 3;
    
    // 2. Champs privés (readonly en premier)
    private readonly ILogger<ExempleService> _logger;
    private readonly ObeliDbContext _context;
    
    // 3. Constructeur
    public ExempleService(ILogger<ExempleService> logger, ObeliDbContext context)
    {
        _logger = logger;
        _context = context;
    }
    
    // 4. Propriétés publiques
    public string Nom { get; set; } = null!;
    
    // 5. Méthodes publiques
    public async Task<Result> DoSomethingAsync() { }
    
    // 6. Méthodes privées
    private void ValidateInput() { }
}
```

## 🔧 Bonnes Pratiques

### 1. Injection de Dépendances

✅ **Bon** :
```csharp
public class CommandeService
{
    private readonly ICommandeRepository _repository;
    private readonly ILogger<CommandeService> _logger;
    
    public CommandeService(ICommandeRepository repository, ILogger<CommandeService> logger)
    {
        _repository = repository;
        _logger = logger;
    }
}
```

❌ **Mauvais** :
```csharp
public class CommandeService
{
    private readonly CommandeRepository _repository = new CommandeRepository();
    // Couplage fort, impossible à tester
}
```

### 2. Async/Await

✅ **Bon** :
```csharp
public async Task<List<Commande>> GetCommandesAsync()
{
    return await _context.Commandes
        .Where(c => c.Supprimer == BusinessConstants.NotDeleted)
        .ToListAsync();
}
```

❌ **Mauvais** :
```csharp
public List<Commande> GetCommandes()
{
    return _context.Commandes
        .Where(c => c.Supprimer == 0)
        .ToList(); // Bloquant
}
```

### 3. Gestion des Erreurs

✅ **Bon** :
```csharp
public async Task<Result<Commande>> CreateCommandeAsync(CreateCommandeViewModel model)
{
    try
    {
        // Validation
        if (!await IsValidAsync(model))
            return Result<Commande>.Failure(ErrorMessages.InvalidData);
        
        // Logique métier
        var commande = await _repository.AddAsync(MapToEntity(model));
        
        _logger.LogInformation("Commande créée: {CommandeId}", commande.CommandeId);
        
        return Result<Commande>.Success(commande);
    }
    catch (DbUpdateException ex)
    {
        _logger.LogError(ex, "Erreur lors de la création de la commande");
        return Result<Commande>.Failure(ErrorMessages.DatabaseError);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Erreur inattendue lors de la création de la commande");
        return Result<Commande>.Failure(ErrorMessages.GenericError);
    }
}
```

❌ **Mauvais** :
```csharp
public async Task<Commande> CreateCommandeAsync(CreateCommandeViewModel model)
{
    try
    {
        // 500 lignes de code
        return commande;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Erreur");
        throw; // Pas de gestion appropriée
    }
}
```

### 4. Logging

✅ **Bon** :
```csharp
_logger.LogInformation("Création de commande pour l'utilisateur {UtilisateurId}", utilisateurId);
_logger.LogWarning("Quota dépassé pour le groupe {GroupeId}: {Quota}/{Max}", groupeId, quota, max);
_logger.LogError(ex, "Erreur lors de la facturation de la commande {CommandeId}", commandeId);
```

❌ **Mauvais** :
```csharp
_logger.LogInformation("🔍 Recherche des commandes..."); // Emojis
_logger.LogInformation($"Utilisateur: {utilisateurId}"); // String interpolation
_logger.LogError("Erreur"); // Pas de contexte
```

### 5. LINQ et Entity Framework

✅ **Bon** :
```csharp
var commandes = await _context.Commandes
    .AsNoTracking() // Lecture seule
    .Include(c => c.Utilisateur)
    .Include(c => c.FormuleJour)
    .Where(c => c.Supprimer == BusinessConstants.NotDeleted)
    .Where(c => c.DateConsommation >= dateDebut)
    .OrderByDescending(c => c.DateConsommation)
    .Take(100)
    .ToListAsync();
```

❌ **Mauvais** :
```csharp
var commandes = _context.Commandes
    .ToList() // Charge tout en mémoire
    .Where(c => c.Supprimer == 0) // Filtrage en mémoire
    .OrderBy(c => c.DateConsommation)
    .ToList();
```

### 6. Validation

✅ **Bon** (FluentValidation) :
```csharp
public class CreateCommandeValidator : AbstractValidator<CreateCommandeViewModel>
{
    public CreateCommandeValidator()
    {
        RuleFor(x => x.UtilisateurId)
            .NotEmpty()
            .WithMessage(ErrorMessages.RequiredField);
            
        RuleFor(x => x.FormuleId)
            .NotEmpty()
            .WithMessage(ErrorMessages.RequiredField);
            
        RuleFor(x => x.DateConsommation)
            .GreaterThan(DateTime.Today)
            .WithMessage("La date de consommation doit être dans le futur");
    }
}
```

❌ **Mauvais** :
```csharp
if (model.UtilisateurId == Guid.Empty)
    ModelState.AddModelError(nameof(model.UtilisateurId), "L'utilisateur est obligatoire");
    
if (model.FormuleId == Guid.Empty)
    ModelState.AddModelError(nameof(model.FormuleId), "La formule est obligatoire");
    
// Répété dans chaque action
```

### 7. Constantes vs Valeurs Hardcodées

✅ **Bon** :
```csharp
if (commande.Supprimer == BusinessConstants.NotDeleted)
{
    var pourcentage = config.FacturationPourcentage ?? BusinessConstants.DefaultFacturationPourcentage;
    var montant = (commande.Montant * pourcentage) / 100;
}
```

❌ **Mauvais** :
```csharp
if (commande.Supprimer == 0) // Nombre magique
{
    var pourcentage = config.FacturationPourcentage ?? 100; // Nombre magique
    var montant = (commande.Montant * pourcentage) / 100;
}
```

### 8. Commentaires

✅ **Bon** :
```csharp
/// <summary>
/// Calcule le montant de facturation pour une commande non consommée.
/// Applique le pourcentage de facturation configuré et vérifie les absences gratuites.
/// </summary>
/// <param name="commande">La commande à facturer</param>
/// <param name="utilisateur">L'utilisateur concerné</param>
/// <returns>Le montant à facturer ou null si gratuit</returns>
public async Task<decimal?> CalculerMontantFacturationAsync(Commande commande, Utilisateur utilisateur)
{
    // Vérifier si l'utilisateur a encore des absences gratuites ce mois
    var absencesGratuites = await GetAbsencesGratuitesMoisAsync(utilisateur.Id);
    if (absencesGratuites > 0)
        return null;
    
    // Appliquer le pourcentage de facturation
    var pourcentage = await GetPourcentageFacturationAsync();
    return (commande.Montant * pourcentage) / 100;
}
```

❌ **Mauvais** :
```csharp
// Calcul
public async Task<decimal?> CalculerMontantFacturationAsync(Commande commande, Utilisateur utilisateur)
{
    // TODO: À implémenter
    var x = await GetAbsencesGratuitesMoisAsync(utilisateur.Id);
    if (x > 0) // Pourquoi > 0 ?
        return null;
    
    var y = await GetPourcentageFacturationAsync();
    return (commande.Montant * y) / 100; // Formule magique
}
```

## 🧪 Tests Unitaires

### Structure des Tests

```csharp
public class CommandeServiceTests
{
    private readonly Mock<ICommandeRepository> _mockRepository;
    private readonly Mock<ILogger<CommandeService>> _mockLogger;
    private readonly CommandeService _service;
    
    public CommandeServiceTests()
    {
        _mockRepository = new Mock<ICommandeRepository>();
        _mockLogger = new Mock<ILogger<CommandeService>>();
        _service = new CommandeService(_mockRepository.Object, _mockLogger.Object);
    }
    
    [Fact]
    public async Task CreateCommandeAsync_ValidModel_ReturnsSuccess()
    {
        // Arrange
        var model = new CreateCommandeViewModel
        {
            UtilisateurId = Guid.NewGuid(),
            FormuleId = Guid.NewGuid()
        };
        
        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<Commande>()))
            .ReturnsAsync(new Commande { CommandeId = Guid.NewGuid() });
        
        // Act
        var result = await _service.CreateCommandeAsync(model);
        
        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Commande>()), Times.Once);
    }
}
```

### Conventions de Nommage des Tests

```csharp
// Pattern: MethodName_Scenario_ExpectedResult
[Fact]
public async Task CreateCommandeAsync_ValidModel_ReturnsSuccess() { }

[Fact]
public async Task CreateCommandeAsync_InvalidModel_ReturnsFailure() { }

[Fact]
public async Task CreateCommandeAsync_QuotaExceeded_ThrowsException() { }
```

## 📋 Checklist de Revue de Code

Avant chaque commit, vérifier :

### Code Quality
- [ ] Pas de code commenté (supprimer ou décommenter)
- [ ] Pas de strings magiques (utiliser les constantes)
- [ ] Pas de nombres magiques (utiliser les constantes)
- [ ] Nommage cohérent et descriptif
- [ ] Méthodes < 50 lignes
- [ ] Classes < 300 lignes
- [ ] Pas de duplication de code

### Documentation
- [ ] Commentaires XML sur les méthodes publiques
- [ ] Commentaires explicatifs sur la logique complexe
- [ ] README mis à jour si nécessaire

### Sécurité
- [ ] Pas de données sensibles en clair
- [ ] Validation des entrées utilisateur
- [ ] Gestion appropriée des erreurs
- [ ] Logging sans données sensibles

### Performance
- [ ] Requêtes LINQ optimisées
- [ ] Utilisation de `AsNoTracking()` pour la lecture seule
- [ ] Pas de N+1 queries
- [ ] Pagination pour les grandes listes

### Tests
- [ ] Tests unitaires ajoutés/mis à jour
- [ ] Tests passent tous
- [ ] Couverture de code maintenue/améliorée

## 🔧 Outils Recommandés

### Extensions Visual Studio / VS Code
- **ReSharper** : Refactorisation et suggestions
- **SonarLint** : Détection de code smell
- **CodeMaid** : Nettoyage et formatage
- **EditorConfig** : Configuration du formatage

### Configuration EditorConfig

Créer un fichier `.editorconfig` à la racine :

```ini
root = true

[*.cs]
indent_style = space
indent_size = 4
end_of_line = crlf
charset = utf-8
trim_trailing_whitespace = true
insert_final_newline = true

# Naming conventions
dotnet_naming_rule.interfaces_should_be_prefixed_with_i.severity = warning
dotnet_naming_rule.interfaces_should_be_prefixed_with_i.symbols = interface
dotnet_naming_rule.interfaces_should_be_prefixed_with_i.style = begins_with_i

# Code style
csharp_prefer_braces = true:warning
csharp_prefer_simple_using_statement = true:suggestion
csharp_style_namespace_declarations = file_scoped:warning
```

## 📚 Ressources

- [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [Clean Code Principles](https://www.freecodecamp.org/news/clean-coding-for-beginners/)
- [SOLID Principles](https://www.digitalocean.com/community/conceptual_articles/s-o-l-i-d-the-first-five-principles-of-object-oriented-design)

---

**Ces standards sont vivants et peuvent évoluer. Toute suggestion d'amélioration est bienvenue !**

*Dernière mise à jour : 10 février 2026*
