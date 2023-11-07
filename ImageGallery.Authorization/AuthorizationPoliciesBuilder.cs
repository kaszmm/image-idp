using Microsoft.AspNetCore.Authorization;

namespace Authorization.Policy;

public static class AuthorizationPoliciesBuilder
{
    public static AuthorizationPolicy CanAddImage()
    {
        var policy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            // .RequireRole("PaidUser")
            .RequireClaim("country", "ind" ,"usa")
            .Build();
        return policy;
    }
}