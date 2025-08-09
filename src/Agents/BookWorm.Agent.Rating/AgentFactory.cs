using A2A;
using BookWorm.Chassis.RAG.A2A;
using BookWorm.Constants.Aspire;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.Ollama;
using ModelContextProtocol.Client;

namespace BookWorm.Agent.Rating;

public static class AgentFactory
{
    private const string Name = "RatingAgent";

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

    public static string GetAgentName() => Name;

    public static async Task<ChatCompletionAgent> CreateAgentAsync(Kernel kernel)
    {
        var agentKernel = kernel.Clone();
        var mcpClient = kernel.Services.GetRequiredService<IMcpClient>();
        var agent = kernel.Services.GetRequiredKeyedService<A2AAgent>(Agents.Summarize);

        var sentimentPlugin = KernelPluginFactory.CreateFromFunctions(
            Agents.Summarize,
            [AgentKernelFunctionFactory.CreateFromAgent(agent)]
        );

        var tools = await mcpClient.ListToolsAsync();

        agentKernel.Plugins.Add(sentimentPlugin);
        agentKernel.Plugins.AddFromFunctions(
            nameof(BookWorm),
            tools.Select(aiFunction => aiFunction.AsKernelFunction())
        );

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
            Provider = new() { Organization = nameof(BookWorm) },
            DefaultInputModes = ["text"],
            DefaultOutputModes = ["text"],
            Capabilities = capabilities,
            Skills = [rating],
        };
    }
}
