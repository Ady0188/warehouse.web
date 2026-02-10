using Ardalis.Result;
using FastEndpoints;
using MediatR;
using Warehouse.Web.Operations.UseCases.Commands;
using Warehouse.Web.Shared;

namespace Warehouse.Web.Operations.Endpoints;

public class Delete : Endpoint<DeleteOperationRequest>
{
    private readonly IMediator _mediator;

    public Delete(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Delete(ApiEndpoints.V1.Operations.Delete);
        Roles(new string[] { "Admin", "User" });
    }

    public override async Task HandleAsync(DeleteOperationRequest req, CancellationToken ct)
    {
        var command = new DeleteOperationCommand(req.Id);
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
