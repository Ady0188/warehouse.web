using FastEndpoints;
using MediatR;
using Warehouse.Web.Reporting.Integrations;
using Warehouse.Web.Shared;
using Warehouse.Web.Shared.Responses;

namespace Warehouse.Web.Reporting.Endpoints;

internal class List : Endpoint<PagedRequest, HistoriesResponse>
{
    private readonly IMediator _mediator;
    private readonly HistoryIngestionService _historyIngestionService;

    public List(IMediator mediator, HistoryIngestionService historyIngestionService)
    {
        _mediator = mediator;
        _historyIngestionService = historyIngestionService;
    }

    public override void Configure()
    {
        Get(ApiEndpoints.V1.History.GetAll);
        Roles(new string[] { "Admin" });
    }

    public override async Task HandleAsync(PagedRequest request, CancellationToken ct)
    {
        var histories = await _historyIngestionService.GetHistoriesAsync();

        await SendAsync(new HistoriesResponse
        {
            Items = histories.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).Select(h => new HistoryResponse
            {
                Id = h.Id,
                StoreName = h.StoreName,
                UserName = h.UserName,
                Method = h.Method.ToString(),
                ObjectName = h.ObjectName,
                OldData = h.OldData,
                NewData = h.NewData,
                CreatedDate = h.CreatedDate,
                ObjectStoreName = h.ObjectStoreName,
                ObjectManagerName = h.ObjectManagerName,
                ObjectAgentName = h.ObjectAgentName
            }).ToList(),
            Total = histories.Count()
        });
    }
}
