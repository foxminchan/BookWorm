using Vogen;

namespace BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate;

[ValueObject<Guid>(conversions: Conversions.SystemTextJson | Conversions.EfCoreValueConverter)]
public readonly partial record struct OrderId;
