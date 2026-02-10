using Ardalis.Result;
using MediatR;
using Warehouse.Web.Shared.Responses;

namespace Warehouse.Web.Agents.Contracts;

public record GetAllAgentsByStoreIdQuery(long StoreId) : IRequest<Result<List<AgentResponse>>>;