using BookWorm.Constants;
using BookWorm.Constants.Core;
using BookWorm.Notification.Domain.Settings;

namespace BookWorm.Notification.UnitTests.Domain;

public sealed class SendGridOptionsTests
{
    [Test]
    public void GivenValidOptions_WhenValidating_ThenShouldPassValidation()
    {
        // Arrange
        var options = new SendGridOptions
        {
            ApiKey = "valid-api-key",
            SenderEmail = "test@example.com",
            SenderName = "Test Sender",
        };

        // Act
        var result = options.Validate(string.Empty, options);

        // Assert
        result.Succeeded.ShouldBeTrue();
    }

    [Test]
    [Arguments(null)]
    [Arguments("")]
    [Arguments("  ")]
    public void GivenEmptyApiKey_WhenValidating_ThenShouldFailValidation(string? apiKey)
    {
        // Arrange
        var options = new SendGridOptions
        {
            ApiKey = apiKey!,
            SenderEmail = "test@example.com",
            SenderName = "Test Sender",
        };

        // Act
        var result = options.Validate(string.Empty, options);

        // Assert
        result.Succeeded.ShouldBeFalse();
        result.FailureMessage?.ShouldContain(nameof(SendGridOptions.ApiKey));
    }

    [Test]
    [Arguments("invalid-email")]
    [Arguments("test@")]
    [Arguments("@example.com")]
    [Arguments("test@.com")]
    [Arguments("test@example")]
    [Arguments("test@example..com")]
    [Arguments("test@example.com.")]
    public void GivenInvalidEmail_WhenValidating_ThenShouldFailValidation(string invalidEmail)
    {
        // Arrange
        var options = new SendGridOptions
        {
            ApiKey = "valid-api-key",
            SenderEmail = invalidEmail,
            SenderName = "Test Sender",
        };

        // Act
        var result = options.Validate(string.Empty, options);

        // Assert
        result.Succeeded.ShouldBeFalse();
        result.FailureMessage?.ShouldContain(nameof(SendGridOptions.SenderEmail));
    }

    [Test]
    [Arguments(null)]
    [Arguments("")]
    [Arguments("  ")]
    public void GivenEmptySenderName_WhenValidating_ThenShouldFailValidation(string? senderName)
    {
        // Arrange
        var options = new SendGridOptions
        {
            ApiKey = "valid-api-key",
            SenderEmail = "test@example.com",
            SenderName = senderName!,
        };

        // Act
        var result = options.Validate(string.Empty, options);

        // Assert
        result.Succeeded.ShouldBeFalse();
        result.FailureMessage?.ShouldContain(nameof(SendGridOptions.SenderName));
    }

    [Test]
    public void GivenSenderNameExceedsMaxLength_WhenValidating_ThenShouldFailValidation()
    {
        // Arrange
        var options = new SendGridOptions
        {
            ApiKey = "valid-api-key",
            SenderEmail = "test@example.com",
            SenderName = new('a', DataSchemaLength.Medium + 1),
        };

        // Act
        var result = options.Validate(string.Empty, options);

        // Assert
        result.Succeeded.ShouldBeFalse();
        result.FailureMessage?.ShouldContain(nameof(SendGridOptions.SenderName));
    }

    [Test]
    public void GivenMultipleValidationFailures_WhenValidating_ThenShouldReportAllFailures()
    {
        // Arrange
        var options = new SendGridOptions
        {
            ApiKey = "",
            SenderEmail = "invalid-email",
            SenderName = "",
        };

        // Act
        var result = options.Validate(string.Empty, options);

        // Assert
        result.Succeeded.ShouldBeFalse();
        result.FailureMessage?.ShouldContain(nameof(SendGridOptions.ApiKey));
        result.FailureMessage?.ShouldContain(nameof(SendGridOptions.SenderEmail));
        result.FailureMessage?.ShouldContain(nameof(SendGridOptions.SenderName));
    }

    [Test]
    public void GivenConfigurationSection_WhenGetting_ThenShouldReturnCorrectValue()
    {
        // Arrange & Act
        const string section = SendGridOptions.ConfigurationSection;

        // Assert
        section.ShouldBe("SendGrid");
    }

    [Test]
    public void GivenType_WhenCheckingAttributes_ThenShouldHaveOptionsValidator()
    {
        // Arrange
        var type = typeof(SendGridOptions);

        // Act
        var attributes = type.GetCustomAttributes(false);

        // Assert
        attributes.ShouldContain(x => x.GetType().Name == "OptionsValidatorAttribute");
    }
}
