using AgentGovernance;
using BookWorm.Chassis.AI.Agents;
using BookWorm.Chassis.AI.Extensions;
using BookWorm.Chassis.AI.Governance;
using BookWorm.Chassis.AI.Middlewares;
using BookWorm.Chassis.AI.Presidio;
using BookWorm.Rating.Tools;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;

namespace BookWorm.Rating.Infrastructure.Agents;

internal static class Extensions
{
    extension(IHostApplicationBuilder builder)
    {
        public void AddAgents()
        {
            var services = builder.Services;

            builder.AddAIServices().WithAITelemetry();
            builder.AddPresidio();

            builder.AddMcpClient(Constants.Aspire.Services.McpTools);

            builder.AddAgentGovernance("Policies/rating-agent.yaml");

            services.AddScoped<ReviewTool>();

            builder.AddAIAgent(
                RatingAgent.Name,
                (sp, key) =>
                {
                    var compactionProvider = RatingCompactionPipelineFactory.Create(
                        sp.GetRequiredService<IChatClient>()
                    );

                    var chatClient = sp.GetRequiredService<IChatClient>()
                        .AsBuilder()
                        .UsePIIMiddleware(sp)
                        .UseGuardrailMiddleware()
                        .UseAIContextProviders(compactionProvider)
                        .Build(sp);

                    using var spScope = sp.CreateScope();
                    var reviewPlugin = spScope.ServiceProvider.GetRequiredService<ReviewTool>();
                    var mcpClient = sp.GetRequiredService<McpClient>();

                    var agent = new ChatClientAgent(
                        chatClient,
                        options: new()
                        {
                            Name = key,
                            Description = RatingAgent.Description,
                            // Review lookup tools are read-only queries, safe to run concurrently.
                            AllowConcurrentInvocation = true,
                            ChatOptions = new()
                            {
                                Instructions = RatingAgent.Instructions,
                                Temperature = 0.4f,
                                MaxOutputTokens = 1500,
                                AllowMultipleToolCalls = true,
                                Tools = [.. reviewPlugin.AsAITools()],
                            },
                        }
                    );

                    return agent
                        .WithRefreshingMcpTools(mcpClient, toolName => toolName == "get_book")
                        .WithBookWormGovernance(sp, key);
                }
            ).WithInMemorySessionStore();
        }
    }

    extension(WebApplication app)
    {
        public void MapAgentsDiscovery()
        {
            app.MapAgentDiscovery("/agents");

            app.MapAGUIServer(RatingAgent.Name, "/ag-ui")
                .WithSummary("Interactive AI Agent")
                .WithTags(nameof(RatingAgent));
        }
    }
}
