using Ardalis.Result;
using FastEndpoints;
using MediatR;
using System.Security.Claims;
using Warehouse.Web.Managers.UseCases.Commands;
using Warehouse.Web.Shared;

namespace Warehouse.Web.Managers.Endpoints;
public class Delete : Endpoint<DeleteManagerRequest>
{
    private readonly IMediator _mediator;

    public Delete(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Delete(ApiEndpoints.V1.Managers.Delete);
        Roles(new string[] { "Admin", "User" });
    }

    public override async Task HandleAsync(DeleteManagerRequest req, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var storeId = User.FindFirstValue("StoreId")!;

        var command = new DeleteManagerCommand(userId, long.Parse(storeId), req.Id);
        var commandResult = await _mediator.Send(command);

        if (commandResult.Status == ResultStatus.NotFound)
        {
            await SendNotFoundAsync();
            return;
        }

        await SendOkAsync();
    }
}
