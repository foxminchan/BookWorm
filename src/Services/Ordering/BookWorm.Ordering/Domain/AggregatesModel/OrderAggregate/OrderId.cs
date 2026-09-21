using Vogen;

namespace BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate;

[ValueObject<Guid>(Conversions.SystemTextJson | Conversions.EfCoreValueConverter)]
public readonly partial record struct OrderId;
