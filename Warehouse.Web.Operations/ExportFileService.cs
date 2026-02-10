using Warehouse.Web.Operations.Endpoints;
using Warehouse.Web.Shared.Responses;
using OfficeOpenXml;

namespace Warehouse.Web.Operations;

public class ExportFileService
{
    public async Task<ExportFileResult> Export(List<OperationResponse> items)
    {
        // Create a new memory stream to hold the Excel file
        using var stream = new MemoryStream();

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        // Load the template
        var templateFilePath = Path.Combine("Templates", "Operations.xltx");

        if (!System.IO.File.Exists(templateFilePath))
            return default;

        using var package = new ExcelPackage(new FileInfo(templateFilePath));

        // Get the first worksheet
        var worksheet = package.Workbook.Worksheets[0];

        int rowIndex = 3;
        if (items.Count() - 3 > 0)
            worksheet.InsertRow(rowIndex + 2, items.Count() - 3);

        foreach (var item in items)
        {
            rowIndex++;
            if (items.Count() > 3 && rowIndex > 4 && rowIndex < items.Count() + 2)
            {
                for (int col = 1; col <= 11; col++)
                {
                    worksheet.Cells[items.Count() + 2, col].CopyStyles(worksheet.Cells[rowIndex, col]);
                }
            }

            worksheet.Cells[rowIndex, 1].Value = rowIndex - 3;
            worksheet.Cells[rowIndex, 2].Value = item.Code.ToString().PadLeft(7, '0');
            worksheet.Cells[rowIndex, 3].Value = item.Date.ToString("dd.MM.yyyy HH:mm");
            worksheet.Cells[rowIndex, 4].Value = item.StoreName;
            worksheet.Cells[rowIndex, 5].Value = item.ManagerName;
            worksheet.Cells[rowIndex, 6].Value = item.AgentName;
            worksheet.Cells[rowIndex, 7].Value = item.Amount;
            worksheet.Cells[rowIndex, 8].Value = item.Discount;
            worksheet.Cells[rowIndex, 9].Value = item.ToPay;
        }

        var storeName = "";
        var managerName = "";
        var agentName = "";
        var stores = items.Select(x => x.StoreName).Distinct();
        var managers = items.Select(x => x.ManagerName).Distinct();
        var types = items.Select(x => x.Type).Distinct();
        if (managers.Count() == 1)
        {
            managerName = $" {managers.First()}";

            worksheet.DeleteColumn(4);
        }
        if (stores.Count() == 1)
        {
            storeName = $" {stores.First()}";

            worksheet.DeleteColumn(3);
        }

        var title = $"Операции{storeName}{managerName}{agentName}";
        if (types.Count() == 1)
            title = $"{types.First().GetTitle()}{storeName}{managerName}{agentName}";

        worksheet.Cells[1, 1].Value = title;

        var bytes = await package.GetAsByteArrayAsync();

        return new ExportFileResult(
            Bytes: bytes,
            FileName: $"{title}.xlsx",
            ContentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }
}
