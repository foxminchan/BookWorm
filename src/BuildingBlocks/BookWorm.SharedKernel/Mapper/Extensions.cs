using Microsoft.Extensions.DependencyInjection;

namespace BookWorm.SharedKernel.Mapper;

public static class Extensions
{
    public static void AddMapper(this IServiceCollection services, Type type)
    {
        var assembly = type.Assembly;

        var implementationTypes = assembly
            .GetTypes()
            .Where(t =>
                t is { IsClass: true, IsAbstract: false }
                && t.GetInterfaces()
                    .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMapper<,>))
            );

        foreach (var implementationType in implementationTypes)
        {
            var serviceType = implementationType
                .GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMapper<,>));

            services.AddScoped(serviceType, implementationType);
        }
    }
}
