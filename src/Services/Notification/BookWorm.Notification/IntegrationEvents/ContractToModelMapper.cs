using BookWorm.Notification.Domain.Models;

namespace BookWorm.Notification.IntegrationEvents;

internal static class ContractToModelMapper
{
    private const string Customer = nameof(Customer);

    extension(CancelOrderCommand command)
    {
        public Order ToOrder()
        {
            return new(
                command.OrderId,
                command.FullName ?? Customer,
                command.TotalMoney,
                Status.Canceled
            );
        }
    }

    extension(PlaceOrderCommand command)
    {
        public Order ToOrder()
        {
            return new(
                command.OrderId,
                command.FullName ?? Customer,
                command.TotalMoney,
                Status.New
            );
        }
    }

    extension(CompleteOrderCommand command)
    {
        public Order ToOrder()
        {
            return new(
                command.OrderId,
                command.FullName ?? Customer,
                command.TotalMoney,
                Status.Completed
            );
        }
    }
}
