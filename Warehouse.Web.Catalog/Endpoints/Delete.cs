using Ardalis.Result;
using FastEndpoints;
using MediatR;
using Warehouse.Web.Catalog.UseCases.Commands;
using Warehouse.Web.Shared;

namespace Warehouse.Web.Catalog.Endpoints;

public class Delete : Endpoint<DeleteProductRequest>
{
    private readonly IMediator _mediator;

    public Delete(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Delete(ApiEndpoints.V1.Products.Delete);
        Roles(new string[] { "Admin" });
    }

    public override async Task HandleAsync(DeleteProductRequest req, CancellationToken ct)
    {
        var command = new DeleteProductCommand(req.Id);
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
