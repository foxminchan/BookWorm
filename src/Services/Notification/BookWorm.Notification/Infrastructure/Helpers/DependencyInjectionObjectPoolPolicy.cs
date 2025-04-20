namespace BookWorm.Notification.Infrastructure.Helpers;

public sealed class DependencyInjectionObjectPoolPolicy<T>(IServiceProvider sp)
    : IPooledObjectPolicy<T>
    where T : class, new()
{
    public T Create()
    {
        return sp.GetRequiredService<T>();
    }

    public bool Return(T obj)
    {
        return true;
    }
}
