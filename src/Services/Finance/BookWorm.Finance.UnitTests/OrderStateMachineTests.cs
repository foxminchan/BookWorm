using BookWorm.Contracts;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Saunter.Attributes;

namespace BookWorm.Finance.UnitTests;

public sealed class OrderStateMachineTests
{
    [Test]
    public async Task GivenCancelOrderCommand_WhenPublishingToMessageBus_ThenShouldBePublishedSuccessfully()
    {
        // Arrange
        var orderId = Guid.CreateVersion7();
        const string email = "test@example.com";
        const decimal totalMoney = 123.45m;

        var command = new CancelOrderCommand(orderId, email, totalMoney);

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness()
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        // Act
        await harness.Bus.Publish(command);

        // Assert
        (await harness.Published.Any<CancelOrderCommand>()).ShouldBeTrue();

        var publishedMessage = harness.Published.Select<CancelOrderCommand>().First();
        publishedMessage.Context.Message.OrderId.ShouldBe(orderId);
        publishedMessage.Context.Message.Email.ShouldBe(email);
        publishedMessage.Context.Message.TotalMoney.ShouldBe(totalMoney);

        await harness.Stop();
    }

    [Test]
    public void GivenCancelOrderCommand_WhenCheckingAsyncApiAttributes_ThenShouldHaveCorrectAttributes()
    {
        // Arrange & Act
        var type = typeof(CancelOrderCommand);

        // Assert
        var asyncApiAttribute = type.GetCustomAttributes(typeof(AsyncApiAttribute), false);
        asyncApiAttribute.ShouldNotBeEmpty();

        var channelAttribute = type.GetCustomAttributes(typeof(ChannelAttribute), false);
        channelAttribute.ShouldNotBeEmpty();
        ((ChannelAttribute)channelAttribute.First()).Name.ShouldBe("notification-cancel-order");

        var subscribeOperation = type.GetCustomAttributes(
            typeof(SubscribeOperationAttribute),
            false
        );
        subscribeOperation.ShouldNotBeEmpty();
        var operation = (SubscribeOperationAttribute)subscribeOperation.First();
        operation.OperationId.ShouldBe(nameof(CancelOrderCommand));
        operation.Summary.ShouldBe("Cancel order notification");
    }

    [Test]
    public void GivenCompleteOrderCommand_WhenCheckingAsyncApiAttributes_ThenShouldHaveCorrectAttributes()
    {
        // Arrange & Act
        var type = typeof(CompleteOrderCommand);

        // Assert
        var asyncApiAttribute = type.GetCustomAttributes(typeof(AsyncApiAttribute), false);
        asyncApiAttribute.ShouldNotBeEmpty();

        var channelAttribute = type.GetCustomAttributes(typeof(ChannelAttribute), false);
        channelAttribute.ShouldNotBeEmpty();
        ((ChannelAttribute)channelAttribute.First()).Name.ShouldBe("notification-complete-order");

        var subscribeOperation = type.GetCustomAttributes(
            typeof(SubscribeOperationAttribute),
            false
        );
        subscribeOperation.ShouldNotBeEmpty();
        var operation = (SubscribeOperationAttribute)subscribeOperation.First();
        operation.OperationId.ShouldBe(nameof(CompleteOrderCommand));
        operation.Summary.ShouldBe("Complete order notification");
    }

    [Test]
    public async Task GivenCompleteOrderCommand_WhenPublishingToMessageBus_ThenShouldBePublishedSuccessfully()
    {
        // Arrange
        var orderId = Guid.CreateVersion7();
        const string email = "test@example.com";
        const decimal totalMoney = 123.45m;

        var command = new CompleteOrderCommand(orderId, email, totalMoney);

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness()
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();

        await harness.Start();

        // Act
        await harness.Bus.Publish(command);

        // Assert
        (await harness.Published.Any<CompleteOrderCommand>()).ShouldBeTrue();

        var publishedMessage = harness.Published.Select<CompleteOrderCommand>().First();
        publishedMessage.Context.Message.OrderId.ShouldBe(orderId);
        publishedMessage.Context.Message.Email.ShouldBe(email);
        publishedMessage.Context.Message.TotalMoney.ShouldBe(totalMoney);

        await harness.Stop();
    }

    [Test]
    public void GivenDeleteBasketCompleteCommand_WhenCheckingAsyncApiAttributes_ThenShouldHaveCorrectAttributes()
    {
        // Arrange & Act
        var type = typeof(DeleteBasketCompleteCommand);

        // Assert
        var asyncApiAttribute = type.GetCustomAttributes(typeof(AsyncApiAttribute), false);
        asyncApiAttribute.ShouldNotBeEmpty();

        var channelAttribute = type.GetCustomAttributes(typeof(ChannelAttribute), false);
        channelAttribute.ShouldNotBeEmpty();
        ((ChannelAttribute)channelAttribute.First()).Name.ShouldBe("basket-checkout-complete");

        var subscribeOperation = type.GetCustomAttributes(
            typeof(SubscribeOperationAttribute),
            false
        );
        subscribeOperation.ShouldNotBeEmpty();
        var operation = (SubscribeOperationAttribute)subscribeOperation.First();
        operation.OperationId.ShouldBe(nameof(DeleteBasketCompleteCommand));
        operation.Summary.ShouldBe("Delete basket complete");
    }

