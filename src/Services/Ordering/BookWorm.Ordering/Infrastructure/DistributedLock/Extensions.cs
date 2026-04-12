using Microsoft.Extensions.Options;
using StackExchange.Redis;
using ZiggyCreatures.Caching.Fusion.Locking.Distributed;
using ZiggyCreatures.Caching.Fusion.Locking.Distributed.Redis;

namespace BookWorm.Ordering.Infrastructure.DistributedLock;

internal static class Extensions
{
    extension(IHostApplicationBuilder builder)
    {
        public void AddDistributedLock()
        {
            builder.Services.AddSingleton<IFusionCacheDistributedLocker>(sp =>
            {
                var multiplexer = sp.GetRequiredService<IConnectionMultiplexer>();
                var options = Options.Create(
                    new RedisDistributedLockerOptions
                    {
                        ConnectionMultiplexerFactory = () => Task.FromResult(multiplexer),
                    }
                );
                var logger = sp.GetRequiredService<ILogger<RedisDistributedLocker>>();
                return new RedisDistributedLocker(options, logger);
            });
        }
    }
}
