using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using Warehouse.Web.Users.Data;

namespace Warehouse.Web.Components.Account;

public class CustomUserClaimsPrincipalFactory
    : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
{
    public CustomUserClaimsPrincipalFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<IdentityOptions> optionsAccessor)
        : base(userManager, roleManager, optionsAccessor)
    {
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        identity.AddClaim(new Claim("FullName", $"{user.Lastname ?? ""} {user.Firstname ?? ""}".Trim()));
        identity.AddClaim(new Claim("StoreId", user.StoreId.ToString() ?? "0"));
        identity.AddClaim(new Claim("StoreName", user.StoreName ?? ""));

        // Можете добавить сколько угодно
        // identity.AddClaim(new Claim("auth_type", "AD")); ← если хотите по аналогии

        return identity;
    }
}