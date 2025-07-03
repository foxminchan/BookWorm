using System.Diagnostics.CodeAnalysis;
using Humanizer;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.Ollama;

namespace BookWorm.Chat.Agents;

[ExcludeFromCodeCoverage]
public static class SentimentAgent
{
    private const string Name = nameof(SentimentAgent);

    private const string Description =
        "An agent that evaluates the sentiment of translated English text as negative, positive, or neutral.";

    private static readonly string _instructions = $"""
                                                    You are a sentiment analysis assistant for BookWorm bookstore. Your role is to evaluate the emotional tone of user messages that have been processed by the {nameof(
                                                        SummarizeAgent
                                                    ).Humanize(LetterCasing.Title)}.

                                                    **Sentiment Analysis:**
                                                    - Analyze the English text received from the {nameof(SummarizeAgent).Humanize(
                                                        LetterCasing.Title
                                                    )}
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

                                                    Your analysis helps the {nameof(BookAgent).Humanize(LetterCasing.Title)} understand the user's emotional state to provide appropriate responses.
                                                    """;

    public static Agent CreateAgent(Kernel kernel)
    {
        return new ChatCompletionAgent
        {
            Instructions = _instructions,
            Name = Name,
            Description = Description,
            Kernel = kernel,
            Arguments = new(new OllamaPromptExecutionSettings { Temperature = 0.2f }),
        };
    }
}
