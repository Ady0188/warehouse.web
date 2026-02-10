using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using Warehouse.Web.Shared;
using Warehouse.Web.Shared.Responses;
using Warehouse.Web.Users.Data;

namespace Warehouse.Web.Users.UserEndpoints;
public class Create : Endpoint<CreateUserRequest, UserResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IUserStore<ApplicationUser> _userStore;
    public Create(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IUserStore<ApplicationUser> userStore)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _userStore = userStore;
    }

    public override void Configure()
    {
        Post(ApiEndpoints.V1.Users.Create);
        Roles(new string[] { "Admin" });
    }

    public override async Task HandleAsync(CreateUserRequest request, CancellationToken ct)
    {
        var role = _roleManager.Roles.FirstOrDefault(x => x.Name == request.Role);
        if (role == null)
        {

            AddError($"Role '{request.Role}' does not exist.");
            await SendErrorsAsync(404, ct);
            return;
        }

        var result = await RegisterUser(request);

        if (result.Errors.Count() > 0)
        {
            AddError(result.Errors.First().Description.Replace("@ibtcard.tj", ""));
            await SendErrorsAsync(400, ct);
            return;
        }

        await SendOkAsync();
    }

    private async Task<(IEnumerable<IdentityError> Errors, string UserId)> RegisterUser(CreateUserRequest request)
    {
        try
        {
            string _userEmail = request.Login.Contains("@") ? request.Login : $"{request.Login}@store.tj";

            var user = CreateUser();
            user.Firstname = request.Firstname;
            user.Lastname = request.Lastname;
            user.Address = request.Address;
            user.StoreId = request.StoreId;
            user.StoreName = request.StoreName;
            user.PhoneNumber = request.Phone;

            await _userStore.SetUserNameAsync(user, request.Login, CancellationToken.None);
            var emailStore = GetEmailStore();
            await emailStore.SetEmailAsync(user, _userEmail, CancellationToken.None);
            //await emailStore.SetEmailConfirmedAsync(user, true, CancellationToken.None);

            var defaultPassword = "P@ssw0rd";
            //var defaultPassword = "T@mp0raryP@ss";
            var result = await _userManager.CreateAsync(user, defaultPassword);

            if (!result.Succeeded)
            {
                return (result.Errors, string.Empty);
            }

            var roleResult = await _userManager.AddToRoleAsync(user, request.Role);
            if (!roleResult.Succeeded)
            {
                return (roleResult.Errors, string.Empty);
            }

            var userId = await _userManager.GetUserIdAsync(user);
            return (Enumerable.Empty<IdentityError>(), userId);
        }
        catch (Exception ex)
        {

            throw;
        }
    }

    private ApplicationUser CreateUser()
    {
        try
        {
            return Activator.CreateInstance<ApplicationUser>();
        }
        catch
        {
            throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor.");
        }
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
