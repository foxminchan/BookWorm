using Vogen;

namespace BookWorm.Basket.Domain;

[ValueObject<string>(Conversions.SystemTextJson | Conversions.EfCoreValueConverter)]
public readonly partial record struct CustomerId;
