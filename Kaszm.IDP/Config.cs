using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using IdentityModel;

namespace IdentityServer;

public static class Config
{
    /// <summary>
    /// Group of claims about a user that can be requested using scoped param
    /// </summary>
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            // this is the required scope the add the subjectId into identity token (WTF?)
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new("roles", "Role for user", new[]
            {
                "role"
            })
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
        };

    public static IEnumerable<Client> Clients =>
        new Client[]
        {
            new ()
            {
                ClientName = "Image Gallery Client",
                ClientId = "imageGalleryClient",
                ClientSecrets = new List<Secret>
                {
                    new("secret".ToSha256())
                },
                AllowedGrantTypes =
                    GrantTypes.Code, // this restricts client to only initiate oidc flows for authenticating and authorization
                RedirectUris =
                {
                    // on this uri the idp will send authorization code, that client will later use to exchange it with access token
                    "https://imagewebclient.qapitacorp.local/signin-oidc"
                },
                // PostLogoutRedirectUris = new List<string>(), // donno what is this for?
                AllowedScopes = new List<string>()
                {
                    // this scope is required for oidc flow
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    "roles"
                },
                PostLogoutRedirectUris =
                {
                    "https://imagewebclient.qapitacorp.local/signout-callback-oidc"
                },
                // for some reason ui for consent page is not visible, need to check later
                RequireConsent =
                    false, // each time user login in idp, consent screen will be displayed to user (like allow access or reject)
            }
        };
}