# Script pour convertir le CSV en Excel avec mise en forme
# Nécessite Excel installé sur Windows

$csvPath = "FONCTIONNALITES_DETAILLEES.csv"
$excelPath = "FONCTIONNALITES_APPLICATION.xlsx"

Write-Host "Conversion du CSV en Excel..." -ForegroundColor Cyan

try {
    # Créer l'objet Excel
    $excel = New-Object -ComObject Excel.Application
    $excel.Visible = $false
    $excel.DisplayAlerts = $false
    
    # Créer un nouveau classeur
    $workbook = $excel.Workbooks.Add()
    $worksheet = $workbook.Worksheets.Item(1)
    $worksheet.Name = "Fonctionnalités"
    
    # Lire le CSV
    $csvData = Import-Csv -Path $csvPath -Encoding UTF8
    
    # Ajouter les en-têtes
    $headers = $csvData[0].PSObject.Properties.Name
    for ($i = 0; $i -lt $headers.Count; $i++) {
        $worksheet.Cells.Item(1, $i + 1) = $headers[$i]
    }
    
    # Formater les en-têtes
    $headerRange = $worksheet.Range("A1", $worksheet.Cells.Item(1, $headers.Count).Address())
    $headerRange.Font.Bold = $true
    $headerRange.Font.Size = 11
    $headerRange.Interior.Color = 15773696  # Bleu clair
    $headerRange.Font.Color = 16777215  # Blanc
    
    # Ajouter les données
    $row = 2
    foreach ($item in $csvData) {
        $col = 1
        foreach ($header in $headers) {
            $worksheet.Cells.Item($row, $col) = $item.$header
            $col++
        }
        $row++
    }
    
    # Ajuster la largeur des colonnes
    $worksheet.Columns.Item(1).ColumnWidth = 20  # Module
    $worksheet.Columns.Item(2).ColumnWidth = 18  # Sous-Module
    $worksheet.Columns.Item(3).ColumnWidth = 25  # Fonctionnalité
    $worksheet.Columns.Item(4).ColumnWidth = 50  # Description
    $worksheet.Columns.Item(5).ColumnWidth = 18  # Rôles
    $worksheet.Columns.Item(6).ColumnWidth = 35  # URL
    $worksheet.Columns.Item(7).ColumnWidth = 12  # Méthode
    $worksheet.Columns.Item(8).ColumnWidth = 40  # Paramètres
    
    # Activer le filtre automatique
    $worksheet.UsedRange.AutoFilter() | Out-Null
    
    # Figer la première ligne
    $worksheet.Application.ActiveWindow.SplitRow = 1
    $worksheet.Application.ActiveWindow.FreezePanes = $true
    
    # Ajouter des bordures
    $dataRange = $worksheet.UsedRange
    $dataRange.Borders.LineStyle = 1
    $dataRange.Borders.Weight = 2
    
    # Aligner le texte
    $dataRange.VerticalAlignment = -4160  # xlTop
    $dataRange.WrapText = $true
    
    # Sauvegarder le fichier
    $workbook.SaveAs($PSScriptRoot + "\" + $excelPath)
    $workbook.Close()
    $excel.Quit()
    
    # Libérer les objets COM
    [System.Runtime.Interopservices.Marshal]::ReleaseComObject($worksheet) | Out-Null
    [System.Runtime.Interopservices.Marshal]::ReleaseComObject($workbook) | Out-Null
    [System.Runtime.Interopservices.Marshal]::ReleaseComObject($excel) | Out-Null
    [System.GC]::Collect()
    [System.GC]::WaitForPendingFinalizers()
    
    Write-Host "✅ Fichier Excel créé avec succès : $excelPath" -ForegroundColor Green
    Write-Host ""
    Write-Host "Le fichier contient :" -ForegroundColor Cyan
    Write-Host "  - $($csvData.Count) fonctionnalités" -ForegroundColor White
    Write-Host "  - Filtres automatiques activés" -ForegroundColor White
    Write-Host "  - En-têtes formatés" -ForegroundColor White
    Write-Host "  - Colonnes ajustées" -ForegroundColor White
    
} catch {
    Write-Host "❌ Erreur lors de la conversion : $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "Note : Ce script nécessite Microsoft Excel installé sur Windows." -ForegroundColor Yellow
    Write-Host "Le fichier CSV est disponible : $csvPath" -ForegroundColor Yellow
}
