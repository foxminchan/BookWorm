namespace BookWorm.Catalog.Domain.AggregatesModel.BookAggregate;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Status : byte
{
    [Description("In Stock")]
    InStock = 0,

    [Description("Out of Stock")]
    OutOfStock = 1,
}
