namespace BookWorm.Ordering.Infrastructure.EntityConfigurations;

internal sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.BookId).IsRequired();

        builder.Property(e => e.OrderId).IsRequired();

        builder.Property(e => e.Quantity).IsRequired();

        builder.Property(e => e.Price).IsRequired();

        builder.HasIndex(e => new { e.BookId, e.OrderId }).IsUnique();

        builder.HasQueryFilter(e => !e.Order!.IsDeleted);
    }
}
