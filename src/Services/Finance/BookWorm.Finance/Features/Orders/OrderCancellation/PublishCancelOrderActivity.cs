using BookWorm.Contracts;
using Microsoft.Azure.Functions.Worker;

namespace BookWorm.Finance.Features.Orders.OrderCancellation;

public sealed class PublishCancelOrderActivity(IMessageBus bus)
{
    [Function(nameof(PublishCancelOrderActivity))]
    public Task RunAsync([ActivityTrigger] CancelOrderCommand command)
    {
        return bus.PublishAsync(command).AsTask();
    }
}
