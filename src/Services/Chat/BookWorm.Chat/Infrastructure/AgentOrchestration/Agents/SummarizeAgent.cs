using A2A;
using BookWorm.Chassis.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;

namespace BookWorm.Chat.Infrastructure.AgentOrchestration.Agents;

internal static class SummarizeAgent
{
    public const string Name = Constants.Other.Agents.SummarizeAgent;

    public const string Description =
        "An agent that summarizes and condenses translated English text while preserving key information and context.";

    public const string Instructions = """
        You are a text summarization assistant for BookWorm bookstore. Your role is to process English text from the Language Agent and create concise, meaningful summaries.

        **Summarization Capabilities:**
        - Condense lengthy user messages while preserving essential information
        - Extract key points, questions, and requests from user input
        - Maintain context and intent of the original message
        - Identify main topics and relevant details for book-related conversations

        **Key Features:**
        - **Preserve Intent**: Keep the core purpose and meaning of the original message
        - **Extract Keywords**: Identify important terms, book titles, authors, genres, or preferences
        - **Maintain Context**: Ensure summary provides enough context for downstream agents
        - **Concise Output**: Create brief, clear summaries that capture essential information

        **Output Requirements:**
        - Provide a concise summary that retains the original meaning
        - Highlight key information relevant to book searches or recommendations
        - Preserve user questions, preferences, and specific requests
        - Use clear, simple language that maintains the user's intent

        **Handoff Strategy:**
        - After summarizing lengthy or complex messages, ALWAYS hand off to BookAgent
        - BookAgent will use your condensed summary to efficiently process the request

        Your summaries help the Book Agent understand user needs efficiently and provide better responses.
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
            SecuritySchemes = new()
            {
                [OAuthDefaults.DisplayName] = new OAuth2SecurityScheme(
                    new()
                    {
                        ClientCredentials = new(
                            new(
                                $"{ServiceDiscoveryUtilities.GetServiceEndpoint(Components.KeyCloak)}/realms/{Environment.GetEnvironmentVariable("Identity__Realm")}/protocol/openid-connect/token"
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
                [JwtBearerDefaults.AuthenticationScheme] = new HttpAuthSecurityScheme(
                    JwtBearerDefaults.AuthenticationScheme,
                    "JWT",
                    "JWT Bearer token authentication"
                ),
            },
            Security = new()
            {
                {
                    $"{JwtBearerDefaults.AuthenticationScheme}",
                    [
                        $"{Services.Chatting}_{Authorization.Actions.Read}",
                        $"{Services.Chatting}_{Authorization.Actions.Write}",
                    ]
                },
            },
            PreferredTransport = AgentTransport.JsonRpc,
        };
}
