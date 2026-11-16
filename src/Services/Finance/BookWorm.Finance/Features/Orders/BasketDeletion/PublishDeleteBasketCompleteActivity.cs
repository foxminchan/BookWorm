using BookWorm.Contracts;
using Microsoft.Azure.Functions.Worker;

namespace BookWorm.Finance.Features.Orders.BasketDeletion;

public sealed class PublishDeleteBasketCompleteActivity(IMessageBus bus)
{
    [Function(nameof(PublishDeleteBasketCompleteActivity))]
    public Task RunAsync([ActivityTrigger] DeleteBasketCompleteCommand command)
    {
        return bus.PublishAsync(command).AsTask();
    }
}
