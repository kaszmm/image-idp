using Duende.IdentityServer.Extensions;
using IdentityServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OtpNet;
using QRCoder;

namespace IdentityServer.Pages.Account.MFA;

[SecurityHeaders]
[Authorize]
public class Register : PageModel
{
    private readonly IUserStoreService _userStoreService;

    [BindProperty]
    public InputModel Input { get; set; }
    
    public Register(IUserStoreService userStoreService)
    {
        _userStoreService = userStoreService ?? throw new ArgumentNullException(nameof(userStoreService));
    }
    
    public async Task<IActionResult> OnGet(string returnUrl)
    {
        if (!User.IsAuthenticated())
        {
            return RedirectToPage("/Account/Login/Index");
        }

        if (Guid.TryParse(User.Identity.GetSubjectId(), out var userId))
        {
            var user = await _userStoreService.GetUserAsync(userId);
            var authenticatorCode = user.AuthenticatorCode;

            if (string.IsNullOrWhiteSpace(authenticatorCode))
            {
                authenticatorCode = GenerateAuthenticatorKey();

                user = user with
                {
                    AuthenticatorCode = authenticatorCode
                };
                await _userStoreService.UpdateUserAsync(user);
            }

            Input = new InputModel()
            {
                ReturnUrl = returnUrl,
                AuthenticatorCode = authenticatorCode,
                QrImage = GetQrCodeImageBase64String("Kaszm.IDP", authenticatorCode, user.UserName)
            };
        }

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (Input.Button != "save")
        {
            // since user didnt saved the verification code we can redirect user back to where it came from
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
        
        if (Guid.TryParse(User.Identity.GetSubjectId(), out var userId))
        {
            var user = await _userStoreService.GetUserAsync(userId);
            var authenticatorCode = user.AuthenticatorCode;

            if (string.IsNullOrWhiteSpace(authenticatorCode))
            {
                Input = new InputModel()
                {
                    ReturnUrl = Input.ReturnUrl
                };
                ModelState.AddModelError(nameof(Input.AuthenticatorCode), "No Authenticator code founded");

                return Page();
            }

            var isVerificationCodeValid = ValidateVerificationCode(authenticatorCode, Input.VerificationCode);
            if (isVerificationCodeValid)
            {
                user = user with
                {
                    TwoFAEnabled = true
                };
                await _userStoreService.UpdateUserAsync(user);
                
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
                ModelState.AddModelError(nameof(Input.AuthenticatorCode), "Invalid verification code founded");

                return Page();
            }
        }

        return Page();
    }

    public string GenerateAuthenticatorKey()
    {
        var key = KeyGeneration.GenerateRandomKey(20);
        return  Base32Encoding.ToString(key);
    }

    public string GetQrCodeImageBase64String(string issuer, string authenticatorCode, string userName)
    {
        var totpUrl = $"otpauth://topt/{issuer}:{userName}?secret={authenticatorCode}&issuer={issuer}";
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(totpUrl, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new Base64QRCode(qrCodeData);
        var base64Image = qrCode.GetGraphic(15);
        return base64Image;
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