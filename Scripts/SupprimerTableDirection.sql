-- Script pour supprimer la table Direction qui n'est pas utilisée

USE [Obeli_K]
GO

-- Vérifier si la table existe
IF OBJECT_ID('dbo.Directions', 'U') IS NOT NULL
BEGIN
    PRINT '🗑️ Suppression de la table Directions...'
    
    -- Supprimer la table
    DROP TABLE dbo.Directions
    
    PRINT '✅ Table Directions supprimée avec succès'
END
ELSE
BEGIN
    PRINT 'ℹ️ La table Directions n''existe pas'
END
GO
