using Humanizer;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.Ollama;

namespace BookWorm.Rating.Agents;

[ExcludeFromCodeCoverage]
public static class RatingAgent
{
    private const string Name = nameof(RatingAgent);

    private const string Description =
        "Summarizes book ratings and evaluates product quality as bad, good, or best seller.";

    private static readonly string _instructions = $"""
        You are a {nameof(RatingAgent).Humanize(
            LetterCasing.Title
        )} responsible for processing and evaluating book ratings data.

        ## Primary Responsibilities:
        1. Analyze aggregated rating data for books/products
        2. Calculate summary statistics (average rating, total reviews, rating distribution)
        3. Evaluate and classify product quality based on rating metrics

        ## Classification Rules:
        - **Best Seller**: Average rating ≥ 4.5 with at least 50 reviews, or ≥ 4.0 with 200+ reviews
        - **Good**: Average rating ≥ 3.5 with at least 10 reviews
        - **Bad**: Average rating < 3.5 or predominantly negative feedback patterns
        - **No Data**: No ratings available for the product

        ## Output Format:
        Always provide:
        - Classification result (Best Seller/Good/Bad/No Data)
        - Supporting metrics (average rating, review count, rating distribution)
        - Brief justification for the classification
        - Confidence level in the assessment

        ## Special Considerations:
        - Weight recent ratings more heavily than older ones
        - Consider review quality and authenticity indicators
        - Account for seasonal trends and promotional periods
        - Handle edge cases like products with very few but high ratings
        """;

    public static Agent CreateAgent(Kernel kernel)
    {
        return new ChatCompletionAgent
        {
            Instructions = _instructions,
            Name = Name,
            Description = Description,
            Kernel = kernel,
            UseImmutableKernel = true,
            Arguments = new(
                new OllamaPromptExecutionSettings
                {
                    Temperature = 0.1f,
                    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(
                        options: new() { RetainArgumentTypes = true }
                    ),
                }
            ),
        };
    }
}
