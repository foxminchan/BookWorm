using BookWorm.Constants;
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
        var messaging = builder.Configuration.GetConnectionString(ServiceName.EventBus);

        if (string.IsNullOrWhiteSpace(messaging))
        {
            return builder;
        }

        builder.Services.AddMassTransit(config =>
        {
            config.SetKebabCaseEndpointNameFormatter();

            config.AddConsumers(type.Assembly);

            config.UsingRabbitMq((context, configurator) =>
            {
                configurator.Host(new Uri(messaging));
                configurator.ConfigureEndpoints(context);
                configurator.UseMessageRetry(AddRetryConfiguration);

                configurator.UseSendFilter(typeof(SendFilter<>), context);
                configurator.UsePublishFilter(typeof(PublishFilter<>), context);
                configurator.UseConsumeFilter(typeof(ConsumeFilter<>), context);
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
