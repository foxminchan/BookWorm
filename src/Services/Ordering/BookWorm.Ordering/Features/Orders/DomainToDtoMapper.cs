namespace BookWorm.Ordering.Features.Orders;

internal static class DomainToDtoMapper
{
    extension(Order model)
    {
        public OrderDetailDto ToOrderDetailDto()
        {
            return new(
                model.Id,
                model.CreatedAt,
                model.TotalPrice,
                model.Status,
                [.. model.OrderItems.Select(x => new OrderItemDto(x.Id, x.Quantity, x.Price))]
            );
        }

        private OrderDto ToOrderDto()
        {
            return new(model.Id, model.CreatedAt, model.TotalPrice, model.Status);
        }
    }

    extension(IReadOnlyList<Order> models)
    {
        public IReadOnlyList<OrderDto> ToOrderDtos()
        {
            return [.. models.Select(x => x.ToOrderDto())];
        }
    }
}
