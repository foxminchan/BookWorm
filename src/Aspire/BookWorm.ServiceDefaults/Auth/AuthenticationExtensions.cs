using BookWorm.Constants.Aspire;
using BookWorm.Constants.Core;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BookWorm.ServiceDefaults.Auth;

public static class AuthenticationExtensions
{
    public static IHostApplicationBuilder AddDefaultAuthentication(
        this IHostApplicationBuilder builder,
        string? realm = null
    )
    {
        var services = builder.Services;

        realm ??=
            Environment.GetEnvironmentVariable("REALM") ?? nameof(BookWorm).ToLowerInvariant();

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddKeycloakJwtBearer(
                Components.KeyCloak,
                realm,
                options =>
                {
                    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
                    options.Audience = "account";
                }
            );

        services
            .AddAuthorizationBuilder()
            .AddPolicy(
                Authorization.Policies.Admin,
                policy => policy.RequireRole(Authorization.Roles.Admin)
            )
            .AddPolicy(Authorization.Policies.User, policy => policy.RequireAuthenticatedUser())
            .SetDefaultPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

        return builder;
    }
}
