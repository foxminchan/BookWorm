using BookWorm.Constants;
using BookWorm.SharedKernel.EventBus.Filters;
using FluentValidation;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace BookWorm.SharedKernel.EventBus;

public static class Extension
{
    public static void AddEventBus(
        this IHostApplicationBuilder builder,
        Type type,
        Action<IBusRegistrationConfigurator>? configure = null
    )
    {
        var connectionString = builder.Configuration.GetConnectionString(Components.Queue);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        builder.Services.AddMassTransit(config =>
        {
            config.SetKebabCaseEndpointNameFormatter();

            config.AddConsumers(type.Assembly);

            config.AddSagas(type.Assembly);

            config.AddActivities(type.Assembly);

            config.UsingRabbitMq(
                (context, configurator) =>
                {
                    configurator.Host(new Uri(connectionString));
                    configurator.ConfigureEndpoints(context);
                    configurator.UseMessageRetry(AddRetryConfiguration);

                    configurator.UseSendFilter(typeof(SendFilter<>), context);
                    configurator.UsePublishFilter(typeof(PublishFilter<>), context);
                    configurator.UseConsumeFilter(typeof(ConsumeFilter<>), context);
                }
            );

            config.AddRequestClient(type);

            configure?.Invoke(config);
        });
    }

    private static void AddRetryConfiguration(IRetryConfigurator retryConfigurator)
    {
        retryConfigurator
            .Exponential(
                3,
                TimeSpan.FromMilliseconds(200),
                TimeSpan.FromMinutes(120),
                TimeSpan.FromMilliseconds(200)
            )
            .Ignore<ValidationException>();
    }
}
