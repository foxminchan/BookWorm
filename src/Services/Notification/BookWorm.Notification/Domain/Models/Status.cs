using System.ComponentModel;
using System.Text.Json.Serialization;

namespace BookWorm.Notification.Domain.Models;

[JsonConverter(typeof(JsonStringEnumConverter<Status>))]
public enum Status : byte
{
    [Description("New")]
    New = 1,

    [Description("Completed")]
    Completed = 2,

    [Description("Canceled")]
    Canceled = 3,
}
