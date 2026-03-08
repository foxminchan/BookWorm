using BookWorm.Chassis.AI.Presidio;
using Microsoft.Extensions.AI;

namespace BookWorm.Chassis.AI.Middlewares;

public static class PIIMiddleware
{
    /// <summary>
    ///     Creates a PII filtering delegate that uses the given <see cref="IPresidioService" />
    ///     to anonymize messages before they reach the inner chat client.
    /// </summary>
    /// <param name="presidioService">The Presidio service for PII anonymization.</param>
    /// <returns>A delegate compatible with <c>ChatClientBuilder.Use()</c>.</returns>
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
