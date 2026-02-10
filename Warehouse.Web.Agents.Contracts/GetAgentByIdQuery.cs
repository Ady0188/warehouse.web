using Ardalis.Result;
using MediatR;
using Warehouse.Web.Shared.Responses;

namespace Warehouse.Web.Agents.Contracts;

public record GetAgentByIdQuery(long Id) : IRequest<Result<AgentResponse>>;