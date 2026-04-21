using BookWorm.Chassis.AI.Presidio;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;

namespace BookWorm.Chassis.AI.Middlewares;

internal static class PIIMiddleware
{
    public static Func<
        IEnumerable<ChatMessage>,
        ChatOptions?,
        IChatClient,
        CancellationToken,
        Task<ChatResponse>
    > Create(IPresidioService presidioService)
    {
        return async (messages, options, innerChatClient, cancellationToken) =>
        {
            var filteredMessages = await FilterMessagesAsync(
                presidioService,
                messages,
                cancellationToken
            );

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
        ///     Resolves <see cref="IPresidioService" /> from the provided <paramref name="sp" /> at build time
        ///     so the service is captured in the middleware closure rather than looked up per-request.
        /// </summary>
        /// <param name="sp">The DI service provider used to resolve <see cref="IPresidioService" />.</param>
        /// <returns>The <see cref="ChatClientBuilder" /> so that additional calls can be chained.</returns>
        public ChatClientBuilder UsePIIMiddleware(IServiceProvider sp)
        {
            var presidioService = sp.GetRequiredService<IPresidioService>();
            return builder.Use(PIIMiddleware.Create(presidioService), null);
        }
    }
}
