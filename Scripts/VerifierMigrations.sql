-- Script SQL pour vérifier l'historique des migrations dans la base de données
-- Base de données: Kobeli_db
-- Serveur: 10.88.179.112

USE Kobeli_db;
GO

-- 1. Vérifier si la table __EFMigrationsHistory existe
IF OBJECT_ID('__EFMigrationsHistory', 'U') IS NOT NULL
BEGIN
    PRINT '✅ La table __EFMigrationsHistory existe';
    
    -- Afficher toutes les migrations appliquées
    PRINT '';
    PRINT '📋 Liste des migrations appliquées dans la base de données:';
    PRINT '-----------------------------------------------------------';
    
    SELECT 
        [MigrationId] AS 'ID Migration',
        [ProductVersion] AS 'Version EF Core',
        ROW_NUMBER() OVER (ORDER BY [MigrationId]) AS 'Numéro'
    FROM [__EFMigrationsHistory]
    ORDER BY [MigrationId];
    
    -- Compter le nombre de migrations
    DECLARE @NombreMigrations INT;
    SELECT @NombreMigrations = COUNT(*) FROM [__EFMigrationsHistory];
    PRINT '';
    PRINT '📊 Nombre total de migrations appliquées: ' + CAST(@NombreMigrations AS VARCHAR(10));
    
    -- Afficher les tables créées par les migrations
    PRINT '';
    PRINT '📊 Liste des tables dans la base de données:';
    PRINT '-----------------------------------------------------------';
    
    SELECT 
        TABLE_SCHEMA AS 'Schéma',
        TABLE_NAME AS 'Nom de la table'
    FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_TYPE = 'BASE TABLE'
        AND TABLE_NAME NOT LIKE '__%'
    ORDER BY TABLE_NAME;
    
    -- Afficher la dernière migration appliquée
    PRINT '';
    PRINT '🕒 Dernière migration appliquée:';
    PRINT '-----------------------------------------------------------';
    
    SELECT TOP 1
        [MigrationId] AS 'Dernière Migration',
        [ProductVersion] AS 'Version EF Core'
    FROM [__EFMigrationsHistory]
    ORDER BY [MigrationId] DESC;
END
ELSE
BEGIN
    PRINT '❌ La table __EFMigrationsHistory n''existe pas';
    PRINT 'Les migrations n''ont pas été appliquées à la base de données.';
    PRINT '';
    PRINT 'Pour appliquer les migrations, exécutez dans PowerShell:';
    PRINT 'dotnet ef database update';
END
GO

-- 2. Vérifier les tables principales
PRINT '';
PRINT '🔍 Vérification des tables principales:';
PRINT '-----------------------------------------------------------';

IF OBJECT_ID('Utilisateurs', 'U') IS NOT NULL
    PRINT '✅ Table Utilisateurs existe'
ELSE
    PRINT '❌ Table Utilisateurs n''existe pas';

IF OBJECT_ID('Commandes', 'U') IS NOT NULL
    PRINT '✅ Table Commandes existe'
ELSE
    PRINT '❌ Table Commandes n''existe pas';

IF OBJECT_ID('FormulesJour', 'U') IS NOT NULL
    PRINT '✅ Table FormulesJour existe'
ELSE
    PRINT '❌ Table FormulesJour n''existe pas';

IF OBJECT_ID('PointsConsommation', 'U') IS NOT NULL
    PRINT '✅ Table PointsConsommation existe'
ELSE
    PRINT '❌ Table PointsConsommation n''existe pas';

IF OBJECT_ID('GroupesNonCit', 'U') IS NOT NULL
    PRINT '✅ Table GroupesNonCit existe'
ELSE
    PRINT '❌ Table GroupesNonCit n''existe pas';

IF OBJECT_ID('Departements', 'U') IS NOT NULL
    PRINT '✅ Table Departements existe'
ELSE
    PRINT '❌ Table Departements n''existe pas';

IF OBJECT_ID('Fonctions', 'U') IS NOT NULL
    PRINT '✅ Table Fonctions existe'
ELSE
    PRINT '❌ Table Fonctions n''existe pas';

IF OBJECT_ID('ConfigurationsCommande', 'U') IS NOT NULL
    PRINT '✅ Table ConfigurationsCommande existe'
ELSE
    PRINT '❌ Table ConfigurationsCommande n''existe pas';

GO

