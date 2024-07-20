using FluentValidation;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace BookWorm.Shared.Bus;

public static class Extension
{
    public static IHostApplicationBuilder AddRabbitMqEventBus(
        this IHostApplicationBuilder builder, 
        Type type, 
        Action<IBusRegistrationConfigurator>? configure = null)

    {
        var messaging = builder.Configuration.GetConnectionString("eventbus");

        if (string.IsNullOrWhiteSpace(messaging))
        {
            return builder;
        }

        builder.Services.AddMassTransit(config =>
        {
            config.SetKebabCaseEndpointNameFormatter();

            config.AddConsumers(type.Assembly);
            config.AddSagaStateMachines(type.Assembly);
            config.AddSagas(type.Assembly);
            config.AddActivities(type.Assembly);

            config.UsingRabbitMq((context, configurator) =>
            {
                configurator.Host(new Uri(messaging));
                configurator.ConfigureEndpoints(context);
                configurator.UseMessageRetry(AddRetryConfiguration);
            });

            configure?.Invoke(config);
        });

        return builder;
    }

    private static void AddRetryConfiguration(IRetryConfigurator retryConfigurator)
    {
        retryConfigurator.Exponential(
                3,
                TimeSpan.FromMilliseconds(200),
                TimeSpan.FromMinutes(120),
                TimeSpan.FromMilliseconds(200))
            .Ignore<ValidationException>();
    }
}
