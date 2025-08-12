using A2A;
using BookWorm.Chassis.RAG.Extensions;
using BookWorm.Rating.Plugins;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.Ollama;

namespace BookWorm.Rating.Infrastructure.Agents;

public static class RatingAgent
{
    private const string Name = Constants.Other.Agents.RatingAgent;

    private const string Description =
        "Summarizes book ratings and evaluates product quality as bad, good, or best seller.";

    private const string Instructions = """
        You are a Rating Agent responsible for processing and evaluating book ratings data with contextual intelligence.

        **Primary Responsibilities**:
        1. Analyze aggregated rating data for books/products using contextually selected functions
        2. Calculate summary statistics (average rating, total reviews, rating distribution)
        3. Evaluate and classify product quality based on rating metrics
        4. Provide sentiment analysis and content summarization

        **Available Functions for Contextual Selection**:
        - **GetCustomerReviews**: Retrieves raw customer review data for analysis
        - **SummarizeAgent functions**: Provides advanced text processing, summarization, and sentiment analysis

        **Contextual Function Usage Guidelines**:
        - Use GetCustomerReviews when you need actual customer feedback data
        - Use SummarizeAgent functions for content analysis, sentiment evaluation, and text processing
        - Select functions based on the specific analysis requirements and context
        - Combine multiple functions for comprehensive rating assessment

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
        - Use contextual intelligence to select the most relevant functions for each analysis task
        """;

    public static ChatCompletionAgent CreateAgent(Kernel kernel)
    {
        var agentKernel = kernel.Clone();

        var reviewPlugin = new ReviewPlugin(kernel.Services.GetRequiredService<ISender>());
        agentKernel.Plugins.AddFromObject(reviewPlugin);

        var summarizePlugin = agentKernel.MapToAgentPlugin(Constants.Other.Agents.SummarizeAgent);
        agentKernel.Plugins.Add(summarizePlugin);

        return new()
        {
            Instructions = Instructions,
            Name = Name,
            Description = Description,
            Kernel = agentKernel,
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

    public static AgentCard GetAgentCard()
    {
        var capabilities = new AgentCapabilities { Streaming = false, PushNotifications = false };

        var rating = new AgentSkill
        {
            Id = "id_rating_agent",
            Name = Name,
            Description = Description,
            Tags = ["rating", "book", "semantic-kernel", "contextual-selection", "a2a"],
            Examples =
            [
                "What is the average rating for the book 'The Great Gatsby'?",
                "Classify the product quality of '1984' by George Orwell.",
                "How many reviews does 'To Kill a Mockingbird' have?",
                "Analyze customer sentiment for reviews of a specific book.",
                "Provide comprehensive rating analysis with quality classification.",
            ],
        };

        return new()
        {
            Name = Name,
            Description =
                "Rating analysis agent with contextual function selection using existing plugins and A2A integration",
            Version = "1.0.0",
            Provider = new() { Organization = nameof(BookWorm) },
            DefaultInputModes = ["text"],
            DefaultOutputModes = ["text"],
            Capabilities = capabilities,
            Skills = [rating],
        };
    }
}
