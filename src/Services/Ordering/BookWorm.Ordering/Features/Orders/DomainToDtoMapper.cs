namespace BookWorm.Ordering.Features.Orders;

public static class DomainToDtoMapper
{
    public static OrderDetailDto ToOrderDetailDto(this Order model)
    {
        return new(
            model.Id,
            model.CreatedAt,
            model.TotalPrice,
            [.. model.OrderItems.Select(x => new OrderItemDto(x.Id, x.Quantity, x.Price))]
        );
    }

    private static OrderDto ToOrderDto(this Order model)
    {
        return new(model.Id, model.CreatedAt, model.TotalPrice, model.Status);
    }

    public static IReadOnlyList<OrderDto> ToOrderDtos(this IReadOnlyList<Order> models)
    {
        return [.. models.Select(x => x.ToOrderDto())];
    }
}
