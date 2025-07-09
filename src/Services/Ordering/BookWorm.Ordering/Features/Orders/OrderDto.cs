namespace BookWorm.Ordering.Features.Orders;

public sealed record OrderDto(Guid Id, DateTime Date, decimal Total, Status Status);

public sealed record OrderDetailDto(
    Guid Id,
    DateTime Date,
    decimal Total,
    Status Status,
    IReadOnlyList<OrderItemDto> Items
);

public sealed record OrderItemDto(Guid Id, int Quantity, decimal Price)
{
    public string? Name { get; set; }
}
