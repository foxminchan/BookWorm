namespace BookWorm.Chat.Infrastructure.AgentOrchestration.Agents;

internal static class SummarizeAgent
{
    public const string Name = Constants.Other.Agents.SummarizeAgent;

    public const string Description =
        "An agent that summarizes and condenses translated English text while preserving key information and context.";

    public const string Instructions = """
        You condense lengthy BookWorm user messages into concise summaries for efficient processing.

        Tasks:
        - Condense long messages while preserving intent and context
        - Extract key points: questions, requests, book titles, authors, genres, preferences
        - Maintain original meaning in clear, simple language

        Output:
        - Brief summary retaining essential information
        - Highlight book-related details for searches/recommendations
        - Preserve user questions and specific requests

        Handoff:
        - After summarizing, ALWAYS hand off to BookAgent with condensed summary

        Goal: Help BookAgent understand user needs efficiently.
        """;

    public static AgentCard AgentCard { get; } =
        new()
        {
            Name = Name,
            Description = Description,
            Version = "1.0",
            Provider = new() { Organization = nameof(OpenAI), Url = "https://openai.com/" },
            DefaultInputModes = ["text"],
            DefaultOutputModes = ["text"],
            Capabilities = new() { Streaming = false, PushNotifications = false },
            Skills =
            [
                new()
                {
                    Id = "summarize_agent_text_summarization",
                    Tags = ["summarization", "condensing", "text-processing"],
                    Name = "Text Summarization",
                    Description =
                        "Condense lengthy user messages while preserving essential information and intent",
                    Examples =
                    [
                        "Summarize this long customer message about book preferences.",
                        "Condense this user inquiry while preserving the main request.",
                        "Create a brief summary of this conversation",
                    ],
                },
                new()
                {
                    Id = "summarize_agent_key_point_extraction",
                    Tags = ["extraction", "key-points", "analysis"],
                    Name = "Key Point Extraction",
                    Description =
                        "Extract key points, questions, and requests from user input including book titles, authors, and genres",
                    Examples =
                    [
                        "Extract the key points from this book review.",
                        "What are the main topics in this user message?",
                        "Identify the book preferences from this text",
                    ],
                },
                new()
                {
                    Id = "summarize_agent_context_preservation",
                    Tags = ["context", "intent", "preservation"],
                    Name = "Context Preservation",
                    Description =
                        "Maintain context and intent while creating concise summaries for downstream agents",
                    Examples =
                    [
                        "Summarize this while keeping the context for the Book Agent",
                        "Preserve the user's intent in a shorter format",
                        "Create a context-aware summary of this request",
                    ],
                },
            ],
            PreferredTransport = AgentTransport.JsonRpc,
        };
}
