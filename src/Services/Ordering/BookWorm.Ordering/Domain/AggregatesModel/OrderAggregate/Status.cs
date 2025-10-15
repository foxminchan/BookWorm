namespace BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate;

[Flags, JsonConverter(typeof(JsonStringEnumConverter<Status>))]
public enum Status : byte
{
    [Description("New")]
    New = 0,

    [Description("Cancelled")]
    Cancelled = 1,

    [Description("Completed")]
    Completed = 2,
}
