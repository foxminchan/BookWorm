using BookWorm.Constants.Aspire;
using FluentValidation;
using MassTransit;
using MassTransit.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BookWorm.Chassis.EventBus;

public static class Extensions
{
    public static void AddEventBus(
        this IHostApplicationBuilder builder,
        Type type,
        Action<IBusRegistrationConfigurator>? busConfigure = null,
        Action<IBusRegistrationContext, IRabbitMqBusFactoryConfigurator>? rabbitMqConfigure = null
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

            config.AddActivities(type.Assembly);

            config.AddRequestClient(type);

            config.UsingRabbitMq(
                (context, configurator) =>
                {
                    configurator.Host(new Uri(connectionString));
                    configurator.ConfigureEndpoints(context);
                    configurator.UseMessageRetry(AddRetryConfiguration);
                    rabbitMqConfigure?.Invoke(context, configurator);
                }
            );

            busConfigure?.Invoke(config);
        });

        builder
            .Services.AddOpenTelemetry()
            .WithMetrics(b => b.AddMeter(DiagnosticHeaders.DefaultListenerName))
            .WithTracing(p => p.AddSource(DiagnosticHeaders.DefaultListenerName));
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
