using Ardalis.Result;
using MediatR;
using Warehouse.Web.Shared.Responses;
using Warehouse.Web.Stores.Contracts;

namespace Warehouse.Web.Managers.UseCases.Queries;

internal record GetAllManagersQuery(long StoreId, GetAllOptions Options) : IRequest<Result<ManagersResponse>>;
internal class GetAllManagersQueryHandler : IRequestHandler<GetAllManagersQuery, Result<ManagersResponse>>
{
    private readonly IReadOnlyManagerRepository _managerRepository;
    private readonly IMediator _mediator;

    public GetAllManagersQueryHandler(IReadOnlyManagerRepository managerRepository, IMediator mediator)
    {
        _managerRepository = managerRepository;
        _mediator = mediator;
    }

    public async Task<Result<ManagersResponse>> Handle(GetAllManagersQuery request, CancellationToken cancellationToken)
    {
        var list = await _managerRepository.ListAsync(request.Options);
        var managers = list.Result;

        if (request.StoreId != 0)
            managers = managers.Where(x => x.StoreId == request.StoreId).ToList();

        //if (managers.Count == 0)
        //    return Result.Success(new ManagersResponse());

        //var storeIds = managers
        //            .Select(m => m.StoreId)
        //            .Distinct()
        //            .ToArray();

        //Dictionary<long, StoreResponse> storesById = new();
        //if (storeIds.Length > 0)
        //{
        //    var storesQuery = new GetStoresByIdsQuery(storeIds);
        //    var storesQueryResult = await _mediator.Send(storesQuery);

        //    if (!storesQueryResult.IsSuccess)
        //        return Result.Error(storesQueryResult.Errors.First());

        //    storesById = storesQueryResult.Value
        //        .GroupBy(s => s.Id)
        //        .Select(g => g.First())
        //        .ToDictionary(s => s.Id, s => s);
        //}

        Dictionary<long, StoreResponse> stores = new();

        if (request.StoreId == 0)
        {
            var storesQuery = new GetAllStoresQuery();
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
            var storesQuery = new GetStoreByIdQuery(request.StoreId);
            var storesQueryResult = await _mediator.Send(storesQuery);

            if (!storesQueryResult.IsSuccess)
                return Result.Error(storesQueryResult.Errors.First());

            stores = new Dictionary<long, StoreResponse> { { storesQueryResult.Value.Id, storesQueryResult.Value } };
        }
     
        return new ManagersResponse
        {
            Total = list.Total,
            Stores = stores.Values.ToList(),
            Items = managers
                .Skip(request.Options.Skip)
                .Take(request.Options.PageSize)
                .OrderBy(x => x.Lastname).Select(x => new ManagerResponse
            {
                Id = x.Id,
                Firstname = x.Firstname,
                Lastname = x.Lastname,
                Address = x.Address,
                StoreId = x.StoreId,
                Phone = x.Phone,
                StoreName = stores.TryGetValue(x.StoreId, out var s) ? s.Name : string.Empty
            }).ToList()
        };
    }
}
