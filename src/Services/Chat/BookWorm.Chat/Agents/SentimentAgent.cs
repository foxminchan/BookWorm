using System.Diagnostics.CodeAnalysis;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.Ollama;
using SharpA2A.Core;

namespace BookWorm.Chat.Agents;

[ExcludeFromCodeCoverage]
public static class SentimentAgent
{
    private const string Name = nameof(SentimentAgent);

    private const string Description =
        "An agent that evaluates the sentiment of translated English text as negative, positive, or neutral.";

    private const string Instructions = """
        You are a sentiment analysis assistant for BookWorm bookstore. Your role is to evaluate the emotional tone of user messages that have been processed by the Summarize Agent.

        **Sentiment Analysis:**
        - Analyze the English text received from the Summarize Agent
        - Classify sentiment as: Positive, Negative, or Neutral
        - Consider context and nuance when making assessments
        - Pay attention to book-related emotions and customer satisfaction indicators

        **Analysis Criteria:**
        - **Positive**: Happy, satisfied, excited, pleased, enthusiastic about books/service
        - **Negative**: Frustrated, disappointed, angry, dissatisfied, complaints
        - **Neutral**: Informational, factual, questions without emotional tone

        **Output Requirements:**
        - Provide clear sentiment classification (Positive/Negative/Neutral)
        - Include confidence level when possible
        - Brief explanation of reasoning behind the sentiment assessment
        - Consider customer service context when evaluating sentiment

        Your analysis helps the Book Agent understand the user's emotional state to provide appropriate responses.
        """;

    /// <summary>
    /// Creates and configures a ChatCompletionAgent for sentiment analysis of English text in a bookstore context.
    /// </summary>
    /// <param name="kernel">The Semantic Kernel instance to be used by the agent.</param>
    /// <returns>A ChatCompletionAgent initialized with sentiment analysis instructions, metadata, and prompt execution settings.</returns>
    public static ChatCompletionAgent CreateAgent(Kernel kernel)
    {
        return new()
        {
            Instructions = Instructions,
            Name = Name,
            Description = Description,
            Kernel = kernel,
            Arguments = new(new OllamaPromptExecutionSettings { Temperature = 0.2f }),
        };
    }

    /// <summary>
    /// Returns an <see cref="AgentCard"/> describing the SentimentAgent's metadata, capabilities, skills, tags, and example prompts for sentiment analysis tasks.
    /// </summary>
    /// <returns>An <see cref="AgentCard"/> containing the agent's name, description, version, input/output modes, capabilities, and skills.</returns>
    public static AgentCard GetAgentCard()
    {
        var capabilities = new AgentCapabilities { Streaming = false, PushNotifications = false };

        var sentiment = new AgentSkill
        {
            Id = "id_sentiment_agent",
            Name = Name,
            Description = Description,
            Tags = ["sentiment", "analysis", "chat", "semantic-kernel"],
            Examples =
            [
                "Analyze the sentiment of this customer feedback about a book.",
                "Is this review positive or negative?",
                "What is the emotional tone of this message?",
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
            Skills = [sentiment],
        };
    }
}
