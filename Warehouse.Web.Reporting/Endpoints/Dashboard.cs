using FastEndpoints;
using MediatR;
using System.Globalization;
using Warehouse.Web.Reporting.Integrations;
using Warehouse.Web.Shared;
using Warehouse.Web.Shared.Responses;

namespace Warehouse.Web.Reporting.Endpoints;

internal class Dashboard : EndpointWithoutRequest<StoreMonthTradeTurnoversResponse>
{
    private readonly ProductTurnoverIngestionService _productTurnoverIngestionService;
    private readonly ICurrentUser _currentUser;

    public Dashboard(ProductTurnoverIngestionService productTurnoverIngestionService, ICurrentUser currentUser)
    {
        _productTurnoverIngestionService = productTurnoverIngestionService;
        _currentUser = currentUser;
    }

    public override void Configure()
    {
        Get(ApiEndpoints.V1.Reports.Dashboard);
        Roles(new string[] { "Admin", "User" });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            var toDate = DateTime.Now;
            var fromDate = toDate.AddMonths(-24);

            List<ProductTurnover> turnovers = await _productTurnoverIngestionService.GetDashboardTurnoversAsync(fromDate, toDate);
            
            await SendAsync(new StoreMonthTradeTurnoversResponse
            {
                StoreId = _currentUser.StoreId,
                DateTrade = turnovers
                    .Where(t => t.Date >= new DateTime(toDate.Year, toDate.Month, 1, 0, 0, 0))
                    .GroupBy(p => new { p.StoreId, Date = p.Date.Date })
                    .Select(g => new StoreDateTradeTurnoverResponse
                    {
                        StoreId = g.Key.StoreId,
                        Date = g.Key.Date,
                        Amount = g.Sum(x => x.Amount - x.Discount) * -1
                    })
                    .OrderBy(r => r.StoreId)
                    .ThenBy(r => r.Date)
                    .ToList(),
                MonthTrade = turnovers
                    .GroupBy(p => new { p.StoreId, Year = p.Date.Year, Month = p.Date.Month })
                    .Select(g => new StoreMonthTradeTurnoverResponse
                    {
                        StoreId = g.Key.StoreId,
                        Date = new DateTime(g.Key.Year, g.Key.Month, 1),
                        Amount = g.Sum(x => x.Amount - x.Discount) * - 1
                    })
                    .OrderBy(r => r.StoreId)
                    .ThenBy(r => r.Date)
                    .ToList()
            });
        }
        catch (Exception ex)
        {
            await SendErrorsAsync(500);
        }
    }
}
