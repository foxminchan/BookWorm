using BookWorm.Contracts;
using Microsoft.Azure.Functions.Worker;

namespace BookWorm.Finance.Features.Orders.BasketDeletion;

public sealed class PublishDeleteBasketFailedActivity(IMessageBus bus)
{
    [Function(nameof(PublishDeleteBasketFailedActivity))]
    public Task RunAsync([ActivityTrigger] DeleteBasketFailedCommand command)
    {
        return bus.PublishAsync(command).AsTask();
    }
}
