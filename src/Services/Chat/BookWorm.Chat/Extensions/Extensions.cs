using System.Security.Claims;
using BookWorm.Chassis.Security.Extensions;
using BookWorm.Chat.Agents.BookSearch;
using BookWorm.Chat.Agents.CustomerSupport;
using BookWorm.Chat.Agents.Rating;
using BookWorm.Chat.Orchestration;
using BookWorm.ServiceDefaults.Cors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;

namespace BookWorm.Chat.Extensions;

internal static class Extensions
{
    extension(IHostApplicationBuilder builder)
    {
        public void AddApplicationServices()
        {
            var services = builder.Services;

            builder.AddDefaultCors();

            builder.AddDefaultAuthentication();

            services
                .AddAuthorizationBuilder()
                .AddPolicy(Authorization.Policies.Admin, policy =>
                {
                    policy
                        .RequireAuthenticatedUser()
                        .RequireRole(Authorization.Roles.Admin)
                        .RequireScope(
                            $"{Services.Chatting}_{Authorization.Actions.Read}",
                            $"{Services.Chatting}_{Authorization.Actions.Write}"
                        );
                })
                .AddPolicy(Authorization.Policies.Customer, policy =>
                {
                    policy
                        .RequireAuthenticatedUser()
                        .RequireRole(Authorization.Roles.Customer)
                        .RequireScope(
                            $"{Services.Chatting}_{Authorization.Actions.Read}",
                            $"{Services.Chatting}_{Authorization.Actions.Write}"
                        );
                })
                .SetDefaultPolicy(
                    new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .RequireRole(Authorization.Roles.Customer)
                        .RequireScope(
                            $"{Services.Chatting}_{Authorization.Actions.Read}",
                            $"{Services.Chatting}_{Authorization.Actions.Write}"
                        )
                        .Build()
                );

            // Configure Blob Storage client for chat session storage
            builder.AddAzureBlobContainerClient(
                Components.Azure.Storage.BlobContainer(Services.Chatting)
            );
            services.AddHttpContextAccessor();
            services.UseClaimsBasedAgentIsolation();

            // Add exception handlers
            services.AddNotFoundExceptionHandler();
            services.AddGlobalExceptionHandler();
            services.AddProblemDetails();

            builder.AddRateLimiting();

            services.AddVersioning();
            services.AddEndpoints(typeof(IChatApiMarker));

            // Configure ClaimsPrincipal
            services.AddTransient(s =>
                s.GetRequiredService<IHttpContextAccessor>().HttpContext?.User
                ?? new ClaimsPrincipal()
            );

            // AI infrastructure (chat client, telemetry, protocols)
            builder.AddAIInfrastructure();

            // Agent governance (policy enforcement, identity, rings, injection detection)
            builder.AddAgentGovernance("Policies/chat-agents.yaml");

            // Agent discovery client for locating
            services.AddAgentDiscoveryClient(
                HttpUtilities
                    .AsUrlBuilder()
                    .WithScheme(Http.Schemes.HttpOrHttps)
                    .WithHost(Services.Rating)
                    .Build()
            );

            // Register each agent as a self-contained vertical slice
            builder.AddBookAgent();
            builder.AddQAAgent();
            builder.AddRatingAgent();

            // Compose the multi-agent workflow
            builder.AddChatWorkflow();
        }
    }
}
