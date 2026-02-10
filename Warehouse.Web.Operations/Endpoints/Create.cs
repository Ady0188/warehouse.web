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
        var dt = DateTime.ParseExact(req.Date, "ddMMyyyyHHmm", CultureInfo.InvariantCulture);

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