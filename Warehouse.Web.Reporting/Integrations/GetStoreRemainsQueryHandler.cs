using Ardalis.Result;
using MediatR;
using Warehouse.Web.Reporting.Contracts;
using Warehouse.Web.Shared.Responses;

namespace Warehouse.Web.Reporting.Integrations;
internal class GetStoreRemainsQueryHandler : IRequestHandler<GetStoreRemainsQuery, Result<RemainsesResponse>>
{
    private readonly ProductTurnoverIngestionService _productTurnoverIngestionService;

    public GetStoreRemainsQueryHandler(ProductTurnoverIngestionService productTurnoverIngestionService)
    {
        _productTurnoverIngestionService = productTurnoverIngestionService;
    }

    public async Task<Result<RemainsesResponse>> Handle(GetStoreRemainsQuery request, CancellationToken cancellationToken)
    {
        var date = DateTime.Now;
        if (request.Date != default)
            date = request.Date.AddMilliseconds(-50);

        Dictionary<long, List<ProductTurnover>> storesTurnovers = new();
        
        if (request.StoreId == 0)
        {
            List<ProductTurnover> turnovers = await _productTurnoverIngestionService.GetAllTurnoversWithProductsAsync(date);
            var storeIds = turnovers.Select(x => x.StoreId).Distinct().ToList();
            foreach (var storeId in storeIds)
            {
                var _turnovers = turnovers.Where(x => x.StoreId == storeId).OrderBy(x => x.Date).ToList();
                var lastAudit = _turnovers.LastOrDefault(x => x.ObjectType == 6);

                if (lastAudit != null)
                    _turnovers.RemoveAll(x => x.Date < lastAudit.Date);

                storesTurnovers.Add(storeId, _turnovers);
            }
        }
        else
        {
            var fromDate = await _productTurnoverIngestionService.GetLastAuditProductsDateAsync(request.StoreId, date);

            List<ProductTurnover> turnovers = await _productTurnoverIngestionService.GetTurnoversWithProductsByStoreIdAsync(request.StoreId, fromDate, date);

            storesTurnovers.Add(request.StoreId, turnovers.OrderBy(x => x.Date).ToList());
        }

        return new RemainsesResponse
        {
            StoreRemains = storesTurnovers
                .ToDictionary(
                    g => g.Key,
                    g =>
                        g.Value.SelectMany(t => t.Products ?? Enumerable.Empty<Product>())
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
        };

        //return new RemainsesResponse
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
        //};
    }
}
