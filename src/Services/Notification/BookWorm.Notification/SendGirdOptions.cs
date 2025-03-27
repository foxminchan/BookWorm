namespace BookWorm.Notification;

[ExcludeFromCodeCoverage]
public sealed class SendGirdOptions
{
    [Required]
    public string ApiKey { get; set; } = string.Empty;

    public string SenderEmail { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
}
