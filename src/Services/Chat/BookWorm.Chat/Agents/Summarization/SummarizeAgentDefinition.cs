namespace BookWorm.Chat.Agents.Summarization;

internal static class SummarizeAgentDefinition
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
}
