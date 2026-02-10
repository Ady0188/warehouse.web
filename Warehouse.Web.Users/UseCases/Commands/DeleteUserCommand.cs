using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Warehouse.Web.Users.Data;

namespace Warehouse.Web.Users.UseCases.Commands;
internal record DeleteUserCommand(string Id):IRequest<Result>;
internal class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Result>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public DeleteUserCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.Id);
        if (user != null)
        {
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return Result.Success();
            }
        }

        return Result.NotFound();
    }
}
