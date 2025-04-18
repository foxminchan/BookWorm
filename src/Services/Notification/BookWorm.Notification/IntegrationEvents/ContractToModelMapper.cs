using BookWorm.Notification.Domain.Models;

namespace BookWorm.Notification.IntegrationEvents;

public static class ContractToModelMapper
{
    private const string Customer = nameof(Customer);

    public static Order ToOrder(this CancelOrderCommand command)
    {
        return new(
            command.OrderId,
            command.FullName ?? Customer,
            command.TotalMoney,
            Status.Canceled
        );
    }

    public static Order ToOrder(this PlaceOrderCommand command)
    {
        return new(command.OrderId, command.FullName ?? Customer, command.TotalMoney, Status.New);
    }

    public static Order ToOrder(this CompleteOrderCommand command)
    {
        return new(
            command.OrderId,
            command.FullName ?? Customer,
            command.TotalMoney,
            Status.Completed
        );
    }
}
