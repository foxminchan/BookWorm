namespace BookWorm.Ordering.Infrastructure.EntityConfigurations;

internal sealed class BuyerConfiguration : IEntityTypeConfiguration<Buyer>
{
    public void Configure(EntityTypeBuilder<Buyer> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name).IsRequired().HasMaxLength(DataSchemaLength.Small);

        builder
            .OwnsOne(
                p => p.Address,
                e =>
                {
                    e.WithOwner();

                    e.Property(c => c.Street).HasMaxLength(DataSchemaLength.Medium);

                    e.Property(c => c.City).HasMaxLength(DataSchemaLength.Medium);

                    e.Property(c => c.Province).HasMaxLength(DataSchemaLength.Medium);
                }
            )
            .UsePropertyAccessMode(PropertyAccessMode.Property);

        builder
            .HasMany(e => e.Orders)
            .WithOne(e => e.Buyer)
            .HasForeignKey(e => e.BuyerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Navigation(x => x.Orders).AutoInclude();
    }
}
