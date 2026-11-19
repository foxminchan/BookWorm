using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;

namespace BookWorm.Ordering.Extensions;

internal static class AuthorizationExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        public void AddSecurityServices()
        {
            var services = builder.Services;

            builder.AddDefaultAuthentication();

            services
                .AddAuthorizationBuilder()
                .AddPolicy(Authorization.Policies.Admin, policy =>
                {
                    policy
                        .RequireAuthenticatedUser()
                        .RequireRole(Authorization.Roles.Admin)
                        .RequireScope(
                            $"{Services.Ordering}_{Authorization.Actions.Read}",
                            $"{Services.Ordering}_{Authorization.Actions.Write}"
                        );
                })
                .AddPolicy(Authorization.Policies.Customer, policy =>
                {
                    policy
                        .RequireAuthenticatedUser()
                        .RequireRole(Authorization.Roles.Customer)
                        .RequireScope(
                            $"{Services.Ordering}_{Authorization.Actions.Read}",
                            $"{Services.Ordering}_{Authorization.Actions.Write}"
                        );
                })
                .SetDefaultPolicy(
                    new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .RequireRole(Authorization.Roles.Customer, Authorization.Roles.Admin)
                        .RequireScope(
                            $"{Services.Ordering}_{Authorization.Actions.Read}",
                            $"{Services.Ordering}_{Authorization.Actions.Write}"
                        )
                        .Build()
                );

            // Configure ClaimsPrincipal
            services.AddTransient(s =>
                s.GetRequiredService<IHttpContextAccessor>().HttpContext?.User
                ?? new ClaimsPrincipal()
            );
        }
    }
}
