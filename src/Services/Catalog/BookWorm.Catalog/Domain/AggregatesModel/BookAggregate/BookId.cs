using Vogen;

namespace BookWorm.Catalog.Domain.AggregatesModel.BookAggregate;

[ValueObject<Guid>(conversions: Conversions.SystemTextJson | Conversions.EfCoreValueConverter)]
public readonly partial record struct BookId;
