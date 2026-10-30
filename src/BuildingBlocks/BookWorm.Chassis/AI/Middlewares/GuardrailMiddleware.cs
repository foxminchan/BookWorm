using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.AI;

namespace BookWorm.Chassis.AI.Middlewares;

internal static partial class GuardrailMiddleware
{
    [GeneratedRegex(
        @"ignore\s+(?:previous|prior|all|above)\s+instructions?"
            + @"|disregard\s+(?:all|your|the|previous|prior)\s+(?:rules?|instructions?|context)"
            + @"|forget\s+everything(?:\s+(?:above|before))?"
            + @"|you\s+are\s+now\s+"
            + @"|system\s+(?:message|prompt|instruction)\s*:"
            + @"|(?:\[/?INST\]|<<SYS>>|<\|(?:im_start|im_end)\|>)"
            + @"|reveal\s+(?:your|the|system)\s+(?:prompt|instructions?)"
            + @"|repeat\s+(?:everything|all(?:\s+text)?)\s+(?:above|before)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled
    )]
    private static partial Regex InjectionPatternRegex();

    [GeneratedRegex(
        @"(?:system|developer)\s+(?:prompt|instructions?)\s*[:=]\s*\S"
            + @"|(?:\[/?INST\]|<<SYS>>|<\|im_start\|>\s*system)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled
    )]
    private static partial Regex PromptLeakagePatternRegex();

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

        response.Messages = FilterMessages(response.Messages, false);

        return response;
    }

    internal static async IAsyncEnumerable<ChatResponseUpdate> InvokeStreamingAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options,
        IChatClient innerChatClient,
        CancellationToken cancellationToken
    )
    {
        // Buffer streamed updates so prompt-leak checks run before any response reaches the caller.
        var filteredMessages = FilterMessages(messages);
        var response = await innerChatClient
            .GetStreamingResponseAsync(filteredMessages, options, cancellationToken)
            .ToChatResponseAsync(cancellationToken);

        response.Messages = FilterMessages(response.Messages, false);

        foreach (var update in response.ToChatResponseUpdates())
        {
            yield return update;
        }
    }

    private static List<ChatMessage> FilterMessages(
        IEnumerable<ChatMessage> contents,
        bool wrapUntrustedContent = true
    ) => [.. contents.Select(message => FilterMessage(message, wrapUntrustedContent))];

    private static ChatMessage FilterMessage(ChatMessage message, bool wrapUntrustedContent)
    {
        var isUntrusted = message.Role == ChatRole.User || message.Role == ChatRole.Tool;
        var combinedText = message.Text;

        var redact =
            (isUntrusted && InjectionPatternRegex().IsMatch(combinedText))
            || (!isUntrusted && PromptLeakagePatternRegex().IsMatch(combinedText));

        var filteredMessage = message.Clone();
        filteredMessage.Contents =
        [
            .. message.Contents.Select(content =>
                content is TextContent textContent
                    ? CreateTextContent(
                        textContent,
                        redact
                            ? "[REDACTED: Safety policy]"
                            : wrapUntrustedContent && isUntrusted
                                ? $"<untrusted-content encoding=\"json\">\n{JsonSerializer.Serialize(textContent.Text)}\n</untrusted-content>"
                                : textContent.Text
                    )
                    : content
            ),
        ];

        return filteredMessage;
    }

    private static TextContent CreateTextContent(TextContent original, string text) =>
        new(text)
        {
            Annotations = original.Annotations,
            AdditionalProperties = original.AdditionalProperties,
            RawRepresentation = original.RawRepresentation,
        };
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
            return builder.Use(
                GuardrailMiddleware.InvokeAsync,
                GuardrailMiddleware.InvokeStreamingAsync
            );
        }
    }
}
