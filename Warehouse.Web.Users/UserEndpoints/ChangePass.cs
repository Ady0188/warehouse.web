using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using Warehouse.Web.Shared;
using Warehouse.Web.Shared.Responses;
using Warehouse.Web.Users.Data;

namespace Warehouse.Web.Users.UserEndpoints;
public class ChangePass : Endpoint<ChangePassRequest, UserResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserStore<ApplicationUser> _userStore;

    public ChangePass(UserManager<ApplicationUser> userManager, IUserStore<ApplicationUser> userStore)
    {
        _userManager = userManager;
        _userStore = userStore;
    }

    public override void Configure()
    {
        Post(ApiEndpoints.V1.Users.ChangePassword);
        AllowAnonymous();
    }

    public override async Task HandleAsync(ChangePassRequest req, CancellationToken ct)
    {
        var user = await _userManager.FindByNameAsync(req.Username);
        if (user is null)
        {
            await SendErrorsAsync(400, ct);
            return;
        }

        var emailStore = GetEmailStore();
        await emailStore.SetEmailConfirmedAsync(user, true, CancellationToken.None);

        var changePasswordResult = await _userManager.ChangePasswordAsync(user, "P@ssw0rd", req.Password);
        if (!changePasswordResult.Succeeded)
        {
            await SendErrorsAsync(400, ct);
            return;
        }

        await SendOkAsync();
    }

    private IUserEmailStore<ApplicationUser> GetEmailStore()
    {
        if (!_userManager.SupportsUserEmail)
        {
            throw new NotSupportedException("The default UI requires a user store with email support.");
        }
        return (IUserEmailStore<ApplicationUser>)_userStore;
    }
}
