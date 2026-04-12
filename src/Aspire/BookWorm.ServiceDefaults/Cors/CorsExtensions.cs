using BookWorm.Chassis.Utilities.Configurations;
using Microsoft.Extensions.Configuration;

namespace BookWorm.ServiceDefaults.Cors;

public static class CorsExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        /// <summary>
        ///     Registers the CORS policy appropriate for the current environment.
        ///     In development, allows any request from localhost.
        ///     In non-development environments, applies the strongly-typed <see cref="CorsSettings" /> configuration.
        /// </summary>
        public void AddDefaultCors()
        {
            var services = builder.Services;

            if (builder.Environment.IsDevelopment())
            {
                services.AddCors(options =>
                {
                    options.AddPolicy(
                        CorsConstants.AllowAllCorsPolicy,
                        policyBuilder =>
                        {
                            policyBuilder
                                .SetIsOriginAllowed(origin =>
                                    new Uri(origin).Host == Network.Localhost
                                )
                                .AllowAnyHeader()
                                .AllowAnyMethod()
                                .AllowCredentials();
                        }
                    );
                });
            }
            else
            {
                builder.Configure<CorsSettings>(CorsSettings.ConfigurationSection);

                var corsSettings =
                    builder
                        .Configuration.GetRequiredSection(CorsSettings.ConfigurationSection)
                        .Get<CorsSettings>()
                    ?? throw new InvalidOperationException(
                        $"Failed to bind CORS settings from configuration section: {CorsSettings.ConfigurationSection}"
                    );

                services.AddCors(options =>
                {
                    options.AddPolicy(
                        CorsConstants.AllowSpecificCorsPolicy,
                        policyBuilder =>
                        {
                            policyBuilder
                                .WithOrigins([.. corsSettings.Origins])
                                .WithHeaders([.. corsSettings.Headers])
                                .WithMethods([.. corsSettings.Methods]);

                            if (corsSettings.MaxAge is not null)
                            {
                                policyBuilder.SetPreflightMaxAge(
                                    TimeSpan.FromSeconds(corsSettings.MaxAge.Value)
                                );
                            }

                            if (corsSettings.AllowCredentials)
                            {
                                policyBuilder.AllowCredentials();
                            }
                        }
                    );
                });
            }
        }
    }

    extension(WebApplication app)
    {
        /// <summary>
        ///     Applies the CORS middleware using the policy registered by <see cref="CorsExtensions" />.
        ///     Selects <c>AllowAll</c> in development and <c>AllowSpecific</c> in all other environments.
        /// </summary>
        public void UseDefaultCors()
        {
            app.UseCors(
                app.Environment.IsDevelopment()
                    ? CorsConstants.AllowAllCorsPolicy
                    : CorsConstants.AllowSpecificCorsPolicy
            );
        }
    }
}
