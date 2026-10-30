using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;

namespace BookWorm.Rating.Infrastructure.Agents;

internal static class RatingOutputGuardrailMiddleware
{
    private const string InvalidOutputMessage =
        "I couldn't provide a rating summary because the response did not meet the required format. Please retry.";

    public static async Task<ChatResponse> InvokeAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options,
        IChatClient innerChatClient,
        CancellationToken cancellationToken
    )
    {
        var response = await innerChatClient.GetResponseAsync(messages, options, cancellationToken);

        if (
            response.Messages.Any(message =>
                message.Contents.Any(content => content is FunctionCallContent)
            )
        )
        {
            return response;
        }

        var output = string.Join(
            "\n",
            response
                .Messages.Where(message => message.Role == ChatRole.Assistant)
                .Select(message => message.Text)
        );

        if (!IsValid(output))
        {
            response.Messages = [new(ChatRole.Assistant, InvalidOutputMessage)];
        }

        return response;
    }

    public static async IAsyncEnumerable<ChatResponseUpdate> InvokeStreamingAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options,
        IChatClient innerChatClient,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        // Validate the complete result before yielding any streamed output.
        var response = await innerChatClient
            .GetStreamingResponseAsync(messages, options, cancellationToken)
            .ToChatResponseAsync(cancellationToken);

        if (
            !response.Messages.Any(message =>
                message.Contents.Any(content => content is FunctionCallContent)
            )
        )
        {
            var output = string.Join(
                "\n",
                response
                    .Messages.Where(message => message.Role == ChatRole.Assistant)
                    .Select(message => message.Text)
            );

            if (!IsValid(output))
            {
                response.Messages = [new(ChatRole.Assistant, InvalidOutputMessage)];
            }
        }

        foreach (var update in response.ToChatResponseUpdates())
        {
            yield return update;
        }
    }

    private static bool IsValid(string output)
    {
        if (string.IsNullOrWhiteSpace(output))
        {
            return false;
        }

        var lines = output.Trim().Split('\n').Select(line => line.TrimEnd('\r')).ToArray();

        return lines.Length == 4
            && IsClassificationLine(lines[0])
            && HasValue(lines[1], "Metrics: ")
            && HasValue(lines[2], "Evidence: ")
            && IsConfidenceLine(lines[3]);

        static bool IsClassificationLine(string line) =>
            line
                is "Classification: No Data"
                    or "Classification: Bad"
                    or "Classification: Good"
                    or "Classification: Top Rated";

        static bool IsConfidenceLine(string line) =>
            line is "Confidence: High" or "Confidence: Medium" or "Confidence: Low";

        static bool HasValue(string line, string prefix) =>
            line.StartsWith(prefix, StringComparison.Ordinal)
            && !string.IsNullOrWhiteSpace(line[prefix.Length..]);
    }
}
