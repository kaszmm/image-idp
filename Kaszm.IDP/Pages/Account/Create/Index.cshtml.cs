using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using IdentityModel;
using IdentityServer.Models;
using IdentityServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityServer.Pages.Create;

[SecurityHeaders]
[AllowAnonymous]
public class Index : PageModel
{
    private readonly IUserStoreService _userStoreService;
    private readonly IIdentityServerInteractionService _interaction;
    private readonly IEmailService _emailService;

    [BindProperty]
    public InputModel Input { get; set; }
        
    public Index(
        IIdentityServerInteractionService interaction, IEmailService emailService, IUserStoreService userStoreService = null)
    {
        // this is where you would plug in your own custom identity management library (e.g. ASP.NET Identity)
        _userStoreService = userStoreService ?? throw new ArgumentNullException(nameof(userStoreService));
        _interaction = interaction ?? throw new ArgumentNullException(nameof(interaction));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
    }

    public IActionResult OnGet(string returnUrl)
    {
        Input = new InputModel { ReturnUrl = returnUrl };
        return Page();
    }
        
    public async Task<IActionResult> OnPost()
    {
        // check if we are in the context of an authorization request
        var context = await _interaction.GetAuthorizationContextAsync(Input.ReturnUrl);

        // the user clicked the "cancel" button
        if (Input.Button != "create")
        {
            if (context != null)
            {
                // if the user cancels, send a result back into Kaszm.IDP as if they 
                // denied the consent (even if this client does not require consent).
                // this will send back an access denied OIDC error response to the client.
                await _interaction.DenyAuthorizationAsync(context, AuthorizationError.AccessDenied);

                // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                if (context.IsNativeClient())
                {
                    // The client is native, so this change in how to
                    // return the response is for better UX for the end user.
                    return this.LoadingPage(Input.ReturnUrl);
                }

                return Redirect(Input.ReturnUrl);
            }
            else
            {
                // since we don't have a valid context, then we just go back to the home page
                return Redirect("~/");
            }
        }

        if (await _userStoreService.GetUserByUserNameAsync(Input.Username) != null)
        {
            ModelState.AddModelError("Input.Username", "Invalid username");
        }

        if (ModelState.IsValid)
        {
            var userClaimsDto = new List<UserClaimDto>
            {
                new("country", Input.Country),
                new(JwtClaimTypes.GivenName, Input.LastName),
                new(JwtClaimTypes.Name, Input.FirstName),
                new(JwtClaimTypes.Email, Input.Email)
            };
            
            var userDto = new UserDto(Input.Username, Input.FirstName, Input.LastName,
                Input.Password, Input.Email, default, default, 
                default, "employee", userClaimsDto);
            
            var createdUser = await _userStoreService.CreatUserAsync(userDto);

            var verifyEmailLink = Url.PageLink("/account/emailVerification/verifyEmail", values: new
            {
                userId = createdUser.Id,
                securityCode = createdUser.SecurityCode
            });
        
            Console.WriteLine($"the verify email link {verifyEmailLink}");

            await _emailService.SendEmailAsync("kasim@mail.com", createdUser.Email, "Email verification",
                $"To verify your email please click on this link <a href='{verifyEmailLink}'>Verify Email</a>");
            
            var pageLink = Url.PageLink("/account/emailVerification/index", values: new
            {
                userId = createdUser.Id,
                securityCode = createdUser.SecurityCode
            });
            
            Console.WriteLine($"the generate link when creating user is {pageLink}");

            if (pageLink != null) return RedirectToPage(pageLink);
            // issue authentication cookie with subject ID and username
            // var isuser = new IdentityServerUser(userId.ToString("D"))
            // {
            //     DisplayName = userDto.UserName
            // };

            // await HttpContext.SignInAsync(isuser);

            // if (context != null)
            // {
            //     if (context.IsNativeClient())
            //     {
            //         // The client is native, so this change in how to
            //         // return the response is for better UX for the end user.
            //         return this.LoadingPage(Input.ReturnUrl);
            //     }
            //
            //     // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
            //     return Redirect(Input.ReturnUrl);
            // }
            //
            // // request for a local page
            // if (Url.IsLocalUrl(Input.ReturnUrl))
            // {
            //     return Redirect(Input.ReturnUrl);
            // }
            // else if (string.IsNullOrEmpty(Input.ReturnUrl))
            // {
            //     return Redirect("~/");
            // }
            // else
            // {
            //     // user might have clicked on a malicious link - should be logged
            //     throw new Exception("invalid return URL");
            // }
        }

        return Page();
    }
}