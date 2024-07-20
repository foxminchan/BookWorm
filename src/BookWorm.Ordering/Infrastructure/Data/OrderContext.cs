using BookWorm.Ordering.Domain.BuyerAggregate;
using BookWorm.Ordering.Domain.OrderAggregate;
using BookWorm.Shared.Constants;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace BookWorm.Ordering.Infrastructure.Data;

public sealed class OrderContext(DbContextOptions<OrderContext> options) : DbContext(options)
{
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
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderContext).Assembly);
    }
}
