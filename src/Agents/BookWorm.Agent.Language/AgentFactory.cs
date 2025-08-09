using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.Ollama;

namespace BookWorm.Agent.Language;

public static class AgentFactory
{
    private const string Name = "LanguageAgent";

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
        - Provide ONLY the translated English text for non-English inputs
        - Keep English inputs unchanged
        - Do NOT provide multiple translation options
        - Do NOT include explanations, alternatives, or additional commentary
        - Output should be the single most natural and clear translation

        Your goal is to ensure all user communications are accessible in English for proper processing by other agents in the system.
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
            Arguments = new(new OllamaPromptExecutionSettings { Temperature = 0.1f }),
        };
    }
}
