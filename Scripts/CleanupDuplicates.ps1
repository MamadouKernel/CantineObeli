# Script PowerShell pour nettoyer les commandes instantanées en doublon
# Utilise Entity Framework pour se connecter à la base de données

param(
    [string]$ConnectionString = "Server=localhost;Database=Obeli_K;Trusted_Connection=true;TrustServerCertificate=true;"
)

Write-Host "=== NETTOYAGE DES COMMANDES INSTANTANÉES EN DOUBLON ===" -ForegroundColor Yellow

try {
    # Charger les assemblies nécessaires
    Add-Type -Path "bin\Debug\net8.0\Obeli_K.dll"
    Add-Type -Path "bin\Debug\net8.0\Microsoft.EntityFrameworkCore.dll"
    Add-Type -Path "bin\Debug\net8.0\Microsoft.EntityFrameworkCore.SqlServer.dll"
    
    # Créer le contexte de base de données
    $optionsBuilder = New-Object Microsoft.EntityFrameworkCore.DbContextOptionsBuilder[Obeli_K.Data.ObeliDbContext]
    $optionsBuilder.UseSqlServer($ConnectionString)
    
    $context = New-Object Obeli_K.Data.ObeliDbContext($optionsBuilder.Options)
    
    # Afficher les commandes en doublon avant nettoyage
    Write-Host "`n--- COMMANDES EN DOUBLON AVANT NETTOYAGE ---" -ForegroundColor Cyan
    
    $duplicateCommands = $context.Commandes
        .Where("Instantanee == true AND Supprimer == false AND DateConsommation >= DateTime.Today")
        .Include("Utilisateur")
        .GroupBy("UtilisateurId")
        .Where("Count() > 1")
        .SelectMany("g => g.OrderByDescending(c => c.Date).Skip(1)")
        .ToList()
    
    Write-Host "Nombre de commandes en doublon trouvées: $($duplicateCommands.Count)" -ForegroundColor Red
    
    foreach ($cmd in $duplicateCommands) {
        Write-Host "  - Utilisateur: $($cmd.Utilisateur.UserName) ($($cmd.Utilisateur.Nom) $($cmd.Utilisateur.Prenoms)) - Date: $($cmd.DateConsommation.ToString('yyyy-MM-dd')) - Commande: $($cmd.IdCommande)" -ForegroundColor Red
    }
    
    if ($duplicateCommands.Count -gt 0) {
        # Marquer les doublons comme supprimés
        Write-Host "`n--- SUPPRESSION DES DOUBLONS ---" -ForegroundColor Yellow
        
        foreach ($cmd in $duplicateCommands) {
            $cmd.Supprimer = $true
            $cmd.ModifiedOn = [DateTime]::Now
            $cmd.ModifiedBy = "SYSTEM_CLEANUP"
        }
        
        $context.SaveChanges()
        Write-Host "✅ $($duplicateCommands.Count) commandes en doublon marquées comme supprimées" -ForegroundColor Green
    } else {
        Write-Host "✅ Aucune commande en doublon trouvée" -ForegroundColor Green
    }
    
    # Afficher le résultat après nettoyage
    Write-Host "`n--- RÉSULTAT APRÈS NETTOYAGE ---" -ForegroundColor Cyan
    
    $remainingCommands = $context.Commandes
        .Where("Instantanee == true AND Supprimer == false AND DateConsommation >= DateTime.Today")
        .Include("Utilisateur")
        .ToList()
    
    Write-Host "Nombre de commandes instantanées restantes: $($remainingCommands.Count)" -ForegroundColor Green
    
    $userStats = $remainingCommands
        .GroupBy("UtilisateurId")
        .Select("g => new { UserId = g.Key, UserName = g.First().Utilisateur.UserName, NomComplet = g.First().Utilisateur.Nom + ' ' + g.First().Utilisateur.Prenoms, Count = g.Count() }")
        .OrderByDescending("Count")
        .ToList()
    
    Write-Host "`nRépartition par utilisateur:" -ForegroundColor White
    foreach ($stat in $userStats) {
        Write-Host "  - $($stat.NomComplet) ($($stat.UserName)): $($stat.Count) commande(s)" -ForegroundColor White
    }
    
} catch {
    Write-Host "❌ Erreur lors du nettoyage: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Détails: $($_.Exception)" -ForegroundColor Red
} finally {
    if ($context) {
        $context.Dispose()
    }
}

Write-Host "`n=== NETTOYAGE TERMINÉ ===" -ForegroundColor Yellow
