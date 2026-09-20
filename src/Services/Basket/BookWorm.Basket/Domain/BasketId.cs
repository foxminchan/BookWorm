using Vogen;

namespace BookWorm.Basket.Domain;

[ValueObject<Guid>(Conversions.SystemTextJson | Conversions.EfCoreValueConverter)]
public readonly partial record struct BasketId;
