namespace BookWorm.Ordering.Features.Orders;

public sealed record OrderDto(OrderId Id, DateTime Date, decimal Total, Status Status);

public sealed record OrderDetailDto(
    OrderId Id,
    DateTime Date,
    decimal Total,
    Status Status,
    IReadOnlyList<OrderItemDto> Items
);

public sealed record OrderItemDto(Guid Id, int Quantity, decimal Price, string? Name = null);
