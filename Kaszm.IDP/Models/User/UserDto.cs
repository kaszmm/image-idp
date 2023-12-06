namespace IdentityServer.Models;

public record UserDto(string UserName, string FirstName, string LastName, string Password,
    string Email, bool IsEmailVerified, string SecurityCode, DateTime SecurityCodeExpiration, 
    string Role, IEnumerable<UserClaimDto> UserClaims)
{
    public Guid Id { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastUpdatedAt { get; set; }
    public string ConcurrencyTimestamp { get; set; }
};