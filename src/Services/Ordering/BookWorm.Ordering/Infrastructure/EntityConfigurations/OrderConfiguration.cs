namespace BookWorm.Ordering.Infrastructure.EntityConfigurations;

internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.BuyerId).IsRequired();

        builder.Property(e => e.Status).IsRequired();

        builder.Property(e => e.Note).HasMaxLength(DataSchemaLength.SuperLarge);

        builder.Property(e => e.CreatedAt).HasDefaultValue(DateTime.UtcNow);

        builder.Property(e => e.LastModifiedAt).HasDefaultValue(DateTime.UtcNow);

        builder.Property(e => e.Version).IsConcurrencyToken();

        builder
            .HasMany(e => e.OrderItems)
            .WithOne(e => e.Order)
            .HasForeignKey(e => e.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.OrderItems).AutoInclude();

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
