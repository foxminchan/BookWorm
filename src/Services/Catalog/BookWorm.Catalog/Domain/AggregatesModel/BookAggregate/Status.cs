namespace BookWorm.Catalog.Domain.AggregatesModel.BookAggregate;

[Flags, JsonConverter(typeof(JsonStringEnumConverter<Status>))]
public enum Status : byte
{
    [Description("In Stock")]
    InStock = 1,

    [Description("Out of Stock")]
    OutOfStock = 2,
}
