-- Vérifier si la table Directions existe dans la base de données

USE [Obeli_K]
GO

IF OBJECT_ID('dbo.Directions', 'U') IS NOT NULL
BEGIN
    PRINT '✅ La table Directions existe'
    
    -- Compter les enregistrements
    DECLARE @count INT
    SELECT @count = COUNT(*) FROM dbo.Directions
    PRINT 'Nombre d''enregistrements : ' + CAST(@count AS VARCHAR(10))
    
    -- Afficher les données
    SELECT * FROM dbo.Directions
END
ELSE
BEGIN
    PRINT '❌ La table Directions n''existe PAS dans la base de données'
    PRINT 'Aucune action nécessaire - la table a déjà été supprimée ou n''a jamais existé'
END
GO
