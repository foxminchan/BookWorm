using System.Runtime.CompilerServices;
using BookWorm.Chassis.ActivityScope;

namespace BookWorm.Notification.Infrastructure.Senders.Extensions;

public static class SenderWithTelemetryExtensions
{
    public static async Task WithTelemetry(
        this ISender sender,
        MimeMessage mailMessage,
        Func<CancellationToken, Task> run,
        CancellationToken cancellationToken,
        [CallerMemberName] string memberName = ""
    )
    {
        await ActivityScope.Instance.Run(
            $"{sender.GetType().Name}/{memberName}",
            (activity, token) =>
            {
                activity?.SetTag(TelemetryTags.SmtpClient.Recipient, mailMessage.To.ToString());
                activity?.SetTag(TelemetryTags.SmtpClient.Subject, mailMessage.Subject);
                activity?.SetTag(TelemetryTags.SmtpClient.EmailOperation, "Send");

                if (!string.IsNullOrEmpty(mailMessage.Headers["Message-ID"]))
                {
                    activity?.SetTag(
                        TelemetryTags.SmtpClient.MessageId,
                        mailMessage.Headers["Message-ID"]
                    );
                }

                return run(token);
            },
            new() { Tags = { { TelemetryTags.SmtpClient.EmailOperation, "Send" } } },
            cancellationToken
        );
    }
}
