namespace BookWorm.Basket.Infrastructure.Redis;

internal static class Extension
{
    public static IHostApplicationBuilder AddRedisCache(this IHostApplicationBuilder builder)
    {
        builder.AddRedisClient(ServiceName.Redis);

        builder.Services.AddSingleton<IRedisService, RedisService>();

        return builder;
    }
}
