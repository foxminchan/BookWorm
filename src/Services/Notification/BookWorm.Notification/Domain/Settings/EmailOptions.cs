using Microsoft.Extensions.Options;

namespace BookWorm.Notification.Domain.Settings;

[OptionsValidator]
public sealed partial class EmailOptions : IValidateOptions<EmailOptions>
{
    public const string ConfigurationSection = "Email";

    public string Name { get; set; } = nameof(BookWorm);

    [Required]
    [EmailAddress]
    public string From { get; set; } = string.Empty;
}
