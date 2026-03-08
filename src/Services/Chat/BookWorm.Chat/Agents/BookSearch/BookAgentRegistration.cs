using BookWorm.Chassis.AI.Agents;
using BookWorm.Chassis.AI.Middlewares;
using BookWorm.Chassis.AI.Presidio;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;

namespace BookWorm.Chat.Agents.BookSearch;

internal static class BookAgentRegistration
{
    public static void AddBookAgent(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddHttpClient<AgentDiscoveryClient>(client =>
            client.BaseAddress = new(
                HttpUtilities
                    .AsUrlBuilder()
                    .WithScheme(Http.Schemes.HttpOrHttps)
                    .WithHost(Services.Rating)
                    .Build()
            )
        );

        builder.AddAIAgent(
            BookAgentDefinition.Name,
            (sp, key) =>
            {
                var presidioService = sp.GetRequiredService<IPresidioService>();
                var chatClient = sp.GetRequiredService<IChatClient>()
                    .AsBuilder()
                    .Use(PIIMiddleware.Create(presidioService), null)
                    .Use(GuardrailMiddleware.InvokeAsync, null)
                    .Build(sp);

                var mcpUrl = HttpUtilities
                    .AsUrlBuilder()
                    .WithBase(
                        ServiceDiscoveryUtilities.GetRequiredServiceEndpoint(Services.McpTools)
                    )
                    .WithPath("mcp")
                    .Build();

                var mcpTool = new HostedMcpServerTool(Services.McpTools, mcpUrl)
                {
                    AllowedTools = ["search_catalog"],
                    ApprovalMode = HostedMcpServerToolApprovalMode.NeverRequire,
                };

                var skillsProvider = new FileAgentSkillsProvider(
                    Path.Combine(AppContext.BaseDirectory, "Skills", "book-catalog"),
                    loggerFactory: sp.GetService<ILoggerFactory>()
                );

                var agent = new ChatClientAgent(
                    chatClient,
                    options: new()
                    {
                        Name = key,
                        Description = BookAgentDefinition.Description,
                        AIContextProviders = [skillsProvider],
                        ChatOptions = new()
                        {
                            Instructions = BookAgentDefinition.Instructions,
                            Temperature = 0.7f,
                            MaxOutputTokens = 2000,
                            TopP = 0.95f,
                            AllowMultipleToolCalls = true,
                            Tools = [mcpTool],
                        },
                    }
                );

                return agent;
            }
        );
    }
}
