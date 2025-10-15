namespace BookWorm.Catalog.Domain.AggregatesModel.BookAggregate;

[Flags, JsonConverter(typeof(JsonStringEnumConverter<Status>))]
public enum Status : byte
{
    [Description("In Stock")]
    InStock = 0,

    [Description("Out of Stock")]
    OutOfStock = 1,
}
