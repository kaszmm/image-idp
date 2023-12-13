namespace IdentityServer.Pages.Account.MFA;

public class InputModel
{
    public string QrImage { get; set; }
    public string AuthenticatorCode { get; set; }
    public string VerificationCode { get; set; }
    public string Message { get; set; }
}