using BookWorm.Chassis.OpenTelemetry.ActivityScope;
using Microsoft.Extensions.DependencyInjection;

namespace BookWorm.Chassis.OpenTelemetry;

public static class Extensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddActivityScope()
        {
            services.AddSingleton<IActivityScope, ActivityScope.ActivityScope>();
            return services;
        }
    }
}