    [Test]
    public async Task GivenDeleteBasketCompleteCommand_WhenPublishingToMessageBus_ThenShouldBePublishedSuccessfully()
    {
        // Arrange
        var orderId = Guid.CreateVersion7();
        const decimal totalMoney = 123.45m;

        var command = new DeleteBasketCompleteCommand(orderId, totalMoney);

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness()
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();

        await harness.Start();

        // Act
        await harness.Bus.Publish(command);

        // Assert
        (await harness.Published.Any<DeleteBasketCompleteCommand>()).ShouldBeTrue();

        var publishedMessage = harness.Published.Select<DeleteBasketCompleteCommand>().First();
        publishedMessage.Context.Message.OrderId.ShouldBe(orderId);
        publishedMessage.Context.Message.TotalMoney.ShouldBe(totalMoney);

        await harness.Stop();
    }

    [Test]
    public void GivenDeleteBasketFailedCommand_WhenCheckingAsyncApiAttributes_ThenShouldHaveCorrectAttributes()
    {
        // Arrange & Act
        var type = typeof(DeleteBasketFailedCommand);

        // Assert
        var asyncApiAttribute = type.GetCustomAttributes(typeof(AsyncApiAttribute), false);
        asyncApiAttribute.ShouldNotBeEmpty();

        var channelAttribute = type.GetCustomAttributes(typeof(ChannelAttribute), false);
        channelAttribute.ShouldNotBeEmpty();
        ((ChannelAttribute)channelAttribute.First()).Name.ShouldBe("basket-checkout-failed");

        var subscribeOperation = type.GetCustomAttributes(
            typeof(SubscribeOperationAttribute),
            false
        );
        subscribeOperation.ShouldNotBeEmpty();
        var operation = (SubscribeOperationAttribute)subscribeOperation.First();
        operation.OperationId.ShouldBe(nameof(DeleteBasketFailedCommand));
        operation.Summary.ShouldBe("Delete basket failed");
    }

    [Test]
    public async Task GivenDeleteBasketFailedCommand_WhenPublishingToMessageBus_ThenShouldBePublishedSuccessfully()
    {
        // Arrange
        var orderId = Guid.CreateVersion7();
        const string email = "test@example.com";
        const decimal totalMoney = 123.45m;
        var basketId = Guid.CreateVersion7();

        var command = new DeleteBasketFailedCommand(basketId, email, orderId, totalMoney);

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness()
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();

        await harness.Start();

        // Act
        await harness.Bus.Publish(command);

        // Assert
        (await harness.Published.Any<DeleteBasketFailedCommand>()).ShouldBeTrue();

        var publishedMessage = harness.Published.Select<DeleteBasketFailedCommand>().First();
        publishedMessage.Context.Message.BasketId.ShouldBe(basketId);
        publishedMessage.Context.Message.OrderId.ShouldBe(orderId);
        publishedMessage.Context.Message.Email.ShouldBe(email);
        publishedMessage.Context.Message.TotalMoney.ShouldBe(totalMoney);

        await harness.Stop();
    }

    [Test]
    public void GivenPlaceOrderCommand_WhenCheckingAsyncApiAttributes_ThenShouldHaveCorrectAttributes()
    {
        // Arrange & Act
        var type = typeof(PlaceOrderCommand);

        // Assert
        var asyncApiAttribute = type.GetCustomAttributes(typeof(AsyncApiAttribute), false);
        asyncApiAttribute.ShouldNotBeEmpty();

        var channelAttribute = type.GetCustomAttributes(typeof(ChannelAttribute), false);
        channelAttribute.ShouldNotBeEmpty();
        ((ChannelAttribute)channelAttribute.First()).Name.ShouldBe("basket-place-order");

        var subscribeOperation = type.GetCustomAttributes(
            typeof(SubscribeOperationAttribute),
            false
        );
        subscribeOperation.ShouldNotBeEmpty();
        var operation = (SubscribeOperationAttribute)subscribeOperation.First();
        operation.OperationId.ShouldBe(nameof(PlaceOrderCommand));
        operation.Summary.ShouldBe("Delete a basket");
    }

    [Test]
    public async Task GivenPlaceOrderCommand_WhenPublishingToMessageBus_ThenShouldBePublishedSuccessfully()
    {
        // Arrange
        var orderId = Guid.CreateVersion7();
        const string email = "test@example.com";
        const decimal totalMoney = 123.45m;
        var basketId = Guid.CreateVersion7();

        var command = new PlaceOrderCommand(basketId, email, orderId, totalMoney);

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness()
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();

        await harness.Start();

        // Act
        await harness.Bus.Publish(command);

        // Assert
        (await harness.Published.Any<PlaceOrderCommand>()).ShouldBeTrue();
        var publishedMessage = harness.Published.Select<PlaceOrderCommand>().First();
        publishedMessage.Context.Message.BasketId.ShouldBe(basketId);
        publishedMessage.Context.Message.OrderId.ShouldBe(orderId);
        publishedMessage.Context.Message.Email.ShouldBe(email);
        publishedMessage.Context.Message.TotalMoney.ShouldBe(totalMoney);

        await harness.Stop();
    }
}
