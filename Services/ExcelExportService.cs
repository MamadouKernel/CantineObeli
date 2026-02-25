using ClosedXML.Excel;
using System.Reflection;

namespace Obeli_K.Services
{
    /// <summary>
    /// Service générique pour l'export Excel des tableaux
    /// </summary>
    public class ExcelExportService
    {
        private readonly ILogger<ExcelExportService> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ExcelExportService(ILogger<ExcelExportService> logger, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        /// <summary>
        /// Exporte une liste d'objets vers un fichier Excel
        /// </summary>
        /// <typeparam name="T">Type des objets à exporter</typeparam>
        /// <param name="data">Liste des données à exporter</param>
        /// <param name="fileName">Nom du fichier (sans extension)</param>
        /// <param name="sheetName">Nom de la feuille</param>
        /// <param name="title">Titre du rapport</param>
        /// <returns>Fichier Excel en bytes</returns>
        public byte[] ExportToExcel<T>(IEnumerable<T> data, string fileName, string sheetName = "Données", string? title = null)
        {
            try
            {
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add(sheetName);

                // Obtenir les propriétés avec DisplayAttribute
                var properties = GetDisplayProperties<T>();
                
                // Vérifier qu'il y a au moins une propriété à exporter
                if (properties.Count == 0)
                {
                    _logger.LogWarning("Aucune propriété à exporter pour le type {Type}", typeof(T).Name);
                    throw new InvalidOperationException($"Aucune propriété exportable trouvée pour le type {typeof(T).Name}");
                }

                // Ajouter le titre si fourni
                if (!string.IsNullOrEmpty(title))
                {
                    var titleCell = worksheet.Cell(1, 1);
                    titleCell.Value = title;
                    titleCell.Style.Font.Bold = true;
                    titleCell.Style.Font.FontSize = 16;
                    titleCell.Style.Font.FontColor = XLColor.FromArgb(163, 45, 24); // Couleur #A32D18
                    titleCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    titleCell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    
                    worksheet.Range(1, 1, 1, properties.Count).Merge();
                    worksheet.Row(1).Height = 30; // Plus de hauteur pour le titre
                    worksheet.Row(2).Height = 5; // Espacement
                }

                var startRow = string.IsNullOrEmpty(title) ? 1 : 3;

                // Créer les en-têtes avec la couleur de la charte graphique
                for (int i = 0; i < properties.Count; i++)
                {
                    var headerCell = worksheet.Cell(startRow, i + 1);
                    headerCell.Value = properties[i].DisplayName;
                    headerCell.Style.Font.Bold = true;
                    headerCell.Style.Font.FontColor = XLColor.White;
                    headerCell.Style.Fill.BackgroundColor = XLColor.FromArgb(237, 172, 0); // Couleur #EDAC00 (jaune-or)
                    headerCell.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
                    headerCell.Style.Border.OutsideBorderColor = XLColor.FromArgb(163, 45, 24); // Bordure #A32D18
                    headerCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerCell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                }

                // Remplir les données avec alternance de couleurs
                var row = startRow + 1;
                var rowIndex = 0;
                foreach (var item in data)
                {
                    if (item == null) continue;
                    
                    for (int i = 0; i < properties.Count; i++)
                    {
                        var cell = worksheet.Cell(row, i + 1);
                        var value = GetPropertyValue(item, properties[i].Property);
                        
                        // Formater selon le type
                        FormatCell(cell, value, properties[i].Property);
                        cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        cell.Style.Border.OutsideBorderColor = XLColor.LightGray;
                        
                        // Alternance de couleurs pour une meilleure lisibilité
                        if (rowIndex % 2 == 0)
                        {
                            cell.Style.Fill.BackgroundColor = XLColor.White;
                        }
                        else
                        {
                            cell.Style.Fill.BackgroundColor = XLColor.FromArgb(250, 250, 250); // Gris très clair
                        }
                    }
                    row++;
                    rowIndex++;
                }

                // Ajuster la largeur des colonnes
                worksheet.Columns().AdjustToContents();

                // Ajouter des filtres automatiques
                if (data.Any())
                {
                    var range = worksheet.Range(startRow, 1, row - 1, properties.Count);
                    range.SetAutoFilter();
                }

                // Ajouter des informations sur l'export avec style - Zone footer
                var infoRow = row + 2;
                
                // Ligne 1: Date d'export
                var dateCell = worksheet.Cell(infoRow, 1);
                dateCell.Value = $"📅 Exporté le : {DateTime.Now:dd/MM/yyyy HH:mm:ss}";
                dateCell.Style.Font.Italic = true;
                dateCell.Style.Font.FontColor = XLColor.FromArgb(89, 49, 31); // Couleur #59311F
                dateCell.Style.Font.FontSize = 9;
                
                // Ligne 2: Nombre d'éléments
                var countCell = worksheet.Cell(infoRow + 1, 1);
                countCell.Value = $"📊 Nombre d'éléments : {data.Count()}";
                countCell.Style.Font.Italic = true;
                countCell.Style.Font.FontColor = XLColor.FromArgb(89, 49, 31); // Couleur #59311F
                countCell.Style.Font.FontSize = 9;
                
                // Ligne vide pour espacement
                worksheet.Row(infoRow + 2).Height = 10;
                
                // Ligne 3: Signature O'Beli (centrée)
                var footerRow = infoRow + 3;
                var footerCell = worksheet.Cell(footerRow, 1);
                footerCell.Value = "🍽️ O'Beli - Système de gestion de restauration";
                footerCell.Style.Font.Bold = true;
                footerCell.Style.Font.FontColor = XLColor.FromArgb(237, 172, 0); // Couleur #EDAC00
                footerCell.Style.Font.FontSize = 11;
                
                // Ligne 4: Ajouter le logo centré en dessous
                worksheet.Row(footerRow).Height = 20; // Hauteur pour le texte
                AddFooterLogoCentered(worksheet, properties.Count, footerRow + 1);

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                return stream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'export Excel pour {FileName}", fileName);
                throw;
            }
        }

        /// <summary>
        /// Ajoute le logo aligné à gauche avec les textes
        /// </summary>
        private void AddFooterLogoCentered(IXLWorksheet worksheet, int columnCount, int footerRow)
        {
            try
            {
                var logoPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "logo.png");
                
                if (File.Exists(logoPath))
                {
                    _logger.LogInformation("Logo trouvé au chemin : {LogoPath}", logoPath);
                    
                    // Positionner le logo à gauche, aligné avec les textes (colonne 1)
                    var logoColumn = 1;
                    
                    _logger.LogInformation("Ajout du logo aligné à gauche (ligne {Row}, colonne {Column})", footerRow, logoColumn);
                    
                    // Ajouter l'image alignée à gauche
                    var picture = worksheet.AddPicture(logoPath);
                    
                    // Positionner à gauche, aligné avec les textes
                    picture.MoveTo(worksheet.Cell(footerRow, logoColumn));
                    
                    // Taille professionnelle pour le footer (150x75 pixels)
                    picture.WithSize(150, 75);
                    
                    // Augmenter la hauteur de la ligne du logo pour qu'il s'affiche correctement
                    worksheet.Row(footerRow).Height = 60;
                    worksheet.Row(footerRow + 1).Height = 5; // Ligne vide après le logo

                    _logger.LogInformation("Logo aligné à gauche ajouté avec succès en bas de page (150x75 pixels)");
                }
                else
                {
                    _logger.LogWarning("Logo non trouvé à l'emplacement : {LogoPath}", logoPath);
                }
            }
            catch (Exception ex)
            {
                // Ne pas bloquer l'export si le logo ne peut pas être ajouté
                _logger.LogError(ex, "Erreur lors de l'ajout du logo aligné en pied de page : {Message}", ex.Message);
                if (ex.InnerException != null)
                {
                    _logger.LogError("Inner exception: {InnerMessage}", ex.InnerException.Message);
                }
            }
        }

