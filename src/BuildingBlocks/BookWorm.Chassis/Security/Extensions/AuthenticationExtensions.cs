using BookWorm.Chassis.Security.Settings;
using BookWorm.Chassis.Utilities;
using BookWorm.Chassis.Utilities.Configuration;
using BookWorm.Constants.Aspire;
using BookWorm.Constants.Core;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BookWorm.Chassis.Security.Extensions;

public static class AuthenticationExtensions
{
    public static IHostApplicationBuilder AddDefaultAuthentication(
        this IHostApplicationBuilder builder
    )
    {
        var services = builder.Services;

        services.Configure<IdentityOptions>(IdentityOptions.ConfigurationSection);

        var realm = services.BuildServiceProvider().GetRequiredService<IdentityOptions>().Realm;
        // Use HTTP in development to match the frontend's Keycloak URL scheme,
        // avoiding token issuer mismatch during introspection.
        var scheme = builder.Environment.IsDevelopment()
            ? Http.Schemes.Http
            : Http.Schemes.HttpOrHttps;

        var keycloakUrl = HttpUtilities
            .AsUrlBuilder()
            .WithScheme(scheme)
            .WithHost(Components.KeyCloak)
            .Build();

        services.AddHttpClient(
            Components.KeyCloak,
            client => client.BaseAddress = new(keycloakUrl)
        );

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
                    options.Audience = "account";
                    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
                    options.TokenValidationParameters.ValidateAudience = !builder.Environment.IsDevelopment();
                    options.TokenValidationParameters.ValidateIssuer = !builder.Environment.IsDevelopment();
                }
            );

        return builder;
    }
}
