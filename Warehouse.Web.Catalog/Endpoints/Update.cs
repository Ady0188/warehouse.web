using Ardalis.Result;
using FastEndpoints;
using MediatR;
using Warehouse.Web.Catalog.UseCases.Commands;
using Warehouse.Web.Shared;

namespace Warehouse.Web.Catalog.Endpoints;

public class Update : Endpoint<UpdateProductRequest>
{
    private readonly IMediator _mediator;

    public Update(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Put(ApiEndpoints.V1.Products.Update);
        Roles(new string[] { "Admin" });
    }

    public override async Task HandleAsync(UpdateProductRequest req, CancellationToken ct)
    {
        var command = new UpdateProductCommand(req.Id, req.Name, req.Manufacturer, req.Unit, req.BuyPrice, req.SellPrice, req.limit);
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
