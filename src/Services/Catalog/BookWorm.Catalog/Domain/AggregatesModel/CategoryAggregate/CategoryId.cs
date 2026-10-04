using Vogen;

namespace BookWorm.Catalog.Domain.AggregatesModel.CategoryAggregate;

[ValueObject<Guid>(Conversions.SystemTextJson)]
public readonly partial record struct CategoryId;
