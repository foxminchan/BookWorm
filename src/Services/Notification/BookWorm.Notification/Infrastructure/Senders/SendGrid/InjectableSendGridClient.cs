using Microsoft.Extensions.Options;
using SendGrid;

namespace BookWorm.Notification.Infrastructure.Senders.SendGrid;

internal sealed class InjectableSendGridClient(
    HttpClient httpClient,
    IOptions<SendGridClientOptions> options
) : BaseClient(httpClient, options.Value);
