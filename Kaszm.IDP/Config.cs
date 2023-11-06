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

    /// <summary>
    /// Scope represents the scope of action requested by client,
    /// whether it can be read scope, write scope etc..
    /// </summary>
    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
            new("imageGalleryApi.View", "View only access for imageGalleryApi"),
            
            // when client requests for 'imageGalleryApi.FullAccess' the access token will have 'imageGalleryApi.FullAccess' scope
            // under scopes list and resource ('imageGalleryApi') linked to this scope under aud list
            new("imageGalleryApi.FullAccess", "Full access for imageGalleryApi")
        }; 
    
    /// <summary>
    /// Resource represents either the physical or logical resource (like single API or multiple API's)
    /// Resources will have scope in them
    /// eg: imageApi is an resource and it scope will be imageApi.read, imageApi.write etc..
    /// </summary>
    public static IEnumerable<ApiResource> ApiResources =>
        new ApiResource[]
        {
            // we defined a 'imageGalleryApi' resource and we linked this resource with scope 'imageGalleryApi.FullAccess'
            new("imageGalleryApi", "Image Gallery Api",
                    new[]
                    {
                        JwtClaimTypes.Role 
                    }) // defining user claims means when access token is generated the 'role' claim type will be added in it
                {
                    Scopes = new List<string> { "imageGalleryApi.FullAccess" }
                }
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
                    "roles",
                    "imageGalleryApi.FullAccess"
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