using BookWorm.Contracts;
using Microsoft.Azure.Functions.Worker;

namespace BookWorm.Finance.Features.Orders.PlaceOrder;

public sealed class PublishPlaceOrderActivity(IMessageBus bus)
{
    [Function(nameof(PublishPlaceOrderActivity))]
    public Task RunAsync([ActivityTrigger] PlaceOrderCommand command)
    {
        return bus.PublishAsync(command).AsTask();
    }
}
