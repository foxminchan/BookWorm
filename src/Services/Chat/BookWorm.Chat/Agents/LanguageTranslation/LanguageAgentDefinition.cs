namespace BookWorm.Chat.Agents.LanguageTranslation;

internal static class LanguageAgentDefinition
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
}
