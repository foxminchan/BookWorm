using StackExchange.Redis;

namespace BookWorm.Ordering.Infrastructure.DistributedLock;

internal static class Extensions
{
    public static void AddDistributedLock(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddSingleton<IDistributedLockProvider>(sp =>
        {
            var multiplexer = sp.GetRequiredService<IConnectionMultiplexer>();
            return new RedisDistributedSynchronizationProvider(multiplexer.GetDatabase());
        });

        services.AddSingleton<IDistributedAccessLockProvider, DistributedAccessLockProvider>();
    }
}
