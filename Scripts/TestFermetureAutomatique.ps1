# Script de test pour la fermeture automatique des commandes
# Ce script teste le système de fermeture automatique le vendredi à 12h

Write-Host "🔒 Test du système de fermeture automatique des commandes" -ForegroundColor Yellow
Write-Host "=========================================================" -ForegroundColor Yellow

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
Write-Host "2. Allez dans Paramètres → Configuration Commandes" -ForegroundColor White
Write-Host "3. Vérifiez les paramètres de fermeture:" -ForegroundColor White
Write-Host "   - Jour de clôture: Friday" -ForegroundColor White
Write-Host "   - Heure de clôture: 12:00" -ForegroundColor White
Write-Host "4. Créez des commandes pour la semaine N+1" -ForegroundColor White
Write-Host "5. Attendez vendredi 12h ou modifiez l'heure système pour tester" -ForegroundColor White

Write-Host "`n🧪 Tests à effectuer:" -ForegroundColor Yellow
Write-Host "• Vérifier que les commandes sont bloquées après vendredi 12h" -ForegroundColor Yellow
Write-Host "• Vérifier que les commandes précommandées sont confirmées automatiquement" -ForegroundColor Yellow
Write-Host "• Vérifier que les points de consommation sont créés" -ForegroundColor Yellow
Write-Host "• Vérifier les logs de fermeture automatique" -ForegroundColor Yellow

Write-Host "`n📊 Vérifications importantes:" -ForegroundColor Magenta
Write-Host "• Les commandes de la semaine N+1 se ferment automatiquement" -ForegroundColor Magenta
Write-Host "• Les commandes précommandées passent en 'Consommée'" -ForegroundColor Magenta
Write-Host "• Les points de consommation sont créés automatiquement" -ForegroundColor Magenta
Write-Host "• Les logs montrent l'exécution de la fermeture" -ForegroundColor Magenta

Write-Host "`n🔧 Configuration technique:" -ForegroundColor Blue
Write-Host "• Service: FermetureAutomatiqueService" -ForegroundColor Blue
Write-Host "• Fréquence: Vérification toutes les 5 minutes" -ForegroundColor Blue
Write-Host "• Déclenchement: Vendredi à 12h00" -ForegroundColor Blue
Write-Host "• Actions: Confirmation + Création points de consommation" -ForegroundColor Blue

Write-Host "`n📝 Logs à surveiller:" -ForegroundColor Green
Write-Host "• '🚀 Service de fermeture automatique démarré'" -ForegroundColor Green
Write-Host "• '🔒 Début de la fermeture automatique des commandes'" -ForegroundColor Green
Write-Host "• '✅ Fermeture automatique terminée'" -ForegroundColor Green
Write-Host "• '🍽️ Point de consommation créé'" -ForegroundColor Green

Write-Host "`n✅ Script de test terminé!" -ForegroundColor Green
Write-Host "Lancez l'application avec: dotnet run" -ForegroundColor Green
Write-Host "Puis surveillez les logs pour voir la fermeture automatique en action." -ForegroundColor Green
