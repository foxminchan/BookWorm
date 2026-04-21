using BookWorm.Chassis.AI.Presidio;
using Microsoft.Extensions.AI;

namespace BookWorm.Chassis.AI.Middlewares;

internal static class PIIMiddleware
{
    public static Func<
        IEnumerable<ChatMessage>,
        ChatOptions?,
        IChatClient,
        CancellationToken,
        Task<ChatResponse>
    > Create()
    {
        return async (messages, options, innerChatClient, cancellationToken) =>
        {
            var presidioService = innerChatClient.GetService<IPresidioService>();

            // If Presidio is not configured (e.g. when running standalone without the
            // Aspire AppHost providing the analyzer/anonymizer connection strings),
            // skip PII redaction and forward messages unchanged rather than failing.
            var filteredMessages = presidioService is null
                ? messages
                : await FilterMessagesAsync(presidioService, messages, cancellationToken);

            return await innerChatClient.GetResponseAsync(
                filteredMessages,
                options,
                cancellationToken
            );
        };
    }

    private static async Task<IList<ChatMessage>> FilterMessagesAsync(
        IPresidioService presidioService,
        IEnumerable<ChatMessage> messages,
        CancellationToken cancellationToken
    )
    {
        var result = new List<ChatMessage>();

        foreach (var message in messages)
        {
            var anonymizedText = await presidioService.AnonymizeAsync(
                message.Text,
                cancellationToken
            );

            result.Add(new(message.Role, anonymizedText));
        }

        return result;
    }
}

public static class PIIMiddlewareExtensions
{
    extension(ChatClientBuilder builder)
    {
        /// <summary>
        ///     Adds PII (Personally Identifiable Information) redaction middleware to the chat client pipeline.
        ///     Intercepts chat messages and anonymizes sensitive information before forwarding to the inner client.
        /// </summary>
        /// <returns>The <see cref="ChatClientBuilder" /> so that additional calls can be chained.</returns>
        public ChatClientBuilder UsePIIMiddleware()
        {
            return builder.Use(PIIMiddleware.Create(), null);
        }
    }
}
