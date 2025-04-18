using BookWorm.Notification.Domain.Models;
using MimeKit.Text;

namespace BookWorm.Notification.Domain.Builders;

public sealed class OrderMimeMessageBuilder
{
    private const string TemplatePath = "Templates/OrderEmail.mjml";

    private OrderMimeMessageBuilder() { }

    private string Subject { get; set; } = string.Empty;
    private MimeEntity Body { get; set; } = new TextPart(TextFormat.Html) { Text = string.Empty };
    private MailboxAddress To { get; set; } = new(string.Empty, string.Empty);
    private MailboxAddress From { get; set; } = new(string.Empty, string.Empty);

    public static OrderMimeMessageBuilder Initialize()
    {
        return new();
    }

    public OrderMimeMessageBuilder WithTo(string fullName, string email)
    {
        To = new(fullName, email);
        return this;
    }

    public OrderMimeMessageBuilder WithFrom(EmailOptions emailOptions)
    {
        From = new(emailOptions.Name, emailOptions.From);
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

    public OrderMimeMessageBuilder WithBody(Order order, IRenderer renderer)
    {
        var htmlBody = renderer.Render(order, TemplatePath);
        var bb = new BodyBuilder { HtmlBody = htmlBody };
        Body = bb.ToMessageBody();
        return this;
    }

    public MimeMessage Build()
    {
        var message = new MimeMessage();
        message.From.Add(From);
        message.To.Add(To);
        message.Subject = Subject;
        message.Body = Body;
        return message;
    }
}
