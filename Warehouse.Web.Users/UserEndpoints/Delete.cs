using Ardalis.Result;
using FastEndpoints;
using MediatR;
using Warehouse.Web.Shared;
using Warehouse.Web.Users.UseCases.Commands;

namespace Warehouse.Web.Users.UserEndpoints;
public class Delete : Endpoint<DeleteUserRequest>
{
    private readonly IMediator _mediator;

    public Delete(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Delete(ApiEndpoints.V1.Users.Delete);
        Roles(new string[] { "Admin" });
    }

    public override async Task HandleAsync(DeleteUserRequest req, CancellationToken ct)
    {
        var command = new DeleteUserCommand(req.Id);
        var commandResult = await _mediator.Send(command, ct);

        if (commandResult.Status == ResultStatus.NotFound)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        await SendNoContentAsync(ct);
    }
}
