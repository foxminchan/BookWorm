using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BookWorm.SharedKernel.EF;

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

        services.AddSingleton<PublishDomainEventsInterceptor>();

        services.AddDbContext<TDbContext>(
            (sp, options) =>
            {
                options
                    .UseNpgsql(builder.Configuration.GetConnectionString(name))
                    .AddInterceptors(sp.GetRequiredService<PublishDomainEventsInterceptor>())
                    .UseSnakeCaseNamingConvention()
                    .ConfigureWarnings(warnings =>
                        warnings.Ignore(RelationalEventId.PendingModelChangesWarning)
                    );
            }
        );

        builder.EnrichAzureNpgsqlDbContext<TDbContext>();

        action?.Invoke(builder);
    }
}
