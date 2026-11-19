using BookWorm.Notification.Domain.Attributes;

namespace BookWorm.Notification.Domain.Models;

internal sealed record Order(
    OrderId Id,
    string FullName,
    [property: Format("C")] decimal TotalMoney,
    Status Status
)
{
    [Format("yyyy-MM-dd")]
    public DateOnly CreatedAt { get; init; } = DateOnly.FromDateTime(DateTime.UtcNow);
}
