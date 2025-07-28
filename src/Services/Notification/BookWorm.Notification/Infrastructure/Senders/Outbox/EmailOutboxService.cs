namespace BookWorm.Notification.Infrastructure.Senders.Outbox;

internal sealed class EmailOutboxService(INotificationDbContext dbContext, ISender actualSender)
    : ISender
{
    public async Task SendAsync(
        MimeMessage mailMessage,
        CancellationToken cancellationToken = default
    )
    {
        var mailbox =
            mailMessage.To.Mailboxes.FirstOrDefault()
            ?? throw new ArgumentNullException(
                nameof(mailMessage),
                "Message must have at least one recipient"
            );

        var outbox = new Domain.Models.Outbox(
            mailbox.Name ?? "Unknown",
            mailbox.Address,
            mailMessage.Subject,
            mailMessage.HtmlBody
        );

        await dbContext.Outboxes.AddAsync(outbox, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        await actualSender.SendAsync(mailMessage, cancellationToken);

        outbox.MarkAsSent();
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
