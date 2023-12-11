using System.Runtime.CompilerServices;

namespace IdentityServer.Models;

public static class ValidationUtility
{
    public static void NotNullOrWhitespace(string value, [CallerArgumentExpression("value")] string propertyName = null)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"'{propertyName}' cannot be null or whitespace.", propertyName);
        }
    }
}