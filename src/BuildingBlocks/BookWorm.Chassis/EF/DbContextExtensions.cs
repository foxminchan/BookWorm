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
        public void AddAzurePostgresDbContext<TDbContext>(
            string name,
            Action<IHostApplicationBuilder>? action = null,
            bool excludeDefaultInterceptors = false
        )
            where TDbContext : DbContext
        {
            var services = builder.Services;

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
}
