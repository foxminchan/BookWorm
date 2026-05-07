using BookWorm.Chassis.Utilities.Configurations;

namespace BookWorm.Finance.Saga;

[ExcludeFromCodeCoverage]
internal static class SagaExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        public void AddSagaStateMachineServices()
        {
            builder.Configure<OrderStateMachineSettings>(
                OrderStateMachineSettings.ConfigurationSection
            );
        }
    }
}
