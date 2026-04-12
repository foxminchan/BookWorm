using Microsoft.Extensions.DependencyInjection;

namespace BookWorm.Chassis.Mapper;

public static class Extensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        ///     Registers all non-abstract mapper implementations from the specified assembly into the dependency injection
        ///     container.
        /// </summary>
        /// <param name="type">
        ///     A type used to resolve the target assembly to scan for implementations of
        ///     <see cref="IMapper{TSource, TDestination}" />.
        /// </param>
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
