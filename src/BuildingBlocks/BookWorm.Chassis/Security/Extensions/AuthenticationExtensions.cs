using BookWorm.Chassis.Security.Settings;
using BookWorm.Chassis.Utilities;
using BookWorm.Chassis.Utilities.Configurations;
using BookWorm.Constants.Aspire;
using BookWorm.Constants.Core;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BookWorm.Chassis.Security.Extensions;

public static class AuthenticationExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        /// <summary>
        ///     Configures the default JWT bearer authentication pipeline using Keycloak settings
        ///     resolved from the configured identity options.
        /// </summary>
        /// <remarks>
        ///     This method also registers a named HTTP client for Keycloak and applies stricter
        ///     token validation outside development environments.
        /// </remarks>
        /// <returns>The same <see cref="IHostApplicationBuilder" /> instance for fluent configuration.</returns>
        public IHostApplicationBuilder AddDefaultAuthentication()
        {
            var services = builder.Services;

            // Binds identity configuration section to <see cref="IdentityOptions"/>.
            builder.Configure<IdentityOptions>(IdentityOptions.ConfigurationSection);

            // Resolves the Keycloak realm from bound identity options.
            var realm = services.BuildServiceProvider().GetRequiredService<IdentityOptions>().Realm;

            // Uses HTTP in development and HTTP/HTTPS for non-development environments.
            var scheme = builder.Environment.IsDevelopment()
                ? Uri.UriSchemeHttp
                : Http.Schemes.HttpOrHttps;

            // Builds the Keycloak base URL from internal component naming conventions.
            var keycloakUrl = HttpUtilities
                .AsUrlBuilder()
                .WithScheme(scheme)
                .WithHost(Components.KeyCloak)
                .Build();

            // Registers a named HTTP client used for Keycloak communication.
            services.AddHttpClient(
                Components.KeyCloak,
                client => client.BaseAddress = new(keycloakUrl)
            );

            // Configures JWT bearer authentication backed by Keycloak.
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
                        // Uses the Keycloak account client audience.
                        options.Audience = "account";
                        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
                        options.TokenValidationParameters.ValidateAudience =
                            !builder.Environment.IsDevelopment();
                        options.TokenValidationParameters.ValidateIssuer =
                            !builder.Environment.IsDevelopment();
                    }
                );

            return builder;
        }
    }
}
