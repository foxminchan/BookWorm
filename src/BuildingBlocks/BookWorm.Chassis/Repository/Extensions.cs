using Microsoft.Extensions.DependencyInjection;

namespace BookWorm.Chassis.Repository;

public static class Extensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        ///     Registers all repository implementations from the assembly that contains the specified type.
        /// </summary>
        /// <param name="type">A type used to locate the target assembly for repository scanning.</param>
        public void AddRepositories(Type type)
        {
            services.Scan(scan =>
                scan.FromAssembliesOf(type)
                    .AddClasses(classes => classes.AssignableTo(typeof(IRepository<>)), false)
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
            );
        }
    }
}
