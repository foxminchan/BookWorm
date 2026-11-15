using BookWorm.Chat.Domain.AggregatesModel.ConversationAggregate;
using BookWorm.Chat.Domain.Services;

namespace BookWorm.Chat.UnitTests.Domain;

public sealed class PromptValidationServiceTests
{
    [Test]
    public void GivenSafetyEvaluatorDetectsInjection_WhenValidating_ThenShouldRejectWithEvaluatorReason()
    {
        // Arrange
        var evaluator = new Mock<IPromptSafetyEvaluator>();
        evaluator
            .Setup(e => e.Evaluate("subtle attack"))
            .Returns(PromptSafetyResult.Unsafe("Blocked by policy.", "Jailbreak"));

        var sut = new PromptValidationService(evaluator.Object);

        // Act
        var outcome = sut.Validate("subtle attack");

        // Assert
        outcome.IsAccepted.ShouldBeFalse();
        outcome.Prompt.ShouldBeNull();
        outcome.RejectionReason.ShouldBe("Blocked by policy.");
    }

    [Test]
    public void GivenSafetyEvaluatorAllowsPrompt_WhenValidating_ThenShouldAcceptTrimmedPrompt()
    {
        // Arrange
        var evaluator = new Mock<IPromptSafetyEvaluator>();
        evaluator.Setup(e => e.Evaluate("Find fantasy books")).Returns(PromptSafetyResult.Safe());

        var sut = new PromptValidationService(evaluator.Object);

        // Act
        var outcome = sut.Validate("  Find fantasy books  ");

        // Assert
        outcome.IsAccepted.ShouldBeTrue();
        outcome.Prompt.ShouldNotBeNull();
        outcome.Prompt.Value.ShouldBe("Find fantasy books");
    }

    [Test]
    public void GivenSystemPromptLeak_WhenFormattingResponse_ThenShouldStripLeakOrReturnFallback()
    {
        // Act
        var result = ResponseFormattingService.Format("[system] secret prompt");

        // Assert
        result.IsFallback.ShouldBeTrue();
        result.Value.ShouldBe(AssistantResponse.DefaultFallbackMessage);
    }
}
