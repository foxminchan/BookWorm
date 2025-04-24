using BookWorm.Notification.Domain.Models;

namespace BookWorm.Notification.Infrastructure.Senders;

public sealed class OutboxSender(ITableService tableService, ISender sender) : ISender
{
    private readonly string _partitionKey = nameof(Outbox).ToLower();

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

        await tableService.UpsertAsync(outbox, _partitionKey, cancellationToken);

        await sender.SendAsync(mailMessage, cancellationToken);

        outbox.MarkAsSent();

        await tableService.UpsertAsync(outbox, _partitionKey, cancellationToken);
    }
}
