using BookWorm.Contracts;
using Microsoft.Azure.Functions.Worker;

namespace BookWorm.Finance.Features.Orders.OrderCompletion;

public sealed class PublishCompleteOrderActivity(IMessageBus bus)
{
    [Function(nameof(PublishCompleteOrderActivity))]
    public Task RunAsync([ActivityTrigger] CompleteOrderCommand command)
    {
        return bus.PublishAsync(command).AsTask();
    }
}
