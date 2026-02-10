using OfficeOpenXml;
using Warehouse.Web.Agents.Endpoints;
using Warehouse.Web.Shared.Responses;

namespace Warehouse.Web.Agents;

public class ExportFileService
{
    public async Task<ExportFileResult> Export(List<AgentResponse> items)
    {
        // Create a new memory stream to hold the Excel file
        using var stream = new MemoryStream();

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        // Load the template
        var templateFilePath = Path.Combine("Templates", "Agent.xltx");

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
            worksheet.Cells[rowIndex, 2].Value = item.Name;
            worksheet.Cells[rowIndex, 3].Value = item.StoreName;
            worksheet.Cells[rowIndex, 4].Value = item.ManagerName;
        }
        
        var storeName = "";
        var managerName = "";
        var stores = items.Select(x => x.StoreName).Distinct();
        var managers = items.Select(x => x.ManagerName).Distinct();
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

        worksheet.Cells[1, 1].Value = $"Контрагенты{storeName}{managerName}";

        var bytes = await package.GetAsByteArrayAsync();

        return new ExportFileResult(
            Bytes: bytes,
            FileName: $"Контрагенты{storeName}{managerName}.xlsx",
            ContentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }

    public async Task<ExportFileResult> ExportRemains(List<AgentResponse> items)
    {
        // Create a new memory stream to hold the Excel file
        using var stream = new MemoryStream();

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        // Load the template
        var templateFilePath = Path.Combine("Templates", "AgentRemains.xltx");

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
            worksheet.Cells[rowIndex, 2].Value = item.Name;
            worksheet.Cells[rowIndex, 3].Value = item.StoreName;
            worksheet.Cells[rowIndex, 4].Value = item.ManagerName;
            worksheet.Cells[rowIndex, 5].Value = Math.Abs(item.Debts.DebtOnEnd);
            worksheet.Cells[rowIndex, 6].Value = item.Debts.DebtOnEnd > 0 ? "предоплата" : (item.Debts.DebtOnEnd == 0 ? "" : "долг");
            worksheet.Cells[rowIndex, 7].Value = item.Debts.Level > 0 ? item.Debts.Level : "";
        }
        var debts = items.Where(x => x.Debts.DebtOnEnd < 0).Sum(x => x.Debts.DebtOnEnd);
        var credits = items.Where(x => x.Debts.DebtOnEnd > 0).Sum(x => x.Debts.DebtOnEnd);

        worksheet.Cells[rowIndex + 1, 5].Value = Math.Abs(debts);
        worksheet.Cells[rowIndex + 1, 6].Value = debts == 0 ? "" : "долг";
        worksheet.Cells[rowIndex + 2, 5].Value = Math.Abs(credits);
        worksheet.Cells[rowIndex + 2, 6].Value = credits == 0 ? "" : "предоплата";

        var storeName = "";
        var managerName = "";
        var stores = items.Select(x => x.StoreName).Distinct();
        var managers = items.Select(x => x.ManagerName).Distinct();
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

        worksheet.Cells[1, 1].Value = $"Долги{storeName}{managerName} на {DateTime.Now.ToString("dd.MM.yyyy")}";

        var bytes = await package.GetAsByteArrayAsync();

        return new ExportFileResult(
            Bytes: bytes,
            FileName: $"Долги{storeName}{managerName} на {DateTime.Now.ToString("dd.MM.yyyy")}.xlsx",
            ContentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }
}
