using BookWorm.Chat.Domain.AggregatesModel.ConversationAggregate;
using BookWorm.Chat.Domain.Events;
using BookWorm.Chat.Domain.Exceptions;

namespace BookWorm.Chat.UnitTests.Domain;

public sealed class ConversationTests
{
    [Test]
    public void GivenValidUserScope_WhenCreatingConversation_ThenShouldRaiseConversationStartedEvent()
    {
        // Arrange
        var scope = UserScope.ForAuthenticatedUser("user-123");

        // Act
        var conversation = new Conversation(scope);

        // Assert
        conversation.Id.ShouldNotBe(Guid.Empty);
        conversation.UserScope.ShouldBe(scope);
        conversation.Turns.ShouldBeEmpty();
        conversation.DomainEvents.Count.ShouldBe(1);
        conversation.DomainEvents.Single().ShouldBeOfType<ConversationStartedEvent>();
    }

    [Test]
    public void GivenConversation_WhenAcceptingPromptAndRecordingResponse_ThenShouldTrackTurnAndRaiseEvents()
    {
        // Arrange
        var conversation = new Conversation(UserScope.CreateAnonymous());
        var prompt = UserPrompt.Create("Recommend a sci-fi book");
        var response = AssistantResponse.Create("I recommend Dune.");

        // Act
        var turn = conversation.AcceptPrompt(prompt);
        conversation.RecordResponse(response);

        // Assert
        conversation.Turns.Count.ShouldBe(1);
        turn.IsRejected.ShouldBeFalse();
        turn.AcceptedPrompt.ShouldBe(prompt);
        turn.Response.ShouldBe(response);
        conversation.DomainEvents.OfType<PromptAcceptedEvent>().Count().ShouldBe(1);
        conversation.DomainEvents.OfType<ResponseRecordedEvent>().Count().ShouldBe(1);
    }

    [Test]
    public void GivenConversation_WhenRejectingPrompt_ThenShouldRecordRejectedTurnAndRaiseEvent()
    {
        // Arrange
        var conversation = new Conversation(UserScope.CreateAnonymous());

        // Act
        var turn = conversation.RejectPrompt("Security policy violation");

        // Assert
        conversation.Turns.Count.ShouldBe(1);
        turn.IsRejected.ShouldBeTrue();
        turn.RejectionReason.ShouldBe("Security policy violation");
        turn.Response.ShouldBeNull();
        conversation.DomainEvents.OfType<PromptRejectedEvent>().Count().ShouldBe(1);
    }

    [Test]
    public void GivenConversationWithoutPendingTurn_WhenRecordingResponse_ThenShouldThrowChatDomainException()
    {
        // Arrange
        var conversation = new Conversation(UserScope.CreateAnonymous());
        var response = AssistantResponse.Create("Hello.");

        // Act & Assert
        Should.Throw<ChatDomainException>(() => conversation.RecordResponse(response));
    }

    [Test]
    public void GivenRejectedTurnOnly_WhenRecordingResponse_ThenShouldThrowChatDomainException()
    {
        // Arrange
        var conversation = new Conversation(UserScope.CreateAnonymous());
        conversation.RejectPrompt("Empty prompt");
        var response = AssistantResponse.Fallback();

        // Act & Assert
        Should.Throw<ChatDomainException>(() => conversation.RecordResponse(response));
    }
}
