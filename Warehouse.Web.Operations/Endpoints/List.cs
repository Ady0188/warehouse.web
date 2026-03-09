using FastEndpoints;
using MediatR;
using Warehouse.Web.Shared.Responses;
using Warehouse.Web.Shared;
using System.Security.Claims;
using Warehouse.Web.Operations.UseCases.Queries;

namespace Warehouse.Web.Operations.Endpoints;
public class List : Endpoint<PagedRequest, OperationsResponse>
{
    private readonly IMediator _mediator;

    public List(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get(ApiEndpoints.V1.Operations.GetAll);
        Roles(new string[] { "Admin", "User" });
    }

    public override async Task HandleAsync(PagedRequest request, CancellationToken ct)
    {
        long storeId = 0;
        if (!User.IsInRole("Admin"))
        {
            storeId = long.Parse(User.FindFirstValue("StoreId")!);
        }

        var query = new GetAllOperationsQuery(storeId, (OperationType)request.OperationType, request.ToOptions(storeId));
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
