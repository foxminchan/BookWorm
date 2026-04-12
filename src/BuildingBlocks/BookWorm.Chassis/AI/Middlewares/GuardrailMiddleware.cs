using Microsoft.Extensions.AI;

namespace BookWorm.Chassis.AI.Middlewares;

internal static class GuardrailMiddleware
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

        static List<ChatMessage> FilterMessages(IEnumerable<ChatMessage> contents) =>
            [.. contents.Select(m => new ChatMessage(m.Role, FilterContent(m.Text)))];

        static string FilterContent(string content) =>
            _sourceArray.Any(keyword =>
                content.Contains(keyword, StringComparison.OrdinalIgnoreCase)
            )
                ? "[REDACTED: Forbidden content]"
                : content;
    }
}

public static class GuardrailMiddlewareExtensions
{
    extension(ChatClientBuilder builder)
    {
        /// <summary>
        ///     Registers the guardrail middleware in the chat client pipeline to filter disallowed content.
        /// </summary>
        /// <returns>The configured <see cref="ChatClientBuilder" /> instance for fluent chaining.</returns>
        public ChatClientBuilder UseGuardrailMiddleware()
        {
            return builder.Use(GuardrailMiddleware.InvokeAsync, null);
        }
    }
}
