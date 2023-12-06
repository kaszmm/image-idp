using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityServer.Pages.Account.EmailVerification;

[AllowAnonymous]
public class Index : PageModel
{
    public async Task<IActionResult> OnGet(Guid userId, string securityCode)
    {
        return Page();
    }
}