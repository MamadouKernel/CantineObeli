using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Obeli_K.Data;
using Obeli_K.Hubs;
using Obeli_K.Models;
using Obeli_K.Models.Enums;
using Obeli_K.Services;
using Obeli_K.Services.Configuration;
using Obeli_K.Services.Security;
using Obeli_K.Services.Users;
using OfficeOpenXml;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// Configuration EPPlus pour éviter l'erreur de licence
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

// Configuration de la base de données selon le provider choisi
var databaseProvider = config.GetValue<string>("DatabaseProvider") ?? "SqlServer";
Console.WriteLine($"🔧 Provider de base de données: {databaseProvider}");

builder.Services.AddDbContext<ObeliDbContext>(opts =>
{
    switch (databaseProvider.ToLower())
    {
        case "postgresql":
        case "postgres":
            var postgresConnection = config.GetConnectionString("PostgreSqlConnection");
            if (string.IsNullOrEmpty(postgresConnection))
            {
                throw new InvalidOperationException("La chaîne de connexion PostgreSqlConnection est manquante dans appsettings.json");
            }
            opts.UseNpgsql(postgresConnection);
            Console.WriteLine("✅ Configuration PostgreSQL activée");
            break;
            
        case "sqlserver":
        case "mssql":
        default:
            var sqlServerConnection = config.GetConnectionString("SqlServerConnection");
            if (string.IsNullOrEmpty(sqlServerConnection))
            {
                throw new InvalidOperationException("La chaîne de connexion SqlServerConnection est manquante dans appsettings.json");
            }
            opts.UseSqlServer(sqlServerConnection);
            Console.WriteLine("✅ Configuration SQL Server activée");
            break;
    }
});

builder.Services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<Obeli_K.Services.Configuration.IConfigurationService, Obeli_K.Services.Configuration.ConfigurationService>();
builder.Services.AddScoped<Obeli_K.Services.ICommandeAutomatiqueService, Obeli_K.Services.CommandeAutomatiqueService>();
builder.Services.AddScoped<Obeli_K.Services.IFacturationService, Obeli_K.Services.FacturationService>();
builder.Services.AddScoped<ExcelExportService>();
builder.Services.AddScoped<GroupeNonCitInitializationService>();

// Services automatiques
builder.Services.AddHostedService<FermetureAutomatiqueService>();
builder.Services.AddHostedService<FacturationAutomatiqueService>();
builder.Services.AddHostedService<ChangementStatutAutomatiqueService>();




// 2) Auth Cookie (sans Identity)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/Auth/Login";
        o.AccessDeniedPath = "/Error/Unauthorized";
        o.Cookie.HttpOnly = true;
        o.Cookie.SameSite = SameSiteMode.Lax; // défaut ok pour nav 1st-party
        o.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        o.SlidingExpiration = true;
        o.ExpireTimeSpan = TimeSpan.FromHours(1); // Durée par défaut, peut être surchargée par les propriétés d'authentification
    });

// 3) MVC
builder.Services.AddControllersWithViews();

// 4) SignalR
builder.Services.AddSignalR();

// 5) Services de reporting automatique
builder.Services.AddHostedService<Obeli_K.Services.ReportingAutomatiqueService>();

// 6) Service de fermeture automatique des commandes
builder.Services.AddHostedService<Obeli_K.Services.FermetureAutomatiqueService>();

var app = builder.Build();

app.MapHub<NotificationsHub>("/hubs/notifications");

// 4) DB migrate + seed au démarrage
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ObeliDbContext>();
    
    // Vérifier si la base de données existe et appliquer les migrations
    try
    {
        Console.WriteLine("🔗 Test de connexion à la base de données...");
        
        // Test de connexion simple d'abord
        await db.Database.OpenConnectionAsync();
        await db.Database.CloseConnectionAsync();
        Console.WriteLine("✅ Connexion à la base de données réussie");
        
        Console.WriteLine("📊 Application des migrations...");
        await db.Database.MigrateAsync();
        Console.WriteLine("✅ Migrations appliquées avec succès");
    }
    catch (InvalidOperationException ex) when (ex.Message.Contains("PendingModelChangesWarning"))
    {
        // Supprimer l'avertissement et continuer
        Console.WriteLine("Avertissement de changements en attente ignoré - continuer le démarrage");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Erreur de connexion à la base de données: {ex.Message}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"❌ Erreur interne: {ex.InnerException.Message}");
        }
        Console.WriteLine("🔄 Vérifiez que SQL Server est démarré et que la base de données existe");
        throw; // Re-throw pour arrêter l'application
    }
    
    // Seeding conditionnel - peut être désactivé en production
    var enableSeeding = Environment.GetEnvironmentVariable("OBELI_ENABLE_SEEDING") ?? "true";
    if (bool.TryParse(enableSeeding, out var shouldSeed) && shouldSeed)
    {
        await SeedAsync(scope.ServiceProvider);
    }
    else
    {
        Console.WriteLine("⚠️  Seeding désactivé par la variable d'environnement OBELI_ENABLE_SEEDING");
    }
}

