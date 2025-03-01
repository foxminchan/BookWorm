namespace BookWorm.Notification;

public sealed class EmailOptions
{
    [Required]
    public required string From { get; set; }
}
