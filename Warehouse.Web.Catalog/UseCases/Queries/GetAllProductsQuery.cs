using Ardalis.Result;
using MediatR;
using Warehouse.Web.Catalog.Contracts;
using Warehouse.Web.Shared.Responses;
using Warehouse.Web.Stores.Contracts;

namespace Warehouse.Web.Catalog.UseCases.Queries;

internal record GetAllProductsQuery(GetAllOptions options) : IRequest<Result<ProductsResponse>>;

internal class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, Result<ProductsResponse>>
{
    private readonly IMediator _mediator;
    private readonly IReadOnlyProductRepository _repository;
    private readonly ICurrentUser _currentUser;

    public GetAllProductsQueryHandler(IReadOnlyProductRepository repository, ICurrentUser currentUser, IMediator mediator)
    {
        _repository = repository;
        _currentUser = currentUser;
        _mediator = mediator;
    }

    public async Task<Result<ProductsResponse>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _repository.ListAsync(request.options);

        Dictionary<long, StoreResponse> stores = new();

        if (request.options.IncluedeRemains)
        {
            if (_currentUser.StoreId == 0)
            {
                var storesQuery = new GetAllStoresQuery(true);
                var storesQueryResult = await _mediator.Send(storesQuery);

                if (!storesQueryResult.IsSuccess)
                    return Result.Error(storesQueryResult.Errors.First());

                stores = storesQueryResult.Value
                        .GroupBy(s => s.Id)
                        .Select(g => g.First())
                        .ToDictionary(s => s.Id, s => s);
            }
            else
            {
                var storesQuery = new GetStoreByIdQuery(_currentUser.StoreId, true);
                var storesQueryResult = await _mediator.Send(storesQuery);

                if (!storesQueryResult.IsSuccess)
                    return Result.Error(storesQueryResult.Errors.First());

                stores = new Dictionary<long, StoreResponse> { { storesQueryResult.Value.Id, storesQueryResult.Value } };
            }

            var remainsQuery = new GetProductsRemainsQuery(stores.Keys.ToArray());
            var remainsQueryResult = await _mediator.Send(remainsQuery);

            if (!remainsQueryResult.IsSuccess)
                return Result.Error(remainsQueryResult.Errors.First());

            return new ProductsResponse
            {
                Total = products.Total,
                Stores = stores.ToDictionary(x => x.Key, x => x.Value.Name),
                Items = products.Result.Select(x =>
                {
                    var productRemains = remainsQueryResult.Value.Items.FirstOrDefault(pr => pr.Id == x.Id);
                    return new ProductResponse
                    {
                        Id = x.Id,
                        Code = x.Code,
                        Name = x.Name,
                        Manufacturer = x.Description,
                        Unit = x.Unit,
                        BuyPrice = x.BuyPrice,
                        SellPrice = x.SellPrice,
                        LimitRemain = x.LimitRemain,
                        CreateDate = x.CreateDate,
                        UpdateDate = x.UpdateDate,
                        StoresRemains = productRemains != null
                            ? productRemains.StoresRemains
                            //stores.Keys.ToDictionary(storeId => storeId,
                                //storeId => productRemains.StoresRemains.ContainsKey(storeId)
                                //    ? productRemains.StoresRemains[storeId]
                                //    : 0)
                            : stores.Keys.ToDictionary(storeId => storeId, storeId => 0L)
                    };
                }).ToList()
            };


            //var response = remainsQueryResult.Value;
            //response.Total = response.Items.Count();
            //response.Items = response.Items.OrderBy(x => x.Name).Skip(request.options.Skip).Take(request.options.PageSize).ToList();
            //response.Stores = stores.ToDictionary(x => x.Key, x => x.Value.Name);
            //return response;
        }

        return new ProductsResponse
        {
            Total = products.Total,
            Items = products.Result.Select(x => new ProductResponse
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Manufacturer = x.Description,
                Unit = x.Unit,
                BuyPrice = x.BuyPrice,
                SellPrice = x.SellPrice,
                LimitRemain = x.LimitRemain,
                CreateDate = x.CreateDate,
                UpdateDate = x.UpdateDate,
            }).ToList()
        };
    }
}
