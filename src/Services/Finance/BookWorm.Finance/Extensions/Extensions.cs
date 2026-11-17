using BookWorm.Chassis.Utilities.Configurations;
using BookWorm.Contracts;

namespace BookWorm.Finance.Extensions;

internal static class Extensions
{
    extension(IHostApplicationBuilder builder)
    {
        public void AddApplicationServices()
        {
            var env = builder.Environment;

            builder.Configuration.AddInMemoryCollection([
                new(
                    "KafkaTopics:UserCheckedOut",
                    KafkaTopicRouter.CreateTopicName<UserCheckedOutIntegrationEvent>(env)
                ),
                new(
                    "KafkaTopics:BasketDeletedComplete",
                    KafkaTopicRouter.CreateTopicName<BasketDeletedCompleteIntegrationEvent>(env)
                ),
                new(
                    "KafkaTopics:BasketDeletedFailed",
                    KafkaTopicRouter.CreateTopicName<BasketDeletedFailedIntegrationEvent>(env)
                ),
                new(
                    "KafkaTopics:OrderStatusChangedToComplete",
                    KafkaTopicRouter.CreateTopicName<OrderStatusChangedToCompleteIntegrationEvent>(
                        env
                    )
                ),
                new(
                    "KafkaTopics:OrderStatusChangedToCancel",
                    KafkaTopicRouter.CreateTopicName<OrderStatusChangedToCancelIntegrationEvent>(
                        env
                    )
                ),
            ]);

            builder.AddEventBus<IFinanceApiMarker>(opts =>
                opts.MessagePartitioning.ByPropertyNamed("OrderId")
            );

            builder.Configure<OrderStateMachineSettings>(
                OrderStateMachineSettings.ConfigurationSection
            );
        }
    }
}
