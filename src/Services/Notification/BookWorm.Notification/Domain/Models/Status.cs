using System.Text.Json.Serialization;

namespace BookWorm.Notification.Domain.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Status : byte
{
    New = 0,
    Completed = 1,
    Canceled = 2,
}
