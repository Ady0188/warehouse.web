using Ardalis.Result;
using MediatR;
using Warehouse.Web.Reporting.Contracts;
using Warehouse.Web.Shared.Responses;

namespace Warehouse.Web.Reporting.Integrations;

internal class GetAgentsDebtsQueryHandler : IRequestHandler<GetAgentsDebtsQuery, Result<List<AgentDebtsResponse>>>
{
    private readonly IMediator _mediator;
    private readonly AgentRemainsIngestionService _agentRemainsIngestionService;

    public GetAgentsDebtsQueryHandler(IMediator mediator, AgentRemainsIngestionService agentRemainsIngestionService)
    {
        _mediator = mediator;
        _agentRemainsIngestionService = agentRemainsIngestionService;
    }

    public async Task<Result<List<AgentDebtsResponse>>> Handle(GetAgentsDebtsQuery request, CancellationToken cancellationToken)
    {
        var all = await _agentRemainsIngestionService.GetAllDebtsAsync();

        var all1 = all.Where(x => x.Date <= request.ToDate).ToList();
        var _all1 = all.Where(x => x.Date < request.FromDate).ToList();

        var thisMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        
        var agentsDebts = all
            .GroupBy(x => x.AgentId)
            .ToDictionary(
                g => g.Key,
                g => {
                    var agentDebts = all.Where(d => d.AgentId == g.Key).ToList();
                    var currentDebt = agentDebts.Where(x => x.AgentId == g.Key).Sum(x => x.Amount - x.Discount);
                    short level = (short)(currentDebt < 0 ? 1 : 0);

                    for ( short i = 0; i < 12; i++)
                    {
                        if (currentDebt >= 0)
                            break;

                        var month = thisMonth.AddMonths(-i);
                        var monthDebt = agentDebts.Where(d => d.Date < month).Sum(x => x.Amount - x.Discount);
                        if (monthDebt < 0)
                            level = (short)(i + 2);
                        else
                            break;
                    }

                    //if (currentDebt < 0 && agentDebts.Where(d => d.Date < thisMonth.AddMonths(-1)).Sum(x => x.Amount - x.Discount) < 0)
                    //    level = 2;
                    //else
                    //    currentDebt = 0;

                    //if (currentDebt < 0 && agentDebts.Where(d => d.Date < thisMonth.AddMonths(-2)).Sum(x => x.Amount - x.Discount) < 0)
                    //    level = 3;

                    return new AgentDebtsResponse
                    {
                        Id = g.Key,
                        DebtOnBegin = _all1.Where(x => x.AgentId == g.Key).Sum(x => x.Amount - x.Discount),
                        Debt = all1.Where(x => x.AgentId == g.Key && x.ObjectName == "Operation").Sum(x => x.Amount - x.Discount),
                        Credit = all1.Where(x => x.AgentId == g.Key && x.ObjectName == "Order").Sum(x => x.Amount - x.Discount),
                        DebtOnEnd = all1.Where(x => x.AgentId == g.Key).Sum(x => x.Amount - x.Discount),
                        Level = level
                    };
                });

        return agentsDebts.Values.ToList();
    }
}
