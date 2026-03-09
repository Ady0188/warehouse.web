using FastEndpoints;
using MediatR;
using System.Security.Claims;
using Warehouse.Web.Orders.UseCases.Queries;
using Warehouse.Web.Shared;
using Warehouse.Web.Shared.Responses;

namespace Warehouse.Web.Orders.Endpoints;

public class List : Endpoint<PagedRequest, OrdersResponse>
{
    private readonly IMediator _mediator;

    public List(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get(ApiEndpoints.V1.Orders.GetAll);
        Roles(new string[] { "Admin", "User" });
    }

    public override async Task HandleAsync(PagedRequest request, CancellationToken ct)
    {
        long storeId = 0;
        if (!User.IsInRole("Admin"))
        {
            storeId = long.Parse(User.FindFirstValue("StoreId")!);
        }

        var query = new GetAllOrdersQuery(storeId, request.ToOptions(), request.IncludeDebts, request.DateFrom, request.DateTo);
        var queryResult = await _mediator.Send(query);

        if (queryResult is null || !queryResult.IsSuccess)
        {
            await SendErrorsAsync(500);
            return;
        }
        else
        {
            await SendAsync(queryResult.Value);
        }
    }
}
