using IdentityServer.Attributes;

namespace IdentityServer.Models;

/// <summary>
/// Defines the user that will be logged in, into our IDP
/// </summary>
/// <param name="UserName">This will be similar to 'Email' for now</param>
/// <param name="FirstName"></param>
/// <param name="LastName"></param>
/// <param name="Password">This should be in encrypted and not in plain text</param>
/// <param name="Email"></param>
/// <param name="Role"></param>
/// <param name="UserClaims">Claims associated to the user</param>
[MongoCollection(name: "users")]
public record User : BaseEntity
{
    public string UserName { get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public string Password { get; init; }
    public string Email { get; init; }
    public bool IsEmailVerified { get; init; }
    
    // For Email verification
    public string SecurityCode { get; init; }
    public DateTime SecurityCodeExpiration { get; init; }
    
    public string Role { get; init; }
    
    public bool TwoFAEnabled { get; init; }
    
    public string AuthenticatorCode { get; init; }
    public IReadOnlyCollection<UserClaim> UserClaims { get; init; }
    public IReadOnlyCollection<UserLogin> UserLogins { get; init; }

    public User(
        string userName,
        string firstName,
        string lastName,
        string password,
        string email,
        bool isEmailVerified,
        string securityCode,
        DateTime securityCodeExpiration,
        string role,
        IReadOnlyCollection<UserClaim> userClaims,
        IReadOnlyCollection<UserLogin> userLogins,
        bool twoFAEnabled = false,
        string authenticatorCode = null)
    {
        ValidationUtility.NotNullOrWhitespace(userName);
        ValidationUtility.NotNullOrWhitespace(firstName);
        // ValidationUtility.NotNullOrWhitespace(password); // for federated users( user with third party logged in, they dont have any password
        ValidationUtility.NotNullOrWhitespace(email);
        ValidationUtility.NotNullOrWhitespace(role);

        UserName = userName;
        FirstName = firstName;
        LastName = lastName;
        Password = password;
        Email = email;
        IsEmailVerified = isEmailVerified;
        SecurityCode = securityCode;
        SecurityCodeExpiration = securityCodeExpiration;
        Role = role;
        UserClaims = userClaims;
        UserLogins = userLogins;
        TwoFAEnabled = twoFAEnabled;
        AuthenticatorCode = authenticatorCode;
    }
}