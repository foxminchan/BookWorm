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
                .AddPolicy(Authorization.Policies.Reporter, policy =>
                {
                    policy
                        .RequireAuthenticatedUser()
                        .RequireRole(Authorization.Roles.Reporter)
                        .RequireScope($"{Services.Ordering}_{Authorization.Actions.Read}");
                })
                .SetDefaultPolicy(
                    new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .RequireRole(Authorization.Roles.User)
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
