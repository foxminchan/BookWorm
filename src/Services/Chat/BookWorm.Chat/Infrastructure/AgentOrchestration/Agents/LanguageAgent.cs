using A2A;
using BookWorm.Chassis.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;

namespace BookWorm.Chat.Infrastructure.AgentOrchestration.Agents;

internal static class LanguageAgent
{
    public const string Name = Constants.Other.Agents.LanguageAgent;

    public const string Description =
        "An agent that detects user input language and translates it to English for better context understanding.";

    public const string Instructions = """
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
