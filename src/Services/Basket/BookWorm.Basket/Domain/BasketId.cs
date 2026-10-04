using Vogen;

namespace BookWorm.Basket.Domain;

[ValueObject<Guid>(Conversions.SystemTextJson)]
public readonly partial record struct BasketId;
