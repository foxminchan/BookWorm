using BookWorm.Notification.Infrastructure.Attributes;

namespace BookWorm.Notification.Domain.Models;

public sealed record Order
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = string.Empty;

    [Format("C", "en-US")]
    public decimal TotalMoney { get; init; }

    public Status Status { get; init; }

    [Format("dd/MM/yyyy")]
    public DateOnly CreatedAt { get; init; } = DateOnly.FromDateTime(DateTime.UtcNow);
}
