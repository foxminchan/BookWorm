using BookWorm.Chassis.Utilities.Configuration;

namespace BookWorm.ServiceDefaults.Kestrel;

public static class CorsExtensions
{
    private const string AllowAllCorsPolicy = "AllowAll";
    private const string AllowSpecificCorsPolicy = "AllowSpecific";

    public static void AddDefaultCors(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(
                    AllowAllCorsPolicy,
                    policyBuilder =>
                    {
                        policyBuilder.SetIsOriginAllowed(origin =>
                            new Uri(origin).Host == Restful.Host.Localhost
                        );
                    }
                );
            });
        }
        else
        {
            services.Configure<CorsSettings>(CorsSettings.ConfigurationSection);

            services.AddCors(options =>
            {
                options.AddPolicy(
                    AllowSpecificCorsPolicy,
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
        app.UseCors(app.Environment.IsDevelopment() ? AllowAllCorsPolicy : AllowSpecificCorsPolicy);
    }
}
