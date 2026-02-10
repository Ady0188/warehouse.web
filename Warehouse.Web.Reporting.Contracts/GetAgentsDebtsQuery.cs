using Ardalis.Result;
using MediatR;
using Warehouse.Web.Shared.Responses;

namespace Warehouse.Web.Reporting.Contracts;

public record GetAgentsDebtsQuery(DateTime FromDate, DateTime ToDate) : IRequest<Result<List<AgentDebtsResponse>>>;