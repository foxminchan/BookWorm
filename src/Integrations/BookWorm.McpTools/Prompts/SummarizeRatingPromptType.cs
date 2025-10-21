using Microsoft.Extensions.AI;
using ModelContextProtocol.Server;

namespace BookWorm.McpTools.Prompts;

[McpServerPromptType]
public sealed class SummarizeRatingPromptType
{
    [McpMeta("category", "review_analysis")]
    [McpServerPrompt(Name = "summarize_rating", Title = "Summarize Rating")]
    [Description("Summarizes book review content for rating classification")]
    [return: Description("A structured summary of the book review content for rating classification")]
    public static ChatMessage SummarizeRatingPrompt(
        [Description("The book review content to be summarized")] string content
    )
    {
        return new(
            ChatRole.User,
            $"""
                Analyze and summarize the following book review content for rating classification:

                {content}

                Provide a comprehensive analysis including:
                - Overall sentiment and emotional tone
                - Key themes and recurring topics
                - Notable strengths or weaknesses mentioned
                - Quality indicators that support rating classification (Best Seller/Good/Bad)

                Format your response in a clear, structured manner suitable for product quality assessment.
            """
        );
    }
}
