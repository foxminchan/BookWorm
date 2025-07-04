namespace BookWorm.Notification.Infrastructure.Senders.Outbox;

internal sealed class EmailOutboxService(ITableService tableService, ISender sender) : ISender
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

        // Store in pending partition first
        var entityId = await tableService.UpsertAsync(
            outbox,
            TablePartition.Pending,
            cancellationToken
        );

        await sender.SendAsync(mailMessage, cancellationToken);

        outbox.MarkAsSent();

        // Move to sent partition
        await tableService.UpsertAsync(outbox, TablePartition.Processed, cancellationToken);

        // Remove from pending partition
        await tableService.DeleteAsync(
            TablePartition.Pending,
            entityId.ToString(),
            cancellationToken
        );
    }
}
