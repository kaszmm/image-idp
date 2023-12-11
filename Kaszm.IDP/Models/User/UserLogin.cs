namespace IdentityServer.Models;

// TODO: Why we need unique identifier for user login if its gonna be part of User.cs,
// can a 'Provider' be a unique identifier?, maybe Provider can be Smart Enum?
public record UserLogin
{
    public string Provider { get; }
    public string ProviderIdentityKey { get; }

    public UserLogin(string provider, string providerIdentityKey)
    {
        ValidationUtility.NotNullOrWhitespace(provider);
        ValidationUtility.NotNullOrWhitespace(providerIdentityKey);

        Provider = provider;
        ProviderIdentityKey = providerIdentityKey;
    }
};