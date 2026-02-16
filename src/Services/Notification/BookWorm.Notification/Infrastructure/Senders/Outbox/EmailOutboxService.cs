using BookWorm.Notification.Domain.Models;

namespace BookWorm.Notification.Infrastructure.Senders.Outbox;

internal sealed class EmailOutboxService(IOutboxRepository repository, ISender actualSender)
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
            mailMessage.Subject ?? "No Subject",
            mailMessage.HtmlBody ?? mailMessage.TextBody ?? string.Empty
        );

        await repository.AddAsync(outbox, cancellationToken);
        await repository.UnitOfWork.SaveChangesAsync(cancellationToken);

        await actualSender.SendAsync(mailMessage, cancellationToken);

        outbox.MarkAsSent();
        await repository.UnitOfWork.SaveChangesAsync(cancellationToken);
    }
}
