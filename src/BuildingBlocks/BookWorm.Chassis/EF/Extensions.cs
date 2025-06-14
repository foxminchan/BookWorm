using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BookWorm.Chassis.EF;

public static class Extensions
{
    public static void AddAzurePostgresDbContext<TDbContext>(
        this IHostApplicationBuilder builder,
        string name,
        Action<IHostApplicationBuilder>? action = null
    )
        where TDbContext : DbContext
    {
        var services = builder.Services;

        services.AddScoped<ISaveChangesInterceptor, PublishDomainEventsInterceptor>();
        services.AddScoped<DbCommandInterceptor, QueryPerformanceInterceptor>();

        services.AddDbContext<TDbContext>(
            (sp, options) =>
            {
                options
                    .UseNpgsql(builder.Configuration.GetConnectionString(name))
                    .AddInterceptors(
                        sp.GetRequiredService<ISaveChangesInterceptor>(),
                        sp.GetRequiredService<DbCommandInterceptor>()
                    )
                    .UseSnakeCaseNamingConvention()
                    // Issue: https://github.com/dotnet/efcore/issues/35285
                    .ConfigureWarnings(warnings =>
                        warnings.Ignore(RelationalEventId.PendingModelChangesWarning)
                    );

                if (builder.Environment.IsDevelopment())
                {
                    options.EnableSensitiveDataLogging();
                }
            }
        );

        builder.EnrichAzureNpgsqlDbContext<TDbContext>();

        action?.Invoke(builder);
    }
}
