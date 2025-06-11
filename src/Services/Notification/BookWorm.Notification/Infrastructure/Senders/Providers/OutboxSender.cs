using BookWorm.Notification.Domain.Models;

namespace BookWorm.Notification.Infrastructure.Senders.Providers;

public sealed class OutboxSender(ITableService tableService, ISender sender) : ISender
{
    private readonly string _partitionKey = nameof(Outbox).ToLowerInvariant();

    public async Task SendAsync(
        MimeMessage mailMessage,
        CancellationToken cancellationToken = default
    )
    {
        var mailbox =
            mailMessage.To.Mailboxes.FirstOrDefault()
            ?? throw new ArgumentException(
                "Message must have at least one recipient",
                nameof(mailMessage)
            );

        var outbox = new Outbox(
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
