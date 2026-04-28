using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BookWorm.Chassis.EF;

public static class MigrateDbContextExtensions
{
    private const string ActivitySourceName = "DbMigrations";
    private static readonly ActivitySource _activitySource = new(ActivitySourceName, "1.0.0");

    private static async Task MigrateDbContextAsync<TContext>(
        this IServiceProvider services,
        Func<TContext, IServiceProvider, Task> seeder
    )
        where TContext : DbContext
    {
        using var scope = services.CreateScope();
        var scopeServices = scope.ServiceProvider;
        var logger = scopeServices.GetRequiredService<ILogger<TContext>>();
        var context = scopeServices.GetService<TContext>();

        using var activity = _activitySource.HasListeners()
            ? _activitySource.StartActivity()
            : null;

        activity?.DisplayName = $"Migration operation {typeof(TContext).Name}";

        try
        {
            logger.LogInformation(
                "Migrating database associated with context {DbContextName}",
                typeof(TContext).Name
            );

            var strategy = context?.Database.CreateExecutionStrategy();

            if (strategy is not null)
            {
                await strategy.ExecuteAsync(() => InvokeSeeder(seeder!, context, scopeServices));
            }

            activity?.SetStatus(ActivityStatusCode.Ok);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "An error occurred while migrating the database used on context {DbContextName}",
                typeof(TContext).Name
            );

            if (activity is null)
            {
                throw new InvalidOperationException(
                    $"Database migration failed for {typeof(TContext).Name}. See inner exception for details.",
                    ex
                );
            }

            activity.SetStatus(ActivityStatusCode.Error);
            activity.SetTag("otel.status_code", "error");
            activity.SetTag("otel.status_description", ex.Message);
            activity.AddException(ex);

            throw new InvalidOperationException(
                $"Database migration failed for {typeof(TContext).Name}. See inner exception for details.",
                ex
            );
        }
    }

    private static async Task InvokeSeeder<TContext>(
        Func<TContext, IServiceProvider, Task> seeder,
        TContext context,
        IServiceProvider services
    )
        where TContext : DbContext?
    {
        using var activity = _activitySource.HasListeners()
            ? _activitySource.StartActivity()
            : null;

        activity?.DisplayName = $"Migrating {typeof(TContext).Name}";

        try
        {
            await context?.Database.MigrateAsync()!;
            await seeder(context, services);

            activity?.SetStatus(ActivityStatusCode.Ok);
        }
        catch (Exception ex)
        {
            if (activity is null)
            {
                throw;
            }

            activity.SetStatus(ActivityStatusCode.Error);
            activity.SetTag("otel.status_code", "error");
            activity.SetTag("otel.status_description", ex.Message);
            activity.AddException(ex);

            throw;
        }
    }

    extension(IServiceCollection services)
    {
        /// <summary>
        ///     Registers a hosted database migration for the specified <typeparamref name="TContext" />
        ///     without custom seed logic.
        /// </summary>
        /// <typeparam name="TContext">The EF Core database context to migrate.</typeparam>
        /// <returns>The same service collection for chaining.</returns>
        public IServiceCollection AddMigration<TContext>()
            where TContext : DbContext
        {
            return services.AddMigration<TContext>((_, _) => Task.CompletedTask);
        }

        /// <summary>
        ///     Registers a hosted database migration for the specified <typeparamref name="TContext" />
        ///     and executes the provided seed delegate after migrations are applied.
        /// </summary>
        /// <typeparam name="TContext">The EF Core database context to migrate.</typeparam>
        /// <param name="seeder">
        ///     A delegate that seeds data after migration using the resolved context and scoped services.
        /// </param>
        /// <returns>The same service collection for chaining.</returns>
        /// <remarks>
        ///     This method also registers OpenTelemetry tracing for the migration activity source.
        /// </remarks>
        public IServiceCollection AddMigration<TContext>(
            Func<TContext, IServiceProvider, Task> seeder
        )
            where TContext : DbContext
        {
            services
                .AddOpenTelemetry()
                .WithTracing(tracing => tracing.AddSource(ActivitySourceName));

            return services.AddHostedService(sp => new MigrationHostedService<TContext>(
                sp,
                seeder
            ));
        }

        /// <summary>
        ///     Registers a typed database seeder and configures hosted migration for
        ///     <typeparamref name="TContext" />.
        /// </summary>
        /// <typeparam name="TContext">The EF Core database context to migrate.</typeparam>
        /// <typeparam name="TDbSeeder">
        ///     The seeder implementation used to seed the migrated database.
        /// </typeparam>
        /// <returns>The same service collection for chaining.</returns>
        public IServiceCollection AddMigration<TContext, TDbSeeder>()
            where TContext : DbContext
            where TDbSeeder : class, IDbSeeder<TContext>
        {
            services.AddScoped<IDbSeeder<TContext>, TDbSeeder>();
            return services.AddMigration<TContext>(
                (context, sp) => sp.GetRequiredService<IDbSeeder<TContext>>().SeedAsync(context)
            );
        }
    }

    private sealed class MigrationHostedService<TContext>(
        IServiceProvider serviceProvider,
        Func<TContext, IServiceProvider, Task> seeder
    ) : BackgroundService
        where TContext : DbContext
    {
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            return serviceProvider.MigrateDbContextAsync(seeder);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }
    }
}

public interface IDbSeeder<in TContext>
    where TContext : DbContext
{
    Task SeedAsync(TContext context);
}
