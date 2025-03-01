namespace BookWorm.Ordering.Features.Orders;

public sealed record OrderDto(Guid OrderId, DateTime OrderDate, decimal Total, Status Status);

public sealed record OrderDetailDto(
    Guid OrderId,
    DateTime OrderDate,
    decimal Total,
    IReadOnlyList<OrderItemDto> OrderItems
);

public sealed record OrderItemDto(Guid Id, int Quantity, decimal Price)
{
    public string? Name { get; set; }
}
