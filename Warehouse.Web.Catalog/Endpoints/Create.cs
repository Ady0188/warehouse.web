using Ardalis.Result;
using FastEndpoints;
using MediatR;
using Warehouse.Web.Catalog.UseCases.Commands;
using Warehouse.Web.Shared;

namespace Warehouse.Web.Catalog.Endpoints;

public class Create : Endpoint<CreateProductRequest>
{
    private readonly IMediator _mediator;

    public Create(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post(ApiEndpoints.V1.Products.Create);
        Roles(new string[] { "Admin" });
    }

    public override async Task HandleAsync(CreateProductRequest req, CancellationToken ct)
    {
        var command = new CreateProductCommand(req.Name, req.Manufacturer, req.Unit, req.BuyPrice, req.SellPrice, req.limit);
        var commandResult = await _mediator.Send(command);

        if (commandResult.Status == ResultStatus.Conflict)
        {
            AddError($"Продукт с названием '{req.Name}' уже существует!");       
            await SendErrorsAsync(409, ct);
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
