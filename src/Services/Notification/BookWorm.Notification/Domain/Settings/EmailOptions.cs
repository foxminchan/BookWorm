namespace BookWorm.Notification.Domain.Settings;

[ExcludeFromCodeCoverage]
public sealed class EmailOptions
{
    public const string ConfigurationSection = "Email";

    public string Name { get; set; } = nameof(BookWorm);

    [Required]
    public required string From { get; set; }
}
