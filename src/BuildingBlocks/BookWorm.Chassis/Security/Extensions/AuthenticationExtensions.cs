using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BookWorm.Chassis.Security.Settings;
using BookWorm.Chassis.Security.TokenExchange;
using BookWorm.Constants.Aspire;
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
        /// <param name="audience">Optional token audience. Audience validation is enabled when specified.</param>
        /// <remarks>
        ///     This method also registers a named HTTP client for Keycloak and applies stricter
        ///     token validation outside development environments.
        /// </remarks>
        /// <returns>The same <see cref="IHostApplicationBuilder" /> instance for fluent configuration.</returns>
        public IHostApplicationBuilder AddDefaultAuthentication(string? audience = null)
        {
            var services = builder.Services;

            builder.AddKeycloakTokenExchange();
            var realm = services.BuildServiceProvider().GetRequiredService<IdentityOptions>().Realm;

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
                        options.Audience = audience ?? "account";
                        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
                        options.TokenValidationParameters.ValidateAudience =
                            audience is not null || !builder.Environment.IsDevelopment();
                        options.TokenValidationParameters.ValidateIssuer =
                            !builder.Environment.IsDevelopment();

                        options.SaveToken = true;
                        options.Events = new()
                        {
                            OnTokenValidated = ctx =>
                            {
                                if (
                                    ctx is
                                    {
                                        SecurityToken: JwtSecurityToken jwt,
                                        Principal.Identity: ClaimsIdentity identity
                                    }
                                )
                                {
                                    identity.AddClaim(new("access_token", jwt.RawData));
                                }

                                return Task.CompletedTask;
                            },
                        };
                    }
                );

            return builder;
        }
    }
}
