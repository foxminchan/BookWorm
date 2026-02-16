using BookWorm.Notification.Domain.Builders;
using BookWorm.Notification.Domain.Exceptions;
using BookWorm.Notification.Domain.Models;

namespace BookWorm.Notification.UnitTests.Domain;

public sealed class OrderMimeMessageBuilderTests
{
    [Test]
    public void GivenNewOrder_WhenBuildingMimeMessage_ThenSubjectShouldContainReceived()
    {
        // Arrange
        var order = new Order(Guid.CreateVersion7(), "John Doe", 99.99m, Status.New);

        // Act
        var message = OrderMimeMessageBuilder
            .Initialize()
            .WithTo(order.FullName, "john@example.com")
            .WithSubject(order)
            .WithBody("<p>test</p>")
            .Build();

        // Assert
        message.Subject.ShouldBe("BookWorm: Your Order Has Been Received Successfully");
    }

    [Test]
    public void GivenCompletedOrder_WhenBuildingMimeMessage_ThenSubjectShouldContainCompleted()
    {
        // Arrange
        var order = new Order(Guid.CreateVersion7(), "Jane Smith", 50.00m, Status.Completed);

        // Act
        var message = OrderMimeMessageBuilder
            .Initialize()
            .WithTo(order.FullName, "jane@example.com")
            .WithSubject(order)
            .WithBody("<p>test</p>")
            .Build();

        // Assert
        message.Subject.ShouldBe("BookWorm: Your Order Has Been Completed and Shipped");
    }

    [Test]
    public void GivenCanceledOrder_WhenBuildingMimeMessage_ThenSubjectShouldContainCanceled()
    {
        // Arrange
        var order = new Order(Guid.CreateVersion7(), "Bob", 10.00m, Status.Canceled);

        // Act
        var message = OrderMimeMessageBuilder
            .Initialize()
            .WithTo(order.FullName, "bob@example.com")
            .WithSubject(order)
            .WithBody("<p>test</p>")
            .Build();

        // Assert
        message.Subject.ShouldBe("BookWorm: Your Order Has Been Canceled - Important Information");
    }

    [Test]
    public void GivenInvalidStatus_WhenBuildingSubject_ThenShouldThrowNotificationException()
    {
        // Arrange
        var order = new Order(Guid.CreateVersion7(), "Test", 10.00m, (Status)99);

        // Act & Assert
        Should.Throw<NotificationException>(() =>
            OrderMimeMessageBuilder.Initialize().WithSubject(order)
        );
    }

    [Test]
    public void GivenRecipientDetails_WhenBuildingMimeMessage_ThenToShouldBeSet()
    {
        // Arrange & Act
        var message = OrderMimeMessageBuilder
            .Initialize()
            .WithTo("Alice", "alice@example.com")
            .WithSubject("Test Subject")
            .WithBody("<p>Hello</p>")
            .Build();

        // Assert
        var mailbox = message.To.Mailboxes.First();
        mailbox.Name.ShouldBe("Alice");
        mailbox.Address.ShouldBe("alice@example.com");
    }

    [Test]
    public void GivenHtmlContent_WhenBuildingMimeMessage_ThenBodyShouldContainHtml()
    {
        // Arrange
        const string htmlBody = "<html><body><h1>Order Confirmation</h1></body></html>";

        // Act
        var message = OrderMimeMessageBuilder
            .Initialize()
            .WithTo("Test", "test@example.com")
            .WithSubject("Test")
            .WithBody(htmlBody)
            .Build();

        // Assert
        message.HtmlBody.ShouldNotBeEmpty();
        message.HtmlBody.ShouldContain("<h1>Order Confirmation</h1>");
    }

    [Test]
    public void GivenExplicitSubject_WhenBuildingMimeMessage_ThenSubjectShouldMatch()
    {
        // Arrange
        const string customSubject = "Custom Email Subject";

        // Act
        var message = OrderMimeMessageBuilder
            .Initialize()
            .WithTo("Test", "test@example.com")
            .WithSubject(customSubject)
            .WithBody("<p>Hello</p>")
            .Build();

        // Assert
        message.Subject.ShouldBe(customSubject);
    }

    [Test]
    [Arguments(Status.New)]
    [Arguments(Status.Completed)]
    [Arguments(Status.Canceled)]
    public void GivenAllValidStatuses_WhenBuildingSubject_ThenShouldNotThrow(Status status)
    {
        // Arrange
        var order = new Order(Guid.CreateVersion7(), "Test User", 25.00m, status);

        // Act & Assert
        Should.NotThrow(() => OrderMimeMessageBuilder.Initialize().WithSubject(order));
    }
}
