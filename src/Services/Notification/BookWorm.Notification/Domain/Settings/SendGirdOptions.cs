namespace BookWorm.Notification.Domain.Settings;

[ExcludeFromCodeCoverage]
public sealed class SendGirdOptions
{
    public const string ConfigurationSection = "SendGrid";

    [Required]
    public string ApiKey { get; set; } = string.Empty;

    public string SenderEmail { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
}
