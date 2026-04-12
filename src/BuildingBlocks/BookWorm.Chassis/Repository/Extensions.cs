using Microsoft.Extensions.DependencyInjection;

namespace BookWorm.Chassis.Repository;

public static class Extensions
{
    extension(IServiceCollection services)
    {
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
