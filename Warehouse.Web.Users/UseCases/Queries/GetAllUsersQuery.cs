using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using Warehouse.Web.Shared.Responses;
using Warehouse.Web.Stores.Contracts;
using Warehouse.Web.Users.Data;

namespace Warehouse.Web.Users.UseCases.Queries;

internal record GetAllUsersQuery(GetAllOptions Options) : IRequest<Result<UsersResponse>>;

internal class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, Result<UsersResponse>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMediator _mediator;

    public GetAllUsersQueryHandler(UserManager<ApplicationUser> userManager, IMediator mediator)
    {
        _userManager = userManager;
        _mediator = mediator;
    }

    public async Task<Result<UsersResponse>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var users = _userManager.Users.OrderBy(x => x.Lastname).ToList();

        var storesQuery = new GetAllStoresQuery();
        var storesQueryResult = await _mediator.Send(storesQuery);

        if (!storesQueryResult.IsSuccess)
            return Result.Error(storesQueryResult.Errors.First());

        Dictionary<long, StoreResponse>  stores = storesQueryResult.Value
            .GroupBy(s => s.Id)
            .Select(g => g.First())
            .ToDictionary(s => s.Id, s => s);

        return new UsersResponse
        {
            Total = users.Count,
            Stores = stores.Values.ToList(),
            Items = users
                .Skip(request.Options.Skip)
                .Take(request.Options.PageSize)
                .Select(x => new UserResponse
            {
                Lastname = x.Lastname,
                Address = x.Address,
                StoreId = x.StoreId,
                StoreName = x.StoreName,
                Firstname = x.Firstname,
                Phone = x.PhoneNumber,
                Login = x.UserName!.Replace("@store.tj", ""),
                Email = x.Email,
                Id = x.Id.ToString(),
            }).ToList()
        };
    }
}
