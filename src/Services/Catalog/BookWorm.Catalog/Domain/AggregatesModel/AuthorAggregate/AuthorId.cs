using Vogen;

namespace BookWorm.Catalog.Domain.AggregatesModel.AuthorAggregate;

[ValueObject<Guid>(Conversions.SystemTextJson | Conversions.EfCoreValueConverter)]
public readonly partial record struct AuthorId;
