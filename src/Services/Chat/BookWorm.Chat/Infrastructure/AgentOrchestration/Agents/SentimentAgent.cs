using A2A;
using BookWorm.Chassis.Security.Settings;
using BookWorm.Chassis.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;

namespace BookWorm.Chat.Infrastructure.AgentOrchestration.Agents;

internal static class SentimentAgent
{
    public const string Name = Constants.Other.Agents.SentimentAgent;

    public const string Description =
        "An agent that evaluates the sentiment of translated English text as negative, positive, or neutral.";

    public const string Instructions = """
        You are a sentiment analysis assistant for BookWorm bookstore. Your role is to evaluate the emotional tone of user messages.

        **Sentiment Analysis:**
        - Analyze the emotional tone of user input
        - Classify sentiment as: Positive, Negative, or Neutral
        - Consider context and nuance when making assessments
        - Pay attention to book-related emotions and customer satisfaction indicators

        **Analysis Criteria:**
        - **Positive**: Happy, satisfied, excited, pleased, enthusiastic about books/service
        - **Negative**: Frustrated, disappointed, angry, dissatisfied, complaints
        - **Neutral**: Informational, factual, questions without emotional tone

        **Output Requirements:**
        - Provide clear sentiment classification (Positive/Negative/Neutral)
        - Include confidence level when possible
        - Brief explanation of reasoning behind the sentiment assessment
        - Consider customer service context when evaluating sentiment

        **Handoff Strategy:**
        - After analyzing sentiment, hand off to BookAgent if the message contains book-related queries
        - Hand off back to RouterAgent if sentiment analysis reveals the message needs different handling
        - Include your sentiment analysis in the handoff context

        Your analysis helps the Book Agent understand the user's emotional state to provide appropriate responses.
        """;

    public static AgentCard AgentCard { get; } =
        new()
        {
            Name = Name,
            Url = $"http://localhost:5026/a2a/{Name}",
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
            SecuritySchemes = new()
            {
                [OAuthDefaults.DisplayName] = new OAuth2SecurityScheme(
                    new()
                    {
                        ClientCredentials = new(
                            new(
                                ServiceDiscoveryUtilities.GetServiceEndpoint(Components.KeyCloak)
                                    + "/realms"
                                    + Environment.GetEnvironmentVariable(
                                        $"{IdentityOptions.ConfigurationSection}__{nameof(IdentityOptions.Realm)}"
                                    )
                                    + "/protocol/openid-connect/token"
                            ),
                            new Dictionary<string, string>
                            {
                                {
                                    $"{Services.Chatting}_{Authorization.Actions.Read}",
                                    "Read access to chat service"
                                },
                                {
                                    $"{Services.Chatting}_{Authorization.Actions.Write}",
                                    "Write access to chat service"
                                },
                            }
                        ),
                    },
                    "OAuth2 security scheme for the BookWorm API"
                ),
            },
            Security =
            [
                new()
                {
                    [$"{JwtBearerDefaults.AuthenticationScheme}"] =
                    [
                        $"{Services.Chatting}_{Authorization.Actions.Read}",
                        $"{Services.Chatting}_{Authorization.Actions.Write}",
                    ],
                },
            ],
            PreferredTransport = AgentTransport.JsonRpc,
        };
}
