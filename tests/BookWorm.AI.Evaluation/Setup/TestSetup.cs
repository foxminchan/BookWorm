using System.ClientModel;
using BookWorm.Constants.Aspire;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using OpenAI;

namespace BookWorm.AI.Evaluation.Setup;

internal static class TestSetup
{
    private static ChatConfiguration? s_chatConfiguration;

    public static ChatConfiguration GetChatConfiguration()
    {
        if (s_chatConfiguration is not null)
        {
            return s_chatConfiguration;
        }

        var openAiClientOptions = EnvironmentVariables.OpenAIBaseUrl is { } baseUrl
            ? new OpenAIClientOptions { Endpoint = new Uri(baseUrl) }
            : null;

        var openAiClient = openAiClientOptions is not null
            ? new OpenAIClient(
                new ApiKeyCredential(EnvironmentVariables.OpenAIApiKey),
                openAiClientOptions
            )
            : new OpenAIClient(new ApiKeyCredential(EnvironmentVariables.OpenAIApiKey));

        var chatClient = openAiClient
            .GetChatClient(Components.OpenAI.OpenAIGpt4oMini)
            .AsIChatClient();

        s_chatConfiguration = new(chatClient);

        return s_chatConfiguration;
    }

    public static async Task<(
        IList<ChatMessage> Messages,
        ChatResponse Response
    )> GetBookstoreConversationAsync(IChatClient chatClient, string userQuestion)
    {
        const string systemPrompt = """
            You are a helpful BookWorm bookstore assistant. You help customers find books,
            provide recommendations, answer questions about store policies, and handle
            customer service inquiries. Be friendly, professional, and knowledgeable.
            Keep your responses concise and relevant to the bookstore domain.
            """;

        IList<ChatMessage> messages =
        [
            new(ChatRole.System, systemPrompt),
            new(ChatRole.User, userQuestion),
        ];

        var chatOptions = new ChatOptions
        {
            Temperature = 0.0f,
            ResponseFormat = ChatResponseFormat.Text,
        };

        var response = await chatClient.GetResponseAsync(messages, chatOptions);

        return (messages, response);
    }
}
