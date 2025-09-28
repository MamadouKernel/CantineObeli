using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Obeli_K.Data;
using Obeli_K.Models;

namespace Obeli_K.Controllers
{
    public class DebugController : Controller
    {
        private readonly ObeliDbContext _context;
        private readonly ILogger<DebugController> _logger;

        public DebugController(ObeliDbContext context, ILogger<DebugController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> CheckDatabase()
        {
            var result = new List<string>();

            // 0. Vérifier les doublons
            result.Add("=== VÉRIFICATION DES DOUBLONS ===");
            
            // Doublons de départements
            var departementsDoublons = await _context.Departements
                .Where(d => d.Supprimer == 0)
                .GroupBy(d => d.Nom)
                .Where(g => g.Count() > 1)
                .Select(g => new { Nom = g.Key, Count = g.Count() })
                .ToListAsync();
            
            if (departementsDoublons.Any())
            {
                result.Add("⚠️ DOUBLONS DÉPARTEMENTS:");
                foreach (var dup in departementsDoublons)
                {
                    result.Add($"  - {dup.Nom}: {dup.Count} occurrences");
                }
            }
            else
            {
                result.Add("✅ Aucun doublon de département");
            }
            
            // Doublons de fonctions
            var fonctionsDoublons = await _context.Fonctions
                .Where(f => f.Supprimer == 0)
                .GroupBy(f => f.Nom)
                .Where(g => g.Count() > 1)
                .Select(g => new { Nom = g.Key, Count = g.Count() })
                .ToListAsync();
            
            if (fonctionsDoublons.Any())
            {
                result.Add("⚠️ DOUBLONS FONCTIONS:");
                foreach (var dup in fonctionsDoublons)
                {
                    result.Add($"  - {dup.Nom}: {dup.Count} occurrences");
                }
            }
            else
            {
                result.Add("✅ Aucun doublon de fonction");
            }
            
            // Doublons d'utilisateurs
            var utilisateursDoublons = await _context.Utilisateurs
                .Where(u => u.Supprimer == 0)
                .GroupBy(u => u.UserName)
                .Where(g => g.Count() > 1)
                .Select(g => new { UserName = g.Key, Count = g.Count() })
                .ToListAsync();
            
            if (utilisateursDoublons.Any())
            {
                result.Add("⚠️ DOUBLONS UTILISATEURS:");
                foreach (var dup in utilisateursDoublons)
                {
                    result.Add($"  - {dup.UserName}: {dup.Count} occurrences");
                }
            }
            else
            {
                result.Add("✅ Aucun doublon d'utilisateur");
            }

            result.Add("");

            // 1. Vérifier les TypeFormule
            result.Add("=== TYPES DE FORMULES ===");
            var typesFormule = await _context.TypesFormule.ToListAsync();
            foreach (var type in typesFormule)
            {
                result.Add($"- {type.Nom} (ID: {type.Id})");
            }
            result.Add($"Total: {typesFormule.Count} types");
            result.Add("");

            // 2. Vérifier les FormuleJour
            result.Add("=== FORMULES DU JOUR ===");
            var formulesJour = await _context.FormulesJour
                .Include(f => f.NomFormuleNavigation)
                .OrderBy(f => f.Date)
                .ToListAsync();

            result.Add($"Total: {formulesJour.Count} formules");
            result.Add("");

            if (formulesJour.Any())
            {
                result.Add("Exemples de formules:");
                foreach (var formule in formulesJour.Take(5))
                {
                    result.Add($"- Date: {formule.Date:yyyy-MM-dd}");
                    result.Add($"  Type: {formule.NomFormuleNavigation?.Nom ?? "NULL"}");
                    result.Add($"  Plat: {formule.Plat ?? "NULL"}");
                    result.Add($"  PlatStandard1: {formule.PlatStandard1 ?? "NULL"}");
                    result.Add($"  PlatStandard2: {formule.PlatStandard2 ?? "NULL"}");
                    result.Add($"  Entree: {formule.Entree ?? "NULL"}");
                    result.Add($"  Dessert: {formule.Dessert ?? "NULL"}");
                    result.Add("");
                }
            }

            // 3. Vérifier la semaine du 15-21 septembre 2025
            result.Add("=== MENUS DE LA SEMAINE 15-21 SEPTEMBRE 2025 ===");
            var debutSemaine = new DateTime(2025, 9, 15);
            var finSemaine = new DateTime(2025, 9, 21);

            var menusSemaine = await _context.FormulesJour
                .Include(f => f.NomFormuleNavigation)
                .Where(f => f.Date >= debutSemaine && f.Date <= finSemaine)
                .OrderBy(f => f.Date)
                .ToListAsync();

            result.Add($"Menus trouvés: {menusSemaine.Count}");
            result.Add("");

            if (menusSemaine.Any())
            {
                var menusParDate = menusSemaine.GroupBy(m => m.Date.ToString("yyyy-MM-dd"));
                foreach (var groupe in menusParDate)
                {
                    result.Add($"{groupe.Key}:");
                    foreach (var menu in groupe)
                    {
                        result.Add($"  - {menu.NomFormuleNavigation?.Nom ?? "NULL"}: {menu.Plat ?? menu.PlatStandard1 ?? menu.PlatStandard2 ?? "Aucun plat"}");
                    }
                    result.Add("");
                }
            }
            else
            {
                result.Add("Aucun menu trouvé pour cette semaine.");
            }

            result.Add("=== FIN DE LA VÉRIFICATION ===");

            ViewBag.Result = result;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateTestData()
        {
            var result = new List<string>();
            result.Add("=== CRÉATION DE DONNÉES DE TEST ===");

            try
            {
                // 1. Créer les types de formules s'ils n'existent pas
                var typeAmeliore = await _context.TypesFormule.FirstOrDefaultAsync(t => t.Nom == "Amélioré");
                if (typeAmeliore == null)
                {
                    typeAmeliore = new TypeFormule
                    {
                        Id = Guid.NewGuid(),
                        Nom = "Amélioré",
                        Description = "Menu amélioré avec entrée, plat, garniture et dessert",
                        CreatedOn = DateTime.UtcNow,
                        CreatedBy = "debug"
                    };
                    _context.TypesFormule.Add(typeAmeliore);
                    result.Add("✅ Type 'Amélioré' créé");
                }

                var typeStandard1 = await _context.TypesFormule.FirstOrDefaultAsync(t => t.Nom == "Standard 1");
                if (typeStandard1 == null)
                {
                    typeStandard1 = new TypeFormule
                    {
                        Id = Guid.NewGuid(),
                        Nom = "Standard 1",
                        Description = "Menu standard 1 avec plat et garniture",
                        CreatedOn = DateTime.UtcNow,
                        CreatedBy = "debug"
                    };
                    _context.TypesFormule.Add(typeStandard1);
                    result.Add("✅ Type 'Standard 1' créé");
                }

                var typeStandard2 = await _context.TypesFormule.FirstOrDefaultAsync(t => t.Nom == "Standard 2");
                if (typeStandard2 == null)
                {
                    typeStandard2 = new TypeFormule
                    {
                        Id = Guid.NewGuid(),
                        Nom = "Standard 2",
                        Description = "Menu standard 2 avec plat et garniture",
                        CreatedOn = DateTime.UtcNow,
                        CreatedBy = "debug"
                    };
                    _context.TypesFormule.Add(typeStandard2);
                    result.Add("✅ Type 'Standard 2' créé");
                }

                await _context.SaveChangesAsync();

                // 2. Créer des menus pour la semaine du 15-21 septembre 2025
                var debutSemaine = new DateTime(2025, 9, 15);
                var finSemaine = new DateTime(2025, 9, 21);

                var menusExistants = await _context.FormulesJour
                    .Where(f => f.Date >= debutSemaine && f.Date <= finSemaine)
                    .CountAsync();

                if (menusExistants == 0)
                {
                    var menus = new List<FormuleJour>();

                    // Données de test basées sur votre tableau (semaine 15-21 septembre 2025)
                    var donneesMenus = new[]
                    {
                        // Lundi 15/09
                        new { Date = new DateTime(2025, 9, 15), Type = "Amélioré", Entree = "Salade de tomates", Plat = "Poulet rôti", Garniture = "Riz pilaf", Dessert = "Tarte aux fruits", PlatStandard1 = (string?)null, GarnitureStandard1 = (string?)null, PlatStandard2 = (string?)null, GarnitureStandard2 = (string?)null },
                        new { Date = new DateTime(2025, 9, 15), Type = "Standard 1", Entree = (string?)null, Plat = (string?)null, Garniture = (string?)null, Dessert = (string?)null, PlatStandard1 = "Sauce arachide", GarnitureStandard1 = "Viande", PlatStandard2 = (string?)null, GarnitureStandard2 = (string?)null },
                        new { Date = new DateTime(2025, 9, 15), Type = "Standard 2", Entree = (string?)null, Plat = (string?)null, Garniture = (string?)null, Dessert = (string?)null, PlatStandard1 = (string?)null, GarnitureStandard1 = (string?)null, PlatStandard2 = "Riz gras", GarnitureStandard2 = "Poisson" },

                        // Mardi 16/09
                        new { Date = new DateTime(2025, 9, 16), Type = "Amélioré", Entree = "Velouté de légumes", Plat = "Brochettes de bœuf", Garniture = "Pommes de terre", Dessert = "Yaourt nature", PlatStandard1 = (string?)null, GarnitureStandard1 = (string?)null, PlatStandard2 = (string?)null, GarnitureStandard2 = (string?)null },
                        new { Date = new DateTime(2025, 9, 16), Type = "Standard 1", Entree = (string?)null, Plat = (string?)null, Garniture = (string?)null, Dessert = (string?)null, PlatStandard1 = "Sauce graine", GarnitureStandard1 = "Poulet", PlatStandard2 = (string?)null, GarnitureStandard2 = (string?)null },
                        new { Date = new DateTime(2025, 9, 16), Type = "Standard 2", Entree = (string?)null, Plat = (string?)null, Garniture = (string?)null, Dessert = (string?)null, PlatStandard1 = (string?)null, GarnitureStandard1 = (string?)null, PlatStandard2 = "Attieke", GarnitureStandard2 = "Poisson" },

                        // Mercredi 17/09
                        new { Date = new DateTime(2025, 9, 17), Type = "Amélioré", Entree = "Salade verte", Plat = "Poisson grillé", Garniture = "Riz basmati", Dessert = "Fruit de saison", PlatStandard1 = (string?)null, GarnitureStandard1 = (string?)null, PlatStandard2 = (string?)null, GarnitureStandard2 = (string?)null },
                        new { Date = new DateTime(2025, 9, 17), Type = "Standard 1", Entree = (string?)null, Plat = (string?)null, Garniture = (string?)null, Dessert = (string?)null, PlatStandard1 = "Kedjenou", GarnitureStandard1 = "Poulet", PlatStandard2 = (string?)null, GarnitureStandard2 = (string?)null },
                        new { Date = new DateTime(2025, 9, 17), Type = "Standard 2", Entree = (string?)null, Plat = (string?)null, Garniture = (string?)null, Dessert = (string?)null, PlatStandard1 = (string?)null, GarnitureStandard1 = (string?)null, PlatStandard2 = "Alloco", GarnitureStandard2 = "Poisson" },

                        // Jeudi 18/09
                        new { Date = new DateTime(2025, 9, 18), Type = "Amélioré", Entree = "Cocktail de fruits", Plat = "Agneau braisé", Garniture = "Riz safrané", Dessert = "Crème dessert", PlatStandard1 = (string?)null, GarnitureStandard1 = (string?)null, PlatStandard2 = (string?)null, GarnitureStandard2 = (string?)null },
                        new { Date = new DateTime(2025, 9, 18), Type = "Standard 1", Entree = (string?)null, Plat = (string?)null, Garniture = (string?)null, Dessert = (string?)null, PlatStandard1 = "Sauce tomate", GarnitureStandard1 = "Viande", PlatStandard2 = (string?)null, GarnitureStandard2 = (string?)null },
                        new { Date = new DateTime(2025, 9, 18), Type = "Standard 2", Entree = (string?)null, Plat = (string?)null, Garniture = (string?)null, Dessert = (string?)null, PlatStandard1 = (string?)null, GarnitureStandard1 = (string?)null, PlatStandard2 = "Riz au gras", GarnitureStandard2 = "Poisson" },

                        // Vendredi 19/09
                        new { Date = new DateTime(2025, 9, 19), Type = "Amélioré", Entree = "Salade de choux", Plat = "Poulet frit", Garniture = "Frites", Dessert = "Glace", PlatStandard1 = (string?)null, GarnitureStandard1 = (string?)null, PlatStandard2 = (string?)null, GarnitureStandard2 = (string?)null },
                        new { Date = new DateTime(2025, 9, 19), Type = "Standard 1", Entree = (string?)null, Plat = (string?)null, Garniture = (string?)null, Dessert = (string?)null, PlatStandard1 = "Sauce gombo", GarnitureStandard1 = "Poulet", PlatStandard2 = (string?)null, GarnitureStandard2 = (string?)null },
                        new { Date = new DateTime(2025, 9, 19), Type = "Standard 2", Entree = (string?)null, Plat = (string?)null, Garniture = (string?)null, Dessert = (string?)null, PlatStandard1 = (string?)null, GarnitureStandard1 = (string?)null, PlatStandard2 = "Riz au poisson", GarnitureStandard2 = "Poisson" },

                        // Samedi 20/09
                        new { Date = new DateTime(2025, 9, 20), Type = "Amélioré", Entree = "Soupe de légumes", Plat = "Bœuf bourguignon", Garniture = "Pâtes", Dessert = "Tarte tatin", PlatStandard1 = (string?)null, GarnitureStandard1 = (string?)null, PlatStandard2 = (string?)null, GarnitureStandard2 = (string?)null },
                        new { Date = new DateTime(2025, 9, 20), Type = "Standard 1", Entree = (string?)null, Plat = (string?)null, Garniture = (string?)null, Dessert = (string?)null, PlatStandard1 = "Sauce arachide", GarnitureStandard1 = "Viande", PlatStandard2 = (string?)null, GarnitureStandard2 = (string?)null },
                        new { Date = new DateTime(2025, 9, 20), Type = "Standard 2", Entree = (string?)null, Plat = (string?)null, Garniture = (string?)null, Dessert = (string?)null, PlatStandard1 = (string?)null, GarnitureStandard1 = (string?)null, PlatStandard2 = "Riz gras", GarnitureStandard2 = "Poisson" },

                        // Dimanche 21/09
                        new { Date = new DateTime(2025, 9, 21), Type = "Amélioré", Entree = "Salade niçoise", Plat = "Saumon grillé", Garniture = "Riz complet", Dessert = "Mousse au chocolat", PlatStandard1 = (string?)null, GarnitureStandard1 = (string?)null, PlatStandard2 = (string?)null, GarnitureStandard2 = (string?)null },
                        new { Date = new DateTime(2025, 9, 21), Type = "Standard 1", Entree = (string?)null, Plat = (string?)null, Garniture = (string?)null, Dessert = (string?)null, PlatStandard1 = "Sauce graine", GarnitureStandard1 = "Poulet", PlatStandard2 = (string?)null, GarnitureStandard2 = (string?)null },
                        new { Date = new DateTime(2025, 9, 21), Type = "Standard 2", Entree = (string?)null, Plat = (string?)null, Garniture = (string?)null, Dessert = (string?)null, PlatStandard1 = (string?)null, GarnitureStandard1 = (string?)null, PlatStandard2 = "Attieke", GarnitureStandard2 = "Poisson" }
                    };

                    foreach (var donnee in donneesMenus)
                    {
                        var typeFormule = donnee.Type switch
                        {
                            "Amélioré" => typeAmeliore,
                            "Standard 1" => typeStandard1,
                            "Standard 2" => typeStandard2,
                            _ => null
                        };

                        if (typeFormule != null)
                        {
                            var menu = new FormuleJour
                            {
                                IdFormule = Guid.NewGuid(),
                                Date = donnee.Date,
                                TypeFormuleId = typeFormule.Id,
                                NomFormule = donnee.Type,
                                Entree = donnee.Entree,
                                Plat = donnee.Plat,
                                Garniture = donnee.Garniture,
                                Dessert = donnee.Dessert,
                                PlatStandard1 = donnee.PlatStandard1,
                                GarnitureStandard1 = donnee.GarnitureStandard1,
                                PlatStandard2 = donnee.PlatStandard2,
                                GarnitureStandard2 = donnee.GarnitureStandard2,
                                Feculent = "Riz",
                                Legumes = "Légumes de saison",
                                Statut = 1,
                                CreatedOn = DateTime.UtcNow,
                                CreatedBy = "debug"
                            };
                            menus.Add(menu);
                        }
                    }

                    _context.FormulesJour.AddRange(menus);
                    await _context.SaveChangesAsync();
                    result.Add($"✅ {menus.Count} menus créés pour la semaine du 15-21 septembre 2025");
                }
                else
                {
                    result.Add($"ℹ️ {menusExistants} menus existent déjà pour cette semaine");
                }

                result.Add("=== DONNÉES DE TEST CRÉÉES AVEC SUCCÈS ===");
            }
            catch (Exception ex)
            {
                result.Add($"❌ Erreur: {ex.Message}");
            }

            ViewBag.Result = result;
            return View("CheckDatabase");
        }

        [HttpPost]
        public async Task<IActionResult> ClearDatabase()
        {
            var result = new List<string>();
            result.Add("=== VIDAGE DE LA BASE DE DONNÉES ===");

            try
            {
                // Supprimer toutes les données dans l'ordre correct (respecter les contraintes FK)
                var commandesCount = await _context.Commandes.CountAsync();
                var formulesCount = await _context.FormulesJour.CountAsync();
                var typesCount = await _context.TypesFormule.CountAsync();
                var utilisateursCount = await _context.Utilisateurs.CountAsync();
                var departementsCount = await _context.Departements.CountAsync();
                var fonctionsCount = await _context.Fonctions.CountAsync();
                var groupesCount = await _context.GroupesNonCit.CountAsync();

                result.Add($"📊 Données trouvées :");
                result.Add($"   - Commandes: {commandesCount}");
                result.Add($"   - FormulesJour: {formulesCount}");
                result.Add($"   - TypesFormule: {typesCount}");
                result.Add($"   - Utilisateurs: {utilisateursCount}");
                result.Add($"   - Départements: {departementsCount}");
                result.Add($"   - Fonctions: {fonctionsCount}");
                result.Add($"   - GroupesNonCit: {groupesCount}");
                result.Add("");

                // Supprimer dans l'ordre inverse des dépendances
                if (commandesCount > 0)
                {
                    _context.Commandes.RemoveRange(_context.Commandes);
                    result.Add($"✅ {commandesCount} commandes supprimées");
                }

                if (formulesCount > 0)
                {
                    _context.FormulesJour.RemoveRange(_context.FormulesJour);
                    result.Add($"✅ {formulesCount} formules du jour supprimées");
                }

                if (utilisateursCount > 0)
                {
                    _context.Utilisateurs.RemoveRange(_context.Utilisateurs);
                    result.Add($"✅ {utilisateursCount} utilisateurs supprimés");
                }

                if (groupesCount > 0)
                {
                    _context.GroupesNonCit.RemoveRange(_context.GroupesNonCit);
                    result.Add($"✅ {groupesCount} groupes non-CIT supprimés");
                }

                if (typesCount > 0)
                {
                    _context.TypesFormule.RemoveRange(_context.TypesFormule);
                    result.Add($"✅ {typesCount} types de formules supprimés");
                }

                if (departementsCount > 0)
                {
                    _context.Departements.RemoveRange(_context.Departements);
                    result.Add($"✅ {departementsCount} départements supprimés");
                }

                if (fonctionsCount > 0)
                {
                    _context.Fonctions.RemoveRange(_context.Fonctions);
                    result.Add($"✅ {fonctionsCount} fonctions supprimées");
                }

                await _context.SaveChangesAsync();
                result.Add("");
                result.Add("🎉 Base de données vidée avec succès !");
                result.Add("=== FIN DU VIDAGE ===");
            }
            catch (Exception ex)
            {
                result.Add($"❌ Erreur lors du vidage: {ex.Message}");
                if (ex.InnerException != null)
                {
                    result.Add($"   Détail: {ex.InnerException.Message}");
                }
            }

            ViewBag.Result = result;
            return View("CheckDatabase");
        }

        /// <summary>
        /// Debug - Vérifier les départements en base
        /// </summary>
    [HttpGet]
    public async Task<IActionResult> CheckDepartements()
    {
        try
        {
            // Récupérer TOUS les départements (y compris supprimés)
            var departements = await _context.Departements
                .OrderBy(d => d.Nom)
                .Select(d => new
                {
                    d.Id,
                    d.Nom,
                    d.Description,
                    d.Supprimer,
                    d.CreatedOn,
                    d.CreatedBy,
                    d.ModifiedOn,
                    d.ModifiedBy
                })
                .ToListAsync();

            ViewBag.Departements = departements;
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la vérification des départements");
            ViewBag.ErrorMessage = $"Erreur: {ex.Message}";
            return View();
        }
    }
}
}
