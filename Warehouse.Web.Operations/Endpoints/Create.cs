using Ardalis.Result;
using FastEndpoints;
using MediatR;
using System.Globalization;
using Warehouse.Web.Operations.UseCases.Commands;
using Warehouse.Web.Shared;

namespace Warehouse.Web.Operations.Endpoints;
public class Create : Endpoint<CreateOperationRequest>
{
    private readonly IMediator _mediator;

    public Create(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post(ApiEndpoints.V1.Operations.Create);
        Roles(new string[] { "Admin", "User" });
    }

    public override async Task HandleAsync(CreateOperationRequest req, CancellationToken ct)
    {
        if (!DateTime.TryParseExact(req.Date, "ddMMyyyyHHmm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
        {
            AddError("Invalid date format. Expected ddMMyyyyHHmm.");
            await SendErrorsAsync(400, ct);
            return;
        }

        var command = new CreateOperationCommand(dt, req.Amount, req.Discount, req.Type, req.StoreId, req.ReceiverId, req.Comment, req.Products);
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
