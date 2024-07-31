using Ardalis.GuardClauses;
using BookWorm.Ordering.Domain.BuyerAggregate;
using BookWorm.Ordering.Domain.OrderAggregate;
using BookWorm.Ordering.Infrastructure.Mediator;
using BookWorm.Shared.Constants;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookWorm.Ordering.Infrastructure.Data;

public sealed class OrderingContext(DbContextOptions<OrderingContext> options, IPublisher publisher)
    : DbContext(options)
{
    private readonly IPublisher _publisher = Guard.Against.Null(publisher);
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Buyer> Buyers => Set<Buyer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
        modelBuilder.HasPostgresExtension(UniqueType.Extension);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderingContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new())
    {
        await _publisher.DispatchDomainEventsAsync(this);

        return await base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        _publisher.DispatchDomainEventsAsync(this).GetAwaiter().GetResult();

        return base.SaveChanges();
    }
}
