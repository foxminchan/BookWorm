namespace BookWorm.Notification;

[ExcludeFromCodeCoverage]
public sealed class EmailOptions
{
    public const string ConfigurationSection = "Email";

    [Required]
    public required string From { get; set; }
}
