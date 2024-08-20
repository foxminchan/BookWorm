namespace BookWorm.Ordering.Domain.OrderAggregate;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Status : byte
{
    Pending = 0,
    Completed = 1,
    Canceled = 2
}
