using Microsoft.Extensions.DependencyInjection;

namespace BookWorm.Chassis.Mapper;

public static class Extensions
{
    extension(IServiceCollection services)
    {
        public void AddMapper(Type type)
        {
            services.Scan(scan =>
                scan.FromAssemblies(type.Assembly)
                    .AddClasses(
                        classes =>
                            classes.AssignableTo(typeof(IMapper<,>)).Where(t => !t.IsAbstract),
                        false
                    )
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
            );
        }
    }
}
