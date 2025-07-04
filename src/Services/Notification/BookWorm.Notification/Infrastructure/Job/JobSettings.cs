namespace BookWorm.Notification.Infrastructure.Job;

[OptionsValidator]
public sealed partial class JobSettings : IValidateOptions<JobSettings>
{
    internal const string ConfigurationSection = "Job";

    [Required]
    [RegularExpression(
        @"^((((\d+,)+\d+|(\d+(\/|-|#)\d+)|\d+L?|\*(\/\d+)?|L(-\d+)?|\?|[A-Z]{3}(-[A-Z]{3})?) ?){5,7})$",
        ErrorMessage = "Invalid cron expression format"
    )]
    public string CleanUpSentEmailCronExpression { get; set; } = string.Empty;

    [Required]
    [RegularExpression(
        @"^((((\d+,)+\d+|(\d+(\/|-|#)\d+)|\d+L?|\*(\/\d+)?|L(-\d+)?|\?|[A-Z]{3}(-[A-Z]{3})?) ?){5,7})$",
        ErrorMessage = "Invalid cron expression format"
    )]
    public string ResendErrorEmailCronExpression { get; set; } = string.Empty;
}
