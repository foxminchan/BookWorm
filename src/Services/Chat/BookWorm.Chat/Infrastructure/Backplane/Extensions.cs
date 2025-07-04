using BookWorm.Chat.Infrastructure.Backplane.Contracts;

namespace BookWorm.Chat.Infrastructure.Backplane;

internal static class Extensions
{
    public static void AddBackplaneServices(this IServiceCollection services)
    {
        services.AddSingleton<IConversationState, RedisConversationState>();
        services.AddSingleton<ICancellationManager, RedisCancellationManager>();
        services.AddSingleton<RedisBackplaneService>();
    }
}
