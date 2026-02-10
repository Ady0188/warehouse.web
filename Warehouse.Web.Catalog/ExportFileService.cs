using OfficeOpenXml;
using Warehouse.Web.Catalog.Endpoints;
using Warehouse.Web.Shared.Responses;

namespace Warehouse.Web.Agents;

public class ExportFileService
{
    public async Task<ExportFileResult> ExportRemains(List<ProductResponse> items, Dictionary<long, string> storesDic)
    {
        // Create a new memory stream to hold the Excel file
        using var stream = new MemoryStream();

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        // Load the template
        var templateFilePath = Path.Combine("Templates", "ProductRemains.xltx");

        if (!System.IO.File.Exists(templateFilePath))
            return default;

        using var package = new ExcelPackage(new FileInfo(templateFilePath));

        // Get the first worksheet
        var worksheet = package.Workbook.Worksheets[0];

        int rowIndex = 3;
        if (items.Count() - 3 > 0)
            worksheet.InsertRow(rowIndex + 2, items.Count() - 3);

        Dictionary<long, long> storeRemains = new();
        if (items.Count() > 0)
        {
            storeRemains = items.First().StoresRemains;
            if (storeRemains.Count() > 1)
                worksheet.DeleteColumn(6, storeRemains.Count() * 2);
            else if (storeRemains.Count() == 1)
                worksheet.DeleteColumn(6, 20);

            var c = 6;
            foreach (var remains in storeRemains)
            {
                worksheet.Cells[2, c].Value = storesDic[remains.Key];
                c = c + 2;
            }
        }

        foreach (var item in items)
        {
            rowIndex++;
            if (items.Count() > 3 && rowIndex > 4 && rowIndex < items.Count() + 2)
            {
                for (int col = 1; col <= 27; col++)
                {
                    worksheet.Cells[items.Count() + 2, col].CopyStyles(worksheet.Cells[rowIndex, col]);
                }
            }

            worksheet.Cells[rowIndex, 1].Value = rowIndex - 3;
            worksheet.Cells[rowIndex, 2].Value = item.Name;
            worksheet.Cells[rowIndex, 3].Value = item.Manufacturer;
            worksheet.Cells[rowIndex, 4].Value = item.SellPrice;
            worksheet.Cells[rowIndex, 5].Value = item.LimitRemain;

            var c = 6;
            if (storeRemains.Count() > 1)
            {
                foreach (var remains in item.StoresRemains)
                {
                    worksheet.Cells[rowIndex, c].Value = remains.Value;
                    worksheet.Cells[rowIndex, c + 1].Value = remains.Value * item.SellPrice;
                    c = c + 2;
                }
            }

            worksheet.Cells[rowIndex, c].Value = item.StoresRemains.Sum(x => x.Value);
            worksheet.Cells[rowIndex, c + 1].Value = item.StoresRemains.Sum(x => item.SellPrice * x.Value);
        }
        
        var storeName = "";
        if (storeRemains.Count() == 1)
        {
            storeName = $" {storesDic[storeRemains.First().Key]}";
        }

        worksheet.Cells[1, 1].Value = $"Остатки товаров{storeName} на {DateTime.Now.ToString("dd.MM.yyyy")}";

        var bytes = await package.GetAsByteArrayAsync();

        return new ExportFileResult(
            Bytes: bytes,
            FileName: $"Остатки товаров{storeName} на {DateTime.Now.ToString("dd.MM.yyyy")}.xlsx",
            ContentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }
}
