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
        @"(?:reveal|repeat|show|print|provide)\s+(?:(?:your|the)\s+prompt"
            + @"|(?:(?:your|the)\s+)?(?:system|developer)\s+(?:prompt|instructions?|message))"
            + @"|(?:system|developer)\s+prompt\s*:",
        RegexOptions.IgnoreCase | RegexOptions.Compiled
    )]
    private static partial Regex PromptLeakagePatternRegex();

    [GeneratedRegex(
        @"\b(?:harmful|illegal|violence|weapon|drug|abuse|hate|discrimination)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant
    )]
    private static partial Regex ForbiddenContentRegex();

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

        static List<ChatMessage> FilterMessages(
            IEnumerable<ChatMessage> contents,
            bool wrapUntrustedContent = true
        ) =>
            [
                .. contents.Select(message => new ChatMessage(
                    message.Role,
                    FilterContent(message, wrapUntrustedContent)
                )),
            ];

        static string FilterContent(ChatMessage message, bool wrapUntrustedContent)
        {
            var content = message.Text;
            var isUntrusted = message.Role == ChatRole.User || message.Role == ChatRole.Tool;

            if (
                (isUntrusted && InjectionPatternRegex().IsMatch(content))
                || (!isUntrusted && PromptLeakagePatternRegex().IsMatch(content))
                || ForbiddenContentRegex().IsMatch(content)
            )
            {
                return "[REDACTED: Safety policy]";
            }

            return wrapUntrustedContent && isUntrusted
                ? $"<untrusted-content>\n{content}\n</untrusted-content>"
                : content;
        }
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
