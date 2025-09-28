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

// 1) EF Core (SQL Server) — lit "DefaultConnection"
builder.Services.AddDbContext<ObeliDbContext>(opts =>
    opts.UseSqlServer(config.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<Obeli_K.Services.Configuration.IConfigurationService, Obeli_K.Services.Configuration.ConfigurationService>();
builder.Services.AddScoped<Obeli_K.Services.ICommandeAutomatiqueService, Obeli_K.Services.CommandeAutomatiqueService>();




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
        o.ExpireTimeSpan = TimeSpan.FromHours(1);
    });

// 3) MVC
builder.Services.AddControllersWithViews();

// 4) SignalR
builder.Services.AddSignalR();

// 5) Services de reporting automatique
builder.Services.AddHostedService<Obeli_K.Services.ReportingAutomatiqueService>();

var app = builder.Build();

app.MapHub<NotificationsHub>("/hubs/notifications");

// 4) DB migrate + seed au démarrage
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ObeliDbContext>();
    
    // Vérifier si la base de données existe et appliquer les migrations
    try
    {
        await db.Database.MigrateAsync();
    }
    catch (InvalidOperationException ex) when (ex.Message.Contains("PendingModelChangesWarning"))
    {
        // Supprimer l'avertissement et continuer
        Console.WriteLine("Avertissement de changements en attente ignoré - continuer le démarrage");
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

    // 8) Département par défaut (⚠ requis car Utilisateur.DepartementId est non-nullable)
    var depDefaut = await db.Departements.FirstOrDefaultAsync(d => d.Nom == "Direction Général" && d.Supprimer == 0);
    if (depDefaut is null)
    {
        depDefaut = new Departement
        {
            Id = Guid.NewGuid(),
            Nom = "Direction Général",
            Description = "Direction par défaut",
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "seed",
            ModifiedBy = "seed",
            Supprimer = 0
        };
        db.Departements.Add(depDefaut);
        Console.WriteLine("✅ Département par défaut 'Direction Général' créé");
    }
    else
    {
        Console.WriteLine("✅ Département par défaut 'Direction Général' existe déjà");
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
            DepartementId = depDefaut.Id, // ✅ FK valide
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

    // Utilisateur RH par défaut
    var rhExiste = await db.Utilisateurs.AnyAsync(u => u.UserName == "rh");
    if (!rhExiste)
    {
        var pwdRH = Environment.GetEnvironmentVariable("OBELI_RH_DEFAULT_PWD") ?? "rh123";
        var bcryptHashRH = BCrypt.Net.BCrypt.HashPassword(pwdRH, workFactor: 12);

        db.Utilisateurs.Add(new Utilisateur
        {
            Id = Guid.NewGuid(),
            Nom = "Ressources",
            Prenoms = "Humaines",
            Email = "rh@obeli.local",
            UserName = "rh",
            MotDePasseHash = bcryptHashRH,
            Role = RoleType.RH,
            DepartementId = depDefaut.Id,
            FonctionId = fonctionDefaut.Id,
            Site = SiteType.CIT_Billing,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow,
            MustResetPassword = true,
            Supprimer = 0
        });

        Console.WriteLine("✅ Utilisateur RH par défaut créé:");
        Console.WriteLine("   UserName: rh");
        Console.WriteLine($"   Mot de passe: {pwdRH}");
        Console.WriteLine("   ⚠️  IMPORTANT: Changez ce mot de passe immédiatement !");
    }
    else
    {
        Console.WriteLine("✅ Utilisateur RH existe déjà.");
    }

    // Prestataire Cantine par défaut
    var prestaExiste = await db.Utilisateurs.AnyAsync(u => u.UserName == "prestataire");
    if (!prestaExiste)
    {
        // Astuce: passer par variable d'env pour éviter le hard-code en prod
        var pwd = Environment.GetEnvironmentVariable("OBELI_PRESTA_DEFAULT_PWD") ?? "presta123";
        var bcryptHashPresta = BCrypt.Net.BCrypt.HashPassword(pwd, workFactor: 12);

        db.Utilisateurs.Add(new Utilisateur
        {
            Id = Guid.NewGuid(),
            Nom = "Prestataire",
            Prenoms = "Cantine",
            Email = "prestataire@obeli.local",
            UserName = "prestataire",
            MotDePasseHash = bcryptHashPresta,
            Role = RoleType.PrestataireCantine, // ⇦ assure-toi que l’enum contient bien cette valeur
            DepartementId = depDefaut.Id,
            FonctionId = fonctionDefaut.Id,
            Site = SiteType.CIT_Terminal,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow,
            MustResetPassword = true,
            Supprimer = 0
        });

        Console.WriteLine("✅ Utilisateur prestataire cantine par défaut créé:");
        Console.WriteLine("   UserName: prestataire");
        Console.WriteLine($"   Mot de passe: {pwd}");
        Console.WriteLine("   ⚠️  IMPORTANT: Changez ce mot de passe immédiatement !");
    }
    else
    {
        Console.WriteLine("✅ Utilisateur prestataire cantine existe déjà.");
    }


    // 10) Données de test pour les menus de la semaine
    var debutSemaine = GetDebutSemaine(DateTime.Now);
    var finSemaine = debutSemaine.AddDays(6);
    
    // Vérifier si TOUS les menus de la semaine existent (3 types × 7 jours = 21 menus)
    var menusExistants = await db.FormulesJour
        .Where(f => f.Date >= debutSemaine && f.Date <= finSemaine)
        .CountAsync();
    
    if (menusExistants < 21) // 3 types de menus × 7 jours
    {
        Console.WriteLine($"📋 Création des menus manquants pour la semaine courante ({menusExistants}/21 existants)");
        var joursSemaine = new[] { "Lundi", "Mardi", "Mercredi", "Jeudi", "Vendredi", "Samedi", "Dimanche" };
        
        for (int i = 0; i < 7; i++)
        {
            var dateJour = debutSemaine.AddDays(i);
            var jourNom = joursSemaine[i];
            
            // Vérifier si le menu Amélioré existe déjà pour ce jour
            var menuAmelioreExiste = await db.FormulesJour
                .AnyAsync(f => f.Date.Date == dateJour.Date && f.NomFormule == "Amélioré");
            
            if (!menuAmelioreExiste)
            {
                // Menu Amélioré
                db.FormulesJour.Add(new FormuleJour
            {
                IdFormule = Guid.NewGuid(),
                NomFormule = "Amélioré",
                Date = dateJour,
                Plat = jourNom switch
                {
                    "Lundi" => "Ballotine de volaille",
                    "Mardi" => "Brochettes de Bœuf",
                    "Mercredi" => "Poulet braisé",
                    "Jeudi" => "Poisson grillé",
                    "Vendredi" => "Agneau rôti",
                    "Samedi" => "Côte de porc",
                    "Dimanche" => "Poulet rôti",
                    _ => "Plat du jour"
                },
                Garniture = jourNom switch
                {
                    "Lundi" => "Riz safrané",
                    "Mardi" => "Pommes de Terre Rôties",
                    "Mercredi" => "Riz basmati",
                    "Jeudi" => "Riz blanc",
                    "Vendredi" => "Pommes de terre",
                    "Samedi" => "Riz parfumé",
                    "Dimanche" => "Riz créole",
                    _ => "Garniture du jour"
                },
                Entree = jourNom switch
                {
                    "Lundi" => "Cocktail de crudités",
                    "Mardi" => "Mini wrap légumes",
                    "Mercredi" => "Salade composée",
                    "Jeudi" => "Salade verte",
                    "Vendredi" => "Salade de tomates",
                    "Samedi" => "Salade de carottes",
                    "Dimanche" => "Salade de chou",
                    _ => "Entrée du jour"
                },
                Dessert = jourNom switch
                {
                    "Lundi" => "Amandine",
                    "Mardi" => "Yaourt",
                    "Mercredi" => "Fruit de saison",
                    "Jeudi" => "Compote",
                    "Vendredi" => "Tarte aux fruits",
                    "Samedi" => "Crème dessert",
                    "Dimanche" => "Salade de fruits",
                    _ => "Dessert du jour"
                },
                Marge = 15,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "seed"
            });
            }
            
            // Vérifier si le menu Standard 1 existe déjà pour ce jour
            var menuStandard1Existe = await db.FormulesJour
                .AnyAsync(f => f.Date.Date == dateJour.Date && f.NomFormule == "Standard 1");
            
            if (!menuStandard1Existe)
            {
                // Menu Standard 1
                db.FormulesJour.Add(new FormuleJour
            {
                IdFormule = Guid.NewGuid(),
                NomFormule = "Standard 1",
                Date = dateJour,
                PlatStandard1 = jourNom switch
                {
                    "Lundi" => "Poisson braisé",
                    "Mardi" => "Poulet grillé",
                    "Mercredi" => "Bœuf sauce",
                    "Jeudi" => "Agneau mijoté",
                    "Vendredi" => "Porc aux légumes",
                    "Samedi" => "Poulet aux herbes",
                    "Dimanche" => "Poisson aux épices",
                    _ => "Plat Standard 1"
                },
                GarnitureStandard1 = jourNom switch
                {
                    "Lundi" => "Riz blanc",
                    "Mardi" => "Frites",
                    "Mercredi" => "Riz parfumé",
                    "Jeudi" => "Pommes de terre",
                    "Vendredi" => "Riz créole",
                    "Samedi" => "Légumes sautés",
                    "Dimanche" => "Riz basmati",
                    _ => "Garniture Standard 1"
                },
                Marge = 2000,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "seed"
            });
            }
            
            // Vérifier si le menu Standard 2 existe déjà pour ce jour
            var menuStandard2Existe = await db.FormulesJour
                .AnyAsync(f => f.Date.Date == dateJour.Date && f.NomFormule == "Standard 2");
            
            if (!menuStandard2Existe)
            {
                // Menu Standard 2
                db.FormulesJour.Add(new FormuleJour
            {
                IdFormule = Guid.NewGuid(),
                NomFormule = "Standard 2",
                Date = dateJour,
                PlatStandard2 = jourNom switch
                {
                    "Lundi" => "Poulet aux légumes",
                    "Mardi" => "Poisson grillé",
                    "Mercredi" => "Agneau rôti",
                    "Jeudi" => "Bœuf aux carottes",
                    "Vendredi" => "Poulet rôti",
                    "Samedi" => "Poisson braisé",
                    "Dimanche" => "Porc aux herbes",
                    _ => "Plat Standard 2"
                },
                GarnitureStandard2 = jourNom switch
                {
                    "Lundi" => "Riz parfumé",
                    "Mardi" => "Riz blanc",
                    "Mercredi" => "Pommes de terre",
                    "Jeudi" => "Riz créole",
                    "Vendredi" => "Légumes sautés",
                    "Samedi" => "Riz basmati",
                    "Dimanche" => "Frites",
                    _ => "Garniture Standard 2"
                },
                Marge = 15,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "seed"
            });
            }
        }
        
        Console.WriteLine("✅ Menus de test créés pour la semaine en cours");
    }
    else
    {
        Console.WriteLine("✅ Menus de la semaine courante existent déjà (21/21)");
    }
    
    // 11) Données de test pour les commandes
    var commandesExistantes = await db.Commandes
        .Where(c => c.DateConsommation >= debutSemaine && c.DateConsommation <= finSemaine)
        .CountAsync();
    
    if (commandesExistantes == 0) // Seulement si aucune commande n'existe
    {
        var admin = await db.Utilisateurs.FirstOrDefaultAsync(u => u.UserName == "admin");
        if (admin != null)
        {
            // Commande pour mardi (Amélioré)
            var menuMardiAmeliore = await db.FormulesJour
                .FirstOrDefaultAsync(f => f.Date.Date == debutSemaine.AddDays(1).Date && f.NomFormule == "Amélioré");
            
            if (menuMardiAmeliore != null)
            {
                db.Commandes.Add(new Commande
                {
                    IdCommande = Guid.NewGuid(),
                    Date = DateTime.UtcNow.AddDays(-1), // Commande passée hier
                    DateConsommation = debutSemaine.AddDays(1), // Pour mardi
                    StatusCommande = 1, // Precommander
                    Montant = menuMardiAmeliore.Marge ?? 0,
                    UtilisateurId = admin.Id,
                    IdFormule = menuMardiAmeliore.IdFormule,
                    PeriodeService = Obeli_K.Enums.Periode.Jour,
                    Quantite = 1,
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = "seed"
                });
            }
            
            Console.WriteLine("✅ Commandes de test créées");
        }
    }
    else
    {
        Console.WriteLine($"✅ Commandes de la semaine courante existent déjà ({commandesExistantes} commandes)");
    }

    // 12) Données de test pour la semaine + 1
    var debutSemaineSuivante = debutSemaine.AddDays(7);
    var finSemaineSuivante = finSemaine.AddDays(7);
    
    var menusSemaineSuivante = await db.FormulesJour
        .Where(f => f.Date >= debutSemaineSuivante && f.Date <= finSemaineSuivante)
        .CountAsync();
    
    if (menusSemaineSuivante < 21) // 3 types de menus × 7 jours
    {
        Console.WriteLine($"📋 Création des menus manquants pour la semaine + 1 ({menusSemaineSuivante}/21 existants)");
        for (int i = 0; i < 7; i++)
        {
            var dateJour = debutSemaineSuivante.AddDays(i);
            var jourNom = dateJour.ToString("dddd", new System.Globalization.CultureInfo("fr-FR"));
            
            // Vérifier si le menu Amélioré existe déjà pour ce jour
            var menuAmelioreExiste = await db.FormulesJour
                .AnyAsync(f => f.Date.Date == dateJour.Date && f.NomFormule == "Amélioré");
            
            if (!menuAmelioreExiste)
            {
                // Menu Amélioré pour la semaine + 1
                db.FormulesJour.Add(new FormuleJour
            {
                IdFormule = Guid.NewGuid(),
                NomFormule = "Amélioré",
                Date = dateJour,
                Plat = jourNom switch
                {
                    "lundi" => "Saumon grillé aux herbes",
                    "mardi" => "Magret de canard",
                    "mercredi" => "Côte de bœuf",
                    "jeudi" => "Filet de porc",
                    "vendredi" => "Dos de cabillaud",
                    "samedi" => "Cuisse de poulet",
                    "dimanche" => "Gigot d'agneau",
                    _ => "Plat amélioré"
                },
                Garniture = jourNom switch
                {
                    "lundi" => "Riz pilaf",
                    "mardi" => "Pommes de terre sautées",
                    "mercredi" => "Gratin dauphinois",
                    "jeudi" => "Riz créole",
                    "vendredi" => "Légumes vapeur",
                    "samedi" => "Riz basmati",
                    "dimanche" => "Haricots verts",
                    _ => "Garniture améliorée"
                },
                Entree = jourNom switch
                {
                    "lundi" => "Carpaccio de bœuf",
                    "mardi" => "Tartare de saumon",
                    "mercredi" => "Salade de chèvre chaud",
                    "jeudi" => "Velouté de potiron",
                    "vendredi" => "Salade de crevettes",
                    "samedi" => "Foie gras",
                    "dimanche" => "Salade de homard",
                    _ => "Entrée améliorée"
                },
                Dessert = jourNom switch
                {
                    "lundi" => "Tiramisu",
                    "mardi" => "Crème brûlée",
                    "mercredi" => "Tarte tatin",
                    "jeudi" => "Profiteroles",
                    "vendredi" => "Mousse au chocolat",
                    "samedi" => "Île flottante",
                    "dimanche" => "Charlotte aux fruits",
                    _ => "Dessert amélioré"
                },
                Marge = 3,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "seed"
            });
            }
            
            // Vérifier si le menu Standard 1 existe déjà pour ce jour
            var menuStandard1Existe = await db.FormulesJour
                .AnyAsync(f => f.Date.Date == dateJour.Date && f.NomFormule == "Standard 1");
            
            if (!menuStandard1Existe)
            {
                // Menu Standard 1 pour la semaine + 1
                db.FormulesJour.Add(new FormuleJour
            {
                IdFormule = Guid.NewGuid(),
                NomFormule = "Standard 1",
                Date = dateJour,
                PlatStandard1 = jourNom switch
                {
                    "lundi" => "Poulet rôti",
                    "mardi" => "Poisson pané",
                    "mercredi" => "Bœuf bourguignon",
                    "jeudi" => "Côtelette de porc",
                    "vendredi" => "Filet de colin",
                    "samedi" => "Poulet aux champignons",
                    "dimanche" => "Rôti de bœuf",
                    _ => "Plat Standard 1"
                },
                GarnitureStandard1 = jourNom switch
                {
                    "lundi" => "Riz blanc",
                    "mardi" => "Pommes de terre",
                    "mercredi" => "Pâtes",
                    "jeudi" => "Riz nature",
                    "vendredi" => "Légumes",
                    "samedi" => "Riz parfumé",
                    "dimanche" => "Pommes de terre",
                    _ => "Garniture Standard 1"
                },
                Marge = 0,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "seed"
            });
            }
            
            // Vérifier si le menu Standard 2 existe déjà pour ce jour
            var menuStandard2Existe = await db.FormulesJour
                .AnyAsync(f => f.Date.Date == dateJour.Date && f.NomFormule == "Standard 2");
            
            if (!menuStandard2Existe)
            {
                // Menu Standard 2 pour la semaine + 1
                db.FormulesJour.Add(new FormuleJour
            {
                IdFormule = Guid.NewGuid(),
                NomFormule = "Standard 2",
                Date = dateJour,
                PlatStandard2 = jourNom switch
                {
                    "lundi" => "Sauté de porc",
                    "mardi" => "Poisson sauce",
                    "mercredi" => "Ragout de bœuf",
                    "jeudi" => "Poulet aux légumes",
                    "vendredi" => "Filet de merlu",
                    "samedi" => "Poulet grillé",
                    "dimanche" => "Bœuf aux oignons",
                    _ => "Plat Standard 2"
                },
                GarnitureStandard2 = jourNom switch
                {
                    "lundi" => "Riz créole",
                    "mardi" => "Pommes de terre",
                    "mercredi" => "Riz nature",
                    "jeudi" => "Légumes sautés",
                    "vendredi" => "Riz blanc",
                    "samedi" => "Pommes de terre",
                    "dimanche" => "Riz parfumé",
                    _ => "Garniture Standard 2"
                },
                Marge = 0,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "seed"
            });
            }
        }
        
        Console.WriteLine("✅ Menus de la semaine + 1 créés");
    }
    else
    {
        Console.WriteLine("✅ Menus de la semaine + 1 existent déjà (21/21)");
    }

    // Directions par défaut
    var directionsParDefaut = new[]
    {
        new { Nom = "Direction Générale", Code = "DG" },
        new { Nom = "Direction des Ressources Humaines", Code = "DRH" },
        new { Nom = "Direction Financière", Code = "DF" },
        new { Nom = "Direction Technique", Code = "DT" },
        new { Nom = "Direction Commerciale", Code = "DC" },
        new { Nom = "Direction Administrative", Code = "DA" }
    };

    foreach (var dir in directionsParDefaut)
    {
        var directionExiste = await db.Directions.AnyAsync(d => d.Nom == dir.Nom && d.Supprimer == 0);
        if (!directionExiste)
        {
            db.Directions.Add(new Direction
            {
                Id = Guid.NewGuid(),
                Nom = dir.Nom,
                Code = dir.Code,
                Description = $"Direction {dir.Nom}",
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "seed",
                Supprimer = 0
            });
            Console.WriteLine($"✅ Direction '{dir.Nom}' créée");
        }
    }

    // Groupes non-CIT par défaut
    var douaniersExiste = await db.GroupesNonCit.AnyAsync(g => g.Nom == "Douaniers" && g.Supprimer == 0);
    if (!douaniersExiste)
    {
        db.GroupesNonCit.Add(new GroupeNonCit
        {
            Id = Guid.NewGuid(),
            Nom = "Douaniers",
            Description = "Groupe des agents des douanes",
            // CodeGroupe = "DOU",
            // QuotaJournalier = 100,
            // RestrictionFormuleStandard = true,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "seed",
            Supprimer = 0
        });
        Console.WriteLine("✅ Groupe non-CIT 'Douaniers' créé");
    }

    var visiteursExiste = await db.GroupesNonCit.AnyAsync(g => g.Nom == "Visiteurs" && g.Supprimer == 0);
    if (!visiteursExiste)
    {
        db.GroupesNonCit.Add(new GroupeNonCit
        {
            Id = Guid.NewGuid(),
            Nom = "Visiteurs",
            Description = "Visiteurs occasionnels",
            // CodeGroupe = "VIS",
            // QuotaJournalier = 50,
            // RestrictionFormuleStandard = false,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "seed",
            Supprimer = 0
        });
        Console.WriteLine("✅ Groupe non-CIT 'Visiteurs' créé");
    }

    var prestatairesExiste = await db.GroupesNonCit.AnyAsync(g => g.Nom == "Prestataires Externes" && g.Supprimer == 0);
    if (!prestatairesExiste)
    {
        db.GroupesNonCit.Add(new GroupeNonCit
        {
            Id = Guid.NewGuid(),
            Nom = "Prestataires Externes",
            Description = "Prestataires et consultants externes",
            // CodeGroupe = "PRE",
            // QuotaJournalier = 30,
            // RestrictionFormuleStandard = false,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "seed",
            Supprimer = 0
        });
        Console.WriteLine("✅ Groupe non-CIT 'Prestataires Externes' créé");
    }

    // Formules pour aujourd'hui (pour tester les commandes instantanées)
    var aujourdhui = DateTime.Today;
    var formuleAujourdhuiExiste = await db.FormulesJour.AnyAsync(f => f.Date.Date == aujourdhui && f.Supprimer == 0);
    if (!formuleAujourdhuiExiste)
    {
        // Formule Standard pour aujourd'hui
        db.FormulesJour.Add(new FormuleJour
        {
            IdFormule = Guid.NewGuid(),
            Date = aujourdhui,
            NomFormule = "Formule Standard",
            Entree = "",
            Plat = "",
            Garniture = "",
            Dessert = "",
            PlatStandard1 = "Sauce graine",
            GarnitureStandard1 = "Viande de bœuf",
            PlatStandard2 = "Attieke",
            GarnitureStandard2 = "Poisson grillé",
            Feculent = "Riz",
            Legumes = "Légumes de saison",
            Marge = 0,
            Statut = 1,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "seed",
            Supprimer = 0
        });

        // Formule Améliorée pour aujourd'hui
        db.FormulesJour.Add(new FormuleJour
        {
            IdFormule = Guid.NewGuid(),
            Date = aujourdhui,
            NomFormule = "Formule Améliorée",
            Entree = "Salade verte aux tomates",
            Plat = "Poulet rôti aux herbes",
            Garniture = "Riz pilaf aux légumes",
            Dessert = "Fruit de saison",
            PlatStandard1 = "",
            GarnitureStandard1 = "",
            PlatStandard2 = "",
            GarnitureStandard2 = "",
            Feculent = "Riz",
            Legumes = "Légumes de saison",
            Marge = 0,
            Statut = 1,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "seed",
            Supprimer = 0
        });

        Console.WriteLine("✅ Formules pour aujourd'hui créées");
    }

    await db.SaveChangesAsync();
    
    // 13) Initialiser les configurations par défaut
    var configurationService = services.GetRequiredService<Obeli_K.Services.Configuration.IConfigurationService>();
    var commandeAutomatiqueService = services.GetRequiredService<Obeli_K.Services.ICommandeAutomatiqueService>();
    await commandeAutomatiqueService.InitialiserConfigurationsParDefautAsync();
    Console.WriteLine("✅ Configurations par défaut initialisées");
    
    Console.WriteLine("✅ Seeding de la base de données terminé");
}

// Fonction helper pour calculer le début de semaine
static DateTime GetDebutSemaine(DateTime date)
{
    var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
    return date.AddDays(-1 * diff).Date;
}


app.Run();