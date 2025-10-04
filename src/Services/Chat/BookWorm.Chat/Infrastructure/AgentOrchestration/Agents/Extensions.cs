using BookWorm.Chassis.AI.Agents;
using BookWorm.Chassis.AI.Extensions;
using BookWorm.Chat.Infrastructure.ChatHistory;
using BookWorm.Chat.Models;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Extensions.VectorData;

namespace BookWorm.Chat.Infrastructure.AgentOrchestration.Agents;

internal static class Extensions
{
    public static void AddAgents(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddA2AClient(
            Constants.Other.Agents.RatingAgent,
            $"{Protocols.HttpOrHttps}://{Services.Rating}",
            "a2a"
        );

        builder.AddAIAgent(
            BookAgent.Name,
            (sp, key) =>
            {
                var chatClient = sp.GetRequiredService<IChatClient>();
                var mcpClient = sp.GetRequiredService<McpClient>();
                var a2aAgent = sp.GetRequiredService<A2AAgentClient>();

                var agent = new ChatClientAgent(
                    chatClient,
                    options: new()
                    {
                        Name = key,
                        Instructions = BookAgent.Instructions,
                        Description = BookAgent.Description,
                        ChatOptions = new()
                        {
                            Tools =
                            [
                                .. mcpClient.ListToolsAsync().Preserve().GetAwaiter().GetResult(),
                                a2aAgent
                                    .GetAIAgent(Constants.Other.Agents.RatingAgent)
                                    .AsAIFunction(),
                            ],
                        },
                        ChatMessageStoreFactory = ctx => new VectorChatMessageStore(
                            sp.GetRequiredService<AppSettings>(),
                            sp.GetRequiredService<VectorStoreCollection<Guid, ChatHistoryItem>>(),
                            sp.GetRequiredService<ClaimsPrincipal>(),
                            ctx.SerializedState,
                            ctx.JsonSerializerOptions
                        ),
                    }
                );

                return agent;
            }
        );

        builder.AddAIAgent(
            LanguageAgent.Name,
            (sp, key) =>
            {
                var chatClient = sp.GetRequiredService<IChatClient>();

                var agent = new ChatClientAgent(
                    chatClient,
                    options: new()
                    {
                        Name = key,
                        Instructions = LanguageAgent.Instructions,
                        Description = LanguageAgent.Description,
                        ChatMessageStoreFactory = ctx => new VectorChatMessageStore(
                            sp.GetRequiredService<AppSettings>(),
                            sp.GetRequiredService<VectorStoreCollection<Guid, ChatHistoryItem>>(),
                            sp.GetRequiredService<ClaimsPrincipal>(),
                            ctx.SerializedState,
                            ctx.JsonSerializerOptions
                        ),
                    }
                );

                return agent;
            }
        );

        builder.AddAIAgent(
            SentimentAgent.Name,
            (sp, key) =>
            {
                var chatClient = sp.GetRequiredService<IChatClient>();

                var agent = new ChatClientAgent(
                    chatClient,
                    options: new()
                    {
                        Name = key,
                        Instructions = SentimentAgent.Instructions,
                        Description = SentimentAgent.Description,
                        ChatMessageStoreFactory = ctx => new VectorChatMessageStore(
                            sp.GetRequiredService<AppSettings>(),
                            sp.GetRequiredService<VectorStoreCollection<Guid, ChatHistoryItem>>(),
                            sp.GetRequiredService<ClaimsPrincipal>(),
                            ctx.SerializedState,
                            ctx.JsonSerializerOptions
                        ),
                    }
                );

                return agent;
            }
        );

        builder.AddAIAgent(
            SummarizeAgent.Name,
            (sp, key) =>
            {
                var chatClient = sp.GetRequiredService<IChatClient>();

                var agent = new ChatClientAgent(
                    chatClient,
                    options: new()
                    {
                        Name = key,
                        Instructions = SummarizeAgent.Instructions,
                        Description = SummarizeAgent.Description,
                        ChatMessageStoreFactory = ctx => new VectorChatMessageStore(
                            sp.GetRequiredService<AppSettings>(),
                            sp.GetRequiredService<VectorStoreCollection<Guid, ChatHistoryItem>>(),
                            sp.GetRequiredService<ClaimsPrincipal>(),
                            ctx.SerializedState,
                            ctx.JsonSerializerOptions
                        ),
                    }
                );

                return agent;
            }
        );
    }
}
