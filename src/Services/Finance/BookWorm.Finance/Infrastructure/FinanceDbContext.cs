using BookWorm.Finance.Infrastructure.EntityConfigurations;
using MassTransit.EntityFrameworkCoreIntegration;

namespace BookWorm.Finance.Infrastructure;

public sealed class FinanceDbContext(DbContextOptions options) : SagaDbContext(options)
{
    protected override IEnumerable<ISagaClassMap> Configurations
    {
        get { yield return new OrderStateConfiguration(); }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
        foreach (var configuration in Configurations)
        {
            configuration.Configure(modelBuilder);
        }
    }
}
