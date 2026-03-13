using System.Reflection;
using BookWorm.Chassis.EventBus.Dispatcher;
using BookWorm.Chassis.EventBus.Kafka;
using BookWorm.Chassis.EventBus.Serialization;
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
        Action<IBusRegistrationConfigurator>? busConfigure = null
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

            config.AddActivities(type.Assembly);

            config.UsingInMemory(
                (context, configurator) =>
                {
                    configurator.UseCloudEvents();
                    configurator.ConfigureEndpoints(context);
                    configurator.UseMessageRetry(AddRetryConfiguration);
                    configurator.UseDelayedMessageScheduler();
                    configurator.UsePublishFilter(
                        typeof(KafkaPublishFilter<>),
                        context
                    );
                }
            );

            config.AddRider(rider =>
            {
                rider.AddConsumers(type.Assembly);

                RegisterKafkaProducers(rider, type.Assembly);

                rider.UsingKafka(
                    (context, k) =>
                    {
                        k.Host(connectionString);

                        ConfigureKafkaTopicEndpoints(k, context, type);
                    }
                );
            });

            busConfigure?.Invoke(config);
        });

        builder
            .Services.AddOpenTelemetry()
            .WithMetrics(b => b.AddMeter(DiagnosticHeaders.DefaultListenerName))
            .WithTracing(p => p.AddSource(DiagnosticHeaders.DefaultListenerName));
    }

    public static void AddEventDispatcher(this IServiceCollection services)
    {
        services.AddScoped<IEventDispatcher, EventDispatcher>();
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

    private static string ToKebabCase(string typeName)
    {
        var chars = new List<char>(typeName.Length + 10);

        for (var i = 0; i < typeName.Length; i++)
        {
            if (char.IsUpper(typeName[i]))
            {
                if (i > 0)
                {
                    chars.Add('-');
                }

                chars.Add(char.ToLowerInvariant(typeName[i]));
            }
            else
            {
                chars.Add(typeName[i]);
            }
        }

        return new(chars.ToArray());
    }

    private static void RegisterKafkaProducers(
        IRiderRegistrationConfigurator rider,
        Assembly assembly
    )
    {
        var messageTypes = DiscoverMessageTypes(assembly);

        foreach (var messageType in messageTypes)
        {
            var topicName = ToKebabCase(messageType.Name);
            var registrarType = typeof(ProducerRegistrar<>).MakeGenericType(messageType);
            var registrar = (IProducerRegistrar)Activator.CreateInstance(registrarType)!;
            registrar.Register(rider, topicName);
        }
    }

    private static void ConfigureKafkaTopicEndpoints(
        IKafkaFactoryConfigurator kafka,
        IRiderRegistrationContext context,
        Type assemblyMarker
    )
    {
        var assembly = assemblyMarker.Assembly;
        var consumerGroup = assembly.GetName().Name!.ToLowerInvariant().Replace('.', '-');
        var consumerInterface = typeof(IConsumer<>);

        var consumerEntries = assembly
            .GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface && t.IsClass)
            .SelectMany(t =>
                t.GetInterfaces()
                    .Where(i =>
                        i.IsGenericType && i.GetGenericTypeDefinition() == consumerInterface
                    )
                    .Select(i => new { ConsumerType = t, MessageType = i.GetGenericArguments()[0] })
            )
            .ToList();

        foreach (var entry in consumerEntries)
        {
            var topicName = ToKebabCase(entry.MessageType.Name);
            var configuratorType = typeof(TopicEndpointConfigurator<,>).MakeGenericType(
                entry.MessageType,
                entry.ConsumerType
            );
            var configurator = (ITopicEndpointConfigurator)
                Activator.CreateInstance(configuratorType)!;
            configurator.Configure(kafka, context, topicName, consumerGroup);
        }
    }

    private static HashSet<Type> DiscoverMessageTypes(Assembly assembly)
    {
        var integrationEventType = typeof(IntegrationEvent);
        var consumerInterface = typeof(IConsumer<>);
        var messageTypes = new HashSet<Type>();

        foreach (var type in assembly.GetTypes())
        {
            if (
                !type.IsAbstract
                && !type.IsInterface
                && integrationEventType.IsAssignableFrom(type)
            )
            {
                messageTypes.Add(type);
            }

            if (type.IsAbstract || type.IsInterface || !type.IsClass)
            {
                continue;
            }

            foreach (var iface in type.GetInterfaces())
            {
                if (
                    iface.IsGenericType
                    && iface.GetGenericTypeDefinition() == consumerInterface
                )
                {
                    messageTypes.Add(iface.GetGenericArguments()[0]);
                }
            }
        }

        return messageTypes;
    }

    private interface IProducerRegistrar
    {
        void Register(IRiderRegistrationConfigurator rider, string topicName);
    }

    private sealed class ProducerRegistrar<TMessage> : IProducerRegistrar
        where TMessage : class
    {
        public void Register(IRiderRegistrationConfigurator rider, string topicName)
        {
            rider.AddProducer<TMessage>(topicName);
        }
    }

    private interface ITopicEndpointConfigurator
    {
        void Configure(
            IKafkaFactoryConfigurator kafka,
            IRiderRegistrationContext context,
            string topicName,
            string consumerGroup
        );
    }

    private sealed class TopicEndpointConfigurator<TMessage, TConsumer>
        : ITopicEndpointConfigurator
        where TMessage : class
        where TConsumer : class, IConsumer<TMessage>
    {
        public void Configure(
            IKafkaFactoryConfigurator kafka,
            IRiderRegistrationContext context,
            string topicName,
            string consumerGroup
        )
        {
            kafka.TopicEndpoint<TMessage>(
                topicName,
                consumerGroup,
                e =>
                {
                    e.ConfigureConsumer<TConsumer>(context);
                }
            );
        }
    }
}
