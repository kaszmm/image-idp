using Duende.IdentityServer;
using IdentityServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityServer.Pages.Account.EmailVerification;

[AllowAnonymous]
public class VerifyEmail : PageModel
{
    private readonly IUserStoreService _userStoreService;

    [BindProperty] public InputModel Input { get; set; }
    
    public VerifyEmail(IUserStoreService userStoreService)
    {
        _userStoreService = userStoreService ?? throw new ArgumentNullException(nameof(userStoreService));
    }

    public async Task<IActionResult> OnGet(Guid userId, string securityCode)
    {
        var user = await _userStoreService.GetUserAsync(userId);

        if (user is null)
        {
            Input = new InputModel
            {
                Message = "Invalid verification link, malformed link."
            }; 
            return Page();
        }

        if (user.IsEmailVerified)
        {
            Input = new InputModel
            {
                Message = "Email is already verified, please navigate to login page."
            };

            return Page();
        }

        var isValidSecurityCode = await _userStoreService.VerifySecurityCode(userId, securityCode);

        if (!isValidSecurityCode)
        {
            Input = new InputModel
            {
                Message = "Invalid verification link, link got expired or malformed."
            };
            return Page();
        }

        user = user with
        {
            SecurityCode = null,
            IsEmailVerified = true
        };

        await _userStoreService.UpdateUserAsync(user);

        var isUser = new IdentityServerUser(user.Id.ToString("D"))
        {
            DisplayName = user.FirstName
        };

        await HttpContext.SignInAsync(isUser);
        
        Input = new InputModel
        {
            Message = "Email verification completed, please go to login page"
        }; 
        
        return Page();
    }
}