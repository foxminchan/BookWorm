namespace BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate;

[Flags, JsonConverter(typeof(JsonStringEnumConverter<Status>))]
public enum Status : byte
{
    [Description("New")]
    New = 1,

    [Description("Cancelled")]
    Cancelled = 2,

    [Description("Completed")]
    Completed = 3,
}
