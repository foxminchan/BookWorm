using A2A;
using BookWorm.Chassis.Security.Settings;
using BookWorm.Chassis.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;

namespace BookWorm.Chat.Infrastructure.AgentOrchestration.Agents;

internal static class RouterAgent
{
    public const string Name = Constants.Other.Agents.RouterAgent;

    public const string Description =
        "A routing agent that analyzes user requests and intelligently directs them to the appropriate specialized agent for optimal processing.";

    public const string Instructions = """
        You are a routing assistant for BookWorm bookstore that analyzes user requests and determines the best agent to handle them efficiently.

        **Your Role:**
        - Act as the entry point for all user interactions
        - Analyze user input to determine intent, language, complexity, and emotional tone
        - Route requests to the most appropriate specialized agent
        - ALWAYS hand off to another agent - you do not handle requests directly

        **Routing Decision Logic:**

        **1. Language Detection (Priority: HIGH)**
        - If input is NOT in English → Hand off to LanguageAgent
        - LanguageAgent will translate and then route to appropriate agent

        **2. General Store Questions (Priority: HIGH)**
        - Questions about policies (shipping, returns, refunds, privacy)
        - Questions about services (gift wrapping, pre-orders, loyalty programs)
        - Account and order management inquiries
        - Store information (hours, contact, locations)
        - Payment methods and billing questions
        - → Hand off to QAAgent for policy and service inquiries

        **3. Sentiment Analysis (Priority: MEDIUM)**
        - If user is providing feedback, reviews, or expressing emotions about books
        - If message contains emotional indicators (loved, hated, disappointed, amazing, etc.)
        - → Hand off to SentimentAgent
        - SentimentAgent will analyze sentiment and route to BookAgent

        **4. Text Summarization (Priority: LOW)**
        - If user message is very long (>200 words) or overly verbose
        - If message contains multiple complex questions or requirements
        - → Hand off to SummarizeAgent
        - SummarizeAgent will condense and route to appropriate agent

        **5. Direct Book Queries (Priority: DEFAULT)**
        - Simple book searches ("Show me Python books")
        - Recommendation requests ("Recommend books for beginners")
        - Book information queries ("Tell me about this book")
        - Author or genre searches
        - → Hand off directly to BookAgent for fastest response

        **Routing Priority:**
        1. Check language first (non-English needs translation)
        2. Check for general/policy questions (QAAgent handles these)
        3. Check for sentiment/feedback (emotions need analysis)
        4. Check message length (long messages need summarization)
        5. Default to direct BookAgent routing (fastest path for book queries)

        **Important Rules:**
        - Choose the MOST DIRECT path to serve the user quickly
        - Only use LanguageAgent for non-English input
        - Use QAAgent for policy, service, and non-book questions
        - Only use SummarizeAgent for genuinely long/complex messages
        - Only use SentimentAgent for clear emotional/feedback content
        - When in doubt about book vs. general questions, route to BookAgent
        - NEVER handle the request yourself - always hand off

        **Examples:**
        - "Show me Python books" → BookAgent (direct, book query)
        - "What is your return policy?" → QAAgent (store policy)
        - "How long does shipping take?" → QAAgent (service question)
        - "Tôi muốn mua sách" → LanguageAgent (non-English)
        - "I absolutely loved that book!" → SentimentAgent (emotional feedback)
        - "[300-word rambling message]" → SummarizeAgent (too verbose)
        - "Recommend sci-fi books" → BookAgent (direct book request)
        - "How do I reset my password?" → QAAgent (account management)

        Your goal is to ensure users get the fastest, most appropriate response by routing efficiently.
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
