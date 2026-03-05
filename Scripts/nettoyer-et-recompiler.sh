#!/bin/bash

# Script Bash pour nettoyer et recompiler le projet

echo "🧹 Nettoyage du projet..."

# Nettoyer les dossiers bin et obj
echo "Suppression des dossiers bin et obj..."
find . -type d -name "bin" -o -name "obj" | xargs rm -rf

echo "✅ Dossiers bin et obj supprimés"

# Nettoyer la solution
echo ""
echo "🔧 Nettoyage de la solution..."
dotnet clean

echo "✅ Solution nettoyée"

# Restaurer les packages NuGet
echo ""
echo "📦 Restauration des packages NuGet..."
dotnet restore

echo "✅ Packages restaurés"

# Compiler le projet
echo ""
echo "🔨 Compilation du projet..."
dotnet build

echo ""
echo "✅ Compilation terminée!"
echo "Vérifiez les erreurs ci-dessus. Si aucune erreur n'apparaît, le projet est prêt!"
