using Vogen;

namespace BookWorm.Catalog.Domain.AggregatesModel.AuthorAggregate;

[ValueObject<Guid>(conversions: Conversions.SystemTextJson | Conversions.EfCoreValueConverter)]
public readonly partial record struct AuthorId;
