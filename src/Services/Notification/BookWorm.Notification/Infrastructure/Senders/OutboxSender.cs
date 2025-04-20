using BookWorm.Notification.Domain.Models;

namespace BookWorm.Notification.Infrastructure.Senders;

public sealed class OutboxSender(ITableService tableService, ISender sender) : ISender
{
    private const string PartitionKey = "outbox";

    public async Task SendAsync(
        MimeMessage mailMessage,
        CancellationToken cancellationToken = default
    )
    {
        var outbox = new Outbox(
            mailMessage.To.Mailboxes.First().Name,
            mailMessage.To.Mailboxes.First().Address,
            mailMessage.Subject,
            mailMessage.HtmlBody
        );

        await tableService.UpsertAsync(outbox, PartitionKey, cancellationToken);

        await sender.SendAsync(mailMessage, cancellationToken);

        outbox.MarkAsSent();

        await tableService.UpsertAsync(outbox, PartitionKey, cancellationToken);
    }
}
