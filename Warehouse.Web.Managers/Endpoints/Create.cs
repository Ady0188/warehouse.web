using Ardalis.Result;
using FastEndpoints;
using MediatR;
using System.Security.Claims;
using Warehouse.Web.Managers.UseCases.Commands;
using Warehouse.Web.Shared;
using Warehouse.Web.Shared.Responses;

namespace Warehouse.Web.Managers.Endpoints;
public class Create : Endpoint<CreateManagerRequest, ManagerResponse>
{
    private readonly IMediator _mediator;

    public Create(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post(ApiEndpoints.V1.Managers.Create);
        Roles(new string[] { "Admin", "User" });
    }

    public override async Task HandleAsync(CreateManagerRequest req, CancellationToken ct)
    {
        //var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        //var fullName = User.FindFirstValue("FullName")!;
        //var storeId = User.FindFirstValue("StoreId")!;
        //var storeName = User.FindFirstValue("StoreName")!;

        var command = new CreateManagerCommand(req.Firstname, req.Lastname, req.StoreId, req.Address, req.Phone);
        var commandResult = await _mediator.Send(command);

        if (commandResult.Status == ResultStatus.NotFound)
        {
            await SendNotFoundAsync();
            return;
        }
        else if (commandResult.Status == ResultStatus.Conflict)
        {
            AddError("Такой объект уже существует!");
            await SendErrorsAsync(409, ct);
            return;
        }

        await SendOkAsync();
    }
}
