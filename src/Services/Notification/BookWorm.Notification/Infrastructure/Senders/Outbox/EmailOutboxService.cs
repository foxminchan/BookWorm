namespace BookWorm.Notification.Infrastructure.Senders.Outbox;

public sealed class EmailOutboxService(ITableService tableService, ISender sender) : ISender
{
    private readonly string _partitionKey = nameof(Domain.Models.Outbox).ToLowerInvariant();

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

        await tableService.UpsertAsync(outbox, _partitionKey, cancellationToken);

        await sender.SendAsync(mailMessage, cancellationToken);

        outbox.MarkAsSent();

        await tableService.UpsertAsync(outbox, _partitionKey, cancellationToken);
    }
}
