using BookWorm.Contracts;
using BookWorm.Finance.Features.Orders.BasketDeletion;
using BookWorm.Finance.Features.Orders.OrderCancellation;
using BookWorm.Finance.Features.Orders.OrderCompletion;
using BookWorm.Finance.Features.Orders.PlaceOrder;
using Wolverine;

namespace BookWorm.Finance.UnitTests;

public sealed class OrderActivitiesTests
{
    private readonly Mock<IMessageBus> _busMock = new();

    [Test]
    public async Task GivenPlaceOrderCommand_WhenExecutingActivity_ThenShouldPublishToBus()
    {
        var command = new PlaceOrderCommand(
            Guid.CreateVersion7(),
            "John Doe",
            "john@example.com",
            Guid.CreateVersion7(),
            99.99m
        );

        await new PublishPlaceOrderActivity(_busMock.Object).RunAsync(command);

        _busMock.Verify(b => b.PublishAsync(command, null), Times.Once);
    }

    [Test]
    public async Task GivenDeleteBasketCompleteCommand_WhenExecutingActivity_ThenShouldPublishToBus()
    {
        var command = new DeleteBasketCompleteCommand(Guid.CreateVersion7(), 75.25m);

        await new PublishDeleteBasketCompleteActivity(_busMock.Object).RunAsync(command);

        _busMock.Verify(b => b.PublishAsync(command, null), Times.Once);
    }

    [Test]
    public async Task GivenDeleteBasketFailedCommand_WhenExecutingActivity_ThenShouldPublishToBus()
    {
        var command = new DeleteBasketFailedCommand(
            Guid.CreateVersion7(),
            "failed@example.com",
            Guid.CreateVersion7(),
            120.00m
        );

        await new PublishDeleteBasketFailedActivity(_busMock.Object).RunAsync(command);

        _busMock.Verify(b => b.PublishAsync(command, null), Times.Once);
    }

    [Test]
    public async Task GivenCompleteOrderCommand_WhenExecutingActivity_ThenShouldPublishToBus()
    {
        var command = new CompleteOrderCommand(
            Guid.CreateVersion7(),
            "Jane Doe",
            "jane@example.com",
            210.00m
        );

        await new PublishCompleteOrderActivity(_busMock.Object).RunAsync(command);

        _busMock.Verify(b => b.PublishAsync(command, null), Times.Once);
    }

    [Test]
    public async Task GivenCancelOrderCommand_WhenExecutingActivity_ThenShouldPublishToBus()
    {
        var command = new CancelOrderCommand(
            Guid.CreateVersion7(),
            "Bob Doe",
            "bob@example.com",
            180.00m
        );

        await new PublishCancelOrderActivity(_busMock.Object).RunAsync(command);

        _busMock.Verify(b => b.PublishAsync(command, null), Times.Once);
    }
}
