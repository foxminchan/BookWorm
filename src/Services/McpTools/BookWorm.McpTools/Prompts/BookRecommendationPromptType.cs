using Microsoft.Extensions.AI;
using ModelContextProtocol.Server;

namespace BookWorm.McpTools.Prompts;

[McpServerPromptType]
public sealed class BookRecommendationPromptType
{
    [McpMeta("category", "recommendation")]
    [McpServerPrompt(Name = "recommend_books", Title = "Recommend Books")]
    [Description("Generates a structured book recommendation request for catalog search")]
    [return: Description("A structured prompt for book recommendation based on user preferences")]
    public static ChatMessage RecommendBooksPrompt(
        [Description("The preferred genre or topic (e.g., 'science fiction', 'self-help')")]
            string? genre = null,
        [Description("The price range in format 'min-max' (e.g., '10-30')")]
            string? priceRange = null,
        [Description("Comma-separated list of preferred authors")] string? preferredAuthors = null,
        [Description(
            "A profile description of the reader (e.g., 'beginner programmer', 'history enthusiast')"
        )]
            string? readerProfile = null
    )
    {
        var constraints = new List<string>();

        if (!string.IsNullOrWhiteSpace(genre))
        {
            constraints.Add($"- Genre/Topic: {genre}");
        }

        if (!string.IsNullOrWhiteSpace(priceRange))
        {
            constraints.Add($"- Price range: ${priceRange}");
        }

        if (!string.IsNullOrWhiteSpace(preferredAuthors))
        {
            constraints.Add($"- Preferred authors: {preferredAuthors}");
        }

        if (!string.IsNullOrWhiteSpace(readerProfile))
        {
            constraints.Add($"- Reader profile: {readerProfile}");
        }

        var constraintText =
            constraints.Count > 0
                ? string.Join("\n", constraints)
                : "- No specific constraints (recommend broadly)";

        return new(
            ChatRole.User,
            $"""
                Search the BookWorm catalog and recommend suitable books based on the following preferences:

                {constraintText}

                Use the search_catalog tool to find matching books, then:
                1. Evaluate each book's relevance to the stated preferences
                2. Consider the average rating and total reviews as quality signals
                3. Present the top 3-5 recommendations with:
                   - Title, author(s), and price
                   - A brief explanation of why it matches the preferences
                   - Rating summary (average rating / total reviews)
                4. Order recommendations by best fit first

                If no books match the criteria, suggest broadening the search parameters.
            """
        );
    }
}
