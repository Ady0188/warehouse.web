using Ardalis.Result;
using FastEndpoints;
using MediatR;
using System.Security.Claims;
using Warehouse.Web.Managers.UseCases.Commands;
using Warehouse.Web.Shared;
using Warehouse.Web.Shared.Responses;

namespace Warehouse.Web.Managers.Endpoints;
public class Update : Endpoint<UpdateManagerRequest, ManagerResponse>
{
    private readonly IMediator _mediator;

    public Update(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Put(ApiEndpoints.V1.Managers.Update);
        Roles(new string[] { "Admin", "User" });
    }

    public override async Task HandleAsync(UpdateManagerRequest req, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var storeId = User.FindFirstValue("StoreId")!;

        var command = new UpdateManagerCommand(userId, long.Parse(storeId), req.Id, req.Firstname, req.Lastname, req.StoreId, req.Address, req.Phone);
        var commandResult = await _mediator.Send(command);

        if (commandResult.Status == ResultStatus.NotFound)
        {
            await SendNotFoundAsync();
            return;
        }

        await SendOkAsync();
    }
}
