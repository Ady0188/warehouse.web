using Ardalis.Result;
using MediatR;
using Warehouse.Web.Shared.Responses;

namespace Warehouse.Web.Reporting.Contracts;

public record GetStoreRemainsQuery(long StoreId, DateTime Date = default) : IRequest<Result<RemainsesResponse>>;
