using Vogen;

namespace BookWorm.Catalog.Domain.AggregatesModel.CategoryAggregate;

[ValueObject<Guid>(Conversions.SystemTextJson | Conversions.EfCoreValueConverter)]
public readonly partial record struct CategoryId;
