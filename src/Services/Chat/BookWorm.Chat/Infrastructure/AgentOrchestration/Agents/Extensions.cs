using BookWorm.Chassis.AI.Agents;
using BookWorm.Chassis.AI.Extensions;
using BookWorm.Chassis.AI.Middlewares;
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

        services.AddHttpClient<AgentDiscoveryClient>(client =>
            client.BaseAddress = new($"{Protocols.HttpOrHttps}://{Services.Rating}")
        );

        builder.AddAIAgent(
            BookAgent.Name,
            (sp, key) =>
            {
                var chatClient = sp.GetRequiredService<IChatClient>()
                    .AsBuilder()
                    .Use(PIIMiddleware.InvokeAsync, null)
                    .Use(GuardrailMiddleware.InvokeAsync, null)
                    .Build();

                var mcpClient = sp.GetRequiredService<McpClient>();

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
                                A2AClientExtensions
                                    .GetA2AAgent(Services.Rating, key)
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
                var chatClient = sp.GetRequiredService<IChatClient>()
                    .AsBuilder()
                    .Use(GuardrailMiddleware.InvokeAsync, null)
                    .Build();

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
                var chatClient = sp.GetRequiredService<IChatClient>()
                    .AsBuilder()
                    .Use(PIIMiddleware.InvokeAsync, null)
                    .Use(GuardrailMiddleware.InvokeAsync, null)
                    .Build();

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
