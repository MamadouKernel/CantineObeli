-- Script de nettoyage des doublons dans la base de données Obeli
-- À exécuter une seule fois pour nettoyer les données existantes

USE DbObeli_VK;

-- 1. Supprimer les doublons de départements (garder le plus ancien)
WITH DuplicateDepartements AS (
    SELECT Id, Nom, CreatedOn,
           ROW_NUMBER() OVER (PARTITION BY Nom ORDER BY CreatedOn ASC) as rn
    FROM Departements
    WHERE Supprimer = 0
)
UPDATE Departements 
SET Supprimer = 1, ModifiedBy = 'cleanup_script', ModifiedAt = GETUTCDATE()
WHERE Id IN (
    SELECT Id FROM DuplicateDepartements WHERE rn > 1
);

-- 2. Supprimer les doublons de fonctions (garder le plus ancien)
WITH DuplicateFonctions AS (
    SELECT Id, Nom, CreatedOn,
           ROW_NUMBER() OVER (PARTITION BY Nom ORDER BY CreatedOn ASC) as rn
    FROM Fonctions
    WHERE Supprimer = 0
)
UPDATE Fonctions 
SET Supprimer = 1, ModifiedBy = 'cleanup_script', ModifiedAt = GETUTCDATE()
WHERE Id IN (
    SELECT Id FROM DuplicateFonctions WHERE rn > 1
);

-- 3. Supprimer les doublons de FormulesJour (garder le plus ancien)
WITH DuplicateFormules AS (
    SELECT IdFormule, NomFormule, Date, CreatedOn,
           ROW_NUMBER() OVER (PARTITION BY NomFormule, Date ORDER BY CreatedOn ASC) as rn
    FROM FormulesJour
    WHERE Supprimer = 0
)
UPDATE FormulesJour 
SET Supprimer = 1, ModifiedBy = 'cleanup_script', ModifiedAt = GETUTCDATE()
WHERE IdFormule IN (
    SELECT IdFormule FROM DuplicateFormules WHERE rn > 1
);

-- 4. Supprimer les doublons d'utilisateurs (garder le plus ancien)
WITH DuplicateUsers AS (
    SELECT Id, UserName, CreatedAt,
           ROW_NUMBER() OVER (PARTITION BY UserName ORDER BY CreatedAt ASC) as rn
    FROM Utilisateurs
    WHERE Supprimer = 0
)
UPDATE Utilisateurs 
SET Supprimer = 1, ModifiedBy = 'cleanup_script', ModifiedAt = GETUTCDATE()
WHERE Id IN (
    SELECT Id FROM DuplicateUsers WHERE rn > 1
);

PRINT 'Nettoyage des doublons terminé avec succès';
