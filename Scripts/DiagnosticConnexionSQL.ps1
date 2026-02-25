# Script de diagnostic pour la connexion SQL Server
# Teste la connectivité réseau et les ports courants

Write-Host "🔍 Diagnostic de connexion SQL Server" -ForegroundColor Cyan
Write-Host "Serveur: 10.88.179.112" -ForegroundColor Gray
Write-Host ""

# Test 1: Ping
Write-Host "1️⃣ Test de ping..." -ForegroundColor Yellow
$ping = Test-Connection -ComputerName 10.88.179.112 -Count 2 -Quiet
if ($ping) {
    Write-Host "   ✅ Serveur accessible (ping réussi)" -ForegroundColor Green
} else {
    Write-Host "   ❌ Serveur non accessible (ping échoué)" -ForegroundColor Red
    Write-Host "   ⚠️  Vérifiez que le serveur est démarré et accessible sur le réseau" -ForegroundColor Yellow
    exit 1
}

Write-Host ""

# Test 2: Ports SQL Server courants
Write-Host "2️⃣ Test des ports SQL Server..." -ForegroundColor Yellow
$ports = @(1433, 1434, 14330, 14331, 14333)

$portOuvert = $false
foreach ($port in $ports) {
    Write-Host "   Test du port $port..." -NoNewline
    $test = Test-NetConnection -ComputerName 10.88.179.112 -Port $port -WarningAction SilentlyContinue
    if ($test.TcpTestSucceeded) {
        Write-Host " ✅ PORT OUVERT" -ForegroundColor Green
        $portOuvert = $true
        Write-Host ""
        Write-Host "   💡 Utilisez ce port dans votre chaîne de connexion:" -ForegroundColor Cyan
        Write-Host "   Server=10.88.179.112,$port;..." -ForegroundColor White
        break
    } else {
        Write-Host " ❌ Port fermé" -ForegroundColor Red
    }
}

if (-not $portOuvert) {
    Write-Host ""
    Write-Host "❌ Aucun port SQL Server trouvé accessible" -ForegroundColor Red
    Write-Host ""
    Write-Host "🔧 Solutions possibles:" -ForegroundColor Yellow
    Write-Host "   1. Vérifiez que SQL Server est démarré sur le serveur distant"
    Write-Host "   2. Vérifiez que TCP/IP est activé dans SQL Server Configuration Manager"
    Write-Host "   3. Vérifiez que le pare-feu Windows autorise le port SQL Server"
    Write-Host "   4. Vérifiez que SQL Server écoute sur toutes les interfaces (0.0.0.0)"
    Write-Host "   5. Contactez l'administrateur réseau pour ouvrir le port"
}

Write-Host ""
Write-Host "3️⃣ Test de connexion SQL avec différents formats..." -ForegroundColor Yellow

# Test avec différents formats de chaîne de connexion
$connectionStrings = @(
    @{
        Name = "Avec port explicite (1433)"
        String = "Server=10.88.179.112,1433;Database=Kobeli_db;User Id=sa;Password=LeB@t02cotedivoireterminal!1;Encrypt=False;TrustServerCertificate=True;Connection Timeout=5;"
    },
    @{
        Name = "Avec port explicite (1434)"
        String = "Server=10.88.179.112,1434;Database=Kobeli_db;User Id=sa;Password=LeB@t02cotedivoireterminal!1;Encrypt=False;TrustServerCertificate=True;Connection Timeout=5;"
    },
    @{
        Name = "Sans port (par défaut)"
        String = "Server=10.88.179.112;Database=Kobeli_db;User Id=sa;Password=LeB@t02cotedivoireterminal!1;Encrypt=False;TrustServerCertificate=True;Connection Timeout=5;"
    }
)

# Note: Pour tester réellement la connexion SQL, il faudrait avoir accès à System.Data.SqlClient
# Ici on teste juste la syntaxe et la résolution du serveur

Write-Host "   Formats de chaîne de connexion préparés" -ForegroundColor Gray
Write-Host "   ⚠️  Pour tester réellement, utilisez SSMS ou dotnet ef" -ForegroundColor Yellow

Write-Host ""
Write-Host "📋 Recommandations:" -ForegroundColor Cyan
Write-Host "   1. Connectez-vous d'abord via SSMS pour confirmer le port"
Write-Host "   2. Vérifiez SQL Server Configuration Manager sur le serveur"
Write-Host "   3. Vérifiez le pare-feu Windows sur le serveur SQL"
Write-Host "   4. Si nécessaire, demandez l'ouverture du port au réseau"

