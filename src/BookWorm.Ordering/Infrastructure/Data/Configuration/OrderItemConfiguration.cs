﻿namespace BookWorm.Ordering.Infrastructure.Data.Configuration;

internal sealed class OrderItemConfiguration : BaseConfiguration<OrderItem>
{
    public override void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        base.Configure(builder);

        builder.Property(e => e.BookId).IsRequired();

        builder.Property(e => e.Quantity).IsRequired();

        builder.Property(e => e.Price).IsRequired();
    }
}
