using BookWorm.Notification.Infrastructure.Attributes;

namespace BookWorm.Notification.Domain.Models;

public sealed record Order(
    Guid Id,
    string FullName,
    [property: Format("C", "en-US")] decimal TotalMoney,
    Status Status
)
{
    [Format("dd/MM/yyyy")]
    public DateOnly CreatedAt { get; init; } = DateOnly.FromDateTime(DateTime.UtcNow);
}
