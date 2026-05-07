using BookWorm.Finance.Infrastructure.EntityConfigurations;
using BookWorm.Finance.Saga;
using Wolverine.EntityFrameworkCore;

namespace BookWorm.Finance.Infrastructure;

public sealed class FinanceDbContext(DbContextOptions options) : DbContext(options)
{
    internal DbSet<OrderSaga> OrderSagas => Set<OrderSaga>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Wolverine inbox/outbox tables
        modelBuilder.MapWolverineEnvelopeStorage();

        modelBuilder.ApplyConfiguration(new OrderSagaConfiguration());
    }
}
