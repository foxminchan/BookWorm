using BookWorm.Constants.Core;
using BookWorm.Finance.Saga;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookWorm.Finance.Infrastructure.EntityConfigurations;

internal sealed class OrderSagaConfiguration : IEntityTypeConfiguration<OrderSaga>
{
    public void Configure(EntityTypeBuilder<OrderSaga> entity)
    {
        entity.HasKey(p => p.Id);

        entity.Property(p => p.OrderId).IsRequired();
        entity.Property(p => p.BasketId).IsRequired();
        entity.Property(p => p.TotalMoney).HasPrecision(18, 2);
        entity.Property(p => p.Email).HasMaxLength(DataSchemaLength.ExtraLarge);
        entity
            .Property(x => x.RowVersion)
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .IsRowVersion();
        entity.Property(p => p.CurrentState).HasConversion<string>();
    }
}
