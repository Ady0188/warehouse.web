using Ardalis.Result;
using MediatR;
using Warehouse.Web.Catalog.Contracts;
using Warehouse.Web.Reporting.Contracts;
using Warehouse.Web.Shared.Responses;

namespace Warehouse.Web.Catalog.Integrations;
internal class GetProductsRemainsQueryHandler : IRequestHandler<GetProductsRemainsQuery, Result<ProductsResponse>>
{
    private readonly IReadOnlyProductRepository _productRepository;
    private readonly IMediator _mediator;

    public GetProductsRemainsQueryHandler(IReadOnlyProductRepository productRepository, IMediator mediator)
    {
        _productRepository = productRepository;
        _mediator = mediator;
    }

    public async Task<Result<ProductsResponse>> Handle(GetProductsRemainsQuery request, CancellationToken cancellationToken)
    {
        var list = await _productRepository.ListAsync();

        var date = DateTime.Now;
        if (request.Date != default)
            date = request.Date;

        var query = new GetStoreRemainsQuery(0, date);
        var queryResult = await _mediator.Send(query);

        var remains = new RemainsesResponse();
        if (queryResult.IsSuccess)
            remains = queryResult.Value;

        return new ProductsResponse
        {
            Total = list.Count,
            Items = list.Select(p =>
            {
                var storesRemains = request.StoresIds.ToDictionary(x => x, x => remains.StoreRemains.ContainsKey(x) ? GetCount(p.Id, remains.StoreRemains.First(sr => sr.Key == x).Value) : 0);
                return new ProductResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    Code = p.Code,
                    BuyPrice = p.BuyPrice,
                    CreateDate = p.CreateDate,
                    LimitRemain = p.LimitRemain,
                    Manufacturer = p.Description,
                    SellPrice = p.SellPrice,
                    Unit = p.Unit,
                    UpdateDate = p.UpdateDate,
                    StoresRemains = storesRemains
                };
            }).ToList()
        };
    }

    private long GetCount(long productId, List<RemainsResponse> value)
    {
        return value.Any(x => x.ProductId == productId) ? value.First(x => x.ProductId == productId).Count : 0;
    }
}
