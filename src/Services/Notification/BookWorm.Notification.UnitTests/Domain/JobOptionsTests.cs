using BookWorm.Notification.Domain.Settings;
using BookWorm.Notification.UnitTests.Fakers;

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
        var options = TestDataFakers.JobOptions.Generate();
        options.CleanUpSentEmailCronExpression = cleanUpCron;
        options.ResendErrorEmailCronExpression = resendErrorCron;

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
        var options = TestDataFakers.JobOptions.Generate();
        options.CleanUpSentEmailCronExpression = invalidCron!;
        options.ResendErrorEmailCronExpression = invalidCron!;

        // Act
        var result = options.Validate(nameof(JobOptions), options);

        // Assert
        result.Failed.ShouldBeTrue();
        result.FailureMessage.ShouldNotBeNullOrEmpty();
    }
}
