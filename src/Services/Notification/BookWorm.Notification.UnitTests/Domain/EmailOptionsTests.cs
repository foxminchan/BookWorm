using BookWorm.Notification.Domain.Settings;

namespace BookWorm.Notification.UnitTests.Domain;

public sealed class EmailOptionsTests
{
    [Test]
    public void GivenDefaultValues_WhenCreatingEmailOptions_ThenShouldHaveDefaultName()
    {
        // Arrange
        var options = new EmailOptions();

        // Assert
        options.Name.ShouldBe(nameof(BookWorm));
        options.From.ShouldBe(string.Empty);
    }

    [Test]
    public void GivenValidEmail_WhenSettingFromAddress_ThenShouldSetCorrectly()
    {
        // Arrange
        var options = new EmailOptions();
        const string validEmail = "test@example.com";

        // Act
        options.From = validEmail;

        // Assert
        options.From.ShouldBe(validEmail);
    }

    [Test]
    [Arguments(null)]
    [Arguments("")]
    [Arguments("  ")]
    [Arguments("invalid-email")]
    public void GivenInvalidEmail_WhenValidatingOptions_ThenShouldThrowValidationException(
        string? invalidEmail
    )
    {
        // Arrange
        var options = new EmailOptions { From = invalidEmail! };

        // Act
        var result = options.Validate(nameof(EmailOptions), options);

        // Assert
        result.Failed.ShouldBeTrue();
        result.FailureMessage.ShouldNotBeNullOrEmpty();
    }

    [Test]
    public void GivenValidEmail_WhenValidatingOptions_ThenShouldSucceed()
    {
        // Arrange
        var options = new EmailOptions { From = "test@example.com" };

        // Act
        var result = options.Validate(nameof(EmailOptions), options);

        // Assert
        result.Succeeded.ShouldBeTrue();
    }

    [Test]
    public void GivenCustomName_WhenSettingName_ThenShouldSetCorrectly()
    {
        // Arrange
        var options = new EmailOptions();
        const string customName = "Custom BookWorm";

        // Act
        options.Name = customName;

        // Assert
        options.Name.ShouldBe(customName);
    }
}
