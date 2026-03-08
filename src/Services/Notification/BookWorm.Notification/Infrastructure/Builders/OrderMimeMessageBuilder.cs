using BookWorm.Notification.Domain.Models;
using MimeKit.Text;

namespace BookWorm.Notification.Infrastructure.Builders;

internal sealed class OrderMimeMessageBuilder
{
    private OrderMimeMessageBuilder() { }

    private string? Subject { get; set; } = string.Empty;
    private MimeEntity Body { get; set; } = new TextPart(TextFormat.Html) { Text = string.Empty };
    private MailboxAddress To { get; set; } = new(string.Empty, string.Empty);

    public static OrderMimeMessageBuilder Initialize()
    {
        return new();
    }

    public OrderMimeMessageBuilder WithTo(string? fullName, string? email)
    {
        ArgumentException.ThrowIfNullOrEmpty(email);
        To = new(fullName, email);
        return this;
    }

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

    public OrderMimeMessageBuilder WithSubject(string? subject)
    {
        Subject = subject;
        return this;
    }

    public OrderMimeMessageBuilder WithBody(string? htmlBody)
    {
        var bb = new BodyBuilder { HtmlBody = htmlBody };
        Body = bb.ToMessageBody();
        return this;
    }

    public MimeMessage Build()
    {
        var message = new MimeMessage();
        message.To.Add(To);
        message.Subject = Subject ?? "No Subject";
        message.Body = Body;
        message.Date = DateTimeOffset.UtcNow;
        return message;
    }
}
