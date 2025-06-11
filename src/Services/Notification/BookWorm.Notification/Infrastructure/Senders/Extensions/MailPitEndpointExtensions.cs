using System.Data.Common;
using BookWorm.ServiceDefaults.Configuration;

namespace BookWorm.Notification.Infrastructure.Senders.Extensions;

public static class MailPitEndpointExtensions
{
    public static UriBuilder GetMailPitEndpoint(
        this IHostApplicationBuilder builder,
        string componentName
    )
    {
        var conn = builder.Configuration.GetRequiredConnectionString(componentName);

        DbConnectionStringBuilder connectionBuilder = new() { ConnectionString = conn };

        return new(connectionBuilder["Endpoint"].ToString()!);
    }
}
