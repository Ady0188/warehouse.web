using Ardalis.Result;
using FastEndpoints;
using MediatR;
using System.Globalization;
using Warehouse.Web.Operations.UseCases.Commands;
using Warehouse.Web.Shared;

namespace Warehouse.Web.Operations.Endpoints;

public class Update : Endpoint<UpdateOperationRequest>
{
    private readonly IMediator _mediator;

    public Update(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Put(ApiEndpoints.V1.Operations.Update);
        Roles(new string[] { "Admin", "User" });
    }
    public override async Task HandleAsync(UpdateOperationRequest req, CancellationToken ct)
    {
        if (!DateTime.TryParseExact(req.Date, "ddMMyyyyHHmm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
        {
            AddError("Invalid date format. Expected ddMMyyyyHHmm.");
            await SendErrorsAsync(400, ct);
            return;
        }

        var command = new UpdateOperationCommand(req.Id, dt, req.Amount, req.Discount, req.Type, req.ParentId, req.StoreId, req.ReceiverId, req.Comment, req.Products);
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
