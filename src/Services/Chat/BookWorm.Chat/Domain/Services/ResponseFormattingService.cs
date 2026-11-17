using System.Text.RegularExpressions;
using BookWorm.Chat.Domain.AggregatesModel.ConversationAggregate;

namespace BookWorm.Chat.Domain.Services;

public static partial class ResponseFormattingService
{
    [GeneratedRegex(@"\n{3,}")]
    private static partial Regex ExcessiveNewlinesRegex();

    [GeneratedRegex(
        @"(\[system\]|<<SYS>>|<\|im_start\|>system|###\s*System\s*Prompt|\[INST\])[\s\S]{0,200}",
        RegexOptions.IgnoreCase
    )]
    private static partial Regex SystemPromptLeakRegex();

    public static AssistantResponse Format(string? rawContent)
    {
        if (string.IsNullOrWhiteSpace(rawContent))
        {
            return AssistantResponse.Fallback();
        }

        var content = SystemPromptLeakRegex().Replace(rawContent.Trim(), string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(content))
        {
            return AssistantResponse.Fallback();
        }

        if (content[^1] is not ('.' or '!' or '?' or '"' or ')'))
        {
            content += ".";
        }

        content = ExcessiveNewlinesRegex().Replace(content, "\n\n").Trim();

        return AssistantResponse.Create(content);
    }
}
