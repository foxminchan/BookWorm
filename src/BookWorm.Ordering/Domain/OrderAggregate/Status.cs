using System.Text.Json.Serialization;

namespace BookWorm.Ordering.Domain.OrderAggregate;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Status : byte
{
    Pending = 0,
    Delivered = 1,
    Canceled = 2
}
