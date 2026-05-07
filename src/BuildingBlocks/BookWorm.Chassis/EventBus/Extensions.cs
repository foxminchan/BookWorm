using BookWorm.Chassis.EventBus.Dispatcher;
using BookWorm.Chassis.EventBus.Wolverine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wolverine;

namespace BookWorm.Chassis.EventBus;

public static class Extensions
{
    extension(IHostApplicationBuilder builder)
    {
        /// <summary>
        ///     Registers and configures the event bus infrastructure for the current host
        ///     by delegating to WolverineFx via <c>UseWolverineEventFramework</c>. When the
        ///     broker connection string is absent the call is a no-op (FR-015).
        /// </summary>
        /// <param name="configure">
        ///     Optional callback for additional <see cref="WolverineOptions" />
        ///     customization (e.g. service-specific handler discovery, Postgres
        ///     persistence wiring, or Kafka listener registration).
        /// </param>
        public void AddEventBus(Action<WolverineOptions>? configure = null)
        {
            builder.UseEventFramework(configure);
        }
    }

    extension(IServiceCollection services)
    {
        /// <summary>
        ///     Registers the event dispatcher service in the dependency injection container.
        /// </summary>
        /// <remarks>
        ///     The dispatcher is registered with a scoped lifetime so a single instance is used per request scope.
        /// </remarks>
        public void AddEventDispatcher()
        {
            services.AddScoped<IEventDispatcher, EventDispatcher>();
        }
    }
}
