using Ardalis.Result;
using FastEndpoints;
using MediatR;
using Warehouse.Web.Agents.UseCases.Commands;
using Warehouse.Web.Shared;

namespace Warehouse.Web.Agents.Endpoints;

public class Update : Endpoint<UpdateAgentRequest>
{
    private readonly IMediator _mediator;

    public Update(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Put(ApiEndpoints.V1.Agents.Update);
        Roles(new string[] { "Admin", "User" });
    }
    public override async Task HandleAsync(UpdateAgentRequest req, CancellationToken ct)
    {
        var command = new UpdateAgentCommand(req.Id, req.Name, req.Phone, req.Address, req.StoreId, req.ManagerId, req.Comment);
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
