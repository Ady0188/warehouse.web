using Ardalis.Result;
using FastEndpoints;
using MediatR;
using Warehouse.Web.Orders.UseCases.Commands;
using Warehouse.Web.Shared;

namespace Warehouse.Web.Orders.Endpoints;

public class Delete : Endpoint<DeleteOrderRequest>
{
    private readonly IMediator _mediator;

    public Delete(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Delete(ApiEndpoints.V1.Orders.Delete);
        Roles(new string[] { "User" });
    }

    public override async Task HandleAsync(DeleteOrderRequest req, CancellationToken ct)
    {
        var command = new DeleteOrderCommand(req.Id);
        var commandResult = await _mediator.Send(command);

        if (commandResult.Status == ResultStatus.NotFound)
        {
            await SendNotFoundAsync();
            return;
        }
        else if (!commandResult.IsSuccess)
        {
            await SendErrorsAsync(500);
            return;
        }

        await SendOkAsync();
    }
}
