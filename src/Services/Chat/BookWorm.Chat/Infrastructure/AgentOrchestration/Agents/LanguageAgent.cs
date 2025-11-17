namespace BookWorm.Chat.Infrastructure.AgentOrchestration.Agents;

internal static class LanguageAgent
{
    public const string Name = Constants.Other.Agents.LanguageAgent;

    public const string Description =
        "An agent that detects user input language and translates it to English for better context understanding.";

    public const string Instructions = """
        You detect language and translate non-English text to clear English for BookWorm bookstore.

        Rules:
        - Detect if input is English or another language
        - Translate non-English to natural English, preserving meaning, intent, and tone
        - Return ONLY the translated text—no explanations or alternatives
        - Keep English inputs unchanged
        - Always hand off to BookAgent after translation

        Goal: Make all communications accessible in English for BookAgent processing.
        """;

    public static AgentCard AgentCard { get; } =
        new()
        {
            Name = Name,
            Description = Description,
            Version = "1.0",
            Provider = new()
            {
                Organization = nameof(BookWorm),
                Url = "https://github.com/foxminchan/BookWorm",
            },
            DefaultInputModes = ["text"],
            DefaultOutputModes = ["text"],
            Capabilities = new() { Streaming = false, PushNotifications = false },
            Skills =
            [
                new()
                {
                    Id = "language_agent_language_detection",
                    Tags = ["detection", "language", "identification"],
                    Name = "Language Detection",
                    Description =
                        "Automatically detect the language of user input and identify if translation is needed",
                    Examples =
                    [
                        "Detect the language of this text",
                        "Is this message in English?",
                        "What language is the user speaking?",
                    ],
                },
                new()
                {
                    Id = "language_agent_translation_to_english",
                    Tags = ["translation", "english", "conversion"],
                    Name = "Translation to English",
                    Description =
                        "Translate non-English text to clear, natural English while preserving meaning and context",
                    Examples =
                    [
                        "Translate this Spanish text to English",
                        "Convert this French message to English",
                        "Provide an English translation of this user input",
                    ],
                },
                new()
                {
                    Id = "language_agent_context_preservation",
                    Tags = ["context", "intent", "tone"],
                    Name = "Context Preservation",
                    Description =
                        "Maintain the original intent, tone, and context during translation for accurate processing",
                    Examples =
                    [
                        "Translate while preserving the user's intent",
                        "Keep the original meaning in the English translation",
                        "Ensure the tone is maintained in translation",
                    ],
                },
            ],
            PreferredTransport = AgentTransport.JsonRpc,
        };
}
