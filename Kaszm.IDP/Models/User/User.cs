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
public record User(string UserName, string FirstName, string LastName, string Password, string Email,
    string Role, IEnumerable<UserClaim> UserClaims) : BaseEntity;