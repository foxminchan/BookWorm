namespace BookWorm.Notification;

[ExcludeFromCodeCoverage]
public sealed class EmailOptions
{
    [Required]
    public required string From { get; set; }
}
