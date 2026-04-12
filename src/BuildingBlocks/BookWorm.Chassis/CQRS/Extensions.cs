using BookWorm.Chassis.CQRS.Command;
using BookWorm.Chassis.CQRS.Pipelines;
using BookWorm.Chassis.CQRS.Query;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BookWorm.Chassis.CQRS;

public static class Extensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        ///     Adds the command handler metrics collector to the service collection.
        /// </summary>
        /// <returns>
        ///     The current service collection.
        /// </returns>
        public IServiceCollection AddCommandHandlerMetrics()
        {
            services.AddSingleton<CommandHandlerMetrics>();
            return services;
        }

        /// <summary>
        ///     Adds the query handler metrics collector to the service collection.
        /// </summary>
        /// <returns>
        ///     The current service collection.
        /// </returns>
        public IServiceCollection AddQueryHandlerMetrics()
        {
            services.AddSingleton<QueryHandlerMetrics>();
            return services;
        }

        /// <summary>
        ///     Registers the activity pipeline behavior.
        /// </summary>
        /// <returns>
        ///     The current service collection.
        /// </returns>
        public IServiceCollection ApplyActivityBehavior()
        {
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ActivityBehavior<,>));
            return services;
        }

        /// <summary>
        ///     Registers the logging pipeline behavior.
        /// </summary>
        /// <returns>
        ///     The current service collection.
        /// </returns>
        public IServiceCollection ApplyLoggingBehavior()
        {
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            return services;
        }

        /// <summary>
        ///     Registers the validation pipeline behavior.
        /// </summary>
        /// <returns>
        ///     The current service collection.
        /// </returns>
        public IServiceCollection ApplyValidationBehavior()
        {
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            return services;
        }

        /// <summary>
        ///     Registers the transaction pipeline behavior for the specified database context.
        /// </summary>
        /// <typeparam name="TDbContext">
        ///     The database context type used to resolve the transactional scope.
        /// </typeparam>
        /// <returns>
        ///     The current service collection.
        /// </returns>
        public IServiceCollection ApplyTransactionBehavior<TDbContext>()
            where TDbContext : DbContext
        {
            services.AddScoped<DbContext>(sp => sp.GetRequiredService<TDbContext>());
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
            return services;
        }
    }
}
