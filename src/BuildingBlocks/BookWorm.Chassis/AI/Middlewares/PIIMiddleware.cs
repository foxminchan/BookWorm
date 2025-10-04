using System.Text.RegularExpressions;
using Microsoft.Extensions.AI;

namespace BookWorm.Chassis.AI.Middlewares;

public static partial class PIIMiddleware
{
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

        return response;

        static IList<ChatMessage> FilterMessages(IEnumerable<ChatMessage> messages)
        {
            return [.. messages.Select(m => new ChatMessage(m.Role, FilterPii(m.Text)))];
        }

        static string FilterPii(string content)
        {
            Regex[] piiPatterns =
            [
                PhoneNumberRegex(),
                EmailRegex(),
                CreditCardRegex(),
                SsnRegex(),
                IpAddressRegex(),
            ];

            return piiPatterns.Aggregate(
                content,
                (current, pattern) => pattern.Replace(current, "[REDACTED: PII]")
            );
        }
    }

    [GeneratedRegex(
        @"\b(?:\+?1[-.\s]?)?\(?([0-9]{3})\)?[-.\s]?([0-9]{3})[-.\s]?([0-9]{4})\b",
        RegexOptions.Compiled
    )]
    private static partial Regex PhoneNumberRegex();

    [GeneratedRegex(@"\b[\w\.-]+@[\w\.-]+\.\w{2,}\b", RegexOptions.Compiled)]
    private static partial Regex EmailRegex();

    [GeneratedRegex(
        @"\b(?:4[0-9]{12}(?:[0-9]{3})?|5[1-5][0-9]{14}|3[47][0-9]{13}|6(?:011|5[0-9]{2})[0-9]{12})\b",
        RegexOptions.Compiled
    )]
    private static partial Regex CreditCardRegex();

    [GeneratedRegex(@"\b\d{3}[-\s]?\d{2}[-\s]?\d{4}\b", RegexOptions.Compiled)]
    private static partial Regex SsnRegex();

    [GeneratedRegex(
        @"\b(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\b|(?:[0-9a-fA-F]{1,4}:){7}[0-9a-fA-F]{1,4}\b",
        RegexOptions.Compiled
    )]
    private static partial Regex IpAddressRegex();
}
