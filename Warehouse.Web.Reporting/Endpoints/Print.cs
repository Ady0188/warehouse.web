using DinkToPdf;
using DinkToPdf.Contracts;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.Drawing.Printing;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Warehouse.Web.Reporting.Contracts;
using Warehouse.Web.Reporting.Integrations;
using Warehouse.Web.Shared;
using Warehouse.Web.Shared.Responses;

namespace Warehouse.Web.Reporting.Endpoints;

internal record PrintRequest([FromRoute] long OperationId, [FromRoute] string TemplateName);

internal class Print : Endpoint<PrintRequest, string>
{
    private readonly ProductTurnoverIngestionService _productTurnoverIngestionService;
    private readonly IReportService _reportService;
    private readonly IWebHostEnvironment _env;
    private readonly IConverter _converter;
    private readonly IMediator _mediator;
    private readonly ILogger<Print> _logger;

    public Print(ProductTurnoverIngestionService productTurnoverIngestionService, IReportService reportService, IConverter converter, IWebHostEnvironment env, IMediator mediator, ILogger<Print> logger)
    {
        _productTurnoverIngestionService = productTurnoverIngestionService;
        _reportService = reportService;
        _converter = converter;
        _env = env;
        _mediator = mediator;
        _logger = logger;
    }

    public override void Configure()
    {
        Get(ApiEndpoints.V1.Reports.Print);
        Roles(new string[] { "Admin", "User" });
    }

    public override async Task HandleAsync(PrintRequest req, CancellationToken ct)
    {
        if (!Regex.IsMatch(req.TemplateName, "^[A-Za-z0-9_-]+$"))
        {
            AddError("Invalid template name.");
            await SendErrorsAsync(400, ct);
            return;
        }

        var operation = await _productTurnoverIngestionService
            .GetTurnoverWithProductsByObjectAsync(req.OperationId, "Operation");

        if (operation is null)
        {
            await SendErrorsAsync(404);
            return;
        }

        var agentsDebtsQuery = new GetAgentsDebtsQuery(operation.Date.AddMilliseconds(-50), operation.Date);
        var debtsResult = await _mediator.Send(agentsDebtsQuery);

        if (!debtsResult.IsSuccess)
        {
            await SendErrorsAsync(400);
            return;
        }

        var agentDebts = debtsResult.Value.FirstOrDefault(x => x.Id == operation.AgentId);

        if (agentDebts is null && req.TemplateName != "MovementReport")
        {
            await SendErrorsAsync(404);
            return;
        }
        
        var template = await File.ReadAllTextAsync(Path.Combine("Templates", $"{req.TemplateName}.html"), ct);
        var result = await PrintOperationAsync(operation, new string[] { template }, agentDebts, ct);

        await SendAsync(result);
    }

    async Task<string> PrintOperationAsync(ProductTurnover request, string[] pages, AgentDebtsResponse agentDebts, CancellationToken ct)
    {
        var imgPath = Path.Combine(_env.WebRootPath);

        if (!Directory.Exists(imgPath))
            throw new DirectoryNotFoundException();

        var tempDir = new DirectoryInfo(Path.Combine(imgPath, "printtemp"));

        if (!tempDir.Exists)
            tempDir.Create();

        var fileName = Guid.NewGuid().ToString();

        var filePathToReturn = Path.Combine("printtemp", $"{fileName}.pdf");
        var pathToSave = Path.Combine(tempDir.FullName, $"{fileName}.pdf");

        foreach (var file in tempDir.GetFiles().Where(x => x.CreationTime <= DateTime.Now.AddMinutes(-10)))
        {
            file.Delete();
        }

        try
        {
            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = new GlobalSettings
                {
                    PaperSize = DinkToPdf.PaperKind.A4,
                    Orientation = Orientation.Portrait,
                    Margins = new MarginSettings { Top = 5, Bottom = 5 }
                }
            };

            foreach (var page in pages)
            {
                doc.Objects.Add(new ObjectSettings
                {
                    HtmlContent = ReplaceTemplateData(page, request, agentDebts),
                    WebSettings = { DefaultEncoding = "utf-8" }
                });
            }

            byte[] pdf = _converter.Convert(doc);

            await System.IO.File.WriteAllBytesAsync(pathToSave, pdf, ct);

            ////Return the file
            //return File(pdf, "application/pdf", "generated.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate print file for operation {OperationId}.", request.ObjectId);
            throw;
        }

