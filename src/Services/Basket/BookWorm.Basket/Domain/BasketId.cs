using Vogen;

namespace BookWorm.Basket.Domain;

[ValueObject<Guid>(conversions: Conversions.SystemTextJson | Conversions.EfCoreValueConverter)]
public readonly partial record struct BasketId;
