using IdentityServer.Attributes;

namespace IdentityServer.Models;

[MongoCollection(name: "users")]
public record User(string FirstName, string LastName, string Password, string Email,
    string Role, IEnumerable<UserClaim> UserClaims) : BaseEntity;