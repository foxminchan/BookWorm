namespace BookWorm.Chat.Infrastructure.AgentOrchestration.Agents;

internal static class SentimentAgent
{
    public const string Name = Constants.Other.Agents.SentimentAgent;

    public const string Description =
        "An agent that evaluates the sentiment of translated English text as negative, positive, or neutral.";

    public const string Instructions = """
        You analyze emotional tone of BookWorm user messages and classify as Positive, Negative, or Neutral.

        Classification:
        - Positive: Happy, satisfied, excited, enthusiastic about books/service
        - Negative: Frustrated, disappointed, angry, dissatisfied, complaints
        - Neutral: Informational, factual, no emotional tone

        Output:
        - Sentiment classification (Positive/Negative/Neutral)
        - Confidence level if possible
        - Brief reasoning

        Handoff:
        - After analysis, hand off to BookAgent for book queries
        - Hand off to RouterAgent if message needs different handling
        - Include sentiment context in handoff

        Goal: Help BookAgent understand user's emotional state for appropriate responses.
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
                    Id = "sentiment_agent_emotion_classification",
                    Tags = ["sentiment", "emotion", "classification"],
                    Name = "Emotion Classification",
                    Description =
                        "Classify the emotional tone of user messages as positive, negative, or neutral",
                    Examples =
                    [
                        "Analyze the sentiment of this customer feedback",
                        "Classify the emotional tone of this review",
                        "Is this message positive or negative?",
                    ],
                },
                new()
                {
                    Id = "sentiment_agent_customer_satisfaction",
                    Tags = ["satisfaction", "feedback", "analysis"],
                    Name = "Customer Satisfaction Analysis",
                    Description =
                        "Evaluate customer satisfaction indicators in feedback and reviews",
                    Examples =
                    [
                        "Analyze customer satisfaction from this review",
                        "Evaluate this feedback for satisfaction indicators",
                        "Determine customer happiness level",
                    ],
                },
                new()
                {
                    Id = "sentiment_agent_context_aware_analysis",
                    Tags = ["context", "nuance", "analysis"],
                    Name = "Context-Aware Analysis",
                    Description =
                        "Perform nuanced sentiment analysis considering context and book-related emotions",
                    Examples =
                    [
                        "Analyze sentiment considering the book context",
                        "Evaluate emotion with contextual understanding",
                        "Provide nuanced sentiment analysis",
                    ],
                },
            ],
            PreferredTransport = AgentTransport.JsonRpc,
        };
}
