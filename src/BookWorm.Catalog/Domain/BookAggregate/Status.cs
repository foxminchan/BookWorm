namespace BookWorm.Catalog.Domain.BookAggregate;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Status : byte
{
    InStock = 1,
    OutOfStock = 2,
    ComingSoon = 3
}
