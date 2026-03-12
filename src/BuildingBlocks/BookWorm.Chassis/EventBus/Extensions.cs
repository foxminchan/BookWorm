using BookWorm.Chassis.EventBus.Dispatcher;
using BookWorm.Constants.Aspire;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wolverine;
using Wolverine.ErrorHandling;
using Wolverine.RabbitMQ;

namespace BookWorm.Chassis.EventBus;

public static class Extensions
{
    public static void AddEventBus(
        this IHostApplicationBuilder builder,
        Type type,
        Action<WolverineOptions>? configure = null
    )
    {
        var connectionString = builder.Configuration.GetConnectionString(Components.Queue);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        builder.UseWolverine(opts =>
        {
            opts.Discovery.IncludeAssembly(type.Assembly);

            opts.UseRabbitMq(new Uri(connectionString)).AutoProvision().UseConventionalRouting();

            opts.Policies.OnException<ValidationException>().Discard();
            opts.Policies.OnException<Exception>()
                .RetryWithCooldown(
                    TimeSpan.FromMilliseconds(200),
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(5)
                );

            configure?.Invoke(opts);
        });
    }

    public static void AddEventDispatcher(this IServiceCollection services)
    {
        services.AddScoped<IEventDispatcher, EventDispatcher>();
    }
}
