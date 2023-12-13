namespace IdentityServer.Models;

public record UserDto(
    string UserName,
    string FirstName,
    string LastName,
    string Password,
    string Email,
    bool IsEmailVerified,
    string SecurityCode,
    DateTime SecurityCodeExpiration,
    string Role,
    IEnumerable<UserClaimDto> UserClaims,
    IReadOnlyCollection<UserLoginDto> UserLogins,
    bool TwoFAEnabled = false,
    string AuthenticatorCode = null)
{
    
    // properties that are good to have but not mandatory on each obj initialization
    public Guid Id { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? LastUpdatedAt { get; init; }
    public string ConcurrencyTimestamp { get; init; }
};