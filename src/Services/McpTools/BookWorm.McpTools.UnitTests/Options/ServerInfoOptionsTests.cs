using BookWorm.McpTools.Options;

namespace BookWorm.McpTools.UnitTests.Options;

public sealed class ServerInfoOptionsTests
{
    private readonly ServerInfoOptions _validator = new() { Version = "1.0.0" };

    [Test]
    [Arguments("1")]
    [Arguments("1.0")]
    [Arguments("1.0.0")]
    [Arguments("1.2.*")]
    [Arguments("0.0.1")]
    public void GivenValidVersion_WhenValidate_ThenShouldReturnSuccess(string version)
    {
        // Arrange
        var options = new ServerInfoOptions { Version = version };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        result.Succeeded.ShouldBeTrue();
    }

    [Test]
    [Arguments("")]
    [Arguments("abc")]
    [Arguments("1.0.0.0")]
    [Arguments("v1.0")]
    public void GivenInvalidVersion_WhenValidate_ThenShouldReturnFailure(string version)
    {
        // Arrange
        var options = new ServerInfoOptions { Version = version };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        result.Failed.ShouldBeTrue();
    }

    [Test]
    public void GivenNullIcons_WhenValidate_ThenShouldReturnSuccess()
    {
        // Arrange
        var options = new ServerInfoOptions { Version = "1.0.0", Icons = null };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        result.Succeeded.ShouldBeTrue();
    }

    [Test]
    public void GivenValidIcons_WhenValidate_ThenShouldReturnSuccess()
    {
        // Arrange
        var options = new ServerInfoOptions
        {
            Version = "1.0.0",
            Icons =
            [
                new()
                {
                    Src = "https://example.com/icon.png",
                    Sizes = ["64x64", "128x128"],
                    MimeType = "image/png",
                },
            ],
        };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        result.Succeeded.ShouldBeTrue();
    }

    [Test]
    public void GivenIconWithAnySizeKeyword_WhenValidate_ThenShouldReturnSuccess()
    {
        // Arrange
        var options = new ServerInfoOptions
        {
            Version = "1.0.0",
            Icons = [new() { Src = "https://example.com/icon.svg", Sizes = ["any"] }],
        };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        result.Succeeded.ShouldBeTrue();
    }

    [Test]
    [Arguments("invalid")]
    [Arguments("64")]
    [Arguments("64x")]
    [Arguments("x64")]
    [Arguments("64X64")]
    public void GivenIconWithInvalidSize_WhenValidate_ThenShouldReturnFailure(string size)
    {
        // Arrange
        var options = new ServerInfoOptions
        {
            Version = "1.0.0",
            Icons = [new() { Src = "https://example.com/icon.png", Sizes = [size] }],
        };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        result.Failed.ShouldBeTrue();
        result.FailureMessage.ShouldContain(size);
    }

    [Test]
    public void GivenIconWithMissingSrc_WhenValidate_ThenShouldReturnFailure()
    {
        // Arrange
        var options = new ServerInfoOptions
        {
            Version = "1.0.0",
            Icons = [new() { Src = string.Empty }],
        };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        result.Failed.ShouldBeTrue();
    }

    [Test]
    public void GivenInvalidWebsiteUrl_WhenValidate_ThenShouldReturnFailure()
    {
        // Arrange
        var options = new ServerInfoOptions { Version = "1.0.0", WebsiteUrl = "not-a-url" };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        result.Failed.ShouldBeTrue();
    }
}
