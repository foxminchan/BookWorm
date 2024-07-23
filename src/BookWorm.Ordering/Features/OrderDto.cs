using BookWorm.Ordering.Domain.OrderAggregate;

namespace BookWorm.Ordering.Features;

public sealed record UserOrderDto(
    BuyerDto Buyer,
    List<OrderDto>? Orders);

public sealed record UserOrderDetailDto(
    BuyerDto Buyer,
    OrderDto Order,
    List<OrderItemDto>? OrderItems);

public sealed record OrderDto(
    Guid Id,
    string? Note,
    Status Status,
    decimal TotalPrice);

public sealed record BuyerDto(
    Guid Id,
    string? Name,
    string? Street,
    string? City,
    string? Province);

public sealed record OrderItemDto(
    Guid BookId,
    string Name,
    int Quantity,
    decimal Price);
