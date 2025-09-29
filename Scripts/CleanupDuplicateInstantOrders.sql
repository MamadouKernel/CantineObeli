-- Script pour nettoyer les commandes instantanées en doublon
-- Garde seulement la commande la plus récente pour chaque utilisateur par jour

-- Afficher les commandes en doublon avant suppression
SELECT 
    c.IdCommande,
    c.UtilisateurId,
    u.UserName,
    u.Nom + ' ' + u.Prenoms AS NomComplet,
    c.DateConsommation,
    c.Date,
    c.Instantanee,
    c.StatusCommande,
    c.Supprimer
FROM Commandes c
INNER JOIN Utilisateurs u ON c.UtilisateurId = u.Id
WHERE c.Instantanee = 1 
    AND c.Supprimer = 0
    AND c.DateConsommation >= CAST(GETDATE() AS DATE) -- Aujourd'hui et jours suivants
ORDER BY c.UtilisateurId, c.DateConsommation, c.Date DESC;

-- Supprimer les doublons en gardant seulement la commande la plus récente
WITH RankedCommands AS (
    SELECT 
        c.IdCommande,
        c.UtilisateurId,
        c.DateConsommation,
        c.Date,
        ROW_NUMBER() OVER (
            PARTITION BY c.UtilisateurId, CAST(c.DateConsommation AS DATE) 
            ORDER BY c.Date DESC, c.IdCommande DESC
        ) as rn
    FROM Commandes c
    WHERE c.Instantanee = 1 
        AND c.Supprimer = 0
        AND c.DateConsommation >= CAST(GETDATE() AS DATE) -- Aujourd'hui et jours suivants
)
UPDATE c 
SET Supprimer = 1,
    ModifiedOn = GETDATE(),
    ModifiedBy = 'SYSTEM_CLEANUP'
FROM Commandes c
INNER JOIN RankedCommands rc ON c.IdCommande = rc.IdCommande
WHERE rc.rn > 1; -- Garde seulement la première (plus récente)

-- Afficher le résultat après nettoyage
SELECT 
    'APRÈS NETTOYAGE' as Status,
    COUNT(*) as NombreCommandesRestantes
FROM Commandes c
WHERE c.Instantanee = 1 
    AND c.Supprimer = 0
    AND c.DateConsommation >= CAST(GETDATE() AS DATE);

-- Afficher les commandes restantes par utilisateur
SELECT 
    c.UtilisateurId,
    u.UserName,
    u.Nom + ' ' + u.Prenoms AS NomComplet,
    COUNT(*) as NombreCommandes,
    MIN(c.DateConsommation) as PremiereDate,
    MAX(c.DateConsommation) as DerniereDate
FROM Commandes c
INNER JOIN Utilisateurs u ON c.UtilisateurId = u.Id
WHERE c.Instantanee = 1 
    AND c.Supprimer = 0
    AND c.DateConsommation >= CAST(GETDATE() AS DATE)
GROUP BY c.UtilisateurId, u.UserName, u.Nom, u.Prenoms
ORDER BY NombreCommandes DESC, u.Nom;