// 5) Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Route pour permettre /{controller} => action Index par défaut
app.MapControllerRoute(
    name: "controller_only",
    pattern: "{controller}",
    defaults: new { action = "Index" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");


// ======================
// Seed minimal (bcrypt) — corrigé avec Département par défaut
// ======================
static async Task SeedAsync(IServiceProvider services)
{
    Console.WriteLine("🌱 Début du seeding de la base de données...");
    var db = services.GetRequiredService<ObeliDbContext>();

    // Les rôles sont maintenant définis dans l'enum RoleType
    // Plus besoin d'initialiser les rôles en base de données

    // 7) Sites par défaut (maintenant enums)
    // Les sites sont maintenant définis dans l'enum SiteType
    // Plus besoin d'initialiser les sites en base de données

    // 8) Direction par défaut (⚠ requis car Utilisateur.DirectionId peut être nullable)
    var dirDefaut = await db.Directions.FirstOrDefaultAsync(d => d.Nom == "Direction Générale" && d.Supprimer == 0);
    if (dirDefaut is null)
    {
        dirDefaut = new Direction
        {
            Id = Guid.NewGuid(),
            Nom = "Direction Générale",
            Description = "Direction par défaut",
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "seed",
            ModifiedBy = "seed",
            Supprimer = 0
        };
        db.Directions.Add(dirDefaut);
        Console.WriteLine("✅ Direction par défaut 'Direction Générale' créée");
    }
    else
    {
        Console.WriteLine("✅ Direction par défaut 'Direction Générale' existe déjà");
    }

    // 8.1) Fonction par défaut (⚠ requis car Utilisateur.FonctionId est non-nullable)
    var fonctionDefaut = await db.Fonctions.FirstOrDefaultAsync(f => f.Nom == "Fonction Général" && f.Supprimer == 0);
    if (fonctionDefaut is null)
    {
        fonctionDefaut = new Fonction
        {
            Id = Guid.NewGuid(),
            Nom = "Fonction Général",
            Description = "Fonction par défaut pour les utilisateurs",
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "seed",
            ModifiedBy = "seed",
            Supprimer = 0
        };
        db.Fonctions.Add(fonctionDefaut);
        Console.WriteLine("✅ Fonction par défaut 'Fonction Général' créée");
    }
    else
    {
        Console.WriteLine("✅ Fonction par défaut 'Fonction Général' existe déjà");
    }

    // Sauvegarder les entités de référence avant de créer l'utilisateur
    await db.SaveChangesAsync();

    // S'assurer que la colonne de paramétrage existe (compat schéma)
    try
    {
        await db.Database.ExecuteSqlRawAsync(@"IF COL_LENGTH('Parametrages','JourFermetureCommande') IS NULL
BEGIN
    ALTER TABLE Parametrages ADD JourFermetureCommande int NOT NULL CONSTRAINT DF_Param_JourFermeture DEFAULT(5);
END");
    }
    catch { /* ignore */ }

    // 9) Admin par défaut (si absent)
    var adminExiste = await db.Utilisateurs.AnyAsync(u => u.UserName == "admin");
    if (!adminExiste)
    {
        var bcryptHash = BCrypt.Net.BCrypt.HashPassword("admin123", workFactor: 12);

        db.Utilisateurs.Add(new Utilisateur
        {
            Id = Guid.NewGuid(),
            Nom = "Administrateur",
            Prenoms = "Système",
            Email = "admin@obeli.local",
            UserName = "admin",
            MotDePasseHash = bcryptHash,
            Role = RoleType.Admin,
            DirectionId = dirDefaut.Id, // ✅ FK valide
            FonctionId = fonctionDefaut.Id, // ✅ FK valide
            Site = SiteType.CIT_Billing,              // Site par défaut
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow,
            MustResetPassword = true,
            Supprimer = 0
        });
        
        Console.WriteLine("✅ Utilisateur administrateur par défaut créé:");
        Console.WriteLine("   UserName: admin");
        Console.WriteLine("   Mot de passe: admin123");
        Console.WriteLine("   ⚠️  IMPORTANT: Changez ce mot de passe immédiatement !");
    }
    else
    {
        Console.WriteLine("✅ Utilisateur administrateur existe déjà.");
    }

    // NOTE: Seul l'utilisateur admin est créé par défaut
    // Les autres utilisateurs (RH, Prestataire) doivent être créés manuellement via l'interface


    // NOTE: Aucune donnée de test n'est créée par défaut
    // Les menus et commandes doivent être créés manuellement via l'interface

    // NOTE: Aucune direction, groupe ou formule par défaut n'est créée
    // Toutes ces données doivent être créées manuellement via l'interface

    await db.SaveChangesAsync();
    
    // 13) Initialiser les configurations par défaut
    var configurationService = services.GetRequiredService<Obeli_K.Services.Configuration.IConfigurationService>();
    var commandeAutomatiqueService = services.GetRequiredService<Obeli_K.Services.ICommandeAutomatiqueService>();
    await commandeAutomatiqueService.InitialiserConfigurationsParDefautAsync();
    Console.WriteLine("✅ Configurations par défaut initialisées");
    
    Console.WriteLine("✅ Seeding de la base de données terminé");
}

// Fonction helper supprimée car plus utilisée


app.Run();