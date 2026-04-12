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
    private static readonly KebabCaseEndpointNameFormatter _formatter = new(false);

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

    private static List<ConsumerEntry> DiscoverConsumerEntries(Assembly assembly)
    {
        var consumerInterface = typeof(IConsumer<>);

        return
        [
            .. assembly
                .GetTypes()
                .Where(t => t is { IsAbstract: false, IsInterface: false, IsClass: true })
                .SelectMany(t =>
                    t.GetInterfaces()
                        .Where(i =>
                            i.IsGenericType && i.GetGenericTypeDefinition() == consumerInterface
                        )
                        .Select(i => new ConsumerEntry(t, i.GetGenericArguments()[0]))
                ),
        ];
    }

    private static void RegisterKafkaConsumers(
        IRiderRegistrationConfigurator rider,
        List<ConsumerEntry> consumerEntries
    )
    {
        foreach (
            var registrar in consumerEntries
                .Select(entry => typeof(ConsumerRegistrar<>).MakeGenericType(entry.ConsumerType))
                .Select(registrarType =>
                    (IConsumerRegistrar)Activator.CreateInstance(registrarType)!
                )
        )
        {
            registrar.Register(rider);
        }
    }

    private static void RegisterKafkaProducers(
        IRiderRegistrationConfigurator rider,
        Assembly assembly
    )
    {
        var messageTypes = DiscoverMessageTypes(assembly);

        foreach (var messageType in messageTypes)
        {
            var topicName = _formatter.SanitizeName(messageType.Name);
            var registrarType = typeof(ProducerRegistrar<>).MakeGenericType(messageType);
            var registrar = (IProducerRegistrar)Activator.CreateInstance(registrarType)!;
            registrar.Register(rider, topicName);
        }
    }

    private static void ConfigureKafkaTopicEndpoints(
        IKafkaFactoryConfigurator kafka,
        IRiderRegistrationContext context,
        Type assemblyMarker,
        List<ConsumerEntry> consumerEntries
    )
    {
        var consumerGroup = _formatter.SanitizeName(assemblyMarker.Assembly.GetName().Name!);

        foreach (var entry in consumerEntries)
        {
            var topicName = _formatter.SanitizeName(entry.MessageType.Name);
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
                type is { IsAbstract: false, IsInterface: false }
                && integrationEventType.IsAssignableFrom(type)
            )
            {
                messageTypes.Add(type);
            }

            if (type.IsAbstract || type.IsInterface || !type.IsClass)
            {
                continue;
            }

            foreach (var face in type.GetInterfaces())
            {
                if (face.IsGenericType && face.GetGenericTypeDefinition() == consumerInterface)
                {
                    messageTypes.Add(face.GetGenericArguments()[0]);
                }
            }
        }

        return messageTypes;
    }

    extension(IHostApplicationBuilder builder)
    {
        /// <summary>
        ///     Registers and configures the event bus infrastructure for the current host.
        /// </summary>
        /// <param name="type">
        ///     A marker type used to discover consumers, producers, and activities from its assembly.
        /// </param>
        /// <param name="busConfigure">
        ///     Optional callback for additional <see cref="IBusRegistrationConfigurator" /> customization.
        /// </param>
        /// <remarks>
        ///     This method configures an in-memory MassTransit transport for local pipeline behavior
        ///     and a Kafka rider for broker-based messaging. If the broker connection string is not configured,
        ///     registration is skipped.
        /// </remarks>
        public void AddEventBus(
            Type type,
            Action<IBusRegistrationConfigurator>? busConfigure = null
        )
        {
            // Resolve broker connection details from configuration.
            var connectionString = builder.Configuration.GetConnectionString(Components.Broker);

            // Skip event bus registration when no broker is configured.
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return;
            }

            builder.Services.AddMassTransit(config =>
            {
                // Enforce kebab-case endpoint naming consistency across message endpoints.
                config.SetKebabCaseEndpointNameFormatter();

                // Auto-register MassTransit activities from the target assembly.
                config.AddActivities(type.Assembly);

                config.UsingInMemory(
                    (context, configurator) =>
                    {
                        // Use CloudEvents envelopes for interoperability.
                        configurator.UseCloudEvents();

                        // Auto-configure discovered consumer endpoints.
                        configurator.ConfigureEndpoints(context);

                        // Apply shared retry policy and ignore validation exceptions.
                        configurator.UseMessageRetry(AddRetryConfiguration);

                        // Enable delayed scheduling for deferred message delivery.
                        configurator.UseDelayedMessageScheduler();

                        // Redirect publish operations through Kafka via publish filter.
                        configurator.UsePublishFilter(typeof(KafkaPublishFilter<>), context);
                    }
                );

                config.AddRider(rider =>
                {
                    // Discover consumers once and reuse the result for registration and endpoint setup.
                    var consumerEntries = DiscoverConsumerEntries(type.Assembly);

                    // Register all discovered Kafka consumers and producers.
                    RegisterKafkaConsumers(rider, consumerEntries);
                    RegisterKafkaProducers(rider, type.Assembly);

                    rider.UsingKafka(
                        (context, k) =>
                        {
                            // Configure Kafka host and serialization strategy.
                            k.Host(connectionString);
                            k.SetSerializationFactory(new CloudEventKafkaSerializerFactory());

                            // Create topic endpoints and bind each consumer to its message topic.
                            ConfigureKafkaTopicEndpoints(k, context, type, consumerEntries);
                        }
                    );
                });

                // Allow callers to append custom MassTransit configuration.
                busConfigure?.Invoke(config);
            });

            // Register MassTransit diagnostic meter/source for OpenTelemetry metrics and traces.
            builder
                .Services.AddOpenTelemetry()
                .WithMetrics(b => b.AddMeter(DiagnosticHeaders.DefaultListenerName))
                .WithTracing(p => p.AddSource(DiagnosticHeaders.DefaultListenerName));
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

    private sealed record ConsumerEntry(Type ConsumerType, Type MessageType);

    private interface IConsumerRegistrar
    {
        void Register(IRiderRegistrationConfigurator rider);
    }

    private sealed class ConsumerRegistrar<TConsumer> : IConsumerRegistrar
        where TConsumer : class, IConsumer
    {
        public void Register(IRiderRegistrationConfigurator rider)
        {
            rider.AddConsumer<TConsumer>();
        }
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

    private sealed class TopicEndpointConfigurator<TMessage, TConsumer> : ITopicEndpointConfigurator
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
                    e.CreateIfMissing();
                    e.ConfigureConsumer<TConsumer>(context);
                }
            );
        }
    }
}
