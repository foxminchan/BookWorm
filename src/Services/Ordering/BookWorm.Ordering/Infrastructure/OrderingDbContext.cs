using BookWorm.SharedKernel.Mediator;

namespace BookWorm.Ordering.Infrastructure;

public sealed class OrderingDbContext : DbContext, IUnitOfWork
{
    private readonly IPublisher _publisher;

    public OrderingDbContext(DbContextOptions<OrderingDbContext> options, IPublisher publisher)
        : base(options)
    {
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        Debug.WriteLine($"OrderingDbContext::ctor -> {GetHashCode()}");
    }

    public DbSet<Buyer> Buyers => Set<Buyer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        await _publisher.DispatchDomainEventsAsync(this);
        await SaveChangesAsync(cancellationToken);
        return true;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderingDbContext).Assembly);
    }
}
