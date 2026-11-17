namespace BookWorm.Notification.Infrastructure.Senders;

internal static class OrderNotificationHelper
{
    public static async Task SendAsync(
        ISender sender,
        IRenderer renderer,
        Order order,
        string? email,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return;
        }

        var htmlBody = await renderer.RenderAsync(order, "Orders/OrderEmail", cancellationToken);

        var mailMessage = OrderMimeMessageBuilder
            .Initialize()
            .WithTo(order.FullName, email)
            .WithSubject(order)
            .WithBody(htmlBody)
            .Build();

        await sender.SendAsync(mailMessage, cancellationToken);
    }
}
