using BookWorm.Contracts;
using BookWorm.SharedKernel.EventBus;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Saunter.Attributes;

namespace BookWorm.Finance.UnitTests;

public sealed class OrderStateMachineTests
{
    [Test]
    public void GivenValidParameters_WhenCreatingCancelOrderCommand_ThenShouldCreateCommandWithCorrectProperties()
    {
        // Arrange
        var orderId = Guid.CreateVersion7();
        const string email = "test@example.com";
        const decimal totalMoney = 123.45m;

        // Act
        var command = new CancelOrderCommand(orderId, email, totalMoney);

        // Assert
        command.OrderId.ShouldBe(orderId);
        command.Email.ShouldBe(email);
        command.TotalMoney.ShouldBe(totalMoney);
    }

    [Test]
    public void GivenNullEmail_WhenCreatingCancelOrderCommand_ThenShouldCreateCommandWithNullEmail()
    {
        // Arrange
        var orderId = Guid.CreateVersion7();
        string? email = null;
        const decimal totalMoney = 123.45m;

        // Act
        var command = new CancelOrderCommand(orderId, email, totalMoney);

        // Assert
        command.OrderId.ShouldBe(orderId);
        command.Email.ShouldBeNull();
        command.TotalMoney.ShouldBe(totalMoney);
    }

    [Test]
    public void GivenTwoCommandsWithSameValues_WhenComparingThem_ThenShouldBeEqual()
    {
        // Arrange
        var orderId = Guid.CreateVersion7();
        const string email = "test@example.com";
        const decimal totalMoney = 123.45m;

        // Act
        var command1 = new CancelOrderCommand(orderId, email, totalMoney);
        var command2 = new CancelOrderCommand(orderId, email, totalMoney);

        // Assert
        command1.OrderId.ShouldBe(command2.OrderId);
        command1.Email.ShouldBe(command2.Email);
        command1.TotalMoney.ShouldBe(command2.TotalMoney);
    }

    [Test]
    public void GivenTwoCommandsWithDifferentValues_WhenComparingThem_ThenShouldNotBeEqual()
    {
        // Arrange
        var orderId1 = Guid.CreateVersion7();
        var orderId2 = Guid.CreateVersion7();
        const string email = "test@example.com";
        const decimal totalMoney = 123.45m;

        // Act
        var command1 = new CancelOrderCommand(orderId1, email, totalMoney);
        var command2 = new CancelOrderCommand(orderId2, email, totalMoney);

        // Assert
        command1.ShouldNotBe(command2);
        command1.GetHashCode().ShouldNotBe(command2.GetHashCode());
    }

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
    public void GivenCancelOrderCommand_WhenCheckingInheritance_ThenShouldInheritFromIntegrationEvent()
    {
        // Arrange & Act
        var command = new CancelOrderCommand(Guid.CreateVersion7(), "test@example.com", 100m);

        // Assert
        command.ShouldBeAssignableTo<IntegrationEvent>();
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
}
