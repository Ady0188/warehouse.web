using Ardalis.Result;
using FastEndpoints;
using MediatR;
using Warehouse.Web.Agents.UseCases.Commands;
using Warehouse.Web.Shared;

namespace Warehouse.Web.Agents.Endpoints;
public class Create : Endpoint<CreateAgentRequest>
{
    private readonly IMediator _mediator;

    public Create(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post(ApiEndpoints.V1.Agents.Create);
        Roles(new string[] { "Admin", "User" });
    }

    public override async Task HandleAsync(CreateAgentRequest req, CancellationToken ct)
    {
        var command = new CreateAgentCommand(req.Name, req.Phone, req.Address, req.StoreId, req.ManagerId, req.Comment);
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