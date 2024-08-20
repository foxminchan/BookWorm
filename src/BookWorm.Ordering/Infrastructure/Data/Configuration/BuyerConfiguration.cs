using BookWorm.Ordering.Domain.BuyerAggregate;

namespace BookWorm.Ordering.Infrastructure.Data.Configuration;

internal sealed class BuyerConfiguration : BaseConfiguration<Buyer>
{
    public override void Configure(EntityTypeBuilder<Buyer> builder)
    {
        base.Configure(builder);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(DataSchemaLength.Small);

        builder.OwnsOne(p => p.Address, e =>
        {
            e.WithOwner();

            e.Property(c => c.Street)
                .HasMaxLength(DataSchemaLength.Medium);

            e.Property(c => c.City)
                .HasMaxLength(DataSchemaLength.Medium);

            e.Property(c => c.Province)
                .HasMaxLength(DataSchemaLength.Medium);
        }).UsePropertyAccessMode(PropertyAccessMode.Property);

        builder.HasMany(e => e.Orders)
            .WithOne(e => e.Buyer)
            .HasForeignKey(e => e.BuyerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Orders)
            .AutoInclude();
    }
}
