using BookWorm.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BookWorm.Chassis.EF;

public static class DbContextExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        /// <summary>
        ///     Registers a PostgreSQL <see cref="DbContext" /> configured for Azure usage, snake_case naming,
        ///     no-tracking queries, and optional EF Core interceptors.
        /// </summary>
        /// <typeparam name="TDbContext">The <see cref="DbContext" /> type to register.</typeparam>
        /// <param name="name">The connection string name from configuration.</param>
        /// <param name="action">An optional callback to apply additional builder configuration.</param>
        /// <param name="excludeDefaultInterceptors">
        ///     Set to <see langword="true" /> to skip registering default interceptors.
        /// </param>
        public void AddAzurePostgresDbContext<TDbContext>(
            string name,
            Action<IHostApplicationBuilder>? action = null,
            bool excludeDefaultInterceptors = false
        )
            where TDbContext : DbContext
        {
            var services = builder.Services;

            // Register cross-cutting interceptors by default for query diagnostics and domain event dispatching.
            if (!excludeDefaultInterceptors)
            {
                services.AddScoped<IInterceptor, QueryPerformanceInterceptor>();
                services.AddScoped<IInterceptor, EventDispatchInterceptor>();
                services.AddScoped<IDomainEventDispatcher, MediatorDomainEventDispatcher>();
            }

            services.AddDbContext<TDbContext>(
                (sp, options) =>
                {
                    options
                        .UseNpgsql(builder.Configuration.GetConnectionString(name))
                        .UseSnakeCaseNamingConvention()
                        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                        // Suppresses known EF Core pending model changes warning.
                        // Issue: https://github.com/dotnet/efcore/issues/35285
                        .ConfigureWarnings(warnings =>
                            warnings.Ignore(RelationalEventId.PendingModelChangesWarning)
                        );

                    // Resolve and apply all registered EF Core interceptors.
                    var interceptors = sp.GetServices<IInterceptor>().ToArray();

                    if (interceptors.Length != 0)
                    {
                        options.AddInterceptors(interceptors);
                    }
                }
            );

            // Apply project-specific Azure Npgsql enrichment.
            builder.EnrichAzureNpgsqlDbContext<TDbContext>();

            action?.Invoke(builder);
        }
    }
}
