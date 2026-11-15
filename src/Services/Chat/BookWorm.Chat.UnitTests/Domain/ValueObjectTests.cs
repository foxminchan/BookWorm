using BookWorm.Chat.Domain.AggregatesModel.ConversationAggregate;
using BookWorm.Chat.Domain.Exceptions;

namespace BookWorm.Chat.UnitTests.Domain;

public sealed class ValueObjectTests
{
    [Test]
    public void GivenWhitespacePrompt_WhenCreatingUserPrompt_ThenShouldThrowChatDomainException()
    {
        Should.Throw<ChatDomainException>(() => UserPrompt.Create("   "));
    }

    [Test]
    public void GivenOversizedPrompt_WhenCreatingUserPrompt_ThenShouldTruncateAndFlag()
    {
        var prompt = UserPrompt.Create(new string('a', 2500));

        prompt.WasTruncated.ShouldBeTrue();
        prompt.Value.ShouldEndWith(UserPrompt.TruncationSuffix);
        prompt.Value.Length.ShouldBe(
            UserPrompt.TruncateLength + UserPrompt.TruncationSuffix.Length
        );
    }

    [Test]
    public void GivenIdenticalPrompts_WhenComparing_ThenShouldBeEqual()
    {
        var first = UserPrompt.Create("  Hello  ");
        var second = UserPrompt.Create("Hello");

        first.ShouldBe(second);
    }

    [Test]
    public void GivenWhitespaceResponse_WhenCreatingAssistantResponse_ThenShouldThrowChatDomainException()
    {
        Should.Throw<ChatDomainException>(() => AssistantResponse.Create("   "));
    }

    [Test]
    public void GivenFallbackResponse_WhenCreating_ThenShouldBeMarkedAsFallback()
    {
        var response = AssistantResponse.Fallback();

        response.IsFallback.ShouldBeTrue();
        response.Value.ShouldBe(AssistantResponse.DefaultFallbackMessage);
    }

    [Test]
    public void GivenNullOrEmptyUserId_WhenResolvingUserScope_ThenShouldCreateAnonymousScope()
    {
        var scope = UserScope.FromUserIdOrAnonymous(null);

        scope.IsAnonymous.ShouldBeTrue();
        scope.Value.ShouldStartWith("anonymous-");
    }

    [Test]
    public void GivenAuthenticatedUserId_WhenResolvingUserScope_ThenShouldPreserveUserId()
    {
        var scope = UserScope.FromUserIdOrAnonymous("  user-42  ");

        scope.IsAnonymous.ShouldBeFalse();
        scope.Value.ShouldBe("user-42");
    }

    [Test]
    public void GivenUnsafeResultWithoutReason_WhenCreatingPromptSafetyResult_ThenShouldThrowChatDomainException()
    {
        Should.Throw<ChatDomainException>(() => PromptSafetyResult.Unsafe(" "));
    }
}
