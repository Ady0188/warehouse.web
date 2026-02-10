using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace Warehouse.Web.Reporting;

internal interface IReportService
{
    Task<string> PrintReport(Dictionary<string, string> values, string templateName, Dictionary<int, List<dynamic?>> table);
    Task<string> PrintReport(Dictionary<string, string> values, string templateName);
    Task<string> PrintHtml(Dictionary<string, string> values, string templateName, Dictionary<int, List<dynamic>> table);
    Task<string> PrintHtml(Dictionary<string, string> values, string templateName);
    Task<FileContentResult> GenerateReport(Dictionary<string, string> values, string templateName);
    Task<FileContentResult> GenerateReport(Dictionary<string, string> values, string templateName, Dictionary<int, List<dynamic?>> table);
}
internal class ReportService : IReportService
{
    private readonly string _templatesPath;
    private readonly List<string> Columns = new List<string> { "", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

    public ReportService()
    {
        _templatesPath = Path.Combine(Directory.GetCurrentDirectory(), "Templates");

        var count = Columns.Count - 1;
        for (int i = 1; i <= count; i++)
        {
            Columns.Add($"A{Columns[i]}");
        }
        for (int i = 1; i <= count; i++)
        {
            Columns.Add($"B{Columns[i]}");
        }
        for (int i = 1; i <= count; i++)
        {
            Columns.Add($"C{Columns[i]}");
        }
    }

    private (string Column, int Row) SplitAddress(string address)
    {
        var match = Regex.Match(address, @"([A-Za-z]+)(\d+)");
        string c = string.Empty;
        int r = 0;
        if (match.Success)
        {
            c = match.Groups[1].Value;
            r = int.Parse(match.Groups[2].Value);
        }

        return (c, r);
    }

    public async Task<string> PrintReport(Dictionary<string, string> values, string templateName, Dictionary<int, List<dynamic?>> table)
    {
        try
        {
            using var stream = new MemoryStream();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // Load the template
            var templateFilePath = Path.Combine("Templates", $"{templateName}.xltx");

            if (!File.Exists(templateFilePath))
                return default;

            using var package = new ExcelPackage(new FileInfo(templateFilePath));

            // Get the first worksheet
            var worksheet = package.Workbook.Worksheets[0];

            var html = "<div id=\"content\"><table style=\"border-collapse: collapse;\"><tbody>";
            List<string> cellsToSkip = new();

            int tableStartRow = 0;
            int tableStartCol = 0;

            for (int row = 1; row <= worksheet.Dimension.End.Row; row++)
            {
                for (int col = 1; col < worksheet.Dimension.End.Column; col++)
                {
                    var cell = worksheet.Cells[row, col];
                    var value = cell.Value;

                    if (value != null)
                    {
                        if (value.ToString().Equals("[rec1]"))
                        {
                            tableStartRow = row;
                            tableStartCol = col;
                            col = worksheet.Dimension.End.Column;
                            row = worksheet.Dimension.End.Row;
                        }
                    }
                }
            }

            for (int i = 0; i < table.Count; i++)
            {
                for (int j = tableStartCol; j <= worksheet.Dimension.End.Column; j++)
                {
                    if (i > 0 && i < table.Count - 1)
                    {
                        if (j == 1)
                            worksheet.InsertRow(i + tableStartRow, 1);

                        worksheet.Cells[i + tableStartRow + 1, j].CopyStyles(worksheet.Cells[i + tableStartRow, j]);
                    }
                }
            }

            Dictionary<int, string> rangesToMagre = new Dictionary<int, string>();
            List<int> colIndToSkip = new List<int>();
            List<int> valueIndexes = new List<int>();

            for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
            {
                var cell = worksheet.Cells[tableStartRow, col];

                var address = cell.Address;

                if (!colIndToSkip.Contains(col))
                {
                    bool isMerged = worksheet.MergedCells[tableStartRow, col] != null;
                    string mergedRange = isMerged ? worksheet.MergedCells[tableStartRow, col] : "Not Merged";

                    int colCount = 0;
                    if (isMerged)
                    {
                        valueIndexes.Add(col);
                        var k = mergedRange.Split(":");

                        var Start = SplitAddress(k[0]);
                        var End = SplitAddress(k[1]);

                        var rangeToMerge = $"{Start.Column}rowind:{End.Column}rowind";
                        rangesToMagre.Add(col, rangeToMerge);

                        colCount = Columns.IndexOf(End.Column) - Columns.IndexOf(Start.Column) + 1;

                        if (colCount > 1)
                        {
                            for (int jj = Columns.IndexOf(Start.Column) + 1; jj <= Columns.IndexOf(End.Column); jj++)
                            {
                                colIndToSkip.Add(jj);
                            }
                        }
                    }
                    else
                        valueIndexes.Add(col);
                }
            }

            for (int row = tableStartRow; row < tableStartRow + table.Count; row++)
            {
                for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                {
                    var cell = worksheet.Cells[tableStartRow, col];

                    var address = cell.Address;

                    if (rangesToMagre.ContainsKey(col))
                    {
                        var rangeToMerge = rangesToMagre[col].Replace("rowind", row.ToString());

                        worksheet.Cells[rangeToMerge].Merge = true;
                    }

                    if (valueIndexes.Contains(col))
                        worksheet.Cells[row, col].Value = table[row - tableStartRow][valueIndexes.IndexOf(col)];
                }
            }

            for (int row = 1; row <= worksheet.Dimension.End.Row; row++)
            {
                int fontSize = (int)(worksheet.Cells[row, 1].Style.Font.Size * 1.3333);
                int height = (int)(worksheet.Row(worksheet.Cells[row, 1].Start.Row).Height * 1.2);
                html += $"<tr style=\"font-size:{fontSize}px;height:{height}px;\">";
                for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                {
                    var cell = worksheet.Cells[row, col];

                    var address = cell.Address;

                    if (cellsToSkip.Contains(address))
                        continue;

                    var value = cell.Value;

                    if (value is not null)
                    {
                        foreach (var item in values)
                        {
                            if (value.ToString().Contains(item.Key))
                            {
                                value = value.ToString().Replace(item.Key, item.Value);
                                worksheet.Cells[row, col].Value = value;
                            }
                        }
                    }

                    int cellFontSize = (int)(worksheet.Cells[row, col].Style.Font.Size * 1.3333);
                    var isFontBold = worksheet.Cells[row, col].Style.Font.Bold;
                    var isFontItalic = worksheet.Cells[row, col].Style.Font.Italic;

                    bool isMerged = worksheet.MergedCells[row, col] != null;
                    string mergedRange = isMerged ? worksheet.MergedCells[row, col] : "Not Merged";

                    int colCount = 0;
                    int rowCount = 0;
                    string colSpan = string.Empty;
                    string rowSpan = string.Empty;
                    if (isMerged)
                    {
                        var k = mergedRange.Split(":");

                        var Start = SplitAddress(k[0]);
                        var End = SplitAddress(k[1]);

                        colCount = Columns.IndexOf(End.Column) - Columns.IndexOf(Start.Column) + 1;
                        rowCount = End.Row - Start.Row + 1;

                        if (rowCount > 1)
                        {
                            for (int i = Start.Row + 1; i <= End.Row; i++)
                            {
                                for (int j = Columns.IndexOf(Start.Column); j <= Columns.IndexOf(End.Column); j++)
                                {
                                    cellsToSkip.Add($"{Columns[j]}{i}");
                                }
                            }
                        }

                        if (colCount > 1)
                        {
                            for (int j = Columns.IndexOf(Start.Column) + 1; j <= Columns.IndexOf(End.Column); j++)
                            {
                                cellsToSkip.Add($"{Columns[j]}{row}");
                            }
                        }
                    }

                    int width = (int)(worksheet.Column(cell.Start.Column).Width * 7);
                    var borderTop = cell.Style.Border.Top.Style;
                    var borderBottom = cell.Style.Border.Bottom.Style;
                    var borderLeft = cell.Style.Border.Left.Style;
                    var borderRight = cell.Style.Border.Right.Style;
                    var background = cell.Style.Fill.BackgroundColor.Rgb;
                    var horizontalAlignment = cell.Style.HorizontalAlignment;
                    var verticalAlignment = cell.Style.VerticalAlignment;

                    if (!string.IsNullOrEmpty(background) && !background.Equals("FFFFFF00"))
                        address = address;

                    string textAlign = string.Empty;
                    if (horizontalAlignment == ExcelHorizontalAlignment.Right || horizontalAlignment == ExcelHorizontalAlignment.Center)
                        textAlign = horizontalAlignment == ExcelHorizontalAlignment.Right ? "text-align:end;" : "text-align:center;";

                    if (verticalAlignment == ExcelVerticalAlignment.Center || verticalAlignment == ExcelVerticalAlignment.Top)
                        textAlign += verticalAlignment == ExcelVerticalAlignment.Center ? "vertical-align:middle;" : "vertical-align:top;";

                    string borders = string.Empty;
                    if (borderTop != ExcelBorderStyle.None &&
                        borderBottom != ExcelBorderStyle.None &&
                        borderLeft != ExcelBorderStyle.None &&
                        borderRight != ExcelBorderStyle.None)
                        borders = "border: 1px solid black;";
                    else if (borderTop != ExcelBorderStyle.None ||
                        borderBottom != ExcelBorderStyle.None ||
                        borderLeft != ExcelBorderStyle.None ||
                        borderRight != ExcelBorderStyle.None)
                    {
                        if (borderTop != ExcelBorderStyle.None)
                            borders = "border-top: 1px solid black;";

                        if (borderBottom != ExcelBorderStyle.None)
                            borders += "border-bottom: 1px solid black;";

                        if (borderLeft != ExcelBorderStyle.None)
                            borders += "border-left: 1px solid black;";

                        if (borderRight != ExcelBorderStyle.None)
                            borders += "border-right: 1px solid black;";
                    }

                    if (colCount > 1)
                    {
                        colSpan = $" colspan=\"{colCount}\"";
                        col += colCount - 1;
                    }

                    if (rowCount > 1) rowSpan = $" rowspan=\"{rowCount}\"";

                    string fSize = cellFontSize != fontSize ? $"font-size:{cellFontSize}px;" : string.Empty;
                    string fBold = isFontBold ? "font-weight:bold;" : string.Empty;
                    string fStyle = isFontItalic ? "font-style:italic;" : string.Empty;
                    string backgroundColor = background is null ? string.Empty : $"background:#{background};";
                    html += $"<td{colSpan}{rowSpan} style=\"width:{width}px;{fSize}{borders}{textAlign}{fBold}{fStyle}{backgroundColor}\">{value}</td>";

                }
                html += "</tr>";
                //if (rowCount > 0) row += rowCount - 1;
            }

            html += "</tbody></table></div>";

            return html;
        }
        catch (Exception ex)
        {

            throw;
        }
    }

    public async Task<string> PrintReport(Dictionary<string, string> values, string templateName)
    {
        using var stream = new MemoryStream();

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        // Load the template
        var templateFilePath = Path.Combine("Templates", $"{templateName}.xltx");

        if (!File.Exists(templateFilePath))
            return default;

        using var package = new ExcelPackage(new FileInfo(templateFilePath));

        // Get the first worksheet
        var worksheet = package.Workbook.Worksheets[0];

        var html = "<div id=\"content\"><table style=\"border-collapse: collapse;\"><tbody>";
        List<string> cellsToSkip = new();
        for (int row = 1; row <= worksheet.Dimension.End.Row; row++)
        {
            int fontSize = (int)(worksheet.Cells[row, 1].Style.Font.Size * 1.3333);
            int height = (int)(worksheet.Row(worksheet.Cells[row, 1].Start.Row).Height * 1.2);
            html += $"<tr style=\"font-size:{fontSize}px;height:{height}px;\">";
            for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
            {
                var cell = worksheet.Cells[row, col];

                var address = cell.Address;

                if (cellsToSkip.Contains(address))
                    continue;

                var value = cell.Value;

                if (value is not null)
                {
                    foreach (var item in values)
                    {
                        if (value.ToString().Contains(item.Key))
                        {
                            value = value.ToString().Replace(item.Key, item.Value);
                            worksheet.Cells[row, col].Value = value;
                        }
                    }
                }

                int cellFontSize = (int)(worksheet.Cells[row, col].Style.Font.Size * 1.3333);
                var isFontBold = worksheet.Cells[row, col].Style.Font.Bold;
                var isFontItalic = worksheet.Cells[row, col].Style.Font.Italic;

                bool isMerged = worksheet.MergedCells[row, col] != null;
                string mergedRange = isMerged ? worksheet.MergedCells[row, col] : "Not Merged";

                int colCount = 0;
                int rowCount = 0;
                string colSpan = string.Empty;
                string rowSpan = string.Empty;
                if (isMerged)
                {
                    var k = mergedRange.Split(":");

                    var Start = SplitAddress(k[0]);
                    var End = SplitAddress(k[1]);

                    colCount = Columns.IndexOf(End.Column) - Columns.IndexOf(Start.Column) + 1;
                    rowCount = End.Row - Start.Row + 1;

                    if (rowCount > 1)
                    {
                        for (int i = Start.Row + 1; i <= End.Row; i++)
                        {
                            for (int j = Columns.IndexOf(Start.Column); j <= Columns.IndexOf(End.Column); j++)
                            {
                                cellsToSkip.Add($"{Columns[j]}{i}");
                            }
                        }
                    }
                }

                if (address.Equals("AI17"))
                    address = address;

                int width = (int)(worksheet.Column(cell.Start.Column).Width * 7);
                var borderTop = cell.Style.Border.Top.Style;
                var borderBottom = cell.Style.Border.Bottom.Style;
                var borderLeft = cell.Style.Border.Left.Style;
                var borderRight = cell.Style.Border.Right.Style;
                var background = cell.Style.Fill.BackgroundColor.Rgb;
                var horizontalAlignment = cell.Style.HorizontalAlignment;
                var verticalAlignment = cell.Style.VerticalAlignment;

                if (!string.IsNullOrEmpty(background) && !background.Equals("FFFFFF00"))
                    address = address;

                string textAlign = string.Empty;
                if (horizontalAlignment == ExcelHorizontalAlignment.Right || horizontalAlignment == ExcelHorizontalAlignment.Center)
                    textAlign = horizontalAlignment == ExcelHorizontalAlignment.Right ? "text-align:end;" : "text-align:center;";

                if (verticalAlignment == ExcelVerticalAlignment.Center || verticalAlignment == ExcelVerticalAlignment.Top)
                    textAlign += verticalAlignment == ExcelVerticalAlignment.Center ? "vertical-align:middle;" : "vertical-align:top;";

                string borders = string.Empty;
                if (borderTop != ExcelBorderStyle.None &&
                    borderBottom != ExcelBorderStyle.None &&
                    borderLeft != ExcelBorderStyle.None &&
                    borderRight != ExcelBorderStyle.None)
                    borders = "border: 1px solid black;";
                else if (borderTop != ExcelBorderStyle.None ||
                    borderBottom != ExcelBorderStyle.None ||
                    borderLeft != ExcelBorderStyle.None ||
                    borderRight != ExcelBorderStyle.None)
                {
                    if (borderTop != ExcelBorderStyle.None)
                        borders = "border-top: 1px solid black;";

                    if (borderBottom != ExcelBorderStyle.None)
                        borders += "border-bottom: 1px solid black;";

                    if (borderLeft != ExcelBorderStyle.None)
                        borders += "border-left: 1px solid black;";

                    if (borderRight != ExcelBorderStyle.None)
                        borders += "border-right: 1px solid black;";
                }

                if (colCount > 1)
                {
                    colSpan = $" colspan=\"{colCount}\"";
                    col += colCount - 1;
                }

                if (rowCount > 1) rowSpan = $" rowspan=\"{rowCount}\"";

                string fSize = cellFontSize != fontSize ? $"font-size:{cellFontSize}px;" : string.Empty;
                string fBold = isFontBold ? "font-weight:bold;" : string.Empty;
                string fStyle = isFontItalic ? "font-style:italic;" : string.Empty;
                string backgroundColor = background is null ? string.Empty : $"background:#{background};";
                html += $"<td{colSpan}{rowSpan} style=\"width:{width}px;{fSize}{borders}{textAlign}{fBold}{fStyle}{backgroundColor}\">{value}</td>";

            }
            html += "</tr>";
            //if (rowCount > 0) row += rowCount - 1;
        }

        html += "</tbody></table></div>";

        return html;
    }

    public async Task<string> PrintHtml(Dictionary<string, string> values, string templateName, Dictionary<int, List<dynamic>> table)
    {
        var templateFilePath = Path.Combine("Templates", $"{templateName}.txt");

        if (!File.Exists(templateFilePath))
            return default;

        var html = File.ReadAllText(templateFilePath);

        foreach (var value in values)
        {
            html = html.Replace(value.Key, value.Value);
        }

        using var stream = new MemoryStream();

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using var package = new ExcelPackage(new FileInfo(templateFilePath));

        // Get the first worksheet
        var worksheet = package.Workbook.Worksheets[0];

        //var html = "<div id=\"content\"><table style=\"border-collapse: collapse;\"><tbody>";
        List<string> cellsToSkip = new();

        int tableStartRow = 0;
        int tableStartCol = 0;

        for (int row = 1; row <= worksheet.Dimension.End.Row; row++)
        {
            for (int col = 1; col < worksheet.Dimension.End.Column; col++)
            {
                var cell = worksheet.Cells[row, col];
                var value = cell.Value;

                if (value != null)
                {
                    if (value.ToString().Equals("[rec1]"))
                    {
                        tableStartRow = row;
                        tableStartCol = col;
                        col = worksheet.Dimension.End.Column;
                        row = worksheet.Dimension.End.Row;
                    }
                }
            }
        }

        for (int i = 0; i < table.Count; i++)
        {
            for (int j = 0; j < table[i].Count; j++)
            {
                if (i > 0 && i < table.Count - 1)
                {
                    if (j == 0)
                        worksheet.InsertRow(i + tableStartRow, 1);

                    worksheet.Cells[i + tableStartRow + 1, j + tableStartCol].CopyStyles(worksheet.Cells[i + tableStartRow, j + tableStartCol]);
                }

                worksheet.Cells[i + tableStartRow, j + tableStartCol].Value = table[i][j];
            }
        }

        for (int row = 1; row <= worksheet.Dimension.End.Row; row++)
        {
            int fontSize = (int)(worksheet.Cells[row, 1].Style.Font.Size * 1.3333);
            int height = (int)(worksheet.Row(worksheet.Cells[row, 1].Start.Row).Height * 1.2);
            html += $"<tr style=\"font-size:{fontSize}px;height:{height}px;\">";
            for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
            {
                var cell = worksheet.Cells[row, col];

                var address = cell.Address;

                if (cellsToSkip.Contains(address))
                    continue;

                var value = cell.Value;

                if (value is not null)
                {
                    foreach (var item in values)
                    {
                        if (value.ToString().Contains(item.Key))
                        {
                            value = value.ToString().Replace(item.Key, item.Value);
                            worksheet.Cells[row, col].Value = value;
                        }
                    }
                }

                int cellFontSize = (int)(worksheet.Cells[row, col].Style.Font.Size * 1.3333);
                var isFontBold = worksheet.Cells[row, col].Style.Font.Bold;
                var isFontItalic = worksheet.Cells[row, col].Style.Font.Italic;

                bool isMerged = worksheet.MergedCells[row, col] != null;
                string mergedRange = isMerged ? worksheet.MergedCells[row, col] : "Not Merged";

                int colCount = 0;
                int rowCount = 0;
                string colSpan = string.Empty;
                string rowSpan = string.Empty;
                if (isMerged)
                {
                    var k = mergedRange.Split(":");

                    var Start = SplitAddress(k[0]);
                    var End = SplitAddress(k[1]);

                    colCount = Columns.IndexOf(End.Column) - Columns.IndexOf(Start.Column) + 1;
                    rowCount = End.Row - Start.Row + 1;

                    if (rowCount > 1)
                    {
                        for (int i = Start.Row + 1; i <= End.Row; i++)
                        {
                            for (int j = Columns.IndexOf(Start.Column); j <= Columns.IndexOf(End.Column); j++)
                            {
                                cellsToSkip.Add($"{Columns[j]}{i}");
                            }
                        }
                    }
                }

                if (address.Equals("AI17"))
                    address = address;

                int width = (int)(worksheet.Column(cell.Start.Column).Width * 7);
                var borderTop = cell.Style.Border.Top.Style;
                var borderBottom = cell.Style.Border.Bottom.Style;
                var borderLeft = cell.Style.Border.Left.Style;
                var borderRight = cell.Style.Border.Right.Style;
                var background = cell.Style.Fill.BackgroundColor.Rgb;
                var horizontalAlignment = cell.Style.HorizontalAlignment;
                var verticalAlignment = cell.Style.VerticalAlignment;

                if (!string.IsNullOrEmpty(background) && !background.Equals("FFFFFF00"))
                    address = address;

                string textAlign = string.Empty;
                if (horizontalAlignment == ExcelHorizontalAlignment.Right || horizontalAlignment == ExcelHorizontalAlignment.Center)
                    textAlign = horizontalAlignment == ExcelHorizontalAlignment.Right ? "text-align:end;" : "text-align:center;";

                if (verticalAlignment == ExcelVerticalAlignment.Center || verticalAlignment == ExcelVerticalAlignment.Top)
                    textAlign += verticalAlignment == ExcelVerticalAlignment.Center ? "vertical-align:middle;" : "vertical-align:top;";

                string borders = string.Empty;
                if (borderTop != ExcelBorderStyle.None &&
                    borderBottom != ExcelBorderStyle.None &&
                    borderLeft != ExcelBorderStyle.None &&
                    borderRight != ExcelBorderStyle.None)
                    borders = "border: 1px solid black;";
                else if (borderTop != ExcelBorderStyle.None ||
                    borderBottom != ExcelBorderStyle.None ||
                    borderLeft != ExcelBorderStyle.None ||
                    borderRight != ExcelBorderStyle.None)
                {
                    if (borderTop != ExcelBorderStyle.None)
                        borders = "border-top: 1px solid black;";

                    if (borderBottom != ExcelBorderStyle.None)
                        borders += "border-bottom: 1px solid black;";

                    if (borderLeft != ExcelBorderStyle.None)
                        borders += "border-left: 1px solid black;";

                    if (borderRight != ExcelBorderStyle.None)
                        borders += "border-right: 1px solid black;";
                }

                if (colCount > 1)
                {
                    colSpan = $" colspan=\"{colCount}\"";
                    col += colCount - 1;
                }

                if (rowCount > 1) rowSpan = $" rowspan=\"{rowCount}\"";

                string fSize = cellFontSize != fontSize ? $"font-size:{cellFontSize}px;" : string.Empty;
                string fBold = isFontBold ? "font-weight:bold;" : string.Empty;
                string fStyle = isFontItalic ? "font-style:italic;" : string.Empty;
                string backgroundColor = background is null ? string.Empty : $"background:#{background};";
                html += $"<td{colSpan}{rowSpan} style=\"width:{width}px;{fSize}{borders}{textAlign}{fBold}{fStyle}{backgroundColor}\">{value}</td>";

            }
            html += "</tr>";
            //if (rowCount > 0) row += rowCount - 1;
        }

        html += "</tbody></table></div>";

        return html;
    }

    public async Task<string> PrintHtml(Dictionary<string, string> values, string templateName)
    {
        var templateFilePath = Path.Combine("Templates", $"{templateName}.txt");

        if (!File.Exists(templateFilePath))
            return default;

        var html = File.ReadAllText(templateFilePath);

        foreach (var value in values)
        {
            html = html.Replace(value.Key, value.Value);
        }

        return html;
    }

    public async Task<FileContentResult> GenerateReport(Dictionary<string, string> values, string templateName)
    {
        // Create a new memory stream to hold the Excel file
        using var stream = new MemoryStream();

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        // Load the template
        var templateFilePath = Path.Combine(_templatesPath, $"{templateName}.xltx");

        if (!File.Exists(templateFilePath))
            return default;

        using var package = new ExcelPackage(new FileInfo(templateFilePath));

        // Get the first worksheet
        var worksheet = package.Workbook.Worksheets[0];

        // Populate data into the worksheet
        for (int row = 1; row <= worksheet.Dimension.End.Row; row++)
        {
            for (int col = 1; col < worksheet.Dimension.End.Column; col++)
            {
                var value = worksheet.Cells[row, col].Value;

                if (value is null)
                    continue;

                foreach (var item in values)
                {
                    if (value.ToString().Contains(item.Key))
                    {
                        value = value.ToString().Replace(item.Key, item.Value);
                        worksheet.Cells[row, col].Value = value;
                    }
                }
            }
        }

        var bytes = await package.GetAsByteArrayAsync();

        var file = new FileContentResult(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        file.FileDownloadName = "Temp.xlsx";

        return file;
    }

    public async Task<FileContentResult> GenerateReport(Dictionary<string, string> values, string templateName, Dictionary<int, List<dynamic?>> table)
    {
        // Create a new memory stream to hold the Excel file
        using var stream = new MemoryStream();

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        // Load the template
        var templateFilePath = Path.Combine(_templatesPath, $"{templateName}.xltx");

        if (!File.Exists(templateFilePath))
            return default;

        using var package = new ExcelPackage(new FileInfo(templateFilePath));

        // Get the first worksheet
        var worksheet = package.Workbook.Worksheets[0];

        int tableStartRow = 0;
        int tableStartCol = 0;

        for (int row = 1; row <= worksheet.Dimension.End.Row; row++)
        {
            for (int col = 1; col < worksheet.Dimension.End.Column; col++)
            {
                var cell = worksheet.Cells[row, col];
                var value = cell.Value;

                if (value != null)
                {
                    if (value.ToString().Equals("[rec1]"))
                    {
                        tableStartRow = row;
                        tableStartCol = col;
                        col = worksheet.Dimension.End.Column;
                        row = worksheet.Dimension.End.Row;
                    }
                }
            }
        }

        for (int i = 0; i < table.Count; i++)
        {
            for (int j = 0; j < table[i].Count; j++)
            {
                if (i > 0 && i < table.Count - 1)
                {
                    if (j == 0)
                        worksheet.InsertRow(i + tableStartRow, 1);

                    worksheet.Cells[i + tableStartRow + 1, j + tableStartCol].CopyStyles(worksheet.Cells[i + tableStartRow, j + tableStartCol]);
                }

                worksheet.Cells[i + tableStartRow, j + tableStartCol].Value = table[i][j];
            }
        }

        // Populate data into the worksheet
        for (int row = 1; row <= worksheet.Dimension.End.Row; row++)
        {
            for (int col = 1; col < worksheet.Dimension.End.Column; col++)
            {
                var value = worksheet.Cells[row, col].Value;

                if (value is null)
                    continue;

                foreach (var item in values)
                {
                    if (value.ToString().Contains(item.Key))
                    {
                        value = value.ToString().Replace(item.Key, item.Value);
                        worksheet.Cells[row, col].Value = value;
                    }
                }
            }
        }

        var bytes = await package.GetAsByteArrayAsync();

        var file = new FileContentResult(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        file.FileDownloadName = "Temp.xlsx";

        return file;
    }
}
