using BookWorm.SharedKernel.Helpers;

namespace BookWorm.Ordering.Infrastructure.EntityConfigurations;

internal sealed class BuyerConfiguration : IEntityTypeConfiguration<Buyer>
{
    public void Configure(EntityTypeBuilder<Buyer> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(p => p.Id).HasDefaultValueSql(UniqueIdentifierHelper.NewUuidV7);

        builder.Property(e => e.Name).IsRequired().HasMaxLength(DataSchemaLength.Small);

        builder.ComplexProperty(
            p => p.Address,
            e =>
            {
                e.IsRequired();
                e.UsePropertyAccessMode(PropertyAccessMode.Property);
            }
        );

        builder.Property(p => p.Address!.Street).HasMaxLength(DataSchemaLength.Medium).IsRequired();

        builder.Property(p => p.Address!.City).HasMaxLength(DataSchemaLength.Medium).IsRequired();

        builder
            .Property(p => p.Address!.Province)
            .HasMaxLength(DataSchemaLength.Medium)
            .IsRequired();

        builder
            .HasMany(e => e.Orders)
            .WithOne(e => e.Buyer)
            .HasForeignKey(e => e.BuyerId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
