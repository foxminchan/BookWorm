using BookWorm.Constants.Core;

namespace BookWorm.Notification.Infrastructure.Senders.SendGrid;

[OptionsValidator]
public sealed partial class SendGridSettings : IValidateOptions<SendGridSettings>
{
    internal const string ConfigurationSection = "SendGrid";

    [Key]
    [Required]
    public string ApiKey { get; set; } = string.Empty;

    [Required]
    [RegularExpression(
        @"^[a-zA-Z0-9._%+-]+@(?!.*\.\.)[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        ErrorMessage = "Invalid email format"
    )]
    public string SenderEmail { get; set; } = string.Empty;

    [Required]
    [MaxLength(DataSchemaLength.Medium)]
    public string SenderName { get; set; } = string.Empty;
}
