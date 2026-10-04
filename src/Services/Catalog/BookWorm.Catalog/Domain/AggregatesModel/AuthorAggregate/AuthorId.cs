using Vogen;

namespace BookWorm.Catalog.Domain.AggregatesModel.AuthorAggregate;

[ValueObject<Guid>(Conversions.SystemTextJson)]
public readonly partial record struct AuthorId;
