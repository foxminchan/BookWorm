using BookWorm.Notification.Domain.Settings;

namespace BookWorm.Notification.UnitTests.Domain;

public sealed class JobOptionsTests
{
    [Test]
    public void GivenDefaultValues_WhenCreatingJobOptions_ThenShouldHaveDefaultCronExpressions()
    {
        // Arrange
        var options = new JobOptions();

        // Assert
        options.CleanUpSentEmailCronExpression.ShouldBe(string.Empty);
        options.ResendErrorEmailCronExpression.ShouldBe(string.Empty);
    }

    [Test]
    [Arguments("0 0 * * *", "0 0 * * *")]
    [Arguments("0 12 * * *", "0 12 * * *")]
    public void GivenValidCronExpressions_WhenSettingValues_ThenShouldSetCorrectly(
        string cleanUpCron,
        string resendErrorCron
    )
    {
        // Arrange
        var options = new JobOptions
        {
            CleanUpSentEmailCronExpression = cleanUpCron,
            ResendErrorEmailCronExpression = resendErrorCron,
        };

        // Act & Assert
        options.CleanUpSentEmailCronExpression.ShouldBe(cleanUpCron);
        options.ResendErrorEmailCronExpression.ShouldBe(resendErrorCron);
    }

    [Test]
    [Arguments(null)]
    [Arguments("")]
    [Arguments("  ")]
    [Arguments("invalid-cron-expression")]
    public void GivenInvalidCronExpression_WhenValidatingOptions_ThenShouldThrowValidationException(
        string? invalidCron
    )
    {
        // Arrange
        var options = new JobOptions
        {
            CleanUpSentEmailCronExpression = invalidCron!,
            ResendErrorEmailCronExpression = invalidCron!,
        };

        // Act
        var result = options.Validate(nameof(JobOptions), options);

        // Assert
        result.Failed.ShouldBeTrue();
        result.FailureMessage.ShouldNotBeNullOrEmpty();
    }
}
