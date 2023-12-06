// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.


using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IdentityServer.Pages.Create;

public class InputModel
{
    [Required]
    public string Username { get; set; }

    [Required]
    public string FirstName { get; set; }
    
    [Required]
    public string LastName { get; set; }

    public string Password { get; set; }
    public string Email { get; set; }

    public string ReturnUrl { get; set; }

    public string Button { get; set; }

    public string Country { get; set; }

    public IEnumerable<SelectListItem> CountryCodes { get; set; } = new[]
    {
        new SelectListItem() { Text = "India", Value = "ind" },
        new SelectListItem() { Text = "United Arab Emirates", Value = "uae" },
        new SelectListItem() { Text = "United States", Value = "us" }
    };
}