namespace IdentityServer.Models;

public record UserDto(string FirstName, string LastName, string Password, string Email,
    string Role, IEnumerable<UserClaimDto> UserClaims);