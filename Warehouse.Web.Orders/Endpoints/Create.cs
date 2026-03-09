using Ardalis.Result;
using FastEndpoints;
using MediatR;
using System.Globalization;
using Warehouse.Web.Orders.UseCases.Commands;
using Warehouse.Web.Shared;

namespace Warehouse.Web.Orders.Endpoints;

public class Create : Endpoint<CreateOrderRequest>
{
    private readonly IMediator _mediator;

    public Create(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post(ApiEndpoints.V1.Orders.Create);
        Roles(new string[] { "Admin", "User" });
    }

    public override async Task HandleAsync(CreateOrderRequest req, CancellationToken ct)
    {
        if (!DateTime.TryParseExact(req.Date, "ddMMyyyyHHmm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
        {
            AddError("Invalid date format. Expected ddMMyyyyHHmm.");
            await SendErrorsAsync(400, ct);
            return;
        }

        var command = new CreateOrderCommand(dt, req.DocId, req.StoreId, req.AgentId, req.Amount, req.Comment, req.Type, req.Agents);
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
