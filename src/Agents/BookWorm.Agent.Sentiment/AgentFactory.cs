using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.Ollama;

namespace BookWorm.Agent.Sentiment;

public static class AgentFactory
{
    private const string Name = "SentimentAgent";

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

    public static string GetAgentName() => Name;

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
}
