# Script de test pour les quotas des Douaniers
# Ce script teste le système de quotas journaliers

Write-Host "🔧 Test du système de quotas des Douaniers" -ForegroundColor Yellow
Write-Host "===============================================" -ForegroundColor Yellow

# 1. Vérifier que l'application démarre correctement
Write-Host "`n1️⃣ Démarrage de l'application..." -ForegroundColor Cyan
try {
    # Démarrer l'application en arrière-plan
    $process = Start-Process -FilePath "dotnet" -ArgumentList "run" -PassThru -WindowStyle Hidden
    Start-Sleep -Seconds 10
    
    if ($process.HasExited) {
        Write-Host "❌ L'application n'a pas démarré correctement" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "✅ Application démarrée avec succès" -ForegroundColor Green
    
    # Arrêter l'application
    Stop-Process -Id $process.Id -Force
    Write-Host "✅ Application arrêtée" -ForegroundColor Green
    
} catch {
    Write-Host "❌ Erreur lors du démarrage: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# 2. Instructions pour tester manuellement
Write-Host "`n2️⃣ Instructions de test manuel:" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan

Write-Host "`n📋 Étapes de test:" -ForegroundColor White
Write-Host "1. Connectez-vous en tant qu'Administrateur (admin/admin123)" -ForegroundColor White
Write-Host "2. Allez dans Paramètres → Quotas Douaniers" -ForegroundColor White
Write-Host "3. Vérifiez qu'un quota existe pour aujourd'hui (50 plats/jour, 30 plats/nuit)" -ForegroundColor White
Write-Host "4. Créez un nouveau quota pour demain si nécessaire" -ForegroundColor White
Write-Host "5. Connectez-vous en tant que PrestataireCantine (prestataire/presta123)" -ForegroundColor White
Write-Host "6. Allez dans Commandes → Commandes Douaniers" -ForegroundColor White
Write-Host "7. Testez la création d'une commande avec différentes quantités" -ForegroundColor White

Write-Host "`n🧪 Tests à effectuer:" -ForegroundColor Yellow
Write-Host "• Créer une commande avec 10 plats (doit fonctionner)" -ForegroundColor Yellow
Write-Host "• Créer une commande avec 60 plats (doit échouer - quota dépassé)" -ForegroundColor Yellow
Write-Host "• Vérifier que les quotas sont mis à jour après chaque commande" -ForegroundColor Yellow
Write-Host "• Tester la validation par code de commande" -ForegroundColor Yellow

Write-Host "`n📊 Vérifications importantes:" -ForegroundColor Magenta
Write-Host "• Les quotas sont respectés par jour" -ForegroundColor Magenta
Write-Host "• Les plats restants sont calculés correctement" -ForegroundColor Magenta
Write-Host "• Les statistiques s'affichent dans l'interface" -ForegroundColor Magenta
Write-Host "• Les codes de commande sont générés correctement" -ForegroundColor Magenta

Write-Host "`n✅ Script de test terminé!" -ForegroundColor Green
Write-Host "Lancez l'application avec: dotnet run" -ForegroundColor Green
Write-Host "Puis suivez les instructions ci-dessus pour tester le système." -ForegroundColor Green
