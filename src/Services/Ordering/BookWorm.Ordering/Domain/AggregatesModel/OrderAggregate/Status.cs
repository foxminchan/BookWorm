namespace BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Status : byte
{
    New = 0,
    Cancelled = 1,
    Completed = 2,
}
