using System.Collections.ObjectModel;
using Amazon.Runtime.Internal;
using IdentityModel;

namespace IdentityServer;

public interface IExternalLoginProviderSettings
{
    string Name { get; }
    string ClientId { get; set; }
    string ClientSecret { get; set; }
    IReadOnlyDictionary<string, string> MappedClaims { get; }
}

public class FacebookLoginProviderSettings : IExternalLoginProviderSettings
{
    public string Name => "Facebook";
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }

    public IReadOnlyDictionary<string, string> MappedClaims => new Dictionary<string, string>
    {
        { "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", JwtClaimTypes.Subject },
        { "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", JwtClaimTypes.Email },
        { "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname", JwtClaimTypes.GivenName },
        { "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname", JwtClaimTypes.FamilyName },
        { "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", JwtClaimTypes.Name }
    };
}