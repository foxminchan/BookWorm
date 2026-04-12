using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace BookWorm.Chassis.EventBus.Kafka;

internal sealed class KafkaPublishFilter<T>(IServiceProvider serviceProvider)
    : IFilter<PublishContext<T>>
    where T : class
{
    public async Task Send(PublishContext<T> context, IPipe<PublishContext<T>> next)
    {
        if (typeof(IntegrationEvent).IsAssignableFrom(typeof(T)))
        {
            var producer = serviceProvider.GetService<ITopicProducer<T>>();

            if (producer is not null)
            {
                await producer.Produce(context.Message, context.CancellationToken);
                return;
            }
        }

        await next.Send(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope("kafkaPublish");
    }
}
