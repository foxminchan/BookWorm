using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;

namespace BookWorm.Rating.Infrastructure.AI;

internal static class RatingOutputGuardrailMiddleware
{
    public static async Task<ChatResponse> InvokeAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options,
        IChatClient innerChatClient,
        CancellationToken cancellationToken
    )
    {
        var response = await innerChatClient.GetResponseAsync(messages, options, cancellationToken);

        return response;
    }

    public static async IAsyncEnumerable<ChatResponseUpdate> InvokeStreamingAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options,
        IChatClient innerChatClient,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        var response = await innerChatClient
            .GetStreamingResponseAsync(messages, options, cancellationToken)
            .ToChatResponseAsync(cancellationToken);

        foreach (var update in response.ToChatResponseUpdates())
        {
            yield return update;
        }
    }
}
