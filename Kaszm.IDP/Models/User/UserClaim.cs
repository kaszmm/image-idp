namespace IdentityServer.Models;

public record UserClaim
{
    public string Type { get; }
    public string Value { get; }

    public UserClaim(string type, string value)
    {
        ValidationUtility.NotNullOrWhitespace(type);
        ValidationUtility.NotNullOrWhitespace(value);

        Type = type;
        Value = value;
    }
};