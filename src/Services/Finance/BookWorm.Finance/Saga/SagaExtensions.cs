using BookWorm.Chassis.Utilities.Configurations;
using BookWorm.Constants.Aspire;
using Wolverine.EntityFrameworkCore;
using Wolverine.Postgresql;

namespace BookWorm.Finance.Saga;

[ExcludeFromCodeCoverage]
internal static class SagaExtensions
{
    public static void AddSagaStateMachineServices(this IHostApplicationBuilder builder)
    {
        builder.Configure<OrderStateMachineSettings>(
            OrderStateMachineSettings.ConfigurationSection
        );

        builder.AddEventBus(
            typeof(IFinanceApiMarker),
            options =>
            {
                var connectionString = builder.Configuration.GetRequiredConnectionString(
                    Components.Database.Finance
                );

                options.UseEntityFrameworkCoreTransactions();

                options.PersistMessagesWithPostgresql(connectionString);
            }
        );
    }
}
