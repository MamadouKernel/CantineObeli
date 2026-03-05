# Script PowerShell pour nettoyer et recompiler le projet

Write-Host "🧹 Nettoyage du projet..." -ForegroundColor Yellow

# Nettoyer les dossiers bin et obj
Write-Host "Suppression des dossiers bin et obj..." -ForegroundColor Cyan
Get-ChildItem -Path . -Include bin,obj -Recurse -Directory | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "✅ Dossiers bin et obj supprimés" -ForegroundColor Green

# Nettoyer la solution
Write-Host "`n🔧 Nettoyage de la solution..." -ForegroundColor Cyan
dotnet clean

Write-Host "✅ Solution nettoyée" -ForegroundColor Green

# Restaurer les packages NuGet
Write-Host "`n📦 Restauration des packages NuGet..." -ForegroundColor Cyan
dotnet restore

Write-Host "✅ Packages restaurés" -ForegroundColor Green

# Compiler le projet
Write-Host "`n🔨 Compilation du projet..." -ForegroundColor Cyan
dotnet build

Write-Host "`n✅ Compilation terminée!" -ForegroundColor Green
Write-Host "`nVérifiez les erreurs ci-dessus. Si aucune erreur n'apparaît, le projet est prêt!" -ForegroundColor Yellow
