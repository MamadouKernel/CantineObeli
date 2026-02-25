-- Script SQL pour vider complètement la base de données Obeli_K
-- Garde seulement les utilisateurs admin et les données de référence minimales

USE [Obeli_K]
GO

PRINT '🗑️ Début du nettoyage de la base de données...'

-- Désactiver les contraintes FK temporairement
EXEC sp_MSforeachtable "ALTER TABLE ? NOCHECK CONSTRAINT all"

-- 1. Supprimer les données transactionnelles
DELETE FROM PointsConsommation
PRINT '✅ Points de consommation supprimés'

DELETE FROM ExportCommandesPrestataire
PRINT '✅ Exports commandes prestataire supprimés'

DELETE FROM Commandes
PRINT '✅ Commandes supprimées'

DELETE FROM QuotasJournaliers
PRINT '✅ Quotas journaliers supprimés'

DELETE FROM ConfigurationsCommande
PRINT '✅ Configurations commande supprimées'

DELETE FROM FormulesJour
PRINT '✅ Formules jour supprimées'

DELETE FROM PrestataireCantines
PRINT '✅ Prestataires cantine supprimés'

DELETE FROM GroupesNonCit
PRINT '✅ Groupes non CIT supprimés'

-- 2. Supprimer les utilisateurs non-admin (soft delete)
UPDATE Utilisateurs 
SET Supprimer = 1, ModifiedAt = GETUTCDATE(), ModifiedBy = 'Script SQL'
WHERE Role != 0 AND Supprimer = 0  -- 0 = Admin dans l'enum RoleType
PRINT '✅ Utilisateurs non-admin supprimés (soft delete)'

-- 3. Supprimer les directions (sauf Direction Général si nécessaire)
DELETE FROM Directions WHERE Nom != 'Direction Général'
PRINT '✅ Directions supprimées (sauf Direction Général)'

-- 4. Supprimer les départements (sauf Direction Général)
DELETE FROM Departements WHERE Nom != 'Direction Général'
PRINT '✅ Départements supprimés (sauf Direction Général)'

-- 5. Supprimer les fonctions (sauf Fonction Général)
DELETE FROM Fonctions WHERE Nom != 'Fonction Général'
PRINT '✅ Fonctions supprimées (sauf Fonction Général)'

-- Réactiver les contraintes FK
EXEC sp_MSforeachtable "ALTER TABLE ? CHECK CONSTRAINT all"

PRINT ''
PRINT '✅ Nettoyage terminé avec succès !'
PRINT '📊 Données conservées :'
PRINT '   - Comptes administrateurs'
PRINT '   - Direction Général'
PRINT '   - Département Direction Général'
PRINT '   - Fonction Général'
PRINT ''

-- Afficher les statistiques
SELECT 'Utilisateurs actifs' AS [Type], COUNT(*) AS [Nombre] FROM Utilisateurs WHERE Supprimer = 0
UNION ALL
SELECT 'Commandes', COUNT(*) FROM Commandes
UNION ALL
SELECT 'Points Consommation', COUNT(*) FROM PointsConsommation
UNION ALL
SELECT 'Formules Jour', COUNT(*) FROM FormulesJour
UNION ALL
SELECT 'Directions', COUNT(*) FROM Directions
UNION ALL
SELECT 'Départements', COUNT(*) FROM Departements
UNION ALL
SELECT 'Fonctions', COUNT(*) FROM Fonctions

GO
