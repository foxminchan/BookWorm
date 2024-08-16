using BookWorm.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookWorm.Ordering.Infrastructure.Data.Configuration;

public sealed class OrderConfiguration : BaseConfiguration<Order>
{
    public override void Configure(EntityTypeBuilder<Order> builder)
    {
        base.Configure(builder);

        builder.Property(e => e.BuyerId)
            .IsRequired();

        builder.Property(e => e.Note)
            .HasMaxLength(DataSchemaLength.SuperLarge);

        builder.HasMany(e => e.OrderItems)
            .WithOne(e => e.Order)
            .HasForeignKey(e => e.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.OrderItems)
            .AutoInclude();
    }
}
