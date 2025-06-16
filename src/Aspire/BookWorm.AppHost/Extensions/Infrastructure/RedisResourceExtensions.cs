using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace BookWorm.AppHost.Extensions.Infrastructure;

public static class RedisResourceExtensions
{
    /// <summary>
    ///     Adds a "clear-cache" command to the <see cref="IResourceBuilder{RedisResource}" />.
    ///     This command allows clearing all data from the Redis cache associated with the resource.
    /// </summary>
    /// <param name="builder">The resource builder for <see cref="RedisResource" />.</param>
    /// <returns>The same <see cref="IResourceBuilder{RedisResource}" /> instance for chaining.</returns>
    public static IResourceBuilder<RedisResource> WithClearCommand(
        this IResourceBuilder<RedisResource> builder
    )
    {
        builder.WithCommand(
            "clear-cache",
            "Clear Cache",
            context => OnRunClearCacheCommandAsync(builder, context),
            new()
            {
                UpdateState = OnUpdateResourceState,
                IconName = "DeleteOff",
                IconVariant = IconVariant.Filled,
                Description = "Clears all data from the Redis cache.",
                IsHighlighted = false,
                ConfirmationMessage =
                    "Are you sure you want to clear the Redis cache? This action cannot be undone.",
            }
        );

        return builder;
    }

    private static async Task<ExecuteCommandResult> OnRunClearCacheCommandAsync(
        IResourceBuilder<RedisResource> builder,
        ExecuteCommandContext context
    )
    {
        var connectionString =
            await builder.Resource.GetConnectionStringAsync()
            ?? throw new InvalidOperationException(
                $"Unable to get the '{context.ResourceName}' connection string."
            );

        await using var connection = await ConnectionMultiplexer.ConnectAsync(connectionString);

        var database = connection.GetDatabase();

        await database.ExecuteAsync("FLUSHALL");

        return CommandResults.Success();
    }

    private static ResourceCommandState OnUpdateResourceState(UpdateCommandStateContext context)
    {
        var logger = context.ServiceProvider.GetRequiredService<ILogger<Program>>();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Updating resource state: {ResourceSnapshot}",
                context.ResourceSnapshot
            );
        }

        return context.ResourceSnapshot.HealthStatus is HealthStatus.Healthy
            ? ResourceCommandState.Enabled
            : ResourceCommandState.Disabled;
    }
}
