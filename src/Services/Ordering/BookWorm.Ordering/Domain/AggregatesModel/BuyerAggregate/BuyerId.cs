using Vogen;

namespace BookWorm.Ordering.Domain.AggregatesModel.BuyerAggregate;

[ValueObject<Guid>(Conversions.SystemTextJson | Conversions.EfCoreValueConverter)]
public readonly partial record struct BuyerId;
