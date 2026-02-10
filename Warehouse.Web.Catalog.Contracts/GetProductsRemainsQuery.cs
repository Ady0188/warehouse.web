using Ardalis.Result;
using MediatR;
using Warehouse.Web.Shared.Responses;

namespace Warehouse.Web.Catalog.Contracts;

public record GetProductsRemainsQuery(long[] StoresIds, DateTime Date = default) : IRequest<Result<ProductsResponse>>;