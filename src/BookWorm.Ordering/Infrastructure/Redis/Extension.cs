namespace BookWorm.Ordering.Infrastructure.Redis;

public static class Extension
{
    public static IHostApplicationBuilder AddRedisCache(this IHostApplicationBuilder builder)
    {
        builder.AddRedisClient("redis");

        builder.Services.AddSingleton<IRedisService, RedisService>();

        return builder;
    }
}