        /// <summary>
        /// Obtient les propriétés avec DisplayAttribute
        /// </summary>
        private List<PropertyDisplayInfo> GetDisplayProperties<T>()
        {
            var properties = typeof(T).GetProperties()
                .Where(p => p.CanRead)
                .Select(p => new PropertyDisplayInfo
                {
                    Property = p,
                    DisplayName = GetDisplayName(p)
                })
                .Where(p => !string.IsNullOrEmpty(p.DisplayName))
                .ToList();

            return properties;
        }

        /// <summary>
        /// Obtient le nom d'affichage d'une propriété
        /// </summary>
        private string GetDisplayName(PropertyInfo property)
        {
            var displayAttribute = property.GetCustomAttribute<System.ComponentModel.DataAnnotations.DisplayAttribute>();
            if (displayAttribute != null)
            {
                return displayAttribute.Name ?? property.Name;
            }

            // Fallback sur le nom de la propriété
            return property.Name;
        }

        /// <summary>
        /// Obtient le nombre de propriétés exportables
        /// </summary>
        private int GetPropertyCount<T>()
        {
            return GetDisplayProperties<T>().Count;
        }

        /// <summary>
        /// Obtient la valeur d'une propriété
        /// </summary>
        private object GetPropertyValue(object obj, PropertyInfo property)
        {
            try
            {
                var value = property.GetValue(obj);
                
                // Gérer les valeurs nulles
                if (value == null)
                    return string.Empty;

                // Gérer les types spéciaux
                if (value is DateTime dateTime)
                    return dateTime.ToString("dd/MM/yyyy HH:mm");
                
                if (value is decimal decimalValue)
                    return decimalValue;
                
                if (value is bool boolValue)
                    return boolValue ? "Oui" : "Non";

                return value.ToString() ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Formate une cellule selon le type de données
        /// </summary>
        private void FormatCell(IXLCell cell, object value, PropertyInfo property)
        {
            cell.Value = value?.ToString() ?? string.Empty;

            // Formatage selon le type
            if (value is decimal || value is double || value is float)
            {
                cell.Style.NumberFormat.Format = "#,##0.00";
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            }
            else if (value is DateTime)
            {
                cell.Style.NumberFormat.Format = "dd/mm/yyyy hh:mm";
            }
            else if (value is bool)
            {
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }
        }

        /// <summary>
        /// Classe pour stocker les informations d'affichage des propriétés
        /// </summary>
        private class PropertyDisplayInfo
        {
            public PropertyInfo Property { get; set; } = default!;
            public string DisplayName { get; set; } = default!;
        }
    }
}
