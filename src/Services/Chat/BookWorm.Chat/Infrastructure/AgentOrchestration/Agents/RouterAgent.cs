namespace BookWorm.Chat.Infrastructure.AgentOrchestration.Agents;

internal static class RouterAgent
{
    public const string Name = Constants.Other.Agents.RouterAgent;

    public const string Description =
        "A routing agent that analyzes user requests and intelligently directs them to the appropriate specialized agent for optimal processing.";

    public const string Instructions = """
        You analyze BookWorm user requests and route to the best agent. NEVER handle requests directly—always hand off.

        Routing Priority (check in order):
        1. Non-English input → LanguageAgent
        2. Store questions (policies, services, account, billing) → QAAgent
        3. Emotional feedback/reviews (loved, hated, amazing, etc.) → SentimentAgent
        4. Very long messages (>200 words) → SummarizeAgent
        5. Book queries (search, recommendations, info) → BookAgent (default)

        Rules:
        - Choose the MOST DIRECT path for speed
        - When unsure between book/general, use BookAgent
        - Only use SummarizeAgent for genuinely verbose messages
        - Only use SentimentAgent for clear emotional content

        Examples:
        - "Show me Python books" → BookAgent
        - "What's your return policy?" → QAAgent
        - "Tôi muốn mua sách" → LanguageAgent
        - "I loved that book!" → SentimentAgent
        - [300-word message] → SummarizeAgent

        Goal: Fast, efficient routing to the right agent.
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
                    Id = "router_agent_intent_classification",
                    Tags = ["routing", "classification", "triage"],
                    Name = "Intent Classification",
                    Description = "Analyze user input to determine intent and optimal routing path",
                    Examples =
                    [
                        "Classify this user request for routing",
                        "Determine which agent should handle this query",
                        "Analyze the intent of this message",
                    ],
                },
                new()
                {
                    Id = "router_agent_language_detection",
                    Tags = ["language", "detection", "routing"],
                    Name = "Language-Based Routing",
                    Description = "Detect non-English input and route to translation services",
                    Examples =
                    [
                        "Detect if this message needs translation",
                        "Route non-English queries appropriately",
                        "Identify language and route accordingly",
                    ],
                },
                new()
                {
                    Id = "router_agent_complexity_analysis",
                    Tags = ["complexity", "analysis", "routing"],
                    Name = "Complexity Analysis",
                    Description =
                        "Evaluate message complexity and route long/verbose messages for summarization",
                    Examples =
                    [
                        "Determine if this message is too complex",
                        "Analyze message length for routing",
                        "Identify verbose messages needing summarization",
                    ],
                },
                new()
                {
                    Id = "router_agent_sentiment_detection",
                    Tags = ["sentiment", "emotion", "routing"],
                    Name = "Sentiment Detection",
                    Description =
                        "Identify emotional content and feedback requiring sentiment analysis",
                    Examples =
                    [
                        "Detect if this is emotional feedback",
                        "Identify sentiment-heavy messages",
                        "Route reviews and feedback appropriately",
                    ],
                },
            ],
            PreferredTransport = AgentTransport.JsonRpc,
        };
}
