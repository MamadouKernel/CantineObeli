# Script d'initialisation de la base de données
# Ce script s'assure que toutes les tables et données nécessaires existent

Write-Host "🔧 Initialisation de la base de données O'Beli" -ForegroundColor Yellow
Write-Host "=============================================" -ForegroundColor Yellow

# 1. Arrêter l'application si elle tourne
Write-Host "`n1️⃣ Arrêt de l'application..." -ForegroundColor Cyan
try {
    Get-Process -Name "Obeli_K" -ErrorAction SilentlyContinue | Stop-Process -Force
    Write-Host "✅ Application arrêtée" -ForegroundColor Green
} catch {
    Write-Host "ℹ️ Aucune application en cours d'exécution" -ForegroundColor Blue
}

# 2. Nettoyer et reconstruire
Write-Host "`n2️⃣ Nettoyage et reconstruction..." -ForegroundColor Cyan
try {
    dotnet clean
    dotnet build
    Write-Host "✅ Projet reconstruit" -ForegroundColor Green
} catch {
    Write-Host "❌ Erreur lors de la reconstruction" -ForegroundColor Red
    exit 1
}

# 3. Supprimer et recréer la base de données
Write-Host "`n3️⃣ Réinitialisation de la base de données..." -ForegroundColor Cyan
try {
    # Supprimer la base de données existante
    dotnet ef database drop --force
    
    # Recréer la base de données
    dotnet ef database update
    
    Write-Host "✅ Base de données réinitialisée" -ForegroundColor Green
} catch {
    Write-Host "❌ Erreur lors de la réinitialisation de la base de données" -ForegroundColor Red
    Write-Host "Tentative de mise à jour simple..." -ForegroundColor Yellow
    
    try {
        dotnet ef database update
        Write-Host "✅ Base de données mise à jour" -ForegroundColor Green
    } catch {
        Write-Host "❌ Impossible de mettre à jour la base de données" -ForegroundColor Red
        exit 1
    }
}

# 4. Vérifier les migrations
Write-Host "`n4️⃣ Vérification des migrations..." -ForegroundColor Cyan
try {
    dotnet ef migrations list
    Write-Host "✅ Migrations vérifiées" -ForegroundColor Green
} catch {
    Write-Host "⚠️ Problème avec les migrations" -ForegroundColor Yellow
}

# 5. Démarrer l'application
Write-Host "`n5️⃣ Démarrage de l'application..." -ForegroundColor Cyan
try {
    Start-Process -FilePath "dotnet" -ArgumentList "run" -WindowStyle Hidden
    Start-Sleep -Seconds 10
    
    Write-Host "✅ Application démarrée" -ForegroundColor Green
    Write-Host "🌐 Ouvrez votre navigateur sur: https://localhost:7021" -ForegroundColor Green
    
} catch {
    Write-Host "❌ Erreur lors du démarrage de l'application" -ForegroundColor Red
    exit 1
}

Write-Host "`n🎉 Initialisation terminée avec succès!" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green

Write-Host "`n📋 Étapes suivantes:" -ForegroundColor White
Write-Host "1. Ouvrez votre navigateur sur https://localhost:7021" -ForegroundColor White
Write-Host "2. Connectez-vous avec admin/admin123" -ForegroundColor White
Write-Host "3. Allez dans Paramètres → Gérer Quotas Permanents" -ForegroundColor White
Write-Host "4. Vérifiez que le groupe 'Douaniers' existe" -ForegroundColor White

Write-Host "`n🔧 Si vous avez encore des problèmes:" -ForegroundColor Yellow
Write-Host "- Vérifiez les logs de l'application" -ForegroundColor Yellow
Write-Host "- Redémarrez l'application manuellement" -ForegroundColor Yellow
Write-Host "- Contactez l'equipe technique" -ForegroundColor Yellow
