using Vogen;

namespace BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate;

[ValueObject<Guid>(Conversions.SystemTextJson)]
public readonly partial record struct OrderId;
