using BookWorm.Chassis.Security.Settings;
using BookWorm.Chassis.Utilities.Configuration;
using BookWorm.Constants.Aspire;
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

        services.AddHttpClient(
            Components.KeyCloak,
            client => client.BaseAddress = new($"{Protocols.HttpOrHttps}://{Components.KeyCloak}")
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
                    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
                    options.Audience = "account";
                }
            );

        return builder;
    }
}
