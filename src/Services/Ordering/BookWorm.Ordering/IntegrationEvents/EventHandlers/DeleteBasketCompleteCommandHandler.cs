using BookWorm.Contracts;
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
        Summary = "Delete basket complete",
        Description = "Represents a domain event that is published when reverse basket is completed"
    )]
    public async Task Consume(ConsumeContext<DeleteBasketCompleteCommand> context)
    {
        await documentSession.Add<OrderSummary>(Guid.CreateVersion7(), context.Message);
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
