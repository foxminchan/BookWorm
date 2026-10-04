using Vogen;

namespace BookWorm.Basket.Domain;

[ValueObject<string>(Conversions.SystemTextJson)]
public readonly partial record struct CustomerId;
