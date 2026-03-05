-- Script SQL pour ajouter le paramètre FACTURATION_HEURE_EXECUTION
-- À exécuter manuellement ou via une migration

-- Vérifier si le paramètre existe déjà
IF NOT EXISTS (SELECT 1 FROM ConfigurationsCommande WHERE Cle = 'FACTURATION_HEURE_EXECUTION' AND Supprimer = 0)
BEGIN
    INSERT INTO ConfigurationsCommande (Cle, Valeur, Description, CreatedOn, CreatedBy, Supprimer)
    VALUES (
        'FACTURATION_HEURE_EXECUTION',
        '2',
        'Heure d''exécution de la facturation automatique (0-23). Par défaut: 2h du matin',
        GETUTCDATE(),
        'System',
        0
    );
    
    PRINT '✅ Paramètre FACTURATION_HEURE_EXECUTION ajouté avec succès (valeur: 2)';
END
ELSE
BEGIN
    PRINT 'ℹ️ Paramètre FACTURATION_HEURE_EXECUTION existe déjà';
END
GO
