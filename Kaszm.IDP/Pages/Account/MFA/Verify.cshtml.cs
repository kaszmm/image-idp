using Duende.IdentityServer.Extensions;
using IdentityServer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OtpNet;

namespace IdentityServer.Pages.Account.MFA;

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
        if (!User.IsAuthenticated())
        {
            return RedirectToPage("/Account/Login/Index", new { returnUrl });
        }
        
        Input = new InputModel()
        {
            ReturnUrl = returnUrl
        };
        
        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (Guid.TryParse(User.Identity.GetSubjectId(), out var userId))
        {
            var user = await _userStoreService.GetUserAsync(userId);
            var authenticatorCode = user.AuthenticatorCode;

            if (string.IsNullOrWhiteSpace(authenticatorCode))
            {
                return RedirectToPage("/Account/MFA/Register", new { Input.ReturnUrl });
            }

            var isVerificationCodeValid = ValidateVerificationCode(authenticatorCode, Input.VerificationCode);
            if (isVerificationCodeValid)
            {
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

        return Page();
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