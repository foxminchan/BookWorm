using BookWorm.Notification.Domain.Models;
using MimeKit.Text;

namespace BookWorm.Notification.Domain.Builders;

/// <summary>
///     Fluent builder for constructing order notification <see cref="MimeMessage" /> instances.
/// </summary>
public sealed class OrderMimeMessageBuilder
{
    private OrderMimeMessageBuilder() { }

    private string? Subject { get; set; } = string.Empty;
    private MimeEntity Body { get; set; } = new TextPart(TextFormat.Html) { Text = string.Empty };
    private MailboxAddress To { get; set; } = new(string.Empty, string.Empty);

    /// <summary>
    ///     Creates a new builder instance.
    /// </summary>
    public static OrderMimeMessageBuilder Initialize() => new();

    /// <summary>
    ///     Sets the recipient of the email.
    /// </summary>
    public OrderMimeMessageBuilder WithTo(string? fullName, string? email)
    {
        To = new(fullName, email);
        return this;
    }

    /// <summary>
    ///     Derives the email subject from the order status.
    /// </summary>
    public OrderMimeMessageBuilder WithSubject(Order order)
    {
        Subject = order.Status switch
        {
            Status.New => "BookWorm: Your Order Has Been Received Successfully",
            Status.Completed => "BookWorm: Your Order Has Been Completed and Shipped",
            Status.Canceled => "BookWorm: Your Order Has Been Canceled - Important Information",
            _ => throw new NotificationException($"Invalid status: {order.Status}"),
        };
        return this;
    }

    /// <summary>
    ///     Sets the email subject explicitly.
    /// </summary>
    public OrderMimeMessageBuilder WithSubject(string? subject)
    {
        Subject = subject;
        return this;
    }

    /// <summary>
    ///     Sets the email body from pre-rendered HTML content.
    /// </summary>
    public OrderMimeMessageBuilder WithBody(string? htmlBody)
    {
        var bb = new BodyBuilder { HtmlBody = htmlBody };
        Body = bb.ToMessageBody();
        return this;
    }

    /// <summary>
    ///     Builds the final <see cref="MimeMessage" />.
    /// </summary>
    public MimeMessage Build()
    {
        var message = new MimeMessage();
        message.To.Add(To);
        message.Subject = Subject;
        message.Body = Body;
        message.Date = DateTimeOffset.UtcNow;
        return message;
    }
}
