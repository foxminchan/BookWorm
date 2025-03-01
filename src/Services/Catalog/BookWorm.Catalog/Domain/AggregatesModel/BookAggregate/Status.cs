namespace BookWorm.Catalog.Domain.AggregatesModel.BookAggregate;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Status : byte
{
    InStock = 0,
    OutOfStock = 1,
}
