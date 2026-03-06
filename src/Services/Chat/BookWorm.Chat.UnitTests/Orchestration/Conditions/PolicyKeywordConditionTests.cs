using BookWorm.Chat.Orchestration.Conditions;
using Microsoft.Extensions.AI;

namespace BookWorm.Chat.UnitTests.Orchestration.Conditions;

public sealed class PolicyKeywordConditionTests
{
    [Test]
    public void GivenNullOutput_WhenEvaluating_ThenShouldReturnFalse()
    {
        // Act
        var result = PolicyKeywordCondition.Evaluate(null);

        // Assert
        result.ShouldBeFalse();
    }

    [Test]
    public void GivenEmptyList_WhenEvaluating_ThenShouldReturnFalse()
    {
        // Act
        var result = PolicyKeywordCondition.Evaluate([]);

        // Assert
        result.ShouldBeFalse();
    }

    [Test]
    [Arguments("What is your return policy?")]
    [Arguments("I need a refund for my order")]
    [Arguments("How does shipping work?")]
    [Arguments("What are the delivery options?")]
    [Arguments("Can I update my payment method?")]
    [Arguments("How do I access my account?")]
    [Arguments("I have a billing question")]
    [Arguments("Does this book have a warranty?")]
    [Arguments("Is there a guarantee on this?")]
    [Arguments("What are your policies?")]
    public void GivenMessageWithPolicyKeyword_WhenEvaluating_ThenShouldReturnTrue(
        string messageText
    )
    {
        // Arrange
        List<ChatMessage> messages = [new(ChatRole.Assistant, messageText)];

        // Act
        var result = PolicyKeywordCondition.Evaluate(messages);

        // Assert
        result.ShouldBeTrue();
    }

    [Test]
    [Arguments("Tell me about the latest fiction books")]
    [Arguments("Who is the author of this novel?")]
    [Arguments("I am looking for a good mystery book")]
    public void GivenMessageWithoutPolicyKeyword_WhenEvaluating_ThenShouldReturnFalse(
        string messageText
    )
    {
        // Arrange
        List<ChatMessage> messages = [new(ChatRole.Assistant, messageText)];

        // Act
        var result = PolicyKeywordCondition.Evaluate(messages);

        // Assert
        result.ShouldBeFalse();
    }

    [Test]
    public void GivenMessageWithUpperCaseKeyword_WhenEvaluating_ThenShouldReturnTrue()
    {
        // Arrange
        List<ChatMessage> messages = [new(ChatRole.Assistant, "What is your RETURN POLICY?")];

        // Act
        var result = PolicyKeywordCondition.Evaluate(messages);

        // Assert
        result.ShouldBeTrue();
    }

    [Test]
    public void GivenWhitespaceOnlyMessage_WhenEvaluating_ThenShouldReturnFalse()
    {
        // Arrange
        List<ChatMessage> messages = [new(ChatRole.Assistant, "   ")];

        // Act
        var result = PolicyKeywordCondition.Evaluate(messages);

        // Assert
        result.ShouldBeFalse();
    }

    [Test]
    public void GivenMultipleMessages_WhenEvaluating_ThenShouldCheckOnlyLastMessage()
    {
        // Arrange
        List<ChatMessage> messages =
        [
            new(ChatRole.User, "What is your return policy?"),
            new(ChatRole.Assistant, "Here is a great book recommendation"),
        ];

        // Act
        var result = PolicyKeywordCondition.Evaluate(messages);

        // Assert
        result.ShouldBeFalse();
    }
}
