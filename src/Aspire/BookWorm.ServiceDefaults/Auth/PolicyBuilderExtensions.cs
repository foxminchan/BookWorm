using BookWorm.ServiceDefaults.Keycloak;

namespace BookWorm.ServiceDefaults.Auth;

public static class PolicyBuilderExtensions
{
    public static AuthorizationPolicyBuilder RequireScope(
        this AuthorizationPolicyBuilder authorizationPolicyBuilder,
        params string[] allowedValues
    )
    {
        var scopeClaim = authorizationPolicyBuilder.RequireAssertion(context =>
        {
            var scopeClaim = context.User.FindFirst(KeycloakClaimTypes.Scope);

            if (scopeClaim is null)
            {
                return false;
            }

            var scopes = scopeClaim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return scopes.Any(s => allowedValues.Contains(s, StringComparer.OrdinalIgnoreCase));
        });

        return scopeClaim;
    }
}
