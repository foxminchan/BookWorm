using Vogen;

namespace BookWorm.Ordering.Domain.AggregatesModel.BuyerAggregate;

[ValueObject<Guid>(Conversions.SystemTextJson)]
public readonly partial record struct BuyerId;
