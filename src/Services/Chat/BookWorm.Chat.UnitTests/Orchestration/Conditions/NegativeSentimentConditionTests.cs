using BookWorm.Chat.Orchestration.Conditions;
using Microsoft.Extensions.AI;

namespace BookWorm.Chat.UnitTests.Orchestration.Conditions;

public sealed class NegativeSentimentConditionTests
{
    [Test]
    public void GivenNullOutput_WhenEvaluating_ThenShouldReturnFalse()
    {
        // Act
        var result = NegativeSentimentCondition.Evaluate(null);

        // Assert
        result.ShouldBeFalse();
    }

    [Test]
    public void GivenEmptyList_WhenEvaluating_ThenShouldReturnFalse()
    {
        // Act
        var result = NegativeSentimentCondition.Evaluate([]);

        // Assert
        result.ShouldBeFalse();
    }

    [Test]
    [Arguments("I am very disappointed with this purchase")]
    [Arguments("This service frustrated me")]
    [Arguments("I am unhappy with the delivery")]
    [Arguments("I am angry about this issue")]
    [Arguments("This is terrible service")]
    [Arguments("This is the worst experience ever")]
    [Arguments("The quality is awful")]
    [Arguments("I hate this product")]
    [Arguments("The service was poor")]
    [Arguments("This is really bad")]
    [Arguments("I am unsatisfied with the result")]
    public void GivenMessageWithNegativeKeyword_WhenEvaluating_ThenShouldReturnTrue(
        string messageText
    )
    {
        // Arrange
        List<ChatMessage> messages = [new(ChatRole.Assistant, messageText)];

        // Act
        var result = NegativeSentimentCondition.Evaluate(messages);

        // Assert
        result.ShouldBeTrue();
    }

    [Test]
    [Arguments("I love this book, it is amazing")]
    [Arguments("Great service, thank you")]
    [Arguments("Can you recommend a good mystery novel?")]
    public void GivenMessageWithoutNegativeKeyword_WhenEvaluating_ThenShouldReturnFalse(
        string messageText
    )
    {
        // Arrange
        List<ChatMessage> messages = [new(ChatRole.Assistant, messageText)];

        // Act
        var result = NegativeSentimentCondition.Evaluate(messages);

        // Assert
        result.ShouldBeFalse();
    }

    [Test]
    public void GivenMessageWithUpperCaseKeyword_WhenEvaluating_ThenShouldReturnTrue()
    {
        // Arrange
        List<ChatMessage> messages = [new(ChatRole.Assistant, "I am DISAPPOINTED and ANGRY")];

        // Act
        var result = NegativeSentimentCondition.Evaluate(messages);

        // Assert
        result.ShouldBeTrue();
    }

    [Test]
    public void GivenWhitespaceOnlyMessage_WhenEvaluating_ThenShouldReturnFalse()
    {
        // Arrange
        List<ChatMessage> messages = [new(ChatRole.Assistant, "   ")];

        // Act
        var result = NegativeSentimentCondition.Evaluate(messages);

        // Assert
        result.ShouldBeFalse();
    }

    [Test]
    public void GivenMultipleMessages_WhenEvaluating_ThenShouldCheckOnlyLastMessage()
    {
        // Arrange
        List<ChatMessage> messages =
        [
            new(ChatRole.User, "I am very disappointed"),
            new(ChatRole.Assistant, "Thank you for your feedback, I will help you"),
        ];

        // Act
        var result = NegativeSentimentCondition.Evaluate(messages);

        // Assert
        result.ShouldBeFalse();
    }

    [Test]
    [Arguments("frustrated")]
    [Arguments("frustrates")]
    public void GivenFrustrateVariants_WhenEvaluating_ThenShouldReturnTrue(string keyword)
    {
        // Arrange
        List<ChatMessage> messages = [new(ChatRole.Assistant, $"I feel {keyword} about this")];

        // Act
        var result = NegativeSentimentCondition.Evaluate(messages);

        // Assert
        result.ShouldBeTrue();
    }

    [Test]
    [Arguments("hated")]
    [Arguments("hates")]
    public void GivenHateVariants_WhenEvaluating_ThenShouldReturnTrue(string keyword)
    {
        // Arrange
        List<ChatMessage> messages = [new(ChatRole.Assistant, $"The customer {keyword} it")];

        // Act
        var result = NegativeSentimentCondition.Evaluate(messages);

        // Assert
        result.ShouldBeTrue();
    }
}
