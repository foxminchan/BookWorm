using System.Diagnostics.CodeAnalysis;
using A2A;
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

    private const string Instructions = """
        You are a text summarization assistant for BookWorm bookstore. Your role is to process English text from the Language Agent and create concise, meaningful summaries.

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

        Your summaries help the Book Agent understand user needs efficiently and provide better responses.
        """;

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

    public static AgentCard GetAgentCard()
    {
        var capabilities = new AgentCapabilities { Streaming = false, PushNotifications = false };

        var summarize = new AgentSkill
        {
            Id = "id_summarize_agent",
            Name = Name,
            Description = Description,
            Tags = ["summarization", "text-processing", "chat", "semantic-kernel"],
            Examples =
            [
                "Summarize this long customer message about book preferences.",
                "Extract the key points from this book review.",
                "Condense this user inquiry while preserving the main request.",
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
            Skills = [summarize],
        };
    }
}
