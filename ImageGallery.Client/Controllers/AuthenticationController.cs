using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace ImageGallery.Client.Controllers;

public class AuthenticationController : Controller
{
    private IHttpClientFactory _httpClientFactory;

    public AuthenticationController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }
    
    [Authorize]
    public async Task LogOut()
    {
        var idpClient = _httpClientFactory.CreateClient("IDPClient");
        ArgumentNullException.ThrowIfNull(idpClient);

        var idpDiscoveryPage = await idpClient.GetDiscoveryDocumentAsync();
        ArgumentNullException.ThrowIfNull(idpDiscoveryPage);

        if (idpDiscoveryPage.IsError)
        {
            Console.WriteLine("Error while fetching the discovery page for idp client");
            throw new Exception(idpDiscoveryPage.Error);
        }

        // On user log out revoke the the access and refresh token
        var accessTokenRevocationRequest = new TokenRevocationRequest
        {
            Address = idpDiscoveryPage.RevocationEndpoint,
            Token = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken),
            ClientId = "imageGalleryClient",
            ClientSecret = "secret"
        };
        var accessTokenRevocationResponse = await idpClient.RevokeTokenAsync(accessTokenRevocationRequest);

        if (accessTokenRevocationResponse.IsError)
        {
            Console.WriteLine("Error while revoking the access token");
        }
        
        var refreshTokenRevocationRequest = new TokenRevocationRequest
        {
            Address = idpDiscoveryPage.RevocationEndpoint,
            Token = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.RefreshToken),
            ClientId = "imageGalleryClient",
            ClientSecret = "secret"
        };
        var refreshTokenRevocationResponse = await idpClient.RevokeTokenAsync(refreshTokenRevocationRequest);
        if (refreshTokenRevocationResponse.IsError)
        {
            Console.WriteLine("Error while revoking the refresh token");
        }
        
        // clearing this will log you out of web client application,
        // if u enabled consent on IDP, then clearing this will reset that consent and will for consent again
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        
        // clearing this will actually log you out of IDP itself
        // this will clear out sessions in IDP itself, and redirect you to IDP
        await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
        return;
    }

    [Authorize]
    public IActionResult AccessDenied()
    {
        return View();
    }
}