using FastEndpoints;
using MediatR;
using System.Globalization;
using Warehouse.Web.Reporting.Integrations;
using Warehouse.Web.Shared;
using Warehouse.Web.Shared.Responses;

namespace Warehouse.Web.Reporting.Endpoints;
internal class Debts : Endpoint<PagedRequest, AgentsResponse>
{
    private readonly IMediator _mediator;
    private readonly AgentRemainsIngestionService _agentRemainsIngestionService;
    private readonly ICurrentUser _currentUser;

    public Debts(IMediator mediator, ICurrentUser currentUser, AgentRemainsIngestionService agentRemainsIngestionService)
    {
        _mediator = mediator;
        _currentUser = currentUser;
        _agentRemainsIngestionService = agentRemainsIngestionService;
    }

    public override void Configure()
    {
        Get(ApiEndpoints.V1.Reports.Debts);
        Roles(new string[] { "Admin", "User" });
    }

    public override async Task HandleAsync(PagedRequest request, CancellationToken ct)
    {
        try
        {
            var date = DateTime.Now;
            if (!string.IsNullOrEmpty(request.DateTime))
                date = DateTime.ParseExact(request.DateTime, "ddMMyyyyHHmmss", CultureInfo.InvariantCulture).AddMilliseconds(-50);

            var all = await _agentRemainsIngestionService.GetAllDebtsAsync();

            var agentsDebts = all
                .GroupBy(x => x.AgentId)
                .ToDictionary(
                    g => g.Key,
                    g => new AgentDebtsResponse
                    {
                        Id = g.Key,
                        DebtOnBegin = 0,
                        Debt = all.Where(x => x.ObjectName == "Operation").Sum(x => x.Amount),
                        Credit = all.Where(x => x.ObjectName == "Order").Sum(x => x.Amount),
                        DebtOnEnd = all.Sum(x => x.Amount),
                    });

            await SendAsync(new AgentsResponse());

            //List<ProductTurnover> turnovers = new();
            //if (_currentUser.StoreId == 0)
            //    turnovers = await _productTurnoverIngestionService.GetAllTurnoversWithProductsAsync(date);
            //else
            //{
            //    var fromDate = await _productTurnoverIngestionService.GetLastAuditProductsDateAsync(_currentUser.StoreId, date);

            //    turnovers = await _productTurnoverIngestionService.GetTurnoversWithProductsByStoreIdAsync(_currentUser.StoreId, fromDate, date);
            //}

            //await SendAsync(new RemainsesResponse
            //{
            //    StoreRemains = turnovers
            //        .GroupBy(t => t.StoreId)
            //        .ToDictionary(
            //            g => g.Key,
            //            g =>
            //                g.SelectMany(t => t.Products ?? Enumerable.Empty<Product>())
            //                 .Where(p => p != null && p.ProductId != 0)
            //                 .GroupBy(p => p.ProductId)
            //                 .Select(pg => new RemainsResponse
            //                 {
            //                     ProductId = pg.Key,
            //                     // Если Quantity = long:
            //                     Count = pg.Sum(x => x.Quantity)

            //                     // Если Quantity = decimal (часто бывает так):
            //                     // либо меняем RemainsResponse.Count на decimal,
            //                     // либо приводим к long (округление по вашей бизнес-логике):
            //                     // Count = (long)pg.Sum(x => Convert.ToDecimal(x.Quantity))
            //                 })
            //                 //.OrderBy(r => r.ProductId)
            //                 .ToList()
            //        )
            //});
        }
        catch (Exception ex)
        {
            await SendErrorsAsync(500);
        }
    }
}
