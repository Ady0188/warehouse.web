using Ardalis.Result;
using MediatR;
using Warehouse.Web.Shared.Responses;

namespace Warehouse.Web.Managers.Contracts;

public record GetManagerByIdQuery(long Id) : IRequest<Result<ManagerResponse>>;
