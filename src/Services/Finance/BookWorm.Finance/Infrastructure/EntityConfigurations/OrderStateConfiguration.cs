using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookWorm.Finance.Infrastructure.EntityConfigurations;

internal sealed class OrderStateConfiguration : SagaClassMap<OrderState>
{
    protected override void Configure(EntityTypeBuilder<OrderState> entity, ModelBuilder model)
    {
        entity.Property(p => p.OrderId).IsRequired();
        entity.Property(p => p.BasketId).IsRequired();
        entity.Property(p => p.TotalMoney).HasPrecision(18, 2);
        entity.Property(p => p.Email).HasMaxLength(DataSchemaLength.ExtraLarge);
    }
}
