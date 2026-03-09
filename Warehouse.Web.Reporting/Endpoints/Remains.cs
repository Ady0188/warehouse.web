using FastEndpoints;
using MediatR;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Warehouse.Web.Reporting.Integrations;
using Warehouse.Web.Shared;
using Warehouse.Web.Shared.Responses;

namespace Warehouse.Web.Reporting.Endpoints;
internal class Remains : Endpoint<PagedRequest, RemainsesResponse>
{
    private readonly IMediator _mediator;
    private readonly ProductTurnoverIngestionService _productTurnoverIngestionService;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<Remains> _logger;

    public Remains(IMediator mediator, ProductTurnoverIngestionService productTurnoverIngestionService, ICurrentUser currentUser, ILogger<Remains> logger)
    {
        _mediator = mediator;
        _productTurnoverIngestionService = productTurnoverIngestionService;
        _currentUser = currentUser;
        _logger = logger;
    }

    public override void Configure()
    {
        Get(ApiEndpoints.V1.Reports.Remains);
        Roles(new string[] { "Admin", "User" });
    }

    public override async Task HandleAsync(PagedRequest request, CancellationToken ct)
    {
        try
        {
            var date = DateTime.Now;
            if (!string.IsNullOrEmpty(request.DateTime))
            {
                if (!DateTime.TryParseExact(request.DateTime, "ddMMyyyyHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                {
                    AddError("Invalid date format. Expected ddMMyyyyHHmmss.");
                    await SendErrorsAsync(400, ct);
                    return;
                }
                date = parsed.AddMilliseconds(-50);
            }

            List<ProductTurnover> turnovers = new();
            if (_currentUser.StoreId == 0)
                turnovers = await _productTurnoverIngestionService.GetAllTurnoversWithProductsAsync(date);
            else
            {
                var fromDate = await _productTurnoverIngestionService.GetLastAuditProductsDateAsync(_currentUser.StoreId, date);

                turnovers = await _productTurnoverIngestionService.GetTurnoversWithProductsByStoreIdAsync(_currentUser.StoreId, fromDate, date);
            }

            await SendAsync(new RemainsesResponse
            {
                StoreRemains = turnovers
                    .GroupBy(t => t.StoreId)
                    .ToDictionary(
                        g => g.Key,
                        g =>
                            g.SelectMany(t => t.Products ?? Enumerable.Empty<Product>())
                             .Where(p => p != null && p.ProductId != 0)
                             .GroupBy(p => p.ProductId)
                             .Select(pg => new RemainsResponse
                             {
                                 ProductId = pg.Key,
                                 // Если Quantity = long:
                                 Count = pg.Sum(x => x.Quantity)

                                 // Если Quantity = decimal (часто бывает так):
                                 // либо меняем RemainsResponse.Count на decimal,
                                 // либо приводим к long (округление по вашей бизнес-логике):
                                 // Count = (long)pg.Sum(x => Convert.ToDecimal(x.Quantity))
                             })
                             //.OrderBy(r => r.ProductId)
                             .ToList()
                    )
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get remains report.");
            await SendErrorsAsync(500);
        }
    }
}
