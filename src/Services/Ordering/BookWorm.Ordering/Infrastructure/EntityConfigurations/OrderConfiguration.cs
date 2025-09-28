using BookWorm.SharedKernel.Helpers;

namespace BookWorm.Ordering.Infrastructure.EntityConfigurations;

internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(p => p.Id).HasDefaultValueSql(UniqueIdentifierHelper.NewUuidV7);

        builder.Property(e => e.BuyerId).IsRequired();

        builder.Property(e => e.Status).IsRequired();

        builder.Property(e => e.Note).HasMaxLength(DataSchemaLength.SuperLarge);

        builder.Property(e => e.CreatedAt).HasDefaultValueSql(DateTimeHelper.SqlUtcNow);

        builder.Property(e => e.LastModifiedAt).HasDefaultValueSql(DateTimeHelper.SqlUtcNow);

        builder.Property(e => e.RowVersion).IsRowVersion();

        builder
            .HasMany(e => e.OrderItems)
            .WithOne(e => e.Order)
            .HasForeignKey(e => e.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.OrderItems).AutoInclude();

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
