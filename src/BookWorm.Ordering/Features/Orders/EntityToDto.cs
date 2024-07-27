using BookWorm.Ordering.Domain.OrderAggregate;

namespace BookWorm.Ordering.Features.Orders;

public static class EntityToDto
{
    public static OrderDto ToOrderDto(this Order order)
    {
        return new(order.Id, order.Note, order.Status, order.TotalPrice, order.BuyerId);
    }

    public static List<OrderDto> ToOrderDtos(this IEnumerable<Order> orders)
    {
        return orders.Select(ToOrderDto).ToList();
    }
}
