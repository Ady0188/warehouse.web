using Ardalis.Result;
using MediatR;
using Warehouse.Web.Shared.Responses;

namespace Warehouse.Web.Stores.Contracts;
public record GetStoresByIdsQuery(IEnumerable<long> Ids) : IRequest<Result<List<StoreResponse>>>;
