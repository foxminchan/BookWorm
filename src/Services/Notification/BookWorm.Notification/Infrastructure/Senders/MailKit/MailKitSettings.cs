using System.Data.Common;

namespace BookWorm.Notification.Infrastructure.Senders.MailKit;

[OptionsValidator]
public sealed partial class MailKitSettings : IValidateOptions<MailKitSettings>
{
    internal const string ConfigurationSection = "Email";

    public string Name { get; set; } = nameof(BookWorm);

    [Required]
    [EmailAddress]
    public string From { get; set; } = string.Empty;

    public Uri? Endpoint { get; private set; }

    public NetworkCredential? Credentials { get; private set; }

    public bool DisableHealthChecks { get; set; } = false;

    public bool DisableTracing { get; set; } = false;

    public bool DisableMetrics { get; set; } = false;

    internal void ParseConnectionString(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"""
                ConnectionString is missing.
                It should be provided in 'ConnectionStrings:{connectionString}'
                or '{ConfigurationSection}:Endpoint' key.'
                configuration section.
                """
            );
        }

        if (Uri.TryCreate(connectionString, UriKind.Absolute, out var uri))
        {
            Endpoint = uri;
        }
        else
        {
            var builder = new DbConnectionStringBuilder { ConnectionString = connectionString };

            if (!builder.TryGetValue(nameof(Endpoint), out var endpoint))
            {
                throw new InvalidOperationException(
                    $"""
                    The 'ConnectionStrings:{connectionString}' (or 'Endpoint' key in
                    '{ConfigurationSection}') is missing.
                    """
                );
            }

            if (!Uri.TryCreate(endpoint.ToString(), UriKind.Absolute, out uri))
            {
                throw new InvalidOperationException(
                    $"""
                    The 'ConnectionStrings:{connectionString}' (or 'Endpoint' key in
                    '{ConfigurationSection}') isn't a valid URI.
                    """
                );
            }

            Endpoint = uri;

            var hasUser = builder.TryGetValue(nameof(NetworkCredential.UserName), out var user);
            var hasPass = builder.TryGetValue(nameof(NetworkCredential.Password), out var pass);

            if (hasUser != hasPass)
            {
                throw new InvalidOperationException(
                    "Both Username and Password must be supplied together."
                );
            }

            if (hasUser)
            {
                Credentials = new(user!.ToString(), pass!.ToString());
            }
        }
    }
}
