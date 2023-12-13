using Duende.IdentityServer.Extensions;
using IdentityServer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OtpNet;
using QRCoder;

namespace IdentityServer.Pages.Account.MFA;

public class Register : PageModel
{
    private readonly IUserStoreService _userStoreService;

    public InputModel Input { get; set; }
    
    public Register(IUserStoreService userStoreService)
    {
        _userStoreService = userStoreService ?? throw new ArgumentNullException(nameof(userStoreService));
    }
    
    public async Task<IActionResult> OnGet()
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
            }

            user = user with
            {
                AuthenticatorCode = authenticatorCode
            };
            await _userStoreService.UpdateUserAsync(user);

            Input = new InputModel()
            {
                AuthenticatorCode = authenticatorCode,
                QrImage = GetQrCodeImageBase64String("Kaszm.IDP", authenticatorCode, user.UserName)
            };
        }

        return Page();
    }

    public async Task<IActionResult> OnPost(string verificationCode)
    {
        throw new NotImplementedException();
    }

    public string GenerateAuthenticatorKey()
    {
        var key = KeyGeneration.GenerateRandomKey(20);
        return  Base32Encoding.ToString(key);
    }

    public string GetQrCodeImageBase64String(string issuer, string authenticatorCode, string userName)
    {
        var toptUrl = $"otpauth://topt/{issuer}:{userName}?secret={authenticatorCode}&issuer={issuer}";
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(toptUrl, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new Base64QRCode(qrCodeData);
        var base64Image = qrCode.GetGraphic(15);
        return base64Image;
    }
}