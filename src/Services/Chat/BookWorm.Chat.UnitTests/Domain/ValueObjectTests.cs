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
        var prompt = UserPrompt.Create(new('a', 2500));

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
    public void GivenValidResponse_WhenCreatingAssistantResponse_ThenShouldTrimAndNotBeFallback()
    {
        var response = AssistantResponse.Create("  Hello there!  ");

        response.IsFallback.ShouldBeFalse();
        response.Value.ShouldBe("Hello there!");
    }

    [Test]
    public void GivenAssistantResponses_WhenComparing_ThenShouldRespectValueAndFallbackEquality()
    {
        var first = AssistantResponse.Create("  Hello  ");
        var second = AssistantResponse.Create("Hello");
        var fallback = AssistantResponse.Fallback();

        first.ShouldBe(second);
        first.GetHashCode().ShouldBe(second.GetHashCode());
        first.ShouldNotBe(fallback);
    }

    [Test]
    public void GivenFallbackResponse_WhenCreating_ThenShouldBeMarkedAsFallback()
    {
        var response = AssistantResponse.Fallback();

        response.IsFallback.ShouldBeTrue();
        response.Value.ShouldBe(AssistantResponse.DefaultFallbackMessage);
    }

    [Test]
    public void GivenWhitespaceUserId_WhenCreatingAuthenticatedUserScope_ThenShouldThrowChatDomainException()
    {
        Should.Throw<ChatDomainException>(() => UserScope.ForAuthenticatedUser("   "));
    }

    [Test]
    public void GivenUserScopes_WhenComparing_ThenShouldRespectValueAndAnonymityEquality()
    {
        var first = UserScope.ForAuthenticatedUser("  user-42  ");
        var second = UserScope.ForAuthenticatedUser("user-42");
        var anonymous = UserScope.CreateAnonymous();

        first.ShouldBe(second);
        first.GetHashCode().ShouldBe(second.GetHashCode());
        first.ShouldNotBe(anonymous);
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
    public void GivenSafeAndUnsafeResults_WhenCreatingPromptSafetyResult_ThenShouldSetPropertiesAndEquality()
    {
        var safe1 = PromptSafetyResult.Safe();
        var safe2 = PromptSafetyResult.Safe();
        var unsafe1 = PromptSafetyResult.Unsafe("Blocked content", "violence");
        var unsafe2 = PromptSafetyResult.Unsafe("Blocked content", "violence");

        safe1.IsSafe.ShouldBeTrue();
        safe1.ViolationReason.ShouldBeNull();
        safe1.ViolationCategory.ShouldBeNull();
        safe1.ShouldBe(safe2);

        unsafe1.IsSafe.ShouldBeFalse();
        unsafe1.ViolationReason.ShouldBe("Blocked content");
        unsafe1.ViolationCategory.ShouldBe("violence");
        unsafe1.ShouldBe(unsafe2);
        unsafe1.GetHashCode().ShouldBe(unsafe2.GetHashCode());
        safe1.ShouldNotBe(unsafe1);
    }

    [Test]
    public void GivenUnsafeResultWithoutReason_WhenCreatingPromptSafetyResult_ThenShouldThrowChatDomainException()
    {
        Should.Throw<ChatDomainException>(() => PromptSafetyResult.Unsafe(" "));
    }
}
