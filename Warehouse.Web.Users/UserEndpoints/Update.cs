using Ardalis.Result;
using FastEndpoints;
using MediatR;
using Warehouse.Web.Shared;
using Warehouse.Web.Users.UseCases.Commands;

namespace Warehouse.Web.Users.UserEndpoints;
public class Update : Endpoint<UpdateUserRequest>
{
    private readonly IMediator _mediator;

    public Update(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Put(ApiEndpoints.V1.Users.Update);
        Roles(new string[] { "Admin" });
    }

    public override async Task HandleAsync(UpdateUserRequest req, CancellationToken ct)
    {
        var command = new UpdateUserCommand(req.Id, req.Firstname, req.Lastname, req.StoreId, req.StoreName, req.Address, req.Phone);
        var commandResult = await _mediator.Send(command);

        if(commandResult.Status == ResultStatus.NotFound)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        await SendNoContentAsync(ct);
    }
}
