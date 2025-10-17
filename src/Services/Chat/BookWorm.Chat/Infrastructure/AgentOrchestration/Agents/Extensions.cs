using BookWorm.Chassis.AI.Agents;
using BookWorm.Chassis.AI.Extensions;
using BookWorm.Chassis.AI.Middlewares;
using BookWorm.Chat.Infrastructure.ChatHistory;
using BookWorm.Chat.Models;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.A2A;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Agents.AI.Hosting.A2A.AspNetCore;
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
                    .UseFunctionInvocation()
                    .Use(PIIMiddleware.InvokeAsync, null)
                    .Use(GuardrailMiddleware.InvokeAsync, null)
                    .Build(sp);

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
                            Temperature = 0.7f,
                            MaxOutputTokens = 2000,
                            TopP = 0.95f,
                            AllowMultipleToolCalls = true,
                            Tools =
                            [
                                .. mcpClient.ListToolsAsync().Preserve().GetAwaiter().GetResult(),
                                A2AClientFactory
                                    .CreateA2AAgentClient(
                                        Services.Rating,
                                        Constants.Other.Agents.RatingAgent
                                    )
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
                    .UseFunctionInvocation()
                    .Use(GuardrailMiddleware.InvokeAsync, null)
                    .Build(sp);

                var agent = new ChatClientAgent(
                    chatClient,
                    options: new()
                    {
                        Name = key,
                        Instructions = LanguageAgent.Instructions,
                        Description = LanguageAgent.Description,
                        ChatOptions = new() { Temperature = 0.3f, MaxOutputTokens = 500 },
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
                var chatClient = sp.GetRequiredService<IChatClient>()
                    .AsBuilder()
                    .UseFunctionInvocation()
                    .Use(GuardrailMiddleware.InvokeAsync, null)
                    .Build(sp);

                var agent = new ChatClientAgent(
                    chatClient,
                    options: new()
                    {
                        Name = key,
                        Instructions = SentimentAgent.Instructions,
                        Description = SentimentAgent.Description,
                        ChatOptions = new() { Temperature = 0.2f, MaxOutputTokens = 300 },
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
                    .UseFunctionInvocation()
                    .Use(PIIMiddleware.InvokeAsync, null)
                    .Use(GuardrailMiddleware.InvokeAsync, null)
                    .Build(sp);

                var agent = new ChatClientAgent(
                    chatClient,
                    options: new()
                    {
                        Name = key,
                        Instructions = SummarizeAgent.Instructions,
                        Description = SummarizeAgent.Description,
                        ChatOptions = new() { Temperature = 0.4f, MaxOutputTokens = 800 },
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
            RouterAgent.Name,
            (sp, key) =>
            {
                var chatClient = sp.GetRequiredService<IChatClient>()
                    .AsBuilder()
                    .UseFunctionInvocation()
                    .Use(GuardrailMiddleware.InvokeAsync, null)
                    .Build(sp);

                var agent = new ChatClientAgent(
                    chatClient,
                    options: new()
                    {
                        Name = key,
                        Instructions = RouterAgent.Instructions,
                        Description = RouterAgent.Description,
                        ChatOptions = new() { Temperature = 0.1f, MaxOutputTokens = 200 },
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

    public static void MapAgentsDiscovery(this WebApplication app)
    {
        var summarizeAgent = app.Services.GetRequiredKeyedService<AIAgent>(SummarizeAgent.Name);

        app.MapA2A(
            new A2AHostAgent(summarizeAgent, SummarizeAgent.AgentCard).TaskManager!,
            $"/a2a/{SummarizeAgent.Name}"
        );

        var languageAgent = app.Services.GetRequiredKeyedService<AIAgent>(LanguageAgent.Name);

        app.MapA2A(
            new A2AHostAgent(languageAgent, LanguageAgent.AgentCard).TaskManager!,
            $"/a2a/{LanguageAgent.Name}"
        );

        var routerAgent = app.Services.GetRequiredKeyedService<AIAgent>(RouterAgent.Name);

        app.MapA2A(
            new A2AHostAgent(routerAgent, RouterAgent.AgentCard).TaskManager!,
            $"/a2a/{RouterAgent.Name}"
        );

        var sentimentAgent = app.Services.GetRequiredKeyedService<AIAgent>(SentimentAgent.Name);

        app.MapA2A(
            new A2AHostAgent(sentimentAgent, SentimentAgent.AgentCard).TaskManager!,
            $"/a2a/{SentimentAgent.Name}"
        );

        app.MapAgentDiscovery("/agents");
    }
}
