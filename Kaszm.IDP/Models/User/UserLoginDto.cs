namespace IdentityServer.Models;

public record UserLoginDto(string Provider, string ProviderIdentityKey);