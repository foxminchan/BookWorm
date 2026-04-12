using BookWorm.Chassis.Security.Keycloak;
using Microsoft.AspNetCore.Authorization;

namespace BookWorm.Chassis.Security.Extensions;

public static class PolicyBuilderExtensions
{
    extension(AuthorizationPolicyBuilder authorizationPolicyBuilder)
    {
        /// <summary>
        ///     Requires that the user contains at least one allowed scope value in the Keycloak scope claim.
        /// </summary>
        /// <param name="allowedValues">
        ///     The allowed scope values.
        /// </param>
        /// <returns>
        ///     The configured authorization policy builder.
        /// </returns>
        public AuthorizationPolicyBuilder RequireScope(params string[] allowedValues)
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
}
