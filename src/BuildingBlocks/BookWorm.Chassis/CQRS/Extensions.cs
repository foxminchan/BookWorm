using BookWorm.Chassis.CQRS.Command;
using BookWorm.Chassis.CQRS.Pipelines;
using BookWorm.Chassis.CQRS.Query;
using Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace BookWorm.Chassis.CQRS;

public static class Extensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddCommandHandlerMetrics()
        {
            services.AddSingleton<CommandHandlerMetrics>();
            return services;
        }

        public IServiceCollection AddQueryHandlerMetrics()
        {
            services.AddSingleton<QueryHandlerMetrics>();
            return services;
        }

        public IServiceCollection ApplyActivityBehavior()
        {
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ActivityBehavior<,>));
            return services;
        }

        public IServiceCollection ApplyLoggingBehavior()
        {
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            return services;
        }

        public IServiceCollection ApplyValidationBehavior()
        {
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            return services;
        }
    }
}
