using BookWorm.Constants.Other;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;

namespace BookWorm.Chat.Orchestration;

internal static class EndpointMapping
{
    extension(WebApplication app)
    {
        public void MapAgentsDiscovery()
        {
            app.MapAgentDiscovery("/agents");

            app.MapAGUIServer(Workflows.Chat, "/ag-ui")
                .WithSummary("Interactive AI Agent")
                .WithTags(nameof(Chat))
                .RequireAuthorization(Authorization.Policies.User)
                .RequirePerUserRateLimit();
        }
    }
}
