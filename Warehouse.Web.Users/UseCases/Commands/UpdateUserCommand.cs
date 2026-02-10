using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Warehouse.Web.Users.Data;

namespace Warehouse.Web.Users.UseCases.Commands;

internal record UpdateUserCommand(string Id, string Firstname, string Lastname, long StoreId, string StoreName, string? Address, string? Phone) : IRequest<Result>;
internal class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Result>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UpdateUserCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.Id);
        if (user != null)
        {
            user.Firstname = request.Firstname;
            user.Lastname = request.Lastname;
            user.StoreId = request.StoreId;
            user.StoreName = request.StoreName;
            user.Address = request.Address;
            user.PhoneNumber = request.Phone;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return Result.Success();
            }
        }

        return Result.NotFound();
    }
}
