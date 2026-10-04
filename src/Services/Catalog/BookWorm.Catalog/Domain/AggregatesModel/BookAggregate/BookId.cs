using Vogen;

namespace BookWorm.Catalog.Domain.AggregatesModel.BookAggregate;

[ValueObject<Guid>(Conversions.SystemTextJson)]
public readonly partial record struct BookId;
