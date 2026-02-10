using FastEndpoints;
using MediatR;
using Warehouse.Web.Shared;
using Warehouse.Web.Shared.Responses;
using Warehouse.Web.Users.UseCases.Queries;

namespace Warehouse.Web.Users.UserEndpoints;

public class List : Endpoint<PagedRequest, UsersResponse>
{
    private readonly IMediator _mediator;

    public List(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get(ApiEndpoints.V1.Users.GetAll);
        Roles(new string[] { "Admin" });
    }

    public override async Task HandleAsync(PagedRequest request, CancellationToken ct)
    {
        try
        {
            var query = new GetAllUsersQuery(request.ToOptions());
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
            {
                await SendErrorsAsync(500, ct);
            }

            await SendAsync(result.Value);
        }
        catch (Exception ex)
        {
            throw;
        }
    }
}
