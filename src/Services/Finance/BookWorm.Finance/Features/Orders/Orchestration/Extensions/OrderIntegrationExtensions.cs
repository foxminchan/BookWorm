using BookWorm.Contracts;
using BookWorm.Finance.Domain.AggregatesModel.OrderAggregate;

namespace BookWorm.Finance.Features.Orders.Orchestration.Extensions;

internal static class OrderIntegrationExtensions
{
    extension(Order order)
    {
        public PlaceOrderCommand ToPlaceOrderCommand()
        {
            return new(
                order.BasketId,
                order.Customer.FullName,
                order.Customer.Email,
                order.Id,
                order.TotalMoney.Amount
            );
        }

        public DeleteBasketCompleteCommand ToDeleteBasketCompleteCommand()
        {
            return new(order.Id, order.TotalMoney.Amount);
        }

        public DeleteBasketFailedCommand ToDeleteBasketFailedCommand()
        {
            return new(order.BasketId, order.Customer.Email, order.Id, order.TotalMoney.Amount);
        }

        public CompleteOrderCommand ToCompleteOrderCommand()
        {
            return new(
                order.Id,
                order.Customer.FullName!,
                order.Customer.Email!,
                order.TotalMoney.Amount
            );
        }

        public CancelOrderCommand ToCancelOrderCommand()
        {
            return new(
                order.Id,
                order.Customer.FullName!,
                order.Customer.Email!,
                order.TotalMoney.Amount
            );
        }

        public OrderOrchestrationResult ToOrchestrationResult()
        {
            return new(
                order.Id,
                order.BasketId,
                order.Customer.FullName,
                order.Customer.Email,
                order.TotalMoney.Amount,
                order.Status switch
                {
                    OrderStatus.Placed => OrderOrchestrationStatus.Placed,
                    OrderStatus.BasketDeletionFailed =>
                        OrderOrchestrationStatus.BasketDeletionFailed,
                    OrderStatus.Completed => OrderOrchestrationStatus.Completed,
                    OrderStatus.Cancelled => OrderOrchestrationStatus.Cancelled,
                    OrderStatus.Failed => OrderOrchestrationStatus.Failed,
                    _ => throw new ArgumentOutOfRangeException(
                        nameof(order),
                        order.Status,
                        "Unsupported order status."
                    ),
                },
                order.TimeoutRetryCount,
                order.OrderPlacedDate,
                order.FailureReason
            );
        }
    }
}
