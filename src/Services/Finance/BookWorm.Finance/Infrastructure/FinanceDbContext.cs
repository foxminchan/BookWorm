using Wolverine.EntityFrameworkCore;

namespace BookWorm.Finance.Infrastructure;

public sealed class FinanceDbContext(DbContextOptions<FinanceDbContext> options)
    : DbContext(options)
{
    public DbSet<OrderSaga> OrderSagas => Set<OrderSaga>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.MapWolverineEnvelopeStorage();
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FinanceDbContext).Assembly);
    }
}
