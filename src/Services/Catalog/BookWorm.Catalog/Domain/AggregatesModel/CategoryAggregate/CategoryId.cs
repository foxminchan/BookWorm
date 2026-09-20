using Vogen;

namespace BookWorm.Catalog.Domain.AggregatesModel.CategoryAggregate;

[ValueObject<Guid>(conversions: Conversions.SystemTextJson | Conversions.EfCoreValueConverter)]
public readonly partial record struct CategoryId;
