using Ardalis.Result;
using MediatR;
using Warehouse.Web.Shared.Responses;

namespace Warehouse.Web.Managers.Contracts;
public record GetAllManagersByStoresIdsQuery(long[] StoreIds, bool IncludeAgents = false) : IRequest<Result<ManagersResponse>>;