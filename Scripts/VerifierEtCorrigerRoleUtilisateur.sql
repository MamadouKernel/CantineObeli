-- Script pour vérifier et corriger le rôle d'un utilisateur
-- Rôles disponibles dans l'enum RoleType:
-- 0 = Admin (Administrateur)
-- 1 = RH (Ressources Humaines)
-- 2 = Employe
-- 3 = PrestataireCantine

-- 1. Vérifier le rôle actuel de l'utilisateur KONE Zomina Raissa
SELECT 
    Id,
    UserName,
    Nom,
    Prenoms,
    Email,
    Role,
    CASE Role
        WHEN 0 THEN 'Administrateur'
        WHEN 1 THEN 'RH'
        WHEN 2 THEN 'Employe'
        WHEN 3 THEN 'PrestataireCantine'
        ELSE 'Inconnu'
    END AS RoleNom,
    Supprimer
FROM Utilisateurs
WHERE (Nom LIKE '%KONE%' OR Prenoms LIKE '%Zomina%' OR Prenoms LIKE '%Raissa%')
    AND Supprimer = 0;

-- 2. Si le rôle n'est pas correct, le corriger (décommenter et adapter l'ID)
-- UPDATE Utilisateurs
-- SET Role = 1,  -- 1 = RH
--     ModifiedBy = 'Admin',
--     ModifiedAt = GETDATE()
-- WHERE Id = 'REMPLACER_PAR_L_ID_DE_L_UTILISATEUR'
--     AND Supprimer = 0;

-- 3. Vérifier tous les utilisateurs avec le rôle RH
SELECT 
    Id,
    UserName,
    Nom,
    Prenoms,
    Email,
    Role,
    CASE Role
        WHEN 0 THEN 'Administrateur'
        WHEN 1 THEN 'RH'
        WHEN 2 THEN 'Employe'
        WHEN 3 THEN 'PrestataireCantine'
        ELSE 'Inconnu'
    END AS RoleNom,
    Supprimer
FROM Utilisateurs
WHERE Role = 1  -- RH
    AND Supprimer = 0
ORDER BY Nom, Prenoms;

-- 4. Vérifier tous les utilisateurs actifs
SELECT 
    Id,
    UserName,
    Nom,
    Prenoms,
    Email,
    Role,
    CASE Role
        WHEN 0 THEN 'Administrateur'
        WHEN 1 THEN 'RH'
        WHEN 2 THEN 'Employe'
        WHEN 3 THEN 'PrestataireCantine'
        ELSE 'Inconnu'
    END AS RoleNom,
    Supprimer,
    CreatedAt,
    ModifiedAt
FROM Utilisateurs
WHERE Supprimer = 0
ORDER BY Role, Nom, Prenoms;
