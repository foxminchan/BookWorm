using BookWorm.Ordering.Domain.BuyerAggregate;
using BookWorm.Ordering.Domain.OrderAggregate;

namespace BookWorm.Ordering.Features;

public static class EntityToDto
{
    public static UserOrderDto ToUserOrderDto(this Buyer buyer)
    {
        return new(buyer.ToBuyerDto(), buyer.Orders.ToOrderDtos());
    }

    public static BuyerDto ToBuyerDto(this Buyer buyer)
    {
        return new(buyer.Id, buyer.Name, buyer.Address?.Street, buyer.Address?.City, buyer.Address?.Province);
    }

    public static OrderDto ToOrderDto(this Order order)
    {
        return new(order.Id, order.Note, order.Status, order.TotalPrice);
    }

    public static List<OrderDto> ToOrderDtos(this IEnumerable<Order> orders)
    {
        return orders.Select(ToOrderDto).ToList();
    }
}
