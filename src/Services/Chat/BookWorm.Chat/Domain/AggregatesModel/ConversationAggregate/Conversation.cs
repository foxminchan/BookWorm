using BookWorm.Chat.Domain.Events;
using BookWorm.Chat.Domain.Exceptions;
using BookWorm.SharedKernel.SeedWork;

namespace BookWorm.Chat.Domain.AggregatesModel.ConversationAggregate;

public sealed class Conversation() : AuditableEntity, IAggregateRoot
{
    private readonly List<ChatTurn> _turns = [];

    public Conversation(UserScope userScope)
        : this()
    {
        ArgumentNullException.ThrowIfNull(userScope);

        Id = Guid.CreateVersion7();
        UserScope = userScope;
        RegisterDomainEvent(new ConversationStartedEvent(Id, userScope.Value));
    }

    public UserScope UserScope { get; private set; } = null!;

    public IReadOnlyCollection<ChatTurn> Turns => _turns.AsReadOnly();

    /// <summary>
    ///     Records an accepted user prompt as a new turn in the conversation.
    /// </summary>
    /// <param name="prompt">The accepted user prompt to record.</param>
    /// <returns>The newly created <see cref="ChatTurn" /> representing the accepted prompt.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="prompt" /> is null.</exception>
    public ChatTurn AcceptPrompt(UserPrompt prompt)
    {
        ArgumentNullException.ThrowIfNull(prompt);

        var turn = ChatTurn.CreateAccepted(prompt);
        _turns.Add(turn);
        RegisterDomainEvent(new PromptAcceptedEvent(Id, prompt.Value, prompt.WasTruncated));
        return turn;
    }

    /// <summary>
    ///     Records a rejected user prompt as a new turn in the conversation.
    /// </summary>
    /// <param name="reason">The reason for rejecting the prompt.</param>
    /// <returns>The newly created <see cref="ChatTurn" /> representing the rejected prompt.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="reason" /> is null.</exception>
    public ChatTurn RejectPrompt(string reason)
    {
        var turn = ChatTurn.CreateRejected(reason);
        _turns.Add(turn);
        RegisterDomainEvent(new PromptRejectedEvent(Id, reason));
        return turn;
    }

    /// <summary>
    ///     Attaches a formatted assistant response to the latest pending accepted turn.
    /// </summary>
    /// <param name="response">The assistant response to attach.</param>
    /// <returns>The current <see cref="Conversation" /> instance with the response attached.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="response" /> is null.</exception>
    /// <exception cref="ChatDomainException">
    ///     Thrown if there is no pending accepted turn to attach the response to, or if the latest accepted turn already has a
    ///     response.
    /// </exception>
    public Conversation RecordResponse(AssistantResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        var pendingTurn = _turns.LastOrDefault(t => !t.IsRejected && t.Response is null)
            ?? throw new ChatDomainException(
                "Cannot record a response without an active accepted prompt turn."
            );

        pendingTurn.AttachResponse(response);
        RegisterDomainEvent(new ResponseRecordedEvent(Id, response.Value, response.IsFallback));
        return this;
    }
}
