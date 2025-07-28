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

        services.AddScoped<DbCommandInterceptor, QueryPerformanceInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, PublishDomainEventsInterceptor>();

        services.AddDbContext<TDbContext>(
            (sp, options) =>
            {
                options
                    .UseNpgsql(builder.Configuration.GetConnectionString(name))
                    .UseSnakeCaseNamingConvention()
                    // Issue: https://github.com/dotnet/efcore/issues/35285
                    .ConfigureWarnings(warnings =>
                        warnings.Ignore(RelationalEventId.PendingModelChangesWarning)
                    );

                var interceptors = sp.GetServices<IInterceptor>().ToArray();

                if (interceptors.Length != 0)
                {
                    options.AddInterceptors(interceptors);
                }
            }
        );

        builder.EnrichAzureNpgsqlDbContext<TDbContext>();

        action?.Invoke(builder);
    }
}
