using BookWorm.Notification.Domain.Builders;
using BookWorm.Notification.Domain.Exceptions;
using BookWorm.Notification.Domain.Models;

namespace BookWorm.Notification.UnitTests.Domain;

public sealed class OrderMimeMessageBuilderTests
{
    [Test]
    [Arguments(Status.New, "BookWorm: Your Order Has Been Received Successfully")]
    [Arguments(Status.Completed, "BookWorm: Your Order Has Been Completed and Shipped")]
    [Arguments(Status.Canceled, "BookWorm: Your Order Has Been Canceled - Important Information")]
    public void GivenOrderWithStatus_WhenBuildingMimeMessage_ThenSubjectShouldMatchStatus(
        Status status,
        string expectedSubject
    )
    {
        // Arrange
        var order = new Order(Guid.CreateVersion7(), "Test User", 99.99m, status);

        // Act
        var message = OrderMimeMessageBuilder
            .Initialize()
            .WithTo(order.FullName, "test@example.com")
            .WithSubject(order)
            .WithBody("<p>test</p>")
            .Build();

        // Assert
        message.Subject.ShouldBe(expectedSubject);
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
        message.HtmlBody.ShouldNotBeNull();
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
}
