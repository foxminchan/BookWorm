namespace BookWorm.Catalog.Domain.AggregatesModel.BookAggregate;

public sealed record BookSummary(
    BookId Id,
    string? Name,
    decimal? OriginalPrice,
    decimal? DiscountPrice,
    Status Status
);
