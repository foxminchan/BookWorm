namespace BookWorm.Ordering.Features.Orders;

public sealed record OrderDto(
    Guid Id,
    string? Note,
    Status Status,
    decimal TotalPrice,
    Guid BuyerId);

public sealed record OrderDetailDto(
    Guid Id,
    string? Note,
    Status Status,
    decimal TotalPrice,
    List<OrderItemDto> OrderItems);

public sealed record OrderItemDto(
    Guid BookId,
    string Name,
    int Quantity,
    decimal Price);
