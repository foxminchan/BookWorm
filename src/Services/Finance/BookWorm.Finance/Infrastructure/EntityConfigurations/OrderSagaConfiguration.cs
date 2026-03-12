using BookWorm.Constants.Core;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookWorm.Finance.Infrastructure.EntityConfigurations;

internal sealed class OrderSagaConfiguration : IEntityTypeConfiguration<OrderSaga>
{
    public void Configure(EntityTypeBuilder<OrderSaga> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(p => p.BasketId).IsRequired();
        builder.Property(p => p.TotalMoney).HasPrecision(18, 2);
        builder.Property(p => p.FullName).HasMaxLength(DataSchemaLength.Large);
        builder.Property(p => p.Email).HasMaxLength(DataSchemaLength.ExtraLarge);
        builder.Property(p => p.CurrentState).IsRequired().HasMaxLength(DataSchemaLength.Medium);
    }
}
