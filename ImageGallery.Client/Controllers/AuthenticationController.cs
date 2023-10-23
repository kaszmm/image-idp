using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ImageGallery.Client.Controllers;

public class AuthenticationController : Controller
{
    public AuthenticationController()
    {
        
    }
    
    [Authorize]
    public void LogOut()
    {
        // clearing this will log you out of web client application,
        // if u enabled consent on IDP, then clearing this will reset that consent and will for consent again
        HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        
        // clearing this will actually log you out of IDP itself
        // this will clear out sessions in IDP itself, and redirect you to IDP
        HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
        return;
    }
}