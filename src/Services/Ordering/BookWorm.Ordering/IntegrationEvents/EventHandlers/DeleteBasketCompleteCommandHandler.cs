using BookWorm.Contracts;
using BookWorm.Ordering.Infrastructure.EventStore.DocumentSession;
using Saunter.Attributes;

namespace BookWorm.Ordering.IntegrationEvents.EventHandlers;

[AsyncApi]
public sealed class DeleteBasketCompleteCommandHandler(IDocumentSession documentSession)
    : IConsumer<DeleteBasketCompleteCommand>
{
    [Channel("basket-checkout-complete")]
    [PublishOperation(
        typeof(DeleteBasketCompleteCommand),
        OperationId = nameof(DeleteBasketCompleteCommand),
        Summary = "Delete basket complete"
    )]
    public async Task Consume(ConsumeContext<DeleteBasketCompleteCommand> context)
    {
        await documentSession.GetAndUpdate<OrderSummary>(Guid.CreateVersion7(), context.Message);
    }
}

[ExcludeFromCodeCoverage]
public sealed class DeleteBasketCompleteCommandHandlerDefinition
    : ConsumerDefinition<DeleteBasketCompleteCommandHandler>
{
    public DeleteBasketCompleteCommandHandlerDefinition()
    {
        Endpoint(x => x.Name = "basket-checkout-complete");
        ConcurrentMessageLimit = 1;
    }
}
