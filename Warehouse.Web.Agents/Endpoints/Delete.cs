using Ardalis.Result;
using FastEndpoints;
using MediatR;
using Warehouse.Web.Agents.UseCases.Commands;
using Warehouse.Web.Shared;

namespace Warehouse.Web.Agents.Endpoints;

public class Delete : Endpoint<DeleteAgentRequest>
{
    private readonly IMediator _mediator;

    public Delete(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Delete(ApiEndpoints.V1.Agents.Delete);
        Roles(new string[] { "Admin", "User" });
    }

    public override async Task HandleAsync(DeleteAgentRequest req, CancellationToken ct)
    {
        var command = new DeleteAgentCommand(req.Id);
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
