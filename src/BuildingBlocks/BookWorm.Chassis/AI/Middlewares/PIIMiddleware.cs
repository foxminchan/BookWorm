using System.Runtime.CompilerServices;
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

            var response = await innerChatClient.GetResponseAsync(
                filteredMessages,
                options,
                cancellationToken
            );

            response.Messages = await FilterMessagesAsync(
                presidioService,
                response.Messages,
                cancellationToken
            );

            return response;
        };
    }

    public static Func<
        IEnumerable<ChatMessage>,
        ChatOptions?,
        IChatClient,
        CancellationToken,
        IAsyncEnumerable<ChatResponseUpdate>
    > CreateStreaming(IPresidioService presidioService)
    {
        return InvokeStreamingAsync;

        async IAsyncEnumerable<ChatResponseUpdate> InvokeStreamingAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options,
            IChatClient innerChatClient,
            [EnumeratorCancellation] CancellationToken cancellationToken
        )
        {
            var filteredMessages = await FilterMessagesAsync(
                presidioService,
                messages,
                cancellationToken
            );
            var response = await innerChatClient
                .GetStreamingResponseAsync(filteredMessages, options, cancellationToken)
                .ToChatResponseAsync(cancellationToken);

            response.Messages = await FilterMessagesAsync(
                presidioService,
                response.Messages,
                cancellationToken
            );

            foreach (var update in response.ToChatResponseUpdates())
            {
                yield return update;
            }
        }
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
            var anonymizedMessage = message.Clone();
            var contents = new List<AIContent>(message.Contents.Count);

            foreach (var content in message.Contents)
            {
                if (content is TextContent textContent)
                {
                    var anonymizedText = await presidioService.AnonymizeAsync(
                        textContent.Text,
                        cancellationToken
                    );

                    contents.Add(
                        new TextContent(anonymizedText)
                        {
                            Annotations = textContent.Annotations,
                            AdditionalProperties = textContent.AdditionalProperties,
                            RawRepresentation = textContent.RawRepresentation,
                        }
                    );
                }
                else
                {
                    contents.Add(content);
                }
            }

            anonymizedMessage.Contents = contents;
            result.Add(anonymizedMessage);
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
            return builder.Use(
                PIIMiddleware.Create(presidioService),
                PIIMiddleware.CreateStreaming(presidioService)
            );
        }
    }
}
