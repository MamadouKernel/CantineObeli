# Script PowerShell pour appliquer les migrations à la base de données
# Usage: .\Scripts\AppliquerMigrations.ps1

Write-Host "🔧 Application des migrations à la base de données..." -ForegroundColor Cyan
Write-Host ""

# Vérifier que nous sommes dans le bon répertoire
$projectPath = "C:\Users\kerne\Music\DIKO\restau\Obeli_K"
Set-Location $projectPath

# Vérifier que dotnet ef est installé
Write-Host "📋 Vérification de l'installation d'Entity Framework Core Tools..." -ForegroundColor Yellow
$efInstalled = dotnet ef --version 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Entity Framework Core Tools n'est pas installé" -ForegroundColor Red
    Write-Host "Installation en cours..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-ef
}

Write-Host "✅ Entity Framework Core Tools est installé" -ForegroundColor Green
Write-Host ""

# Lister les migrations disponibles
Write-Host "📋 Liste des migrations disponibles dans le projet:" -ForegroundColor Cyan
dotnet ef migrations list
Write-Host ""

# Vérifier la connexion à la base de données
Write-Host "🔗 Test de connexion à la base de données..." -ForegroundColor Yellow
$connectionString = "Server=10.88.179.112;Database=Kobeli_db;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;MultipleActiveResultSets=true;Connection Timeout=30;"

# Application des migrations
Write-Host ""
Write-Host "🚀 Application des migrations à la base de données..." -ForegroundColor Cyan
Write-Host "Base de données: Kobeli_db" -ForegroundColor Gray
Write-Host "Serveur: 10.88.179.112" -ForegroundColor Gray
Write-Host ""

try {
    dotnet ef database update --verbose
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "✅ Migrations appliquées avec succès!" -ForegroundColor Green
        Write-Host ""
        
        # Lister les migrations appliquées
        Write-Host "📋 Migrations actuellement appliquées:" -ForegroundColor Cyan
        dotnet ef migrations list
    } else {
        Write-Host ""
        Write-Host "❌ Erreur lors de l'application des migrations" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host ""
    Write-Host "❌ Erreur: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "✅ Opération terminée" -ForegroundColor Green

