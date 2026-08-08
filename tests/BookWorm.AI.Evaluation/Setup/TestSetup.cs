using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;
using BookWorm.Constants.Aspire;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;

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

        var endpoint = new Uri(EnvironmentVariables.AzureOpenAIEndpoint);

        var azureClient = EnvironmentVariables.AzureOpenAIApiKey is { } apiKey
            ? new AzureOpenAIClient(endpoint, new AzureKeyCredential(apiKey))
            : new AzureOpenAIClient(endpoint, new DefaultAzureCredential());

        var chatClient = azureClient
            .GetChatClient(Components.OpenAI.OpenAIGpt56Sol)
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
