using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.Ollama;
using SharpA2A.Core;

namespace BookWorm.Rating.Agents;

[ExcludeFromCodeCoverage]
public static class RatingAgent
{
    private const string Name = nameof(RatingAgent);

    private const string Description =
        "Summarizes book ratings and evaluates product quality as bad, good, or best seller.";

    private const string Instructions = """
        You are a Rating Agent responsible for processing and evaluating book ratings data.

        **Primary Responsibilities**:
        1. Analyze aggregated rating data for books/products
        2. Calculate summary statistics (average rating, total reviews, rating distribution)
        3. Evaluate and classify product quality based on rating metrics

        **Classification Rules**:
        - **Best Seller**: Average rating ≥ 4.5 with at least 50 reviews, or ≥ 4.0 with 200+ reviews
        - **Good**: Average rating ≥ 3.5 with at least 10 reviews
        - **Bad**: Average rating < 3.5 or predominantly negative feedback patterns
        - **No Data**: No ratings available for the product

        **Output Format**:
        Always provide:
        - Classification result (Best Seller/Good/Bad/No Data)
        - Supporting metrics (average rating, review count, rating distribution)
        - Brief justification for the classification
        - Confidence level in the assessment

        **Special Considerations**:
        - Weight recent ratings more heavily than older ones
        - Consider review quality and authenticity indicators
        - Account for seasonal trends and promotional periods
        - Handle edge cases like products with very few but high ratings
        """;

    /// <summary>
    /// Creates and configures a chat completion agent specialized in analyzing and classifying book rating data.
    /// </summary>
    /// <param name="kernel">The semantic kernel instance to be used by the agent.</param>
    /// <returns>A <see cref="ChatCompletionAgent"/> preconfigured with instructions, metadata, and prompt execution settings for book rating analysis.</returns>
    public static ChatCompletionAgent CreateAgent(Kernel kernel)
    {
        return new()
        {
            Instructions = Instructions,
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

    /// <summary>
    /// Returns an <see cref="AgentCard"/> describing the RatingAgent's metadata, capabilities, and supported skills for integration or UI display.
    /// </summary>
    /// <returns>An <see cref="AgentCard"/> containing the agent's name, description, version, input/output modes, capabilities, and example queries related to book rating analysis.</returns>
    public static AgentCard GetAgentCard()
    {
        var capabilities = new AgentCapabilities { Streaming = false, PushNotifications = false };

        var rating = new AgentSkill
        {
            Id = "id_rating_agent",
            Name = Name,
            Description = Description,
            Tags = ["rating", "book", "semantic-kernel"],
            Examples =
            [
                "What is the average rating for the book 'The Great Gatsby'?",
                "Classify the product quality of '1984' by George Orwell.",
                "How many reviews does 'To Kill a Mockingbird' have?",
            ],
        };

        return new()
        {
            Name = Name,
            Description = Description,
            Version = "1.0.0",
            DefaultInputModes = ["text"],
            DefaultOutputModes = ["text"],
            Capabilities = capabilities,
            Skills = [rating],
        };
    }
}
