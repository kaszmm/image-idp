using System.Security.Claims;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;

namespace IdentityServer.Services;

/// <summary>
/// As we implemented our own user store, while generation of access token, the idp needs a way to extract the
/// required claims and other properties of the user, and this class helps with that
/// IProfileService is provided by Duende itself.
/// </summary>
public class UserProfileService : IProfileService
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
        var subId = context.Subject.GetSubjectId();
        
        // if IsActive flag is false, the user will not be able to login into the application
        context.IsActive = await _userStoreService.IsActiveUserAsync(subId);
    }
}