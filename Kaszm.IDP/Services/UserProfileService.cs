using System.Security.Claims;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;

namespace IdentityServer.Services;

/// <summary>
/// As we implemented our own user store, while generation of access token, the idp needs a way to extract the
/// required claims and other properties of the user, and this class helps with that
/// </summary>
public class UserProfileService :IProfileService
{
    private readonly IUserStoreService _userStoreService;

    public UserProfileService(IUserStoreService userStoreService)
    {
        _userStoreService = userStoreService ?? throw new ArgumentNullException(nameof(userStoreService));
    }
    public async Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        var subId = context.Subject.GetSubjectId();
        var claimsForUser = await _userStoreService.GetUserClaimsAsync(subId);
        context.AddRequestedClaims(claimsForUser.Select(x => new Claim(x.Type, x.Value)).ToList());
    }

    public async Task IsActiveAsync(IsActiveContext context)
    {
        // NOTE: To allow external login like facebook we are setting the user active flag to be true for now
        // var subId = context.Subject.GetSubjectId();
        // context.IsActive = await _userStoreService.IsActiveUserAsync(subId);

        context.IsActive = true;
    }
}