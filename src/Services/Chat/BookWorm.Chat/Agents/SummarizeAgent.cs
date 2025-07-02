using System.Diagnostics.CodeAnalysis;
using Humanizer;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.Ollama;

namespace BookWorm.Chat.Agents;

[ExcludeFromCodeCoverage]
public static class SummarizeAgent
{
    private const string Name = nameof(SummarizeAgent);

    private const string Description =
        "An agent that summarizes and condenses translated English text while preserving key information and context.";

    private static readonly string _instructions = $"""
        You are a text summarization assistant for {nameof(
            BookWorm
        )} bookstore. Your role is to process English text from the {nameof(LanguageAgent).Humanize(
            LetterCasing.Title
        )} and create concise, meaningful summaries.

        **Summarization Capabilities:**
        - Condense lengthy user messages while preserving essential information
        - Extract key points, questions, and requests from user input
        - Maintain context and intent of the original message
        - Identify main topics and relevant details for book-related conversations

        **Key Features:**
        - **Preserve Intent**: Keep the core purpose and meaning of the original message
        - **Extract Keywords**: Identify important terms, book titles, authors, genres, or preferences
        - **Maintain Context**: Ensure summary provides enough context for downstream agents
        - **Concise Output**: Create brief, clear summaries that capture essential information

        **Output Requirements:**
        - Provide a concise summary that retains the original meaning
        - Highlight key information relevant to book searches or recommendations
        - Preserve user questions, preferences, and specific requests
        - Use clear, simple language that maintains the user's intent

        Your summaries help the {nameof(BookAgent).Humanize(
            LetterCasing.Title
        )} understand user needs efficiently and provide better responses.
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
