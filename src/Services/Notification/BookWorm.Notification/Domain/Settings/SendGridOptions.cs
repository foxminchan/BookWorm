using BookWorm.Constants.Core;
using Microsoft.Extensions.Options;

namespace BookWorm.Notification.Domain.Settings;

[OptionsValidator]
public sealed partial class SendGridOptions : IValidateOptions<SendGridOptions>
{
    public const string ConfigurationSection = "SendGrid";

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
