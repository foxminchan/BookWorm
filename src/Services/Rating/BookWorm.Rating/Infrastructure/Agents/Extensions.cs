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
    private const string DefinitionFile = "Agents/rating-agent.yaml";

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

            builder
                .AddAIAgent(
                    Constants.Other.Agents.RatingAgent,
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

                        var (options, toolNames) = AgentDefinitionLoader.Load(DefinitionFile, key);
                        var mcpTools = mcpClient.GetAgentTools(toolNames);

                        options.AllowConcurrentInvocation = true;
                        options.ChatOptions!.Tools = [.. reviewPlugin.AsAITools(), .. mcpTools];

                        var agent = new ChatClientAgent(chatClient, options);

                        return agent.WithBookWormGovernance(sp, key);
                    }
                )
                .WithInMemorySessionStore();
        }
    }

    extension(WebApplication app)
    {
        public void MapAgentsDiscovery()
        {
            app.MapAgentDiscovery("/agents");

            app.MapAGUIServer(Constants.Other.Agents.RatingAgent, "/ag-ui")
                .WithSummary("Interactive AI Agent")
                .WithTags(nameof(Constants.Other.Agents.RatingAgent));
        }
    }
}
