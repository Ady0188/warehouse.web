using FastEndpoints;
using MediatR;
using System.Security.Claims;
using Warehouse.Web.Managers.UseCases.Queries;
using Warehouse.Web.Shared;
using Warehouse.Web.Shared.Responses;

namespace Warehouse.Web.Managers.Endpoints;

public class List : Endpoint<PagedRequest, ManagersResponse>
{
    private readonly IMediator _mediator;

    public List(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get(ApiEndpoints.V1.Managers.GetAll);
        Roles(new string[] { "Admin", "User" });
    }

    public override async Task HandleAsync(PagedRequest request, CancellationToken ct)
    {
        long storeId = 0;
        if (!User.IsInRole("Admin"))
        {
            storeId = long.Parse(User.FindFirstValue("StoreId")!);
        }

        var query = new GetAllManagersQuery(storeId, request.ToOptions());
        var queryResult = await _mediator.Send(query);

        await SendAsync(queryResult.Value);
    }
}