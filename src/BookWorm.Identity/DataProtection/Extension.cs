using Microsoft.AspNetCore.DataProtection;
using StackExchange.Redis;

namespace BookWorm.Identity.DataProtection;

public static class Extension
{
    public static IHostApplicationBuilder AddRedisDataProtection(this IHostApplicationBuilder builder)
    {
        var conn = builder.Configuration.GetConnectionString("redis");

        if (string.IsNullOrWhiteSpace(conn))
        {
            return builder;
        }

        builder.Services.AddDataProtection()
            .SetDefaultKeyLifetime(TimeSpan.FromDays(14))
            .SetApplicationName(nameof(BookWorm))
            .PersistKeysToStackExchangeRedis(ConnectionMultiplexer.Connect(conn), nameof(DataProtectionProvider));

        return builder;
    }
}
