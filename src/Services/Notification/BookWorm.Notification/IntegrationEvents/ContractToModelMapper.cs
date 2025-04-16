using BookWorm.Notification.Domain.Models;

namespace BookWorm.Notification.IntegrationEvents;

public static class ContractToModelMapper
{
    private const string Customer = nameof(Customer);

    public static Order ToOrder(this CancelOrderCommand command)
    {
        return new()
        {
            Id = command.OrderId,
            FullName = command.FullName ?? Customer,
            TotalMoney = command.TotalMoney,
            Status = Status.Canceled,
        };
    }

    public static Order ToOrder(this PlaceOrderCommand command)
    {
        return new()
        {
            Id = command.OrderId,
            FullName = command.FullName ?? Customer,
            TotalMoney = command.TotalMoney,
            Status = Status.New,
        };
    }

    public static Order ToOrder(this CompleteOrderCommand command)
    {
        return new()
        {
            Id = command.OrderId,
            FullName = command.FullName ?? Customer,
            TotalMoney = command.TotalMoney,
            Status = Status.Completed,
        };
    }
}
