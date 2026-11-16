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

            builder.Configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["KafkaTopics:UserCheckedOut"] =
                        KafkaTopicRouter.CreateTopicName<UserCheckedOutIntegrationEvent>(env),
                    ["KafkaTopics:BasketDeletedComplete"] =
                        KafkaTopicRouter.CreateTopicName<BasketDeletedCompleteIntegrationEvent>(
                            env
                        ),
                    ["KafkaTopics:BasketDeletedFailed"] =
                        KafkaTopicRouter.CreateTopicName<BasketDeletedFailedIntegrationEvent>(env),
                    ["KafkaTopics:OrderStatusChangedToComplete"] =
                        KafkaTopicRouter.CreateTopicName<OrderStatusChangedToCompleteIntegrationEvent>(
                            env
                        ),
                    ["KafkaTopics:OrderStatusChangedToCancel"] =
                        KafkaTopicRouter.CreateTopicName<OrderStatusChangedToCancelIntegrationEvent>(
                            env
                        ),
                }
            );

            builder.AddEventBus<IFinanceApiMarker>(opts =>
                opts.MessagePartitioning.ByPropertyNamed("OrderId")
            );

            builder.Configure<OrderStateMachineSettings>(
                OrderStateMachineSettings.ConfigurationSection
            );
        }
    }
}
