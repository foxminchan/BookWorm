using BookWorm.Chat.Orchestration.Conditions;
using Microsoft.Extensions.AI;

namespace BookWorm.Chat.UnitTests.Orchestration.Conditions;

public sealed class InputValidationConditionTests
{
    [Test]
    public void GivenNullMessage_WhenCheckingIsAccepted_ThenShouldReturnFalse()
    {
        // Act
        var result = InputValidationCondition.IsAccepted(null);

        // Assert
        result.ShouldBeFalse();
    }

    [Test]
    public void GivenNullMessage_WhenCheckingIsRejected_ThenShouldReturnFalse()
    {
        // Act
        var result = InputValidationCondition.IsRejected(null);

        // Assert
        result.ShouldBeFalse();
    }

    [Test]
    public void GivenUserRoleMessage_WhenCheckingIsAccepted_ThenShouldReturnTrue()
    {
        // Arrange
        var message = new ChatMessage(ChatRole.User, "Hello");

        // Act
        var result = InputValidationCondition.IsAccepted(message);

        // Assert
        result.ShouldBeTrue();
    }

    [Test]
    public void GivenUserRoleMessage_WhenCheckingIsRejected_ThenShouldReturnFalse()
    {
        // Arrange
        var message = new ChatMessage(ChatRole.User, "Hello");

        // Act
        var result = InputValidationCondition.IsRejected(message);

        // Assert
        result.ShouldBeFalse();
    }

    [Test]
    public void GivenAssistantRoleMessage_WhenCheckingIsAccepted_ThenShouldReturnFalse()
    {
        // Arrange
        var message = new ChatMessage(ChatRole.Assistant, "Rejected");

        // Act
        var result = InputValidationCondition.IsAccepted(message);

        // Assert
        result.ShouldBeFalse();
    }

    [Test]
    public void GivenAssistantRoleMessage_WhenCheckingIsRejected_ThenShouldReturnTrue()
    {
        // Arrange
        var message = new ChatMessage(ChatRole.Assistant, "Rejected");

        // Act
        var result = InputValidationCondition.IsRejected(message);

        // Assert
        result.ShouldBeTrue();
    }

    [Test]
    public void GivenSystemRoleMessage_WhenCheckingIsAccepted_ThenShouldReturnFalse()
    {
        // Arrange
        var message = new ChatMessage(ChatRole.System, "System prompt");

        // Act
        var result = InputValidationCondition.IsAccepted(message);

        // Assert
        result.ShouldBeFalse();
    }

    [Test]
    public void GivenSystemRoleMessage_WhenCheckingIsRejected_ThenShouldReturnFalse()
    {
        // Arrange
        var message = new ChatMessage(ChatRole.System, "System prompt");

        // Act
        var result = InputValidationCondition.IsRejected(message);

        // Assert
        result.ShouldBeFalse();
    }
}
