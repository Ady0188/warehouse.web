using Ardalis.Result;
using MediatR;
using Warehouse.Web.Shared.Responses;

namespace Warehouse.Web.Stores.Contracts;
public record GetStoreByIdQuery(long Id, bool IncludeManager = false) : IRequest<Result<StoreResponse>>;