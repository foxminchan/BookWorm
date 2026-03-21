using BookWorm.Chassis.Utilities.Configurations;

namespace BookWorm.ServiceDefaults.Cors;

public static class CorsExtensions
{
    public static void AddDefaultCors(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(
                    CorsConstants.AllowAllCorsPolicy,
                    policyBuilder =>
                    {
                        policyBuilder
                            .SetIsOriginAllowed(origin => new Uri(origin).Host == Network.Localhost)
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

            services.AddCors(options =>
            {
                options.AddPolicy(
                    CorsConstants.AllowSpecificCorsPolicy,
                    policyBuilder =>
                    {
                        var serviceProvider = services.BuildServiceProvider();
                        var corsOptions = serviceProvider.GetRequiredService<CorsSettings>();

                        policyBuilder
                            .WithOrigins([.. corsOptions.Origins])
                            .WithHeaders([.. corsOptions.Headers])
                            .WithMethods([.. corsOptions.Methods]);

                        if (corsOptions.MaxAge is not null)
                        {
                            policyBuilder.SetPreflightMaxAge(
                                TimeSpan.FromSeconds(corsOptions.MaxAge.Value)
                            );
                        }

                        if (corsOptions.AllowCredentials)
                        {
                            policyBuilder.AllowCredentials();
                        }
                    }
                );
            });
        }
    }

    public static void UseDefaultCors(this WebApplication app)
    {
        app.UseCors(
            app.Environment.IsDevelopment()
                ? CorsConstants.AllowAllCorsPolicy
                : CorsConstants.AllowSpecificCorsPolicy
        );
    }
}
