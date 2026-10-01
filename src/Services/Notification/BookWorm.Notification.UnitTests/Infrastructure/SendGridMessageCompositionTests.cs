using BookWorm.Notification.Domain.Models;
using BookWorm.Notification.Infrastructure.Builders;
using BookWorm.Notification.Infrastructure.Senders.SendGrid;
using MimeKit;
using SendGrid.Helpers.Mail;

namespace BookWorm.Notification.UnitTests.Infrastructure;

public sealed class SendGridMessageCompositionTests
{
    private const string SenderEmail = "bookworm@example.com";
    private const string SenderName = "BookWorm";

    [Test]
    public void GivenNewOrder_WhenComposingSendGridMessage_ThenShouldIncludeOrderDetails()
    {
        // Arrange
        var order = new Order(
            new("01961f3a-0000-7000-8000-000000000001"),
            "John Doe",
            99.99m,
            Status.New
        )
        {
            CreatedAt = new(2025, 6, 15),
        };

        var mailMessage = OrderMimeMessageBuilder
            .Initialize()
            .WithTo(order.FullName, "john.doe@example.com")
            .WithSubject(order)
            .WithBody("<html><body>Order email content</body></html>")
            .Build();

        // Act
        var sendGridMessage = BuildSendGridMessage(mailMessage);

        // Assert
        sendGridMessage.From.Email.ShouldBe(SenderEmail);
        sendGridMessage.From.Name.ShouldBe(SenderName);
        sendGridMessage.Subject.ShouldBe("BookWorm: Your Order Has Been Received Successfully");
        sendGridMessage.HtmlContent.ShouldBe("<html><body>Order email content</body></html>");
        sendGridMessage
            .Personalizations.Single()
            .Tos.Single()
            .Email.ShouldBe("john.doe@example.com");
        Convert.ToInt64(sendGridMessage.SendAt).ShouldBe(mailMessage.Date.ToUnixTimeSeconds());
    }

    [Test]
    public void GivenCompletedOrder_WhenComposingSendGridMessage_ThenShouldIncludeOrderDetails()
    {
        // Arrange
        var order = new Order(
            new("01961f3a-0000-7000-8000-000000000002"),
            "Jane Smith",
            249.50m,
            Status.Completed
        )
        {
            CreatedAt = new(2025, 8, 20),
        };

        var mailMessage = OrderMimeMessageBuilder
            .Initialize()
            .WithTo(order.FullName, "jane.smith@example.com")
            .WithSubject(order)
            .WithBody("<html><body>Completed order content</body></html>")
            .Build();

        // Act
        var sendGridMessage = BuildSendGridMessage(mailMessage);

        // Assert
        sendGridMessage.Subject.ShouldBe("BookWorm: Your Order Has Been Completed and Shipped");
        sendGridMessage.HtmlContent.ShouldBe("<html><body>Completed order content</body></html>");
        sendGridMessage
            .Personalizations.Single()
            .Tos.Single()
            .Email.ShouldBe("jane.smith@example.com");
        Convert.ToInt64(sendGridMessage.SendAt).ShouldBe(mailMessage.Date.ToUnixTimeSeconds());
    }

    [Test]
    public void GivenCanceledOrder_WhenComposingSendGridMessage_ThenShouldIncludeOrderDetails()
    {
        // Arrange
        var order = new Order(
            new("01961f3a-0000-7000-8000-000000000003"),
            "Bob Wilson",
            15.00m,
            Status.Canceled
        )
        {
            CreatedAt = new(2025, 12, 1),
        };

        var mailMessage = OrderMimeMessageBuilder
            .Initialize()
            .WithTo(order.FullName, "bob.wilson@example.com")
            .WithSubject(order)
            .WithBody("<html><body>Canceled order content</body></html>")
            .Build();

        // Act
        var sendGridMessage = BuildSendGridMessage(mailMessage);

        // Assert
        sendGridMessage.Subject.ShouldBe(
            "BookWorm: Your Order Has Been Canceled - Important Information"
        );
        sendGridMessage.HtmlContent.ShouldBe("<html><body>Canceled order content</body></html>");
        sendGridMessage
            .Personalizations.Single()
            .Tos.Single()
            .Email.ShouldBe("bob.wilson@example.com");
        Convert.ToInt64(sendGridMessage.SendAt).ShouldBe(mailMessage.Date.ToUnixTimeSeconds());
    }

    [Test]
    public void GivenMultipleRecipients_WhenComposingSendGridMessage_ThenShouldIncludeAllRecipients()
    {
        // Arrange
        var order = new Order(
            new("01961f3a-0000-7000-8000-000000000004"),
            "Admin Batch",
            500.00m,
            Status.Completed
        )
        {
            CreatedAt = new(2025, 4, 1),
        };

        var mailMessage = OrderMimeMessageBuilder
            .Initialize()
            .WithTo(order.FullName, "admin@example.com")
            .WithSubject(order)
            .WithBody("<html><body>Batch notification</body></html>")
            .Build();

        // Act
        var sendGridMessage = BuildSendGridMessage(mailMessage);
        sendGridMessage.AddCc(new EmailAddress("cc@example.com", "CC User"));

        // Assert
        var recipients = sendGridMessage.Personalizations.Single();
        recipients.Tos.Single().Email.ShouldBe("admin@example.com");
        recipients.Ccs.Single().Email.ShouldBe("cc@example.com");
        Convert.ToInt64(sendGridMessage.SendAt).ShouldBe(mailMessage.Date.ToUnixTimeSeconds());
    }

    [Test]
    public void GivenStagingEnvironment_WhenComposingSendGridMessage_ThenSandboxModeShouldBeSet()
    {
        // Arrange
        var order = new Order(
            new("01961f3a-0000-7000-8000-000000000005"),
            "Staging User",
            10.00m,
            Status.New
        )
        {
            CreatedAt = new(2025, 2, 14),
        };

        var mailMessage = OrderMimeMessageBuilder
            .Initialize()
            .WithTo(order.FullName, "staging@example.com")
            .WithSubject(order)
            .WithBody("<html><body>Staging test</body></html>")
            .Build();

        // Act
        var sendGridMessage = BuildSendGridMessage(mailMessage);
        sendGridMessage.SetSandBoxMode(true);

        // Assert
        sendGridMessage.MailSettings.SandboxMode.Enable.ShouldBe(true);
        Convert.ToInt64(sendGridMessage.SendAt).ShouldBe(mailMessage.Date.ToUnixTimeSeconds());
    }

    /// <summary>
    ///     Mirrors the SendGrid message composition logic from <see cref="SendGridSender" />.
    /// </summary>
    private static SendGridMessage BuildSendGridMessage(MimeMessage mailMessage)
    {
        var message = new SendGridMessage
        {
            From = new(SenderEmail, SenderName),
            Subject = mailMessage.Subject,
            HtmlContent = mailMessage.HtmlBody,
            SendAt = Math.Clamp(mailMessage.Date.ToUnixTimeSeconds(), 0, long.MaxValue),
        };

        foreach (var recipient in mailMessage.To.Mailboxes)
        {
            message.AddTo(new EmailAddress(recipient.Address, recipient.Name ?? string.Empty));
        }

        return message;
    }
}
