using Microsoft.Extensions.AI;

namespace BookWorm.Chassis.AI.Middlewares;

public static class GuardrailMiddleware
{
    private static readonly string[] _sourceArray =
    [
        "harmful",
        "illegal",
        "violence",
        "weapon",
        "drug",
        "abuse",
        "hate",
        "discrimination",
    ];

    public static async Task<ChatResponse> InvokeAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options,
        IChatClient innerChatClient,
        CancellationToken cancellationToken
    )
    {
        var filteredMessages = FilterMessages(messages);

        var response = await innerChatClient.GetResponseAsync(
            filteredMessages,
            options,
            cancellationToken
        );

        response.Messages = FilterMessages(response.Messages);

        return response;

        static List<ChatMessage> FilterMessages(IEnumerable<ChatMessage> contents)
        {
            return [.. contents.Select(m => new ChatMessage(m.Role, FilterContent(m.Text)))];
        }

        static string FilterContent(string content)
        {
            return _sourceArray.Any(keyword =>
                content.Contains(keyword, StringComparison.OrdinalIgnoreCase)
            )
                ? "[REDACTED: Forbidden content]"
                : content;
        }
    }
}