        return filePathToReturn;
    }

    private string ReplaceTemplateData(string html, ProductTurnover operation, AgentDebtsResponse agentDebts)
    {
        html = html.Replace("[Number]", operation.ObjectCode.ToString().PadLeft(6, '0'))
            .Replace("[DateTime]", operation.Date.ToString("dd.MM.yyyy HH:mm") ?? string.Empty)
            .Replace("[Store]", operation.StoreName ?? string.Empty)
            .Replace("[FromStore]", operation.StoreName ?? string.Empty)
            .Replace("[ToStore]", operation.AgentName ?? string.Empty)
            .Replace("[Manager]", operation.ManagerName ?? string.Empty)
            .Replace("[ManagerPhone]", operation.ManagerPhone ?? string.Empty)
            .Replace("[Agent]", operation.AgentName ?? string.Empty)
            .Replace("[Address]", operation.AgentAddress ?? string.Empty)
            .Replace("[AgentPhone]", operation.AgentPhone ?? string.Empty)
            .Replace("[Total]", Math.Abs(operation.Amount).ToString("F2") ?? string.Empty)
            .Replace("[Discount]", Math.Abs(operation.Discount).ToString("F2") ?? string.Empty)
            .Replace("[AfterDiscount]", Math.Abs(operation.Amount - operation.Discount).ToString("F2"))
            .Replace("[Paid]", "0" ?? string.Empty);

        if (agentDebts is not null)
            html = html
            .Replace("[Debt]", Math.Abs(agentDebts.DebtOnBegin).ToString("F2") ?? string.Empty)
            .Replace("[TotalDebt]", Math.Abs(agentDebts.DebtOnEnd).ToString("F2") ?? string.Empty)
            .Replace("[DebtTitle]", agentDebts.DebtOnBegin < 0 ? " (қарз)" : (agentDebts.DebtOnBegin == 0 ? "" : " (пешпардохт)"))
            .Replace("[TotalDebtTitle]", agentDebts.DebtOnEnd < 0 ? " (қарз)" : (agentDebts.DebtOnEnd == 0 ? "" : " (пешпардохт)"));

        //string tdStryle = " style=\"padding-top:0;padding-bottom:0;\"";
        string tdRightStryle = " style=\"text-align:right;\"";
        string tdCenterStryle = " style=\"text-align:center;\"";
        if (operation.Products is not null && operation.Products.Count > 0)
        {
            string productsTable = string.Empty;

            int ind = 0;
            foreach (var product in operation.Products)
            {
                ind++;
                productsTable += $"<tr><td{tdCenterStryle}>[Num]</td><td>[Manufacturer]</td><td>[Name]</td><td{tdCenterStryle}>[Quantity]</td><td{tdCenterStryle}>[Price]</td><td{tdRightStryle}>[Total]</td></tr>";
                productsTable = productsTable.Replace("[Num]", ind.ToString())
                    .Replace("[Manufacturer]", product.Manufacturer ?? string.Empty)
                    .Replace("[Name]", product.ProductName ?? string.Empty)
                    .Replace("[Quantity]", Math.Abs(product.Quantity).ToString() ?? string.Empty)
                    .Replace("[Price]", product.Price.ToString("F2") ?? string.Empty)
                    .Replace("[Total]", Math.Abs(product.Price * product.Quantity).ToString("F2") ?? string.Empty);
            }

            html = html.Replace("[ProductsTable]", $"<tbody>{productsTable}</tbody>" ?? string.Empty);
        }
        else
            html = html.Replace("[ProductsTable]", string.Empty);

        return html;
    }
}
