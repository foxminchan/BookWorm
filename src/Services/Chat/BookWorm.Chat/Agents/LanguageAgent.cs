using System.Diagnostics.CodeAnalysis;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.Ollama;

namespace BookWorm.Chat.Agents;

[ExcludeFromCodeCoverage]
public static class LanguageAgent
{
    private const string Name = nameof(LanguageAgent);

    private const string Description =
        "An agent that detects user input language and translates it to English for better context understanding.";

    private const string Instructions = """
        You are a language detection and translation assistant for BookWorm bookstore. Your primary responsibilities are:

        **Language Detection:**
        - Automatically detect the language of user input
        - Identify whether the text is in English or another language

        **Translation to English:**
        - If user input is not in English, translate it to clear, understandable English
        - Preserve the original meaning and context during translation
        - Maintain the intent and tone of the original message
        - Ensure translations are natural and grammatically correct

        **Output Format:**
        - Always provide the translated English version for non-English inputs
        - Keep English inputs unchanged
        - Focus on clarity and comprehension to help downstream agents understand the context better

        Your goal is to ensure all user communications are accessible in English for proper processing by other agents in the system.
        """;

    public static Agent CreateAgent(Kernel kernel)
    {
        return new ChatCompletionAgent
        {
            Instructions = Instructions,
            Name = Name,
            Description = Description,
            Kernel = kernel,
            Arguments = new(new OllamaPromptExecutionSettings { Temperature = 0.1f }),
        };
    }
}
