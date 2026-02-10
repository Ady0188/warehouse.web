using Ardalis.Result;
using MediatR;
using Warehouse.Web.Managers.Contracts;
using Warehouse.Web.Shared.Responses;

namespace Warehouse.Web.Managers.Integrations;
internal class GetAllManagersByStoresIdsQueryHandler : IRequestHandler<GetAllManagersByStoresIdsQuery, Result<ManagersResponse>>
{
    private readonly IReadOnlyManagerRepository _managerRepository;

    public GetAllManagersByStoresIdsQueryHandler(IReadOnlyManagerRepository managerRepository)
    {
        _managerRepository = managerRepository;
    }

    public async Task<Result<ManagersResponse>> Handle(GetAllManagersByStoresIdsQuery request, CancellationToken cancellationToken)
    {
        var managers = await _managerRepository.ListByStoreIdsAsync(request.StoreIds);

        return new ManagersResponse
        {
            Items = managers.Select(x => new ManagerResponse
            {
                Id = x.Id,
                Address = x.Address,
                Firstname = x.Firstname,
                Lastname = x.Lastname,
                Phone = x.Phone,
                StoreId = x.StoreId
            }).ToList()
        };
    }
}
