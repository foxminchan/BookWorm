using Microsoft.Extensions.Options;

namespace BookWorm.Scheduler.Configuration;

/// <summary>
/// Strongly-typed options for Quartz job cron schedules.
/// </summary>
internal sealed class QuartzOptions
{
    public const string SectionName = nameof(Quartz);

    /// <summary>
    /// Gets or sets the cron expression for the <see cref="Jobs.CleanUpSentEmailJob"/>.
    /// </summary>
    public string CleanUpSentEmailJob { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the cron expression for the <see cref="Jobs.ResendErrorEmailJob"/>.
    /// </summary>
    public string ResendErrorEmailJob { get; set; } = string.Empty;
}

/// <summary>
/// Validates <see cref="QuartzOptions"/> at startup, ensuring all cron expressions
/// are present and syntactically valid.
/// </summary>
internal sealed class QuartzOptionsValidator : IValidateOptions<QuartzOptions>
{
    public ValidateOptionsResult Validate(string? name, QuartzOptions options)
    {
        var failures = new List<string>();

        ValidateCronExpression(
            failures,
            nameof(options.CleanUpSentEmailJob),
            options.CleanUpSentEmailJob
        );

        ValidateCronExpression(
            failures,
            nameof(options.ResendErrorEmailJob),
            options.ResendErrorEmailJob
        );

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }

    private static void ValidateCronExpression(
        List<string> failures,
        string propertyName,
        string? cronExpression
    )
    {
        if (string.IsNullOrWhiteSpace(cronExpression))
        {
            failures.Add(
                $"Cron expression for '{propertyName}' is required in configuration section '{QuartzOptions.SectionName}'."
            );
            return;
        }

        if (!CronExpression.IsValidExpression(cronExpression))
        {
            failures.Add(
                $"Cron expression '{cronExpression}' for '{propertyName}' is not a valid Quartz cron expression."
            );
        }
    }
}
