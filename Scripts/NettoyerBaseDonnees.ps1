# Script de nettoyage de la base de données Obeli_K
# Ce script vide complètement la base de données en gardant seulement les comptes administrateurs

param(
    [switch]$Force,
    [string]$ConnectionString = ""
)

Write-Host "🗑️  Script de nettoyage de la base de données Obeli_K" -ForegroundColor Red
Write-Host "=================================================" -ForegroundColor Red

# Vérification de la confirmation
if (-not $Force) {
    $confirmation = Read-Host "⚠️  ATTENTION ! Ce script va supprimer TOUTES les données de la base de données (sauf les admins). Êtes-vous sûr de vouloir continuer ? (tapez 'OUI' pour confirmer)"
    if ($confirmation -ne "OUI") {
        Write-Host "❌ Opération annulée par l'utilisateur." -ForegroundColor Yellow
        exit 1
    }
}

Write-Host "🔍 Recherche de la chaîne de connexion..." -ForegroundColor Blue

# Lire la chaîne de connexion depuis appsettings.json
$appsettingsPath = "appsettings.json"
if (Test-Path $appsettingsPath) {
    $appsettings = Get-Content $appsettingsPath | ConvertFrom-Json
    if ($appsettings.ConnectionStrings -and $appsettings.ConnectionStrings.DefaultConnection) {
        $ConnectionString = $appsettings.ConnectionStrings.DefaultConnection
        Write-Host "✅ Chaîne de connexion trouvée dans appsettings.json" -ForegroundColor Green
    } else {
        Write-Host "❌ Chaîne de connexion non trouvée dans appsettings.json" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "❌ Fichier appsettings.json non trouvé" -ForegroundColor Red
    exit 1
}

Write-Host "🔗 Connexion à la base de données..." -ForegroundColor Blue

try {
    # Importer le module SqlServer si disponible
    if (Get-Module -ListAvailable -Name SqlServer) {
        Import-Module SqlServer
        Write-Host "✅ Module SqlServer importé" -ForegroundColor Green
    } else {
        Write-Host "⚠️  Module SqlServer non trouvé. Tentative avec sqlcmd..." -ForegroundColor Yellow
    }

    # Extraire les informations de connexion
    if ($ConnectionString -match "Server=([^;]+);.*Database=([^;]+)") {
        $Server = $matches[1]
        $Database = $matches[2]
        
        Write-Host "📊 Serveur: $Server" -ForegroundColor Cyan
        Write-Host "📊 Base de données: $Database" -ForegroundColor Cyan
    } else {
        Write-Host "❌ Impossible d'extraire les informations de connexion" -ForegroundColor Red
        exit 1
    }

    # Script SQL pour nettoyer la base de données
    $sqlScript = @"
-- Nettoyage complet de la base de données Obeli_K
-- Garde seulement les utilisateurs admin et les données de référence

PRINT '🗑️  Début du nettoyage de la base de données...'

-- Désactiver les contraintes FK temporairement
EXEC sp_MSforeachtable "ALTER TABLE ? NOCHECK CONSTRAINT all"

-- Supprimer les données dans l'ordre pour respecter les contraintes FK
DELETE FROM PointsConsommation
PRINT '✅ Points de consommation supprimés'

DELETE FROM ExportCommandesPrestataire
PRINT '✅ Exports commandes prestataire supprimés'

DELETE FROM Commandes
PRINT '✅ Commandes supprimées'

DELETE FROM QuotasJournaliers
PRINT '✅ Quotas journaliers supprimés'

DELETE FROM ConfigurationsCommande
PRINT '✅ Configurations commande supprimées'

DELETE FROM FormulesJour
PRINT '✅ Formules jour supprimées'

DELETE FROM PrestataireCantines
PRINT '✅ Prestataires cantine supprimés'

DELETE FROM GroupesNonCit
PRINT '✅ Groupes non CIT supprimés'

-- Supprimer les utilisateurs non-admin (soft delete)
UPDATE Utilisateurs 
SET Supprimer = 1, ModifiedAt = GETUTCDATE(), ModifiedBy = 'Script de nettoyage'
WHERE Role != 0 AND Supprimer = 0  -- 0 = Admin dans l'enum RoleType
PRINT '✅ Utilisateurs non-admin supprimés (soft delete)'

-- Réactiver les contraintes FK
EXEC sp_MSforeachtable "ALTER TABLE ? CHECK CONSTRAINT all"

PRINT '✅ Nettoyage terminé avec succès !'
PRINT '📊 Seuls les comptes administrateurs et les données de référence ont été conservés.'
"@

    # Exécuter le script SQL
    Write-Host "🚀 Exécution du script de nettoyage..." -ForegroundColor Blue
    
    if (Get-Command Invoke-Sqlcmd -ErrorAction SilentlyContinue) {
        Invoke-Sqlcmd -ConnectionString $ConnectionString -Query $sqlScript -Verbose
    } else {
        # Utiliser sqlcmd si Invoke-Sqlcmd n'est pas disponible
        $tempSqlFile = [System.IO.Path]::GetTempFileName() + ".sql"
        $sqlScript | Out-File -FilePath $tempSqlFile -Encoding UTF8
        
        $sqlcmdArgs = @("-S", $Server, "-d", $Database, "-E", "-i", $tempSqlFile, "-o", "nettoyage_result.txt")
        & sqlcmd $sqlcmdArgs
        
        if (Test-Path "nettoyage_result.txt") {
            Get-Content "nettoyage_result.txt" | Write-Host
            Remove-Item "nettoyage_result.txt"
        }
        
        Remove-Item $tempSqlFile
    }

    Write-Host "✅ Nettoyage de la base de données terminé avec succès !" -ForegroundColor Green
    Write-Host "📊 Résumé des données conservées :" -ForegroundColor Cyan
    Write-Host "   ✅ Comptes administrateurs" -ForegroundColor Green
    Write-Host "   ✅ Directions" -ForegroundColor Green
    Write-Host "   ✅ Départements" -ForegroundColor Green
    Write-Host "   ✅ Fonctions" -ForegroundColor Green
    Write-Host "   ✅ Types de formules" -ForegroundColor Green
    Write-Host ""
    Write-Host "📊 Résumé des données supprimées :" -ForegroundColor Yellow
    Write-Host "   ❌ Toutes les commandes" -ForegroundColor Red
    Write-Host "   ❌ Tous les points de consommation" -ForegroundColor Red
    Write-Host "   ❌ Toutes les formules du jour" -ForegroundColor Red
    Write-Host "   ❌ Tous les utilisateurs non-admin" -ForegroundColor Red
    Write-Host "   ❌ Tous les prestataires cantine" -ForegroundColor Red
    Write-Host "   ❌ Tous les groupes non-CIT" -ForegroundColor Red
    Write-Host ""
    Write-Host "🎯 Vous pouvez maintenant redémarrer l'application avec une base de données propre !" -ForegroundColor Green

} catch {
    Write-Host "❌ Erreur lors du nettoyage : $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "✨ Script terminé !" -ForegroundColor Green
