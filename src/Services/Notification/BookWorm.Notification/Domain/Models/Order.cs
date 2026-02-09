using BookWorm.Notification.Infrastructure.Attributes;
using BookWorm.SharedKernel.Helpers;

namespace BookWorm.Notification.Domain.Models;

public sealed record Order(
    Guid Id,
    string FullName,
    [property: Format("C", "en-US")] decimal TotalMoney,
    Status Status
)
{
    [Format("yyyy-MM-dd")]
    public DateOnly CreatedAt { get; init; } = DateOnly.FromDateTime(DateTimeHelper.UtcNow());
}
