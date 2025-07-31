using System.Runtime.CompilerServices;
using A2A;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;

namespace BookWorm.Chassis.RAG.A2A;

public sealed class A2AAgent : Agent
{
    public A2AAgent(A2AClient client, AgentCard agentCard)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(agentCard);

        Client = client;
        Name = agentCard.Name;
        Description = agentCard.Description;
    }

    public A2AClient Client { get; }

    public override IAsyncEnumerable<AgentResponseItem<ChatMessageContent>> InvokeAsync(
        ICollection<ChatMessageContent> messages,
        AgentThread? thread = null,
        AgentInvokeOptions? options = null,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(messages);

        return InvokeAsyncCore(messages, thread, cancellationToken);
    }

    private async IAsyncEnumerable<AgentResponseItem<ChatMessageContent>> InvokeAsyncCore(
        ICollection<ChatMessageContent> messages,
        AgentThread? thread,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        var agentThread = await EnsureThreadExistsWithMessagesAsync(
                messages,
                thread,
                () => new A2AAgentThread(Client),
                cancellationToken
            )
            .ConfigureAwait(false);

        // Invoke the agent.
        var invokeResults = InternalInvokeAsync(messages, agentThread, cancellationToken);

        // Notify the thread of new messages and return them to the caller.
        await foreach (var result in invokeResults.ConfigureAwait(false))
        {
            await NotifyThreadOfNewMessage(agentThread, result, cancellationToken)
                .ConfigureAwait(false);

            yield return new(result, agentThread);
        }
    }

    public override IAsyncEnumerable<
        AgentResponseItem<StreamingChatMessageContent>
    > InvokeStreamingAsync(
        ICollection<ChatMessageContent> messages,
        AgentThread? thread = null,
        AgentInvokeOptions? options = null,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(messages);

        return InvokeStreamingAsyncCore(messages, thread, cancellationToken);
    }

    private async IAsyncEnumerable<
        AgentResponseItem<StreamingChatMessageContent>
    > InvokeStreamingAsyncCore(
        ICollection<ChatMessageContent> messages,
        AgentThread? thread,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        var agentThread = await EnsureThreadExistsWithMessagesAsync(
                messages,
                thread,
                () => new A2AAgentThread(Client),
                cancellationToken
            )
            .ConfigureAwait(false);

        // Invoke the agent.
        var invokeResults = InternalInvokeStreamingAsync(messages, agentThread, cancellationToken);

        // Return the chunks to the caller.
        await foreach (var result in invokeResults.ConfigureAwait(false))
        {
            yield return new(result, agentThread);
        }
    }

    protected override Task<AgentChannel> CreateChannelAsync(CancellationToken cancellationToken)
    {
        throw new NotSupportedException(
            $"{nameof(A2AAgent)} is not for use with {nameof(AgentChat)}."
        );
    }

    protected override IEnumerable<string> GetChannelKeys()
    {
        throw new NotSupportedException(
            $"{nameof(A2AAgent)} is not for use with {nameof(AgentChat)}."
        );
    }

    protected override Task<AgentChannel> RestoreChannelAsync(
        string channelState,
        CancellationToken cancellationToken
    )
    {
        throw new NotSupportedException(
            $"{nameof(A2AAgent)} is not for use with {nameof(AgentChat)}."
        );
    }

    private IAsyncEnumerable<AgentResponseItem<ChatMessageContent>> InternalInvokeAsync(
        ICollection<ChatMessageContent> messages,
        A2AAgentThread thread,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(messages);

        // Ensure all messages have the correct role.
        if (messages.Any(m => m.Role != AuthorRole.User))
        {
            throw new ArgumentException(
                $"All messages must have the role {AuthorRole.User}.",
                nameof(messages)
            );
        }

        return InternalInvokeAsyncCore(messages, thread, cancellationToken);
    }

    private async IAsyncEnumerable<AgentResponseItem<ChatMessageContent>> InternalInvokeAsyncCore(
        ICollection<ChatMessageContent> messages,
        A2AAgentThread thread,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        // Send all messages to the remote agent in a single request.
        await foreach (
            var result in InvokeAgentAsync(messages, thread, cancellationToken)
                .ConfigureAwait(false)
        )
        {
            await NotifyThreadOfNewMessage(thread, result, cancellationToken).ConfigureAwait(false);
            yield return new(result, thread);
        }
    }

    private async IAsyncEnumerable<AgentResponseItem<ChatMessageContent>> InvokeAgentAsync(
        ICollection<ChatMessageContent> messages,
        A2AAgentThread thread,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        var parts = ConvertMessagesToParts(messages);
        var messageSendParams = CreateMessageSendParams(parts);

        var response = await Client
            .SendMessageAsync(messageSendParams, cancellationToken)
            .ConfigureAwait(false);

        await foreach (
            var result in ProcessAgentResponse(response, thread)
                .WithCancellation(cancellationToken)
                .ConfigureAwait(false)
        )
        {
            yield return result;
        }
    }

    private static List<Part> ConvertMessagesToParts(ICollection<ChatMessageContent> messages)
    {
        List<Part> parts = [];

        foreach (var message in messages)
        {
            foreach (var item in message.Items)
            {
                if (item is TextContent textContent)
                {
                    parts.Add(new TextPart { Text = textContent.Text ?? string.Empty });
                }
                else
                {
                    throw new NotSupportedException(
                        $"Unsupported content type: {item.GetType().Name}. Only TextContent are supported."
                    );
                }
            }
        }

        return parts;
    }

    private static MessageSendParams CreateMessageSendParams(List<Part> parts)
    {
        return new()
        {
            Message = new()
            {
                MessageId = Guid.CreateVersion7().ToString(),
                Role = MessageRole.User,
                Parts = parts,
            },
        };
    }

    private static IAsyncEnumerable<AgentResponseItem<ChatMessageContent>> ProcessAgentResponse(
        object response,
        A2AAgentThread thread
    )
    {
        return response switch
        {
            AgentTask agentTask => ProcessAgentTaskResponse(agentTask, thread),
            Message messageResponse => ProcessMessageResponse(messageResponse, thread),
            _ => throw new InvalidOperationException("Unexpected response type from A2A client."),
        };
    }

    private static async IAsyncEnumerable<
        AgentResponseItem<ChatMessageContent>
    > ProcessAgentTaskResponse(AgentTask agentTask, A2AAgentThread thread)
    {
        if (agentTask.Artifacts is { Count: > 0 })
        {
            foreach (var part in agentTask.Artifacts.SelectMany(artifact => artifact.Parts))
            {
                if (part is TextPart textPart)
                {
                    yield return new(new(AuthorRole.Assistant, textPart.Text), thread);
                }
            }
        }

        await Task.CompletedTask; // Satisfy async requirement
    }

    private static async IAsyncEnumerable<
        AgentResponseItem<ChatMessageContent>
    > ProcessMessageResponse(Message messageResponse, A2AAgentThread thread)
    {
        foreach (var part in messageResponse.Parts)
        {
            if (part is TextPart textPart)
            {
                yield return new(new(AuthorRole.Assistant, textPart.Text), thread);
            }
        }

        await Task.CompletedTask; // Satisfy async requirement
    }

    private IAsyncEnumerable<
        AgentResponseItem<StreamingChatMessageContent>
    > InternalInvokeStreamingAsync(
        ICollection<ChatMessageContent> messages,
        A2AAgentThread thread,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(messages);

        // Ensure all messages have the correct role.
        if (messages.Any(m => m.Role != AuthorRole.User))
        {
            throw new ArgumentException(
                $"All messages must have the role {AuthorRole.User}.",
                nameof(messages)
            );
        }

        return InternalInvokeStreamingAsyncCore(messages, thread, cancellationToken);
    }

    private async IAsyncEnumerable<
        AgentResponseItem<StreamingChatMessageContent>
    > InternalInvokeStreamingAsyncCore(
        ICollection<ChatMessageContent> messages,
        A2AAgentThread thread,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        // Send all messages to the remote agent in a single request.
        await foreach (
            var result in InvokeAgentAsync(messages, thread, cancellationToken)
                .ConfigureAwait(false)
        )
        {
            await NotifyThreadOfNewMessage(thread, result, cancellationToken).ConfigureAwait(false);
            yield return new(ToStreamingAgentResponseItem(result), thread);
        }
    }

    private static AgentResponseItem<StreamingChatMessageContent> ToStreamingAgentResponseItem(
        AgentResponseItem<ChatMessageContent> responseItem
    )
    {
        var messageContent = new StreamingChatMessageContent(
            responseItem.Message.Role,
            responseItem.Message.Content,
            responseItem.Message.InnerContent,
            modelId: responseItem.Message.ModelId,
            encoding: responseItem.Message.Encoding,
            metadata: responseItem.Message.Metadata
        );

        return new(messageContent, responseItem.Thread);
    }
}
