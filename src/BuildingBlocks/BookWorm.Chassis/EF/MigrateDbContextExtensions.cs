using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BookWorm.Chassis.EF;

public static class MigrateDbContextExtensions
{
    private const string ActivitySourceName = "DbMigrations";
    private static readonly ActivitySource _activitySource = new(ActivitySourceName);

    public static IServiceCollection AddMigration<TContext>(this IServiceCollection services)
        where TContext : DbContext
    {
        return services.AddMigration<TContext>((_, _) => Task.CompletedTask);
    }

    public static IServiceCollection AddMigration<TContext>(
        this IServiceCollection services,
        Func<TContext, IServiceProvider, Task> seeder
    )
        where TContext : DbContext
    {
        services.AddOpenTelemetry().WithTracing(tracing => tracing.AddSource(ActivitySourceName));

        return services.AddHostedService(sp => new MigrationHostedService<TContext>(sp, seeder));
    }

    public static IServiceCollection AddMigration<TContext, TDbSeeder>(
        this IServiceCollection services
    )
        where TContext : DbContext
        where TDbSeeder : class, IDbSeeder<TContext>
    {
        services.AddScoped<IDbSeeder<TContext>, TDbSeeder>();
        return services.AddMigration<TContext>(
            (context, sp) => sp.GetRequiredService<IDbSeeder<TContext>>().SeedAsync(context)
        );
    }

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

        using var activity = _activitySource.StartActivity(
            $"Migration operation {typeof(TContext).Name}"
        );

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
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "An error occurred while migrating the database used on context {DbContextName}",
                typeof(TContext).Name
            );

            activity?.AddException(ex);

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
        using var activity = _activitySource.StartActivity($"Migrating {typeof(TContext).Name}");

        try
        {
            await context?.Database.MigrateAsync()!;
            await seeder(context, services);
        }
        catch (Exception ex)
        {
            activity?.AddException(ex);

            throw;
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
