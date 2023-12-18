using Duende.IdentityServer;
using IdentityModel;
using IdentityServer.Pages.Login;
using IdentityServer.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OtpNet;

namespace IdentityServer.Pages.Account.MFA;

[AllowAnonymous] // not securing the endpoint, as 2FA is enabled we need to first verify code before we log in user
public class VerifyMfa : PageModel
{
    private readonly IUserStoreService _userStoreService;

    public VerifyMfa(IUserStoreService userStoreService)
    {
        _userStoreService = userStoreService ?? throw new ArgumentNullException(nameof(userStoreService));
    }
    
    [BindProperty]
    public InputModel Input { get; set; }
    
    public IActionResult OnGet(string returnUrl)
    {
        Input = new InputModel()
        {
            ReturnUrl = returnUrl
        };
        
        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        var userId = HttpContext.Session.GetString(JwtClaimTypes.Subject);

        if (!Guid.TryParse(userId, out var guidUserId))
        {
            return RedirectToPage("/Account/Login/Index", new { Input.ReturnUrl });
        }
        
        var user = await _userStoreService.GetUserAsync(guidUserId);

        if (user is null)
        {
            return RedirectToPage("/Account/Login/Index", new { Input.ReturnUrl });
        }

        var authenticatorCode = user.AuthenticatorCode;

        if (string.IsNullOrWhiteSpace(authenticatorCode))
        {
            return RedirectToPage("/Account/MFA/Register", new { Input.ReturnUrl });
        }

        var isVerificationCodeValid = ValidateVerificationCode(authenticatorCode, Input.VerificationCode);
        if (isVerificationCodeValid)
        {
            bool.TryParse(HttpContext.Session.GetString("RememberMe"), out var rememberMe);
            
            // clearing out the session as it is no longer required
            HttpContext.Session.Clear();
            
            AuthenticationProperties props = null;
            if (LoginOptions.AllowRememberLogin && rememberMe)
            {
                props = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.Add(LoginOptions.RememberMeLoginDuration)
                };
            }
            
            // issue authentication cookie with subject ID and username
            var isuser = new IdentityServerUser(user.Id.ToString("D"))
            {
                DisplayName = user.UserName
            };

            await HttpContext.SignInAsync(isuser, props);

            if (!string.IsNullOrWhiteSpace(Input.ReturnUrl))
            {
                return Redirect(Input.ReturnUrl);
            }
            else
            {
                // redirect to home if no returnUrl provided
                return Redirect("~/");
            }
        }
        else
        {
            Input = new InputModel()
            {
                ReturnUrl = Input.ReturnUrl
            };

            ModelState.AddModelError(nameof(Input.AuthenticatorCode), "Invalid verification code");

            return Page();
        }
    }
    
    public bool ValidateVerificationCode(string authenticatorCode, string verificationCode)
    {
        var authenticatorCodeBytes = Base32Encoding.ToBytes(authenticatorCode);
        var totp = new Totp(authenticatorCodeBytes);
        bool isValid = totp.VerifyTotp(verificationCode, out long timeStepMatched,
            VerificationWindow.RfcSpecifiedNetworkDelay);
        return isValid;
    }
}