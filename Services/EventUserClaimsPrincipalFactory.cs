using System.Security.Claims;
using EventRegistrationSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace EventRegistrationSystem.Services;

public sealed class EventUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<EventUser, IdentityRole<int>>
{
    public EventUserClaimsPrincipalFactory(
        UserManager<EventUser> userManager,
        RoleManager<IdentityRole<int>> roleManager,
        IOptions<IdentityOptions> optionsAccessor)
        : base(userManager, roleManager, optionsAccessor)
    {
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(EventUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        identity.AddClaim(new Claim("display_name", user.DisplayName));
        return identity;
    }
}
